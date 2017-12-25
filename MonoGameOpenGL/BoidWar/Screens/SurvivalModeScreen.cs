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
        int currentPlanetIndex = 0;
        GravitationalField field;


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
            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0), 50f, 1000f);
            mouseCamera.moveSpeed = 0.1f;
            mouseCamera.SetPositionAndLook(new Vector3(0, 200, -200), (float)Math.PI, (float)-Math.PI / 5);
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

            chaseCamera.DesiredPositionOffset = new Vector3(0.0f, 100.0f, 45);
            chaseCamera.LookAtOffset = new Vector3(0.0f, 0.0f, 0);
            chaseCamera.Stiffness = 1000;
            chaseCamera.Damping = 600;
            chaseCamera.Mass = 50f;
            chaseCamera.NearZ = 0.5f;
            chaseCamera.FarZ = 1000.0f;
            SystemCore.SetActiveCamera(chaseCamera);

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


            //b = new MainBase();
            //b.Transform.AbsoluteTransform.Translation = new Vector3(0, 10, 0);
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(b);

            //AddEnemies();

            //AddPhysicsCubes();


            planets = new List<MiniPlanet>();



            var sunShape = new ProceduralSphere(10, 10);
            sunShape.SetColor(Color.Red);
            var sun = GameObjectFactory.CreateRenderableGameObjectFromShape(sunShape, EffectLoader.LoadSM5Effect("flatshaded"));
            sun.Transform.Scale = 100f;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(sun);

            float planetARadius = 200;
            var planetA = new MiniPlanet(new Vector3(700, 0, 0), planetARadius,
               NoiseGenerator.ParameterisedFastPlanet(planetARadius, NoiseGenerator.miniPlanetParameters, RandomHelper.GetRandomInt(1000)), 101, 4,
               RandomHelper.RandomColor, RandomHelper.RandomColor, false, 0.97f, 1.05f, 10, 4, 1);
            planets.Add(planetA);

            MiniPlanet moonA = new MiniPlanet(new Vector3(1300, 0, 0), 40,
                NoiseGenerator.RidgedMultiFractal(0.01f), 41, 2,
                RandomHelper.RandomColor, RandomHelper.RandomColor);
            planets.Add(moonA);

            var planetB = new MiniPlanet(new Vector3(2700, 0, 0), planetARadius,
              NoiseGenerator.ParameterisedFastPlanet(planetARadius, NoiseGenerator.miniPlanetParameters, RandomHelper.GetRandomInt(1000)), 101, 4,
             RandomHelper.RandomColor, RandomHelper.RandomColor, false, 0.97f, 1.05f, 10, 4, 1);
            planets.Add(planetB);


            var planetC = new MiniPlanet(new Vector3(3700, 0, 0), planetARadius,
              NoiseGenerator.ParameterisedFastPlanet(planetARadius, NoiseGenerator.miniPlanetParameters, RandomHelper.GetRandomInt(1000)), 101, 4,
              RandomHelper.RandomColor, RandomHelper.RandomColor, false, 0.97f, 1.05f, 10, 4, 1);
            planets.Add(planetC);


            MiniPlanet moonB = new MiniPlanet(new Vector3(5000, 0, 0), 150,
             NoiseGenerator.RidgedMultiFractal(0.01f), 101, 4,
             RandomHelper.RandomColor, RandomHelper.RandomColor, false, 0.97f, 1.05f, 10, 4, 5);
            planets.Add(moonB);

            field = new GravitationalField(new InfiniteForceFieldShape(), planetA.CurrentCenterPosition.ToBepuVector(), 1000000, 100);
            SystemCore.PhysicsSimulation.Add(field);



            duneBuggyOne = new DuneBuggy(PlayerIndex.One, Color.Red, new Vector3(planetA.CurrentCenterPosition.X, planetARadius * 1.05f, planetA.CurrentCenterPosition.Z));





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
                gameObject.Transform.SetPosition(planets[0].CurrentCenterPosition + RandomHelper.GetRandomVector3(new Vector3(-range, 50, -range), new Vector3(range, 50, range)));
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


            Vector3 upVector = duneBuggyOne.body.Transform.AbsoluteTransform.Translation - planets[currentPlanetIndex].CurrentCenterPosition;
            upVector.Normalize();
            duneBuggyOne.Update(gameTime);
            //duneBuggyOne.uprightSpringConstraint.LocalUpVector = upVector.ToBepuVector();
            //duneBuggyTwo.Update(gameTime);
            //duneBuggyThree.Update(gameTime);

            chaseCamera.Update(gameTime);



            chaseCamera.ChasePosition = duneBuggyOne.body.Transform.AbsoluteTransform.Translation;
            chaseCamera.ChaseDirection = duneBuggyOne.body.Transform.AbsoluteTransform.Forward;
            //chaseCamera.ChasePosition = duneBuggyOne.smoothedPosition;
            //chaseCamera.ChaseDirection = duneBuggyOne.smoothedForward;
            chaseCamera.Up = upVector;


            if (input.KeyPress(Keys.N))
                SwitchToNextPlanet();

            foreach (MiniPlanet miniPlanet in planets)
            {
                float distanceFromSurface =
                 (SystemCore.ActiveCamera.Position - miniPlanet.CurrentCenterPosition).Length();

                miniPlanet.Update(gameTime, distanceFromSurface, SystemCore.ActiveCamera.Position);
            }

            base.Update(gameTime);
        }

        private void SwitchToNextPlanet()
        {
            currentPlanetIndex++;
            MiniPlanet next;
            if (currentPlanetIndex < planets.Count)
            {
                next = planets[currentPlanetIndex];
            }
            else
            {
                next = planets[0];
                currentPlanetIndex = 0;
            }


            //switch camera, gravity and dune buggy
            field.Origin = next.CurrentCenterPosition.ToBepuVector();

            duneBuggyOne.Teleport(next.CurrentCenterPosition + new Vector3(0, 220, 0));

            mouseCamera.SetPositionAndLook(next.CurrentCenterPosition + new Vector3(0, 320, -100), (float)Math.PI, (float)-Math.PI / 5);

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

            Vector3 toCenter = planets[currentPlanetIndex].CurrentCenterPosition - duneBuggyOne.body.Transform.AbsoluteTransform.Translation;

            float height = toCenter.Length();

            DebugText.Write(height.ToString());

            base.Render(gameTime);

        }


    }
}
