using Microsoft.Xna.Framework;
using MonoGameEngineCore.DoomLib;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using NickLib.Pathfinding;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameDirectX11.Screens.Doom.DoomLib
{
    public class DoomMovementComponent : IComponent, IUpdateable
    {
        private AStar aStar;
        public List<NavigationNode> path;
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
        DateTime lastMovement = DateTime.Now;
        DateTime lastTurn = DateTime.Now;
        float movementFrequency = 200;
        float turnFrquency = 80;
        float minDistanceToNode = 0.5f;
        float movementRangeThreshold = 0.3f;
        int movementAttemptsWithNoPositionChange;
        bool turning = false;
        bool moving = false;
        Vector3 lastKnownPositionBeforeMovementCommand;

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
            apiHandler.CreateRegularRequest(5000, use, x => { });

        }

        public void PostInitialise()
        {

        }


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
                if (!CanStillPathToNode(currentNode) && currentNode.WorldPosition != mapHandler.LevelEnd)
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


                float heading = MathHelper.ToDegrees(MonoMathHelper.GetHeadingFromVector((currentNode.WorldPosition - ParentObject.Transform.AbsoluteTransform.Translation).ToVector2XZ()));
                heading = (heading + 360) % 360;
                //DebugText.Write(dot.ToString());
                // DebugText.Write(angle.ToString());
                // DebugText.Write(spinAmount.ToString());
                DebugText.Write("Heading: " + heading.ToString());



                if (dot > 0.1f)
                {
                    if (!turning)
                    {
                        //TurnLeft(2);
                        TurnLeftToHeading(heading);
                    }
                }
                if (dot < -0.1f)
                {
                    if (!turning)
                    {

                        //TurnRight(2);
                        TurnRightToHeading(heading);
                    }
                }

                if (dot > -movementRangeThreshold && dot < movementRangeThreshold)
                {
                    //the node we need is right behind us. Instigate a turn.
                    if (MonoMathHelper.AlmostEquals(180d, angle, 10))
                    {
                        TurnLeft(3);
                        return;
                    }

                    if (!moving)
                    {
                        //we're likely stuck. Try moving back
                        if (movementAttemptsWithNoPositionChange > 50)
                        {
                            MoveBackward(4);
                            movementAttemptsWithNoPositionChange = 0;
                        }
                        else
                        {
                            MoveForward(4);
                        }

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
            for (int i = 1; i < path.Count; i++)
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

                //don't optimize route when we cross a hazard line boundary
                if (mapHandler.IntersectsHazardLine(pos, nodeToExamine.WorldPosition))
                    continue;


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

            if (MonoMathHelper.DistanceBetween(lastKnownPositionBeforeMovementCommand, ParentObject.Transform.AbsoluteTransform.Translation) < 0.2f)
                movementAttemptsWithNoPositionChange++;

            lastKnownPositionBeforeMovementCommand = ParentObject.Transform.AbsoluteTransform.Translation;
        }

        public void MoveBackward(float amountToMove)
        {
            if ((DateTime.Now - lastMovement).TotalMilliseconds < movementFrequency)
                return;

            lastMovement = DateTime.Now;

            var back = new RestRequest("player/actions", Method.POST);
            back.RequestFormat = DataFormat.Json;
            back.AddBody(new PlayerAction() { type = "backward", amount = amountToMove });
            apiHandler.EnqueueRequest(false, back, x =>
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

}
