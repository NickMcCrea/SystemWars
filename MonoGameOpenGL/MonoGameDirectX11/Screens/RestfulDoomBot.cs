using MonoGameEngineCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using MarcelJoachimKloubert.DWAD;
using System.IO;
using MarcelJoachimKloubert.DWAD.WADs.Lumps.Linedefs;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Rendering.Camera;
using System;
using System.Net;
using RestSharp;
using MonoGameEngineCore.Rendering;

namespace MonoGameDirectX11.Screens
{
    class RestfulDoomBot : Screen
    {

        class DoomComponent : IComponent
        {

            public string DoomType { get; set; }
            public double Health { get; set; }
            public double Angle { get; set; }
            public double Distance { get; set; }
            public bool ForwardHitVector { get; set; }
            public bool LeftHitVector { get; set; }
            public bool RightHightVector { get; set; }
            public float HitVectorSize { get; set; }
            public GameObject ParentObject
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


        GameObject cameraObject;
        private IWADFile currentFile;
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\\Doom1.WAD";
        float scale = 50f;
        float offsetX = 0;
        float offsetY = 0;
        string restHost = "http://192.168.1.77";
        int restPort = 6001;
        float playerUpdateFrequency = 100;
        float worldUpdateFrequency = 1000;
        DateTime timeOfLastPlayerUpdate = DateTime.Now;
        DateTime timeOfLastWorldUpdate = DateTime.Now;

        GameObject playerObj;
        RestClient restClient;
        RestRequest playerRequest;
        RestRequest worldRequest;

        double playerAngle;

        Dictionary<int, GameObject> worldObjects;

        public RestfulDoomBot()
        {

        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = true;
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();

            cameraObject = new GameObject();

            //give it some random ID to keep it out the range of the doom objects
            cameraObject.ID = 990000;


            cameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.25f, 1000.0f, false));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            cameraObject.Transform.AbsoluteTransform = Matrix.CreateWorld(new Vector3(0, -500, 0), new Vector3(0, 1, 0), new Vector3(0, 0, 1));


            var file = new FileInfo(filePath);
            using (var fs = file.OpenRead())
            {
                currentFile = WADFileFactory.FromStream(fs).FirstOrDefault();
            }



            restClient = new RestClient(restHost + ":" + restPort + "/api/");


            playerRequest = new RestRequest("player");
            worldRequest = new RestRequest("world/objects");

            worldObjects = new Dictionary<int, GameObject>();

            RequestPlayerDetails();

            RequestAllWorldObects();


            base.OnInitialise();
        }

        private void RequestAllWorldObects()
        {


            foreach (GameObject worldObject in worldObjects.Values)
                SystemCore.GameObjectManager.RemoveObject(worldObject);

            worldObjects.Clear();


            var response = restClient.Execute(worldRequest);
            IDictionary<string, object> jsonValues = Json.JsonParser.FromJson(response.Content);
            UpdateWorldObjects(jsonValues);

        }

        private void RequestPlayerDetails()
        {

            var response = restClient.Execute(playerRequest);
            IDictionary<string, object> jsonValues = Json.JsonParser.FromJson(response.Content);
            UpdatePlayer(jsonValues);
            RequestHitVectorData();


        }

        private void RequestHitVectorData()
        {
            DoomComponent playerDoomComponent = playerObj.GetComponent<DoomComponent>();

            Vector3 forwardVec = playerObj.Transform.AbsoluteTransform.Translation
                + playerObj.Transform.AbsoluteTransform.Forward * playerDoomComponent.HitVectorSize;

            Vector3 rightVec = MonoMathHelper.RotateAroundPoint(forwardVec, playerObj.Transform.AbsoluteTransform.Translation, Vector3.Up, MathHelper.PiOver4);
            Vector3 leftVec = MonoMathHelper.RotateAroundPoint(forwardVec, playerObj.Transform.AbsoluteTransform.Translation, Vector3.Up, -MathHelper.PiOver4);


            forwardVec *= scale;
            leftVec *= scale;
            rightVec *= scale;

            //forward hit vector
            string paramString = "id=" + playerObj.ID + "&x=" + forwardVec.X + "&y=" + forwardVec.Z;
            var hitTestResponse = restClient.Execute(new RestRequest("world/movetest?" + paramString));
            var content = Json.JsonParser.FromJson(hitTestResponse.Content);
            playerDoomComponent.ForwardHitVector = (bool)content["result"];

            //left
            string paramStringLeft = "id=" + playerObj.ID + "&x=" + leftVec.X + "&y=" + leftVec.Z;
            var hitTestResponseLeft = restClient.Execute(new RestRequest("world/movetest?" + paramStringLeft));
            var contentLeft = Json.JsonParser.FromJson(hitTestResponseLeft.Content);
            playerDoomComponent.LeftHitVector = (bool)contentLeft["result"];

            //right
            string paramStringRight = "id=" + playerObj.ID + "&x=" + rightVec.X + "&y=" + rightVec.Z;
            var hitTestResponseRight = restClient.Execute(new RestRequest("world/movetest?" + paramStringRight));
            var contentRight = Json.JsonParser.FromJson(hitTestResponseRight.Content);
            playerDoomComponent.RightHightVector = (bool)contentRight["result"];
        }

