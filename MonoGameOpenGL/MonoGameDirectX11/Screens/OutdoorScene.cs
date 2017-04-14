using BEPUphysics;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Vehicle;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using System;
using System.Collections.Generic;
using SystemWar;

namespace MonoGameDirectX11.Screens
{
    class OutdoorScene : TestScreen
    {
        GradientSkyDome skyDome;
        Vehicle testVehicle;
        GameObject vehicleObject;
        GameObject wheel1, wheel2, wheel3, wheel4;

        public float BackwardSpeed = -13;
        public float ForwardSpeed = 30;
        public float MaximumTurnAngle = (float)Math.PI / 6;
        public float TurnSpeed = MathHelper.Pi;
     
        public OutdoorScene()
            : base()
        {


        }


        public override void OnInitialise()
        {
            base.OnInitialise();

            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(1, 1, 1));

            SystemCore.ActiveScene.FogEnabled = true;


            skyDome = new GradientSkyDome(Color.MediumBlue, Color.LightCyan);



            mouseCamera.moveSpeed = 0.01f;

            mouseCamera.SetPositionAndLook(new Vector3(50, 30, -20), (float)Math.PI, (float)-Math.PI / 5);

            for (int i = 0; i < 50; i++)
                AddPhysicsCube();


            var heightMap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.003f), 100, 1, 50, 1, 1, 1);
             var vertexArray = heightMap.GenerateVertexArray();
            var indexArray = heightMap.GenerateIndices();
            GameObject heightMapObject = new GameObject();
            ProceduralShape shape = new ProceduralShape(vertexArray, indexArray);
            shape.SetColor(Color.OrangeRed);
            RenderGeometryComponent renderGeom = new RenderGeometryComponent(shape);
            EffectRenderComponent renderComponent = new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded"));
            heightMapObject.AddComponent(renderGeom);
            heightMapObject.AddComponent(renderComponent);
            heightMapObject.AddComponent(new StaticMeshColliderComponent(heightMapObject, heightMap.GetVertices(), heightMap.GetIndices().ToArray()));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(heightMapObject);

            SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);

          

            SetupVehicle();

        }

        private void SetupVehicle()
        {
            float width = 2.5f;
            float height = 0.75f;
            float length = 4.5f;
            float scale = 1f;

            var vehicleShape = new ProceduralCuboid(width/2 * scale, length/2 * scale, height/2 * scale);
            vehicleShape.Translate(new Vector3(0, 0.5f, 0));
            vehicleShape.SetColor(Color.Red);
            vehicleObject = GameObjectFactory.CreateRenderableGameObjectFromShape(vehicleShape, EffectLoader.LoadSM5Effect("flatshaded"));
            vehicleObject.AddComponent(new ShadowCasterComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(vehicleObject);


            ProceduralSphere cyl = new ProceduralSphere(10, 10);
            cyl.Scale(0.375f * scale);
            //cyl.Transform(Matrix.CreateRotationZ(MathHelper.PiOver2));
            wheel1 = GameObjectFactory.CreateRenderableGameObjectFromShape(cyl, EffectLoader.LoadSM5Effect("flatshaded"));      
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(wheel1);



            var bodies = new List<CompoundShapeEntry>
                {
                    new CompoundShapeEntry(new BoxShape(width * scale, height *scale, length * scale), new BEPUutilities.Vector3(0, 0, 0), 60),
                    //new CompoundShapeEntry(new BoxShape(2.5f, .3f, 2f), new BEPUutilities.Vector3(0, .75f / 2 + .3f / 2, .5f), 1)
                };
            var body = new CompoundBody(bodies, 61);
            body.CollisionInformation.LocalPosition = new BEPUutilities.Vector3(0, .5f, 0);
            body.Position = new BEPUutilities.Vector3(10, 20, 10);
            testVehicle = new Vehicle(body);

            BEPUutilities.Quaternion localWheelRotation = BEPUutilities.Quaternion.Identity;
            BEPUutilities.Matrix wheelGraphicRotation = BEPUutilities.Matrix.Identity;
            testVehicle.AddWheel(new Wheel(
                                new CylinderCastWheelShape(.375f * scale, 0.2f * scale, localWheelRotation, wheelGraphicRotation, false),
                                new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(-1.1f, -0.1f, 1.8f)),
                                new WheelDrivingMotor(2.5f, 30000, 10000),
                                new WheelBrake(1.5f, 2, .02f),
                                new WheelSlidingFriction(4, 5)));
            testVehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(.375f * scale, 0.2f * scale, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(-1.1f, -0.1f, -1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));
            testVehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(.375f * scale, 0.2f * scale, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(1.1f, -0.1f, 1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));
            testVehicle.AddWheel(new Wheel(
                                 new CylinderCastWheelShape(.375f * scale, 0.2f * scale, localWheelRotation, wheelGraphicRotation, false),
                                 new WheelSuspension(2000, 100f, BEPUutilities.Vector3.Down, 0.325f, new BEPUutilities.Vector3(1.1f, -0.1f, -1.8f)),
                                 new WheelDrivingMotor(2.5f, 30000, 10000),
                                 new WheelBrake(1.5f, 2, .02f),
                                 new WheelSlidingFriction(4, 5)));

            foreach (Wheel wheel in testVehicle.Wheels)
            {
                //This is a cosmetic setting that makes it looks like the car doesn't have antilock brakes.
                wheel.Shape.FreezeWheelsWhileBraking = true;

                //By default, wheels use as many iterations as the space.  By lowering it,
                //performance can be improved at the cost of a little accuracy.
                //However, because the suspension and friction are not really rigid,
                //the lowered accuracy is not so much of a problem.
                wheel.Suspension.SolverSettings.MaximumIterationCount = 1;
                wheel.Brake.SolverSettings.MaximumIterationCount = 1;
                wheel.SlidingFriction.SolverSettings.MaximumIterationCount = 1;
                wheel.DrivingMotor.SolverSettings.MaximumIterationCount = 1;
            }

            SystemCore.PhysicsSimulation.Add(testVehicle);
        }

        private void AddPhysicsCube()
        {
            ProceduralCube shape = new ProceduralCube();
            var gameObject = new GameObject();
            gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
            gameObject.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            gameObject.AddComponent(new ShadowCasterComponent());
            gameObject.AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.box));
            gameObject.Transform.SetPosition(RandomHelper.GetRandomVector3(10, 100));
            SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(gameObject);
        }

        public override void OnRemove()
        {

            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            RayCastResult result;
            if (input.MouseLeftPress())
            {



                if (SystemCore.PhysicsSimulation.RayCast(input.GetBepuProjectedMouseRay(), out result))
                {
                    GameObject parent = result.HitObject.Tag as GameObject;
                }
            }

            UpdateVehicle(dt);

            base.Update(gameTime);
        }

        private void UpdateVehicle(float dt)
        {

            vehicleObject.Transform.WorldMatrix = MonoMathHelper.GenerateMonoMatrixFromBepu(testVehicle.Body.WorldTransform);

            wheel1.Transform.WorldMatrix = MonoMathHelper.GenerateMonoMatrixFromBepu(testVehicle.Wheels[0].Shape.WorldTransform);

            if (input.IsKeyDown(Keys.E))
            {
                //Drive
                testVehicle.Wheels[1].DrivingMotor.TargetSpeed = ForwardSpeed;
                testVehicle.Wheels[3].DrivingMotor.TargetSpeed = ForwardSpeed;
            }
            else if (input.IsKeyDown(Keys.D))
            {
                //Reverse
                testVehicle.Wheels[1].DrivingMotor.TargetSpeed = BackwardSpeed;
                testVehicle.Wheels[3].DrivingMotor.TargetSpeed = BackwardSpeed;
            }
            else
            {
                //Idle
                testVehicle.Wheels[1].DrivingMotor.TargetSpeed = 0;
                testVehicle.Wheels[3].DrivingMotor.TargetSpeed = 0;
            }
            if (input.IsKeyDown(Keys.Space))
            {
                //Brake
                foreach (Wheel wheel in testVehicle.Wheels)
                {
                    wheel.Brake.IsBraking = true;
                }
            }
            else
            {
                //Release brake
                foreach (Wheel wheel in testVehicle.Wheels)
                {
                    wheel.Brake.IsBraking = false;
                }
            }
            //Use smooth steering; while held down, move towards maximum.
            //When not pressing any buttons, smoothly return to facing forward.
            float angle;
            bool steered = false;
            if (input.IsKeyDown(Keys.S))
            {
                steered = true;
                angle = Math.Max(testVehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * dt, -MaximumTurnAngle);
                testVehicle.Wheels[1].Shape.SteeringAngle = angle;
                testVehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (input.IsKeyDown(Keys.F))
            {
                steered = true;
                angle = Math.Min(testVehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * dt, MaximumTurnAngle);
                testVehicle.Wheels[1].Shape.SteeringAngle = angle;
                testVehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (!steered)
            {
                //Neither key was pressed, so de-steer.
                if (testVehicle.Wheels[1].Shape.SteeringAngle > 0)
                {
                    angle = Math.Max(testVehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * dt, 0);
                    testVehicle.Wheels[1].Shape.SteeringAngle = angle;
                    testVehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
                else
                {
                    angle = Math.Min(testVehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * dt, 0);
                    testVehicle.Wheels[1].Shape.SteeringAngle = angle;
                    testVehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
            }
        }

        public override void Render(GameTime gameTime)
        {
            


            base.Render(gameTime);
        }
    }
}
