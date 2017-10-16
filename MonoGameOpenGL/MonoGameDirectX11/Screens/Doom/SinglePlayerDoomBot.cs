using MonoGameEngineCore;
using MonoGameEngineCore.DoomLib;
using MonoGameEngineCore.GameObject;
using System;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Rendering.Camera;
using MonoGameEngineCore.Helper;
using RestSharp;
using System.Collections.Generic;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace MonoGameDirectX11.Screens.Doom
{
    public class SinglePlayerDoomBot : Screen
    {
        GameObject cameraObject;
        DoomMapHandler mapHandler;
        DoomAPIHandler apiHandler;
        GameObject playerObj;
        Dictionary<int, GameObject> worldObjects;
        List<string> pickUpTypes;
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\\Doom1.WAD";
        bool doNothing = false;
        bool collectItems = true;
        bool endOfLevelSeeking = false;

        public SinglePlayerDoomBot()
        {

        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = true;
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();

            cameraObject = new GameObject();
            worldObjects = new Dictionary<int, GameObject>();
            //give it some random ID to keep it out the range of the doom objects
            cameraObject.ID = 990000;

            cameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.25f, 1000.0f, false));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            cameraObject.Transform.AbsoluteTransform = Matrix.CreateWorld(new Vector3(0, -500, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));


            mapHandler = new DoomMapHandler(filePath, "E1M1", "E1M2");
            mapHandler.ParseWad();
            mapHandler.InitialiseFloodFill();


            apiHandler = new DoomAPIHandler("http://192.168.1.77", 6001);

            apiHandler.CreateRegularRequest(1000, new RestRequest("player"), x =>
            {
                UpdatePlayer(x);
            });

            apiHandler.CreateRegularRequest(4000, new RestRequest("world/objects"), x =>
            {
                foreach (GameObject worldObject in worldObjects.Values)
                    SystemCore.GameObjectManager.RemoveObject(worldObject);

                worldObjects.Clear();

                IDictionary<string, object> jsonValues = Json.JsonParser.FromJson(x.Content);
                List<object> objectList = jsonValues["array0"] as List<object>;


                List<string> types = new List<string>();


                foreach (object o in objectList)
                {
                    double id, angle, health, distance, xpos, ypos;
                    string type;

                    ParseObjectData(o, out id, out type, out angle, out health, out distance, out xpos, out ypos);

                    if (id == 0)
                        continue;

                    GameObject worldObject;
                    DoomComponent component;

                    CreateLocalWorldObject(id, type, out worldObject, out component);

                    UpdateObject(worldObject, angle, xpos, ypos, type, health, distance);

                    types.Add(type);
                }


            });


            pickUpTypes = new List<string>();



            base.OnInitialise();
        }

        private static void ParseObjectData(object o, out double id, out string type, out double angle, out double health, out double distance, out double x, out double y)
        {
            Dictionary<string, object> properties = o as Dictionary<string, object>;

            id = (double)properties["id"];
            type = (string)properties["type"];
            angle = (double)properties["angle"];
            health = (double)properties["health"];
            distance = (double)properties["distance"];

            IDictionary<string, object> pos = properties["position"] as IDictionary<string, object>;
            x = (double)pos["x"];
            y = (double)pos["y"];
        }
        private void CreateLocalWorldObject(double id, string type, out GameObject worldObject, out DoomComponent component)
        {
            var shape = CreateShape(type);

            worldObject = GameObjectFactory.CreateRenderableGameObjectFromShape(
                        shape, EffectLoader.LoadSM5Effect("flatshaded"));

            component = new DoomComponent();
            worldObject.AddComponent(component);
            worldObject.ID = (int)id;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(worldObject);
            worldObject.Transform.Scale = 0.3f;
            worldObjects.Add(worldObject.ID, worldObject);
        }
        private ProceduralShape CreateShape(string type)
        {
            return new ProceduralCube();
        }
        private void UpdateObject(GameObject objectToUpdate, double angle, double x, double y, string type, double health, double distance)
        {
            //position the object in the world
            objectToUpdate.Transform.SetPosition(new Vector3((float)x / mapHandler.scale, 0, (float)y / mapHandler.scale));

            //turn to face the appopriate angle
            float angleInRadians = MathHelper.ToRadians((float)angle);
            Vector3 headingVector = new Vector3((float)Math.Cos(angleInRadians), 0, (float)Math.Sin(angleInRadians));
            objectToUpdate.Transform.SetLookAndUp(headingVector, Vector3.Up);

            DoomComponent component = objectToUpdate.GetComponent<DoomComponent>();
            component.DoomType = type;
            component.Health = health;
            component.Angle = angle;
            component.Distance = distance;

        }

        private void UpdatePlayer(RestResponse x)
        {
            IDictionary<string, object> jsonValues = Json.JsonParser.FromJson(x.Content);

            //in doom, 0 degrees is East. Increased value turns us counter clockwise, so north is 90, west 180 etc
            var playerAngle = (double)jsonValues["angle"];



            IDictionary<string, object> pos = jsonValues["position"] as IDictionary<string, object>;
            double xpos = (double)pos["x"];
            double ypos = (double)pos["y"];

            //create if first time
            if (playerObj == null)
            {
                playerObj = GameObjectFactory.CreateRenderableGameObjectFromShape(
                        new ProceduralCube(), EffectLoader.LoadSM5Effect("flatshaded"));
                playerObj.Transform.Scale = 0.5f;
                var id = (double)jsonValues["id"];
                playerObj.ID = (int)id;
                DoomComponent component = new DoomComponent();
                component.DoomType = "Player";
                component.HitVectorSize = 0.5f;
                playerObj.AddComponent(component);
                playerObj.AddComponent(new DoomMovementComponent(mapHandler, apiHandler));
                playerObj.AddComponent(new DoomCombatComponent(mapHandler, apiHandler, worldObjects));
                SystemCore.GameObjectManager.AddAndInitialiseGameObject(playerObj);



            }



            //remember to scale it appropriately. In doom, Z is up, but here it's Y, so swap those coords
            playerObj.Transform.SetPosition(new Vector3((float)xpos / mapHandler.scale, 0, (float)ypos / mapHandler.scale));

            //turn us to face the appopriate angle
            float playerAngleInRadians = MathHelper.ToRadians((float)playerAngle);
            Vector3 headingVector = new Vector3((float)Math.Cos(playerAngleInRadians), 0, (float)Math.Sin(playerAngleInRadians));

            playerObj.Transform.SetLookAndUp(headingVector, Vector3.Up);
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.O))
                apiHandler.TurnLeft(10);
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.P))
                apiHandler.TurnRight(10);

            if (input.MouseLeftPress())
            {
                //PathToMousePoint();
            }


            UpdateCamera();

            if (!mapHandler.FloodFillComplete)
                mapHandler.FloodFillStep();

            if (mapHandler.FloodFillComplete && playerObj.GetComponent<DoomMovementComponent>().path == null && !doNothing)
            {
                playerObj.GetComponent<DoomMovementComponent>().PathToPoint(mapHandler.LevelEnd);
                endOfLevelSeeking = true;
            }

            //check if we can see anything. If so, path to it.
            if (!doNothing)
            {
                if (collectItems)
                    PickUpObjects();
            }


            apiHandler.Update();


            if (playerObj != null)
            {

                DebugText.Write(playerObj.Transform.AbsoluteTransform.Translation.ToString());
            }


            base.Update(gameTime);
        }

        private void PathToMousePoint()
        {
            Plane p = new Plane(Vector3.Down, 0);
            Ray ray = input.GetProjectedMouseRay();
            float? result;
            ray.Intersects(ref p, out result);
            if (result.HasValue)
            {
                Vector3 mouseLeftPoint = ray.Position + ray.Direction * result.Value;
                playerObj.GetComponent<DoomMovementComponent>().PathToPoint(mouseLeftPoint);
            }
        }

        GameObject currentClosestPickup = null;
        private void PickUpObjects()
        {
            List<GameObject> visiblePickups = new List<GameObject>();
            foreach (GameObject o in worldObjects.Values)
            {

                if (mapHandler.IntersectsLevel(playerObj.Transform.AbsoluteTransform.Translation, o.Transform.AbsoluteTransform.Translation))
                    continue;

                DoomComponent d = o.GetComponent<DoomComponent>();

                if (d.DoomType.ToLower().Contains("armor"))
                    visiblePickups.Add(o);
                if (d.DoomType.ToLower().Contains("health"))
                    visiblePickups.Add(o);
                if (d.DoomType.ToLower().Contains("shotgun"))
                    visiblePickups.Add(o);
                if (d.DoomType.ToLower().Contains("ammo"))
                    visiblePickups.Add(o);
                if (d.DoomType.ToLower().Contains("keycard"))
                    visiblePickups.Add(o);

            }

            float closest = float.MaxValue;
            GameObject newClosestPickup = null;
            foreach (GameObject o in visiblePickups)
            {
                float dist = (playerObj.Transform.AbsoluteTransform.Translation - o.Transform.AbsoluteTransform.Translation).Length();
                if (dist < closest)
                {
                    newClosestPickup = o;
                    closest = dist;
                }


            }

            //if we've collected the item, but the path is still there, drop the path
            if (!visiblePickups.Contains(currentClosestPickup) && !endOfLevelSeeking)
            {
                if (playerObj != null)
                    playerObj.GetComponent<DoomMovementComponent>().path = null;
            }


            if (newClosestPickup != null)
            {

                if (playerObj.GetComponent<DoomMovementComponent>().path == null || endOfLevelSeeking)
                {
                    endOfLevelSeeking = false;
                    playerObj.GetComponent<DoomMovementComponent>().PathToPoint(newClosestPickup.Transform.AbsoluteTransform.Translation);
                    currentClosestPickup = newClosestPickup;
                }
                else if (newClosestPickup != currentClosestPickup)
                {
                    playerObj.GetComponent<DoomMovementComponent>().PathToPoint(newClosestPickup.Transform.AbsoluteTransform.Translation);
                    currentClosestPickup = newClosestPickup;
                }
            }
        }

        private void UpdateCamera()
        {
            float currentHeight = cameraObject.Transform.AbsoluteTransform.Translation.Y;
            cameraObject.Transform.Translate(new Vector3(0, input.ScrollDelta / 10f, 0));

            float cameraSpeed = 1f;

            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                cameraObject.Transform.Translate(new Vector3(0, 0, cameraSpeed));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                cameraObject.Transform.Translate(new Vector3(0, 0, -cameraSpeed));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                cameraObject.Transform.Translate(new Vector3(-cameraSpeed, 0, 0));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                cameraObject.Transform.Translate(new Vector3(cameraSpeed, 0, 0));
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Black);


            foreach (DoomLine l in mapHandler.GetLevelOutline())
            {

                DebugShapeRenderer.AddLine(l.start, l.end, l.color);

            }
            foreach (DoomLine d in mapHandler.HazardLines)
            {
                DebugShapeRenderer.AddLine(d.start, d.end, d.color);
            }
            foreach (DoomLine d in mapHandler.Doors)
            {
                DebugShapeRenderer.AddLine(d.start, d.end, d.color);
            }
            foreach (DoomLine d in mapHandler.InternalLines)
            {
                DebugShapeRenderer.AddLine(d.start, d.end, d.color);
            }
            if (!mapHandler.FloodFillComplete)
                RenderFloodFill();

            if (playerObj != null)
            {
                var moveComponent = playerObj.GetComponent<DoomMovementComponent>();

                if (moveComponent.path != null)
                {
                    if (moveComponent.path.Count > 0)
                    {
                        foreach (NavigationNode n in moveComponent.path)
                        {
                            DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(n.WorldPosition, 0.3f), Color.Purple);
                        }

                    }
                }
            }

            DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(mapHandler.LevelEnd, 1f), Color.Orange);

            base.Render(gameTime);
        }

        private void RenderFloodFill()
        {
            foreach (NavigationNode vec in mapHandler.floodFiller.positions)
            {
                Color colorOfSphere = Color.Orange;

                if (vec == mapHandler.floodFiller.next)
                    colorOfSphere = Color.Blue;

                if (mapHandler.floodFiller.justAdded.Contains(vec))
                    colorOfSphere = Color.Red;

                if (vec.Cost == 0)
                    DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(vec.WorldPosition, 0.2f), colorOfSphere);
                else
                    DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(vec.WorldPosition, 0.2f), Color.Red);

                foreach (NavigationNode neighbour in vec.Neighbours)
                {
                    DebugShapeRenderer.AddLine(vec.WorldPosition, neighbour.WorldPosition, Color.Green);
                }
            }


        }


    }




}