        private void UpdateWorldObjects(IDictionary<string, object> jsonValues)
        {
            List<object> objectList = jsonValues["array0"] as List<object>;

            foreach (object o in objectList)
            {
                double id, angle, health, distance, x, y;
                string type;

                ParseObjectData(o, out id, out type, out angle, out health, out distance, out x, out y);

                GameObject worldObject;
                DoomComponent component;

                CreateLocalWorldObject(id, type, out worldObject, out component);

                UpdateObject(worldObject, angle, x, y, type, health, distance);


            }

            timeOfLastWorldUpdate = DateTime.Now;
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

        private void UpdateObject(GameObject objectToUpdate, double angle, double x, double y, string type, double health, double distance)
        {
            //position the object in the world
            objectToUpdate.Transform.SetPosition(new Vector3((float)x / scale, 0, (float)y / scale));

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

        private void UpdatePlayer(IDictionary<string, object> jsonValues)
        {
            //in doom, 0 degrees is East. Increased value turns us counter clockwise, so north is 90, west 180 etc
            playerAngle = (double)jsonValues["angle"];



            IDictionary<string, object> pos = jsonValues["position"] as IDictionary<string, object>;
            double x = (double)pos["x"];
            double y = (double)pos["y"];

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


                SystemCore.GameObjectManager.AddAndInitialiseGameObject(playerObj);

            }

            //remember to scale it appropriately. In doom, Z is up, but here it's Y, so swap those coords
            playerObj.Transform.SetPosition(new Vector3((float)x / scale, 0, (float)y / scale));

            //turn us to face the appopriate angle
            float playerAngleInRadians = MathHelper.ToRadians((float)playerAngle);
            Vector3 headingVector = new Vector3((float)Math.Cos(playerAngleInRadians), 0, (float)Math.Sin(playerAngleInRadians));

            playerObj.Transform.SetLookAndUp(headingVector, Vector3.Up);

            timeOfLastPlayerUpdate = DateTime.Now;
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

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


            TimeSpan timeSincePlayerUpdate = DateTime.Now - timeOfLastPlayerUpdate;
            TimeSpan timeSinceWorldUpdate = DateTime.Now - timeOfLastWorldUpdate;

            if (timeSincePlayerUpdate.TotalMilliseconds > playerUpdateFrequency)
                RequestPlayerDetails();

            if (timeSinceWorldUpdate.TotalMilliseconds > worldUpdateFrequency)
                RequestAllWorldObects();

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Black);
            DebugShapeRenderer.VisualiseAxes(5f);

            foreach (var lump in currentFile.EnumerateLumps().OfType<ILinedefsLump>())
            {
                foreach (var linedef in lump.EnumerateLinedefs())
                {
                    var p1 = new Vector3((linedef.Start.X) / scale + offsetX, 0,
                                       (linedef.Start.Y) / scale + offsetY);

                    var p2 = new Vector3((linedef.End.X) / scale + offsetX, 0,
                                       (linedef.End.Y) / scale + offsetY);

                    DebugShapeRenderer.AddLine(p1, p2, Color.Orange);
                }
            }


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
                DebugText.Write(playerAngle.ToString());
            }

            base.Render(gameTime);
        }




    }

    class DoomAPIHandler
    {
        private RestClient client;
        public List<IRestResponse> completedResponses;
        private bool inFlight = false;

        public  DoomAPIHandler(RestClient client)
        {
            this.client = client;
            completedResponses = new List<IRestResponse>();
        }

        public void EnqueueRequest(IRestRequest request)
        {
            if (inFlight)
                return;

            client.ExecuteAsync(request, response =>
            {
                completedResponses.Add(response);
            });
        }

    }
}
