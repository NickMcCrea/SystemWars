using MonoGameEngineCore;
using System;
using Microsoft.Xna.Framework;
using SystemWar;
using MonoGameEngineCore.Rendering.Camera;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GameObject.Components;
using BoidWar.Gameplay;
using MonoGameEngineCore.Camera;
using GridForgeResurrected.Screens;
using BEPUphysics.UpdateableSystems.ForceFields;
using System.Collections.Generic;

namespace BoidWar.Screens
{


    class SurvivalModeScreen : Screen
    {
        private List<MiniPlanet> planets;
        MouseFreeCamera mouseCamera;
        GameObject cameraObject;
        DuneBuggy duneBuggyOne, duneBuggyTwo, duneBuggyThree;
        MainBase b;
        ChaseCamera chaseCamera;

        public SurvivalModeScreen() : base()
        {

        }

        public override void OnInitialise()
        {
            fpsLabel.Visible = true;
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(0.01f, 1, 0.01f));
            SystemCore.ActiveScene.FogEnabled = false;




            //mouse camera
            mouseCamera = new MouseFreeCamera(new Vector3(10000, 0, 0), 0.5f, 500f);
            mouseCamera.moveSpeed = 0.1f;
            mouseCamera.SetPositionAndLook(new Vector3(10000, 200, -200), (float)Math.PI, (float)-Math.PI / 5);
            SystemCore.SetActiveCamera(mouseCamera);



            //component camera
            cameraObject = new GameObject();
            cameraObject.AddComponent(new ComponentCamera());
            cameraObject.Transform.AbsoluteTransform = Matrix.CreateWorld(new Vector3(10000, 80, -150), Vector3.Backward, Vector3.Up);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);        
            cameraObject.Transform.Rotate(Vector3.Up, (float)Math.PI / 4);
            cameraObject.Transform.Rotate(cameraObject.Transform.AbsoluteTransform.Right, -(float)Math.PI / 8);



            // Create the chase camera
            chaseCamera = new ChaseCamera();

            chaseCamera.DesiredPositionOffset = new Vector3(0.0f, 40.0f, 35.0f);
            chaseCamera.LookAtOffset = new Vector3(0.0f, 0.0f, 0);
            chaseCamera.Stiffness = 1000;
            chaseCamera.Damping = 600;
            chaseCamera.Mass = 50f;
            chaseCamera.NearZ = 0.5f;
            chaseCamera.FarZ = 1000.0f;
            //SystemCore.SetActiveCamera(chaseCamera);

            AddInputBindings();



            SetUpGameWorld();



