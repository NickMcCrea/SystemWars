using Microsoft.Xna.Framework;
using MonoGameDirectX11.Screens.Doom;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using NickLib.Pathfinding;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameEngineCore.DoomLib
{


    public class DoomAPIHandler
    {


        private bool awaitRequestReturn = true;
        private static object _listLock = new object();
        private RestClient client;
        private Queue<RestResponse> completedResponses;
        private LinkedList<RestRequest> unsentRequests;
        public List<FrquentRequestHandler> frequentRequests;
        RestRequest playerRequest;
        RestRequest worldRequest;
        Dictionary<string, CoolDownHandler> coolDownTimers;


        public class CoolDownHandler
        {
            public int CoolDown;
            public DateTime LastExecuted;
        }
        public class FrquentRequestHandler
        {
            public DateTime TimeSinceLastRequestPlaced { get; set; }
            public int Fequency { get; set; }
            public RestRequest Request { get; set; }
            public Action<RestResponse> Action { get; set; }

            public FrquentRequestHandler()
            {


            }

        }

        private bool inFlight = false;

        public DoomAPIHandler(string baseUrl, int port)
        {
            client = new RestClient(baseUrl + ":" + port + "/api/");
            completedResponses = new Queue<RestResponse>();
            unsentRequests = new LinkedList<RestRequest>();
            playerRequest = new RestRequest("player");
            worldRequest = new RestRequest("world/objects");
            frequentRequests = new List<FrquentRequestHandler>();
            coolDownTimers = new Dictionary<string, CoolDownHandler>();

        }

        public void EnqueueRequest(bool priority, RestRequest request, Action<RestResponse> responseAction)
        {
            request.UserState = responseAction;

            if (priority)
                unsentRequests.AddFirst(request);
            else
                unsentRequests.AddLast(request);
        }

        public void EnqueueRequest(bool priority, string tag, RestRequest request)
        {
            request.UserState = tag;

            if (priority)
                unsentRequests.AddFirst(request);
            else
                unsentRequests.AddLast(request);


        }

        public void CreateRegularRequest(int frequencyInMilliseconds, RestRequest request, Action<RestResponse> response)
        {
            FrquentRequestHandler rq = new FrquentRequestHandler()
            {
                Request = request,
                Fequency = frequencyInMilliseconds,
                Action = response,
                TimeSinceLastRequestPlaced = DateTime.Now
            };

            frequentRequests.Add(rq);

        }

        public void Update()
        {
            //don't send anything if we have an outstanding request. Restful doom don't like it!
            if (!inFlight || !awaitRequestReturn)
            {
                if (unsentRequests.Count > 0)
                {

                    //get the first
                    RestRequest unsentRequest = unsentRequests.First.Value;
                    unsentRequests.RemoveFirst();

                    inFlight = true;
                    client.ExecuteAsync(unsentRequest, response =>
                    {
                        lock (_listLock)
                        {
                            completedResponses.Enqueue(response as RestResponse);
                            inFlight = false;
                        }
                    });
                }
            }


            foreach (FrquentRequestHandler rq in frequentRequests)
            {
                var timeSinceInvoked = DateTime.Now - rq.TimeSinceLastRequestPlaced;
                if (timeSinceInvoked.TotalMilliseconds > rq.Fequency)
                {
                    rq.TimeSinceLastRequestPlaced = DateTime.Now;
                    EnqueueRequest(false, rq.Request, rq.Action);
                }
            }

            InvokeResponseActions();
        }

        private void InvokeResponseActions()
        {
            if (completedResponses.Count == 0)
                return;

            var request = completedResponses.Peek().Request as RestRequest;

            if (request.UserState is Action<RestResponse>)
            {
                var action = request.UserState as Action<RestResponse>;
                action.Invoke(completedResponses.Dequeue());
            }
        }

        public RestResponse GetNextResponse()
        {
            lock (_listLock)
            {
                if (completedResponses.Count > 0)
                    return completedResponses.Dequeue();
            }

            return null;
        }

        public void RequestPlayerDetails()
        {
            EnqueueRequest(false, "playerDetails", playerRequest);
        }

        public void RequestAllWorldDetails()
        {
            EnqueueRequest(false, "worldObjects", worldRequest);
        }

        public void MovePlayerForward(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "forward", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void MovePlayerBackward(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "backward", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void TurnLeft(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "turn-left", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void TurnRight(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "turn-right", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void StrafeLeft(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "strafe-left", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        public void StrafeRight(float moveAmount)
        {

            var forwardRequest = new RestRequest("player/actions", Method.POST);
            forwardRequest.RequestFormat = DataFormat.Json;
            forwardRequest.AddBody(new PlayerAction() { type = "strafe-right", amount = moveAmount });
            EnqueueRequest(true, "", forwardRequest);
        }

        internal void EnqueueCoolDownRequest(string type, RestRequest request, int coolDownInMilliseconds, Action<RestResponse> responseAction)
        {
            if (!coolDownTimers.ContainsKey(type))
            {
                coolDownTimers.Add(type, new CoolDownHandler() { LastExecuted = DateTime.Now, CoolDown = coolDownInMilliseconds });
                EnqueueRequest(false, request, responseAction);
            }
            else
            {
                //get the timer
                var coolDownThing = coolDownTimers[type];

                if ((DateTime.Now - coolDownThing.LastExecuted).TotalMilliseconds > coolDownThing.CoolDown)
                {
                    coolDownThing.LastExecuted = DateTime.Now;
                    EnqueueRequest(false, request, responseAction);
                }
            }
        }
    }


    public class PlayerAction
    {
        public string type { get; set; }
        public float amount { get; set; }
    }

    public class DoomComponent : IComponent
    {

        public string DoomType { get; set; }
        public double Health { get; set; }
        public double Angle { get; set; }
        public double Distance { get; set; }
        public bool ForwardHitVector { get; set; }
        public bool LeftHitVector { get; set; }
        public bool RightHightVector { get; set; }
        public float HitVectorSize { get; set; }
        public GameObject.GameObject ParentObject
        {
            get; set;
        }

        public void Initialise()
        {

        }

        public void PostInitialise()
        {

        }
    }

    public class DoomMovementComponent : IComponent, IUpdateable
    {
        private AStar aStar;
        public List<NavigationNode> path;
        public bool Enabled
        {
            get; set;
        }
        public GameObject.GameObject ParentObject
        {
            get; set;
        }
        public int UpdateOrder
        {
            get; set;
        }
        private DoomMapHandler mapHandler;
        private DoomAPIHandler apiHandler;
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        DateTime lastMovement = DateTime.Now;
        DateTime lastTurn = DateTime.Now;
        float movementFrequency = 500;
        float turnFrquency = 100;
        float minDistanceToNode = 0.4f;

        public DoomMovementComponent(DoomMapHandler mapHandler, DoomAPIHandler apiHandler)
        {
            this.mapHandler = mapHandler;
            this.apiHandler = apiHandler;
        }

        public void Initialise()
        {
            Enabled = true;
            aStar = new AStar();
            var use = new RestRequest("player/actions", Method.POST);
            use.RequestFormat = DataFormat.Json;
            use.AddBody(new PlayerAction() { type = "use", amount = 1 });
            apiHandler.CreateRegularRequest(10000, use, x => { });

        }

        public void PostInitialise()
        {

        }

        bool turning = false;
        bool moving = false;
        public void Update(GameTime gameTime)
        {
            if (path == null)
                return;

            if (path.Count > 0)
            {
                if (path.Count > 1)
                    OptimisePath();


                NavigationNode currentNode = path[0];

                //check we can still see our node. Sometimes we can't, after 
                //applying an unexpectedly large move.
                if (!CanStillPathToNode(currentNode))
                {
                    PathToPoint(path[path.Count - 1].WorldPosition);
                }


                Vector3 toTarget = currentNode.WorldPosition - ParentObject.Transform.AbsoluteTransform.Translation;
                if (toTarget.Length() < minDistanceToNode)
                {
                    path.RemoveAt(0);
                    return;
                }

                toTarget.Y = 0;
                toTarget.Normalize();


                Vector3 rightV = ParentObject.Transform.AbsoluteTransform.Right;
                rightV.Y = 0;
                rightV.Normalize();

                Vector3 forwardVec = ParentObject.Transform.AbsoluteTransform.Forward;
                forwardVec.Y = 0;
                forwardVec.Normalize();

                float dot = Vector3.Dot(toTarget, rightV);

                //game units are roughly 105 in a circle.
                //so 1 unit = 360 / 105 degrees
                //1 degree = 105 / 360
                var angle = MathHelper.ToDegrees(MonoMathHelper.GetAngleBetweenVectors(toTarget, forwardVec));
                float spinAmount = angle * (105f / 360);


                DebugText.Write(dot.ToString());
                DebugText.Write(angle.ToString());
                DebugText.Write(spinAmount.ToString());



                if (dot > 0.1f)
                {
                    if (!turning)
                    {
                        TurnLeft(2);
                    }
                }
                if (dot < -0.1f)
                {
                    if (!turning)
                    {

                        TurnRight(2);
                    }
                }

                if (dot > -0.1f && dot < 0.1f)
                {
                    //the node we need is right behind us. Instigate a turn.
                    if (MonoMathHelper.AlmostEquals(180d, angle, 10))
                    {
                        TurnLeft(3);
                        return;
                    }

                    if (!moving)
                    {
                        MoveForward(4);
                    }
                }


                FeelForward();

            }
            else
                path = null;
        }

        private bool CanStillPathToNode(NavigationNode currentNode)
        {

            Vector3 pos = ParentObject.Transform.AbsoluteTransform.Translation;
            pos.Y = 0;

            Vector3 toNode = currentNode.WorldPosition - pos;

            Vector3 perpendicularVec = Vector3.Cross(toNode, Vector3.Up);
            perpendicularVec.Y = 0;
            //width
            perpendicularVec *= 0.01f;


            //if we pass all 3 of these tests, this means we have clear LOS 
            //to this node, wide enough to squeeze through. So we can skip prior nodes.
            if (!mapHandler.IntersectsLevel(pos, currentNode.WorldPosition))
            {
                if (!mapHandler.IntersectsLevel(pos + perpendicularVec, currentNode.WorldPosition + perpendicularVec))
                {
                    if (!mapHandler.IntersectsLevel(pos - perpendicularVec, currentNode.WorldPosition - perpendicularVec))
                    {

                        return true;
                    }
                    else
                        return false;

                }
                else
                    return false;

            }
            else
                return false;

        }

        private void FeelForward()
        {
            DoomComponent playerDoomComponent = ParentObject.GetComponent<DoomComponent>();

            Vector3 pos = ParentObject.Transform.AbsoluteTransform.Translation;
            Vector3 forwardVec = ParentObject.Transform.AbsoluteTransform.Translation + ParentObject.Transform.AbsoluteTransform.Forward * playerDoomComponent.HitVectorSize;
            Vector3 rightVec = MonoMathHelper.RotateAroundPoint(forwardVec, ParentObject.Transform.AbsoluteTransform.Translation, Vector3.Up, MathHelper.PiOver4 / 2);
            Vector3 leftVec = MonoMathHelper.RotateAroundPoint(forwardVec, ParentObject.Transform.AbsoluteTransform.Translation, Vector3.Up, -MathHelper.PiOver4 / 2);
            pos.Y = 0;
            forwardVec.Y = 0;
            rightVec.Y = 0;
            leftVec.Y = 0;



            Color forwardColor = Color.Red;
            Color leftColor = Color.Red;
            Color rightColor = Color.Red;


            if (mapHandler.IntersectsLevel(pos, forwardVec))
            {
                forwardColor = Color.Blue;

            }
            if (mapHandler.IntersectsLevel(pos, rightVec))
            {
                rightColor = Color.Blue;

                var strafe = new RestRequest("player/actions", Method.POST);
                strafe.RequestFormat = DataFormat.Json;
                strafe.AddBody(new PlayerAction() { type = "strafe-left", amount = 2 });
                apiHandler.EnqueueCoolDownRequest("strafe-left", strafe, 400, x => { });

            }
            if (mapHandler.IntersectsLevel(pos, leftVec))
            {
                leftColor = Color.Blue;

                var strafe = new RestRequest("player/actions", Method.POST);
                strafe.RequestFormat = DataFormat.Json;
                strafe.AddBody(new PlayerAction() { type = "strafe-right", amount = 2 });
                apiHandler.EnqueueCoolDownRequest("strafe-right", strafe, 400, x => { });
            }

            DebugShapeRenderer.AddLine(pos, forwardVec, forwardColor);
            DebugShapeRenderer.AddLine(pos, leftVec, leftColor);
            DebugShapeRenderer.AddLine(pos, rightVec, rightColor);
        }

        private void OptimisePath()
        {
            //tries to look ahead and skip nodes we have clear unobstructed path to already

            Vector3 pos = ParentObject.Transform.AbsoluteTransform.Translation;
            pos.Y = 0;
            int indexToRemove = -1;
            for (int i = 1; i < path.Count - 1; i++)
            {
                NavigationNode nodeToExamine = path[i];


                bool centerClear = false;
                bool leftClear = false;
                bool rightClear = false;

                Vector3 toNode = nodeToExamine.WorldPosition - pos;

                Vector3 perpendicularVec = Vector3.Cross(toNode, Vector3.Up);
                perpendicularVec.Y = 0;
                //width
                perpendicularVec *= 0.01f;


                //if we pass all 3 of these tests, this means we have clear LOS 
                //to this node, wide enough to squeeze through. So we can skip prior nodes.
                if (!mapHandler.IntersectsLevel(pos, nodeToExamine.WorldPosition))
                {
                    centerClear = true;
                    if (!mapHandler.IntersectsLevel(pos + perpendicularVec, nodeToExamine.WorldPosition + perpendicularVec))
                    {
                        leftClear = true;
                        if (!mapHandler.IntersectsLevel(pos - perpendicularVec, nodeToExamine.WorldPosition - perpendicularVec))
                        {
                            rightClear = true;
                            indexToRemove = i;
                        }
                        else
                            break;
                    }
                    else
                        break;
                }
                else
                    break;



                DebugShapeRenderer.AddLine(pos, nodeToExamine.WorldPosition, rightClear ? Color.Blue : Color.Red);
                DebugShapeRenderer.AddLine(pos + perpendicularVec, nodeToExamine.WorldPosition + perpendicularVec, leftClear ? Color.Blue : Color.Red);
                DebugShapeRenderer.AddLine(pos - perpendicularVec, nodeToExamine.WorldPosition - perpendicularVec, rightClear ? Color.Blue : Color.Red);
            }

            if (indexToRemove != -1)
                path.RemoveRange(0, indexToRemove);


        }

        public void MoveForward(float amountToMove)
        {
            if ((DateTime.Now - lastMovement).TotalMilliseconds < movementFrequency)
                return;

            lastMovement = DateTime.Now;

            var forward = new RestRequest("player/actions", Method.POST);
            forward.RequestFormat = DataFormat.Json;
            forward.AddBody(new PlayerAction() { type = "forward", amount = amountToMove });
            apiHandler.EnqueueRequest(false, forward, x =>
            {
                moving = false;
            });
        }

        public void TurnRight(float amountToTurn)
        {
            if ((DateTime.Now - lastTurn).TotalMilliseconds < turnFrquency)
                return;

            turning = true;
            lastTurn = DateTime.Now;

            var right = new RestRequest("player/actions", Method.POST);
            right.RequestFormat = DataFormat.Json;
            right.AddBody(new PlayerAction() { type = "turn-right", amount = amountToTurn });
            apiHandler.EnqueueRequest(false, right, x =>
            {
                turning = false;
            });
        }

        public void TurnLeft(float amountToTurn)
        {
            if ((DateTime.Now - lastTurn).TotalMilliseconds < turnFrquency)
                return;

            turning = true;
            lastTurn = DateTime.Now;

            var left = new RestRequest("player/actions", Method.POST);
            left.RequestFormat = DataFormat.Json;
            left.AddBody(new PlayerAction() { type = "turn-left", amount = amountToTurn });
            apiHandler.EnqueueRequest(false, left, x =>
            {
                turning = false;
            });
        }

        public void PathToPoint(Vector3 target)
        {
            NavigationNode startNode = mapHandler.FindNavPoint(ParentObject.Transform.AbsoluteTransform.Translation);
            NavigationNode endnode = mapHandler.FindNavPoint(target);

            if (startNode == null || endnode == null)
                return;

            bool result;
            path = aStar.FindPath(startNode, endnode, out result);

            //add a node on for our final target to make sure we reach it.
            NavigationNode n = new NavigationNode();
            n.Navigable = true;
            n.WorldPosition = target;
            n.Neighbours.Add(path[path.Count - 1]);
            path[path.Count - 1].Neighbours.Add(n);
            path.Add(n);

        }
    }

    public class DoomCombatComponent : IComponent, IUpdateable
    {

        public bool Enabled
        {
            get; set;
        }
        public GameObject.GameObject ParentObject
        {
            get; set;
        }
        public int UpdateOrder
        {
            get; set;
        }
        private DoomMapHandler mapHandler;
        private DoomAPIHandler apiHandler;
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        private RestRequest shootRequest;
        DateTime lastTurn = DateTime.Now;
        DateTime lastShoot = DateTime.Now;
        Dictionary<int, GameObject.GameObject> worldObjects;
        float turnFrquency = 100;
        float shootFrequency = 500;
        float minimumCombatDistance = 14;
        bool turning;
        int shotsFired;

        public DoomCombatComponent(DoomMapHandler mapHandler, DoomAPIHandler apiHandler, Dictionary<int, GameObject.GameObject> worldObjects)
        {
            this.mapHandler = mapHandler;
            this.apiHandler = apiHandler;
            this.worldObjects = worldObjects;

        }

        public void Initialise()
        {
            Enabled = true;
            shootRequest = new RestRequest("player/actions", Method.POST);
            shootRequest.RequestFormat = DataFormat.Json;
            shootRequest.AddBody(new PlayerAction() { type = "shoot", amount = 1 });

        }

        public void PostInitialise()
        {

        }

        bool IsAllUpper(string input)
        {

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == ' ')
                    continue;

                if (!Char.IsUpper(input[i]))
                    return false;
            }

            return true;
        }

        public void Update(GameTime gameTime)
        {
            ParentObject.GetComponent<DoomMovementComponent>().Enabled = true;

            //when in combat, get much more frequent updates on enemy movements.
            apiHandler.frequentRequests.Find(x => x.Request.Resource.Contains("world")).Fequency = 4000;

            //check LOS to monsters. If 
            if (worldObjects == null)
            {
                shotsFired = 0;
                return;
            }
            if (worldObjects.Count == 0)
            {
                shotsFired = 0;
                return;

            }




            //get all the monsters from the world object list.
            //monsters are ALL IN CAPS
            var monsters = worldObjects.Values.Where(x => IsAllUpper(x.GetComponent<DoomComponent>().DoomType)).ToList();

            //forget about the dead ones.
            monsters.RemoveAll(x => x.GetComponent<DoomComponent>().Health <= 0);

            //can we see any of these guys?
            List<GameObject.GameObject> visibleMonsters = new List<GameObject.GameObject>();
            foreach (GameObject.GameObject monster in monsters)
            {

                if (!mapHandler.IntersectsLevel(ParentObject.Transform.AbsoluteTransform.Translation, monster.Transform.AbsoluteTransform.Translation, true))
                {
                    //we can see it. 
                    visibleMonsters.Add(monster);

                    foreach (DoomLine door in mapHandler.Doors)
                    {
                        if (MonoMathHelper.LineIntersection(ParentObject.Transform.AbsoluteTransform.Translation.ToVector2XZ(),
                            monster.Transform.AbsoluteTransform.Translation.ToVector2XZ(),
                            door.start.ToVector2XZ(), door.end.ToVector2XZ()))
                        {

                            //there's a door between us and the monster. remove it
                            if (visibleMonsters.Contains(monster))
                                visibleMonsters.Remove(monster);
                        }
                    }


                }
            }

            if (visibleMonsters.Count == 0)
            {
                shotsFired = 0;
                return;
            }


            //when in combat, get much more frequent updates on enemy movements.
            apiHandler.frequentRequests.Find(x => x.Request.Resource.Contains("world")).Fequency = 500;


            GameObject.GameObject target = null;
            float closestDist = float.MaxValue;
            foreach (GameObject.GameObject vm in visibleMonsters)
            {
                Vector3 toMonsterVec = ParentObject.Transform.AbsoluteTransform.Translation - vm.Transform.AbsoluteTransform.Translation;

                if (toMonsterVec.Length() < closestDist)
                {
                    target = vm;
                    closestDist = toMonsterVec.Length();
                }
            }

            if (closestDist > minimumCombatDistance)
            {
                shotsFired = 0;
                return;
            }

            //disable movement.
            ParentObject.GetComponent<DoomMovementComponent>().Enabled = false;

            DebugShapeRenderer.AddLine(ParentObject.Transform.AbsoluteTransform.Translation,
                target.Transform.AbsoluteTransform.Translation, Color.OrangeRed);


            Vector3 targetPosition = target.Transform.AbsoluteTransform.Translation;

            Vector3 toTarget = targetPosition - ParentObject.Transform.AbsoluteTransform.Translation;
            toTarget.Y = 0;
            toTarget.Normalize();


            Vector3 rightV = ParentObject.Transform.AbsoluteTransform.Right;
            rightV.Y = 0;
            rightV.Normalize();

            Vector3 forwardVec = ParentObject.Transform.AbsoluteTransform.Forward;
            forwardVec.Y = 0;
            forwardVec.Normalize();

            float dot = Vector3.Dot(toTarget, rightV);

            //game units are roughly 105 in a circle.
            //so 1 unit = 360 / 105 degrees
            //1 degree = 105 / 360
            var angle = MathHelper.ToDegrees(MonoMathHelper.GetAngleBetweenVectors(toTarget, forwardVec));


            if (dot > 0.1f)
            {
                if (!turning)
                {
                    TurnLeft(2);
                }

            }
            if (dot < -0.1f)
            {
                if (!turning)
                {
                    TurnRight(2);
                }

            }

            if (dot > -0.1f && dot < 0.1f)
            {
                //the node we need is right behind us. Instigate a turn.
                if (MonoMathHelper.AlmostEquals(180d, angle, 10))
                {
                    TurnLeft(3);
                    return;
                }

                Shoot();

                if(shotsFired > 8)
                {
                    //we keep missing. shift the aim a bit.
                    bool left = RandomHelper.CoinToss();
                    if (left)
                        TurnLeft(1);
                    else
                    {
                        TurnRight(1);
                    }
                    shotsFired = 0;

                }
            }


        }

        private void Shoot()
        {
            if ((DateTime.Now - lastShoot).TotalMilliseconds < shootFrequency)
                return;
            shotsFired++;
            lastShoot = DateTime.Now;
            apiHandler.EnqueueRequest(false, shootRequest, x =>
            {
            

            });
        }

        public void TurnRight(float amountToTurn)
        {
            if ((DateTime.Now - lastTurn).TotalMilliseconds < turnFrquency)
                return;


            lastTurn = DateTime.Now;
            turning = true;
            var right = new RestRequest("player/actions", Method.POST);
            right.RequestFormat = DataFormat.Json;
            right.AddBody(new PlayerAction() { type = "turn-right", amount = amountToTurn });
            apiHandler.EnqueueRequest(false, right, x =>
            {
                turning = false;
            });
        }

        public void TurnLeft(float amountToTurn)
        {
            if ((DateTime.Now - lastTurn).TotalMilliseconds < turnFrquency)
                return;


            lastTurn = DateTime.Now;
            turning = true;
            var left = new RestRequest("player/actions", Method.POST);
            left.RequestFormat = DataFormat.Json;
            left.AddBody(new PlayerAction() { type = "turn-left", amount = amountToTurn });
            apiHandler.EnqueueRequest(false, left, x =>
            {
                turning = false;
            });
        }


    }

}
