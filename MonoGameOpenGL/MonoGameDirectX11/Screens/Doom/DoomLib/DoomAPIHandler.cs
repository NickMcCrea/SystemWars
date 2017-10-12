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
        private RestRequest left;
        private RestRequest right;
        private RestRequest forward;
        DateTime lastMovement = DateTime.Now;
        float movementFrequency = 200;

        public DoomMovementComponent(DoomMapHandler mapHandler, DoomAPIHandler apiHandler)
        {
            this.mapHandler = mapHandler;
            this.apiHandler = apiHandler;
        }

        public void Initialise()
        {
            Enabled = true;
            aStar = new AStar();
            left = new RestRequest("player/actions", Method.POST);
            left.RequestFormat = DataFormat.Json;
            left.AddBody(new PlayerAction() { type = "turn-left", amount = 2 });

            right = new RestRequest("player/actions", Method.POST);
            right.RequestFormat = DataFormat.Json;
            right.AddBody(new PlayerAction() { type = "turn-right", amount = 2 });

            forward = new RestRequest("player/actions", Method.POST);
            forward.RequestFormat = DataFormat.Json;
            forward.AddBody(new PlayerAction() { type = "forward", amount = 2 });
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
                NavigationNode currentNode = path[0];


                Vector3 toTarget = currentNode.WorldPosition - ParentObject.Transform.AbsoluteTransform.Translation;
                if (toTarget.Length() < 0.5f)
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

                if ((DateTime.Now - lastMovement).TotalMilliseconds < movementFrequency)
                    return;

                if (dot > 0.1f)
                {
                    if (!turning)
                    {

                        turning = true;
                        lastMovement = DateTime.Now;

                        left = new RestRequest("player/actions", Method.POST);
                        left.RequestFormat = DataFormat.Json;
                        left.AddBody(new PlayerAction() { type = "turn-left", amount = 2 });
                        apiHandler.EnqueueRequest(false, left, x => 
                        {
                            turning = false;
                        });
                    }
                }
                if (dot < -0.1f )
                {
                    if (!turning)
                    {
                        turning = true;
                        lastMovement = DateTime.Now;
                        right = new RestRequest("player/actions", Method.POST);
                        right.RequestFormat = DataFormat.Json;
                        right.AddBody(new PlayerAction() { type = "turn-right", amount = 2 });
                        apiHandler.EnqueueRequest(false, right, x => 
                        {
                            turning = false;
                        });
                    }
                }

                if(dot > -0.1f && dot < 0.1f)
                {
                    if (!moving)
                    {
                        lastMovement = DateTime.Now;
                        apiHandler.EnqueueRequest(false, forward, x =>
                        {
                            moving = false;
                        });
                    }
                }

            }
        }

        public void PathToPoint(Vector3 target)
        {
            NavigationNode startNode = mapHandler.FindNavPoint(ParentObject.Transform.AbsoluteTransform.Translation);
            NavigationNode endnode = mapHandler.FindNavPoint(target);
            bool result;
            path = aStar.FindPath(startNode, endnode, out result);

        }
    }

}