            base.OnInitialise();
        }

        private void AddInputBindings()
        {

            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("CameraForward", Keys.Up);
            input.AddKeyDownBinding("CameraBackward", Keys.Down);
            input.AddKeyDownBinding("CameraLeft", Keys.Left);
            input.AddKeyDownBinding("CameraRight", Keys.Right);


            input.AddKeyPressBinding("MainMenu", Keys.Escape);


            var binding = input.AddKeyPressBinding("WireframeToggle", Keys.Space);
            binding.InputEventActivated += (x, y) => { SystemCore.Wireframe = !SystemCore.Wireframe; };


            input.AddKeyPressBinding("CameraSwitch", Keys.Enter).InputEventActivated += (x, y) =>
            {
                if (SystemCore.ActiveCamera is MouseFreeCamera)
                    SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
                else
                    SystemCore.SetActiveCamera(mouseCamera);
            };

        }

        private void SetUpGameWorld()
        {

            //Sky dome first (depth buffer will be disabled on draw for this)
            // var skyDome = new GradientSkyDome(Color.MediumBlue, Color.Black);
            // SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);

            //Heightmap heightMap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.03f), 100, 5, 20, 1, 1, 1);
            //var terrainObject = heightMap.CreateTranslatedRenderableHeightMap(Color.OrangeRed, EffectLoader.LoadSM5Effect("flatshaded"), new Vector3(-250, 0, -250));
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrainObject);



            planets = new List<MiniPlanet>();

            float radius = 200;

            var earth = new MiniPlanet(new Vector3(10000, 0, 0), radius,
               NoiseGenerator.ParameterisedFastPlanet(radius, NoiseGenerator.miniPlanetParameters, RandomHelper.GetRandomInt(1000)), 101, 4,
               Color.Orange, Color.DarkOrange, false, 0.97f, 1.05f, 10, 4);
            planets.Add(earth);

            MiniPlanet moon = new MiniPlanet(new Vector3(400, 0, 0), 40,
                NoiseGenerator.RidgedMultiFractal(0.01f), 41, 2,
                Color.DarkGray, Color.DarkGray);
            //moon.SetOrbit(earth, Vector3.Up, 0.001f);
            planets.Add(moon);

            var field = new GravitationalField(new InfiniteForceFieldShape(), earth.CurrentCenterPosition.ToBepuVector(), 1000000, 100);
            SystemCore.PhysicsSimulation.Add(field);

            //b = new MainBase();
            //b.Transform.AbsoluteTransform.Translation = new Vector3(0, 10, 0);
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(b);

            //AddEnemies();

            AddPhysicsCubes();

            duneBuggyOne = new DuneBuggy(PlayerIndex.One, Color.Red, new Vector3(10000, radius * 1.05f, 0));
            // duneBuggyTwo = new DuneBuggy(PlayerIndex.Two, Color.Blue, new Vector3(20, 20, 0));
            // duneBuggyThree = new DuneBuggy(PlayerIndex.Three, Color.Green, new Vector3(0, 20, 20));




        }

        private float GetTerrainHeightAt(float v1, float v2)
        {
            return 0;
        }

        private void AddEnemies()
        {
            float range = 150;
            for (int i = 0; i < 10; i++)
            {
                SimpleEnemy e = new SimpleEnemy();
                e.Transform.AbsoluteTransform.Translation = RandomHelper.GetRandomVector3(new Vector3(-range, 20, -range), new Vector3(range, 20, range));
                SystemCore.GameObjectManager.AddAndInitialiseGameObject(e);
                e.DesiredPosiiton = b.Transform.AbsoluteTransform.Translation;
            }
        }

        private void AddPhysicsCubes()
        {
            for (int i = 0; i < 100; i++)
            {
                float range = 500;
                ProceduralCube shape = new ProceduralCube();
                var gameObject = new GameObject();
                gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                    BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
                gameObject.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
                gameObject.AddComponent(new ShadowCasterComponent());
                gameObject.AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.box));
                gameObject.Transform.SetPosition(planets[0].CurrentCenterPosition +  RandomHelper.GetRandomVector3(new Vector3(-range, 50, -range), new Vector3(range, 50, range)));
                SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(gameObject);
            }
        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            SystemCore.GameObjectManager.ClearAllObjects();
            SystemCore.ActiveScene.ClearLights();
            input.ClearBindings();
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {

            EvaluateMouseCamControls(gameTime);


            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());


            Vector3 upVector = duneBuggyOne.body.Transform.AbsoluteTransform.Translation - planets[0].CurrentCenterPosition;
            upVector.Normalize();
            duneBuggyOne.Update(gameTime);
            duneBuggyOne.uprightSpringConstraint.LocalUpVector = upVector.ToBepuVector();
            //duneBuggyTwo.Update(gameTime);
            //duneBuggyThree.Update(gameTime);

            chaseCamera.Update(gameTime);
            chaseCamera.ChasePosition = duneBuggyOne.body.Transform.AbsoluteTransform.Translation;
            chaseCamera.ChaseDirection = duneBuggyOne.body.Transform.AbsoluteTransform.Forward;
            chaseCamera.Up = duneBuggyOne.body.Transform.AbsoluteTransform.Up;



            foreach (MiniPlanet miniPlanet in planets)
            {
                float distanceFromSurface =
                 (SystemCore.ActiveCamera.Position - miniPlanet.CurrentCenterPosition).Length();

                miniPlanet.Update(gameTime, distanceFromSurface, SystemCore.ActiveCamera.Position);
            }

            base.Update(gameTime);
        }

        private void EvaluateMouseCamControls(GameTime gameTime)
        {

            if (SystemCore.ActiveCamera is MouseFreeCamera)
            {
                mouseCamera.Slow = input.EvaluateInputBinding("SlowCamera");
                if (input.EvaluateInputBinding("CameraForward"))
                    mouseCamera.MoveForward();
                if (input.EvaluateInputBinding("CameraBackward"))
                    mouseCamera.MoveBackward();
                if (input.EvaluateInputBinding("CameraLeft"))
                    mouseCamera.MoveLeft();
                if (input.EvaluateInputBinding("CameraRight"))
                    mouseCamera.MoveRight();



                mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);

            }





        }

        public override void Render(GameTime gameTime)
        {

            DebugText.Write(duneBuggyOne.body.Transform.AbsoluteTransform.Translation.ToString());
            base.Render(gameTime);
        }


    }
}
