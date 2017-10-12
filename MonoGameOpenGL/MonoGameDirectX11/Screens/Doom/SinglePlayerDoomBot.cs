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

        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\\Doom1.WAD";


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

            apiHandler.CreateRegularRequest(20000, new RestRequest("world/objects"), x =>
            {
                foreach (GameObject worldObject in worldObjects.Values)
                    SystemCore.GameObjectManager.RemoveObject(worldObject);

                worldObjects.Clear();

                IDictionary<string, object> jsonValues = Json.JsonParser.FromJson(x.Content);
                List<object> objectList = jsonValues["array0"] as List<object>;


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
                }
            });

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

                var id = (double)jsonValues["id"];
                playerObj.ID = (int)id;
                DoomComponent component = new DoomComponent();
                component.DoomType = "Player";
                component.HitVectorSize = 2f;
                playerObj.AddComponent(component);
                playerObj.AddComponent(new DoomMovementComponent(mapHandler, apiHandler));
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

        bool pathTest = false;
        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.O))
                apiHandler.TurnLeft(10);
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.P))
                apiHandler.TurnRight(10);


            UpdateCamera();

            if (!mapHandler.FloodFillComplete)
                mapHandler.FloodFillStep();
            else
            {
                if (!pathTest)
                {
                    pathTest = true;
                    playerObj.GetComponent<DoomMovementComponent>().PathToPoint(new Vector3(27, 0, -50));
                }
            }

            apiHandler.Update();


            if (playerObj != null)
            {
                DoomComponent playerDoomComponent = playerObj.GetComponent<DoomComponent>();

                Color forwardColor = Color.Red;
                Color leftColor = Color.Red;
                Color rightColor = Color.Red;

                if (playerDoomComponent.ForwardHitVector)
                    forwardColor = Color.Blue;
                if (playerDoomComponent.LeftHitVector)
                    leftColor = Color.Blue;
                if (playerDoomComponent.RightHightVector)
                    rightColor = Color.Blue;


                Vector3 pos = playerObj.Transform.AbsoluteTransform.Translation;
                Vector3 forwardVec = playerObj.Transform.AbsoluteTransform.Translation + playerObj.Transform.AbsoluteTransform.Forward * playerDoomComponent.HitVectorSize;
                Vector3 rightVec = MonoMathHelper.RotateAroundPoint(forwardVec, playerObj.Transform.AbsoluteTransform.Translation, Vector3.Up, MathHelper.PiOver4);
                Vector3 leftVec = MonoMathHelper.RotateAroundPoint(forwardVec, playerObj.Transform.AbsoluteTransform.Translation, Vector3.Up, -MathHelper.PiOver4);

                DebugShapeRenderer.AddLine(pos, forwardVec, forwardColor);
                DebugShapeRenderer.AddLine(pos, leftVec, leftColor);
                DebugShapeRenderer.AddLine(pos, rightVec, rightColor);



                DebugText.Write(playerObj.Transform.AbsoluteTransform.Translation.ToString());
                // DebugText.Write(playerAngle.ToString());
            }


            base.Update(gameTime);
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

                if (!vec.done)
                    DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(vec.WorldPosition, 0.2f), colorOfSphere);

                foreach (NavigationNode neighbour in vec.Neighbours)
                {
                    DebugShapeRenderer.AddLine(vec.WorldPosition, neighbour.WorldPosition, Color.Green);
                }
            }


        }


    }



    //move to
    //1. Find nav points
    //2. Obtain path between them
    //3. Take first point on the path
    //4. turn towards it
    //5. move forward
    //6. measure distnance to node
    //7. If < threshold, transition to new distance.

}
