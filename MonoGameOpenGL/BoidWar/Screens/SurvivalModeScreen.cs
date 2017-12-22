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
using BEPUphysics.UpdateableSystems;
using System.Collections.Generic;
using BoidWar;
using BoidWar.Gameplay;
using BEPUphysics.Vehicle;

namespace BoidWar.Screens
{

    public class DuneBuggy
    {
        private Vehicle vehicle;
        private GameObject body;
        private List<GameObject> wheels;
        /// <summary>
        /// Speed that the Vehicle tries towreach when moving backward.
        /// </summary>
        public float BackwardSpeed = -13;

        /// <summary>
        /// Speed that the Vehicle tries to reach when moving forward.
        /// </summary>
        public float ForwardSpeed = 30;

        /// <summary>
        /// Whether or not to use the Vehicle's input.
        /// </summary>
        public bool IsActive;


        /// <summary>
        /// Maximum turn angle of the wheels.
        /// </summary>
        public float MaximumTurnAngle = (float)Math.PI / 6;

        /// <summary>
        /// Turning speed of the wheels in radians per second.
        /// </summary>
        public float TurnSpeed = MathHelper.Pi;


        public DuneBuggy()
        {
            this.vehicle = VehicleFactory.Create(new BEPUutilities.Vector3(20, 20, 20));
            SystemCore.PhysicsSimulation.Add(vehicle);

            var shape = new ProceduralCuboid(2.5f/2, 4.5f/2, .75f/2);
            shape.SetColor(Color.Maroon);
            body = GameObjectFactory.CreateRenderableGameObjectFromShape(shape, EffectLoader.LoadSM5Effect("flatshaded"));
            body.AddComponent(new ShadowCasterComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(body);


        }


        public void Update(GameTime gameTime)
        {
            body.Transform.AbsoluteTransform = MonoMathHelper.GenerateMonoMatrixFromBepu(vehicle.Body.WorldTransform);



            if (SystemCore.Input.IsKeyDown(Keys.E))
            {
                //Drive
                vehicle.Wheels[1].DrivingMotor.TargetSpeed = ForwardSpeed;
                vehicle.Wheels[3].DrivingMotor.TargetSpeed = ForwardSpeed;
            }
            else if (SystemCore.Input.IsKeyDown(Keys.D))
            {
                //Reverse
                vehicle.Wheels[1].DrivingMotor.TargetSpeed = BackwardSpeed;
                vehicle.Wheels[3].DrivingMotor.TargetSpeed = BackwardSpeed;
            }
            else
            {
                //Idle
                vehicle.Wheels[1].DrivingMotor.TargetSpeed = 0;
                vehicle.Wheels[3].DrivingMotor.TargetSpeed = 0;
            }
            if (SystemCore.Input.IsKeyDown(Keys.Space))
            {
                //Brake
                foreach (Wheel wheel in vehicle.Wheels)
                {
                    wheel.Brake.IsBraking = true;
                }
            }
            else
            {
                //Release brake
                foreach (Wheel wheel in vehicle.Wheels)
                {
                    wheel.Brake.IsBraking = false;
                }
            }
            //Use smooth steering; while held down, move towards maximum.
            //When not pressing any buttons, smoothly return to facing forward.
            float angle;
            bool steered = false;
            if (SystemCore.Input.IsKeyDown(Keys.S))
            {
                steered = true;
                angle = Math.Max(vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * gameTime.ElapsedGameTime.Milliseconds, -MaximumTurnAngle);
                vehicle.Wheels[1].Shape.SteeringAngle = angle;
                vehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (SystemCore.Input.IsKeyDown(Keys.F))
            {
                steered = true;
                angle = Math.Min(vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * gameTime.ElapsedGameTime.Milliseconds, MaximumTurnAngle);
                vehicle.Wheels[1].Shape.SteeringAngle = angle;
                vehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (!steered)
            {
                //Neither key was pressed, so de-steer.
                if (vehicle.Wheels[1].Shape.SteeringAngle > 0)
                {
                    angle = Math.Max(vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * gameTime.ElapsedGameTime.Milliseconds, 0);
                    vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
                else
                {
                    angle = Math.Min(vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * gameTime.ElapsedGameTime.Milliseconds, 0);
                    vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
            }
        }
       
    }

    class SurvivalModeScreen : Screen
    {

        MouseFreeCamera mouseCamera;
        GameObject cameraObject;
        DuneBuggy vehicleRenderer;
        MainBase b;
        public SurvivalModeScreen() : base()
        {

        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = false;

            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(0.01f, 1, 0.01f));
            SystemCore.ActiveScene.FogEnabled = false;

            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0), 0.5f, 500f);
            SystemCore.SetActiveCamera(mouseCamera);
            mouseCamera.moveSpeed = 0.1f;
            mouseCamera.SetPositionAndLook(new Vector3(50, 30, -20), (float)Math.PI, (float)-Math.PI / 5);



            cameraObject = new GameObject();
            cameraObject.AddComponent(new ComponentCamera());
            cameraObject.Transform.AbsoluteTransform = Matrix.CreateWorld(new Vector3(0, 10, 0), Vector3.Down, Vector3.Backward);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);

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
            var skyDome = new GradientSkyDome(Color.MediumBlue, Color.Black);
            SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);



            Heightmap heightMap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.03f), 100, 3, 20, 1, 1, 1);
            var terrainObject = heightMap.CreateTranslatedRenderableHeightMap(Color.OrangeRed, EffectLoader.LoadSM5Effect("flatshaded"), new Vector3(-150,0,-150));     
          
               
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrainObject);

            b = new MainBase();
            b.Transform.AbsoluteTransform.Translation = new Vector3(0, 10, 0);
          
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(b);


            AddEnemies();

            AddPhysicsCubes();

            vehicleRenderer = new DuneBuggy();


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
                float range = 150;
                ProceduralCube shape = new ProceduralCube();
                var gameObject = new GameObject();
                gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                    BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
                gameObject.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
                gameObject.AddComponent(new ShadowCasterComponent());
                gameObject.AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.box));
                gameObject.Transform.SetPosition(RandomHelper.GetRandomVector3(new Vector3(-range, 50, -range), new Vector3(range, 50, range)));
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

            vehicleRenderer.Update(gameTime);


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
                input.CenterMouse();
            }
            else
            {
                if (input.EvaluateInputBinding("CameraForward"))
                    cameraObject.Transform.Translate(-Vector3.Forward * 0.1f);
                if (input.EvaluateInputBinding("CameraBackward"))
                    cameraObject.Transform.Translate(-Vector3.Backward * 0.1f);
                if (input.EvaluateInputBinding("CameraLeft"))
                    cameraObject.Transform.Translate(-Vector3.Left * 0.1f);
                if (input.EvaluateInputBinding("CameraRight"))
                    cameraObject.Transform.Translate(-Vector3.Right * 0.1f);


                cameraObject.Transform.Translate(new Vector3(0, -input.ScrollDelta * 0.01f, 0));
            }




        }

        public override void Render(GameTime gameTime)
        {
            DebugText.Write(SystemCore.ActiveCamera.Position.ToString());

            base.Render(gameTime);
        }


    }
}
