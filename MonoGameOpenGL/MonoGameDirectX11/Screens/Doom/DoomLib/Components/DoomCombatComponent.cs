using Microsoft.Xna.Framework;
using MonoGameEngineCore.DoomLib;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameDirectX11.Screens.Doom.DoomLib
{
    public class DoomCombatComponent : IComponent, IUpdateable
    {

        public bool Enabled
        {
            get; set;
        }
        public GameObject ParentObject
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
        Dictionary<int, GameObject> worldObjects;
        float turnFrquency = 80;
        float shootFrequency = 500;
        float minimumCombatDistance = 15;
        bool turning;
        int shotsFired;

        public DoomCombatComponent(DoomMapHandler mapHandler, DoomAPIHandler apiHandler, Dictionary<int, GameObject> worldObjects)
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
            List<GameObject> visibleMonsters = new List<GameObject>();
            foreach (GameObject monster in monsters)
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


            GameObject target = null;
            float closestDist = float.MaxValue;
            foreach (GameObject vm in visibleMonsters)
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

            float heading = MathHelper.ToDegrees(MonoMathHelper.GetHeadingFromVector((toTarget).ToVector2XZ()));
            heading = (heading + 360) % 360;

            if (dot > 0.05f)
            {
                if (!turning)
                {
                    //TurnLeft(2);
                    TurnLeftToHeading(heading);
                }

            }
            if (dot < -0.05f)
            {
                if (!turning)
                {
                    //TurnRight(2);
                    TurnRightToHeading(heading);
                }

            }

            if (dot > -0.3f && dot < 0.3f)
            {
                //the node we need is right behind us. Instigate a turn.
                if (MonoMathHelper.AlmostEquals(180d, angle, 10))
                {
                    TurnLeftToHeading(heading);
                    return;
                }

                Shoot();

                if (shotsFired > 8)
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

        public void TurnRightToHeading(float heading)
        {
            if ((DateTime.Now - lastTurn).TotalMilliseconds < turnFrquency)
                return;

            turning = true;
            lastTurn = DateTime.Now;

            var right = new RestRequest("player/turn", Method.POST);
            right.RequestFormat = DataFormat.Json;
            right.AddBody(new PlayerTurnAction() { type = "right", target_angle = heading });
            apiHandler.EnqueueRequest(false, right, x =>
            {
                turning = false;
            });
        }

        public void TurnLeftToHeading(float heading)
        {
            if ((DateTime.Now - lastTurn).TotalMilliseconds < turnFrquency)
                return;

            turning = true;
            lastTurn = DateTime.Now;

            var right = new RestRequest("player/turn", Method.POST);
            right.RequestFormat = DataFormat.Json;
            right.AddBody(new PlayerTurnAction() { type = "left", target_angle = heading });
            apiHandler.EnqueueRequest(false, right, x =>
            {
                turning = false;
            });
        }


    }
}
