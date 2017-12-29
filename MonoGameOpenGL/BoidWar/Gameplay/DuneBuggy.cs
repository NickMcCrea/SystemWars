using BEPUphysics.Entities;
using BEPUphysics.Vehicle;
using BEPUphysicsDemos.SampleCode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using Particle3DSample;
using System;
using System.Collections.Generic;

namespace BoidWar.Gameplay
{

    public class DuneBuggyPlanetCamera
    {
        float xTilt;
        float yTilt;
        public GameObject CameraObject;
        private DuneBuggy buggy;

        public DuneBuggyPlanetCamera(DuneBuggy buggy)
        {
            this.buggy = buggy;
            CameraObject = new GameObject();
            CameraObject.AddComponent(new ComponentCamera());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(CameraObject);
        }

        public void Update(GameTime gameTime, Planet currentPlanet, PlayerIndex playerIndex)
        {
            float radius = currentPlanet.radius;

            Vector3 desiredCameraPos = buggy.BuggyObject.Transform.AbsoluteTransform.Translation;
            desiredCameraPos.Normalize();
            desiredCameraPos *= (radius + 200f);


            CameraObject.Transform.AbsoluteTransform = MonoMathHelper.GenerateWorldMatrixFromPositionAndTarget(desiredCameraPos, buggy.BuggyObject.Transform.AbsoluteTransform.Translation);

            Vector3 desiredForward = currentPlanet.Position - buggy.BuggyObject.Transform.AbsoluteTransform.Translation;
            desiredForward.Normalize();

            Vector3 desiredUp = Vector3.Cross(desiredForward, Vector3.Right);
            desiredUp.Normalize();

            Vector3 desiredRight = Vector3.Cross(desiredUp, desiredForward);
            desiredRight.Normalize();


            CameraObject.Transform.RotateAround(desiredUp, buggy.BuggyObject.Transform.AbsoluteTransform.Translation, xTilt);
            CameraObject.Transform.RotateAround(desiredRight, buggy.BuggyObject.Transform.AbsoluteTransform.Translation, yTilt);


            if (SystemCore.Input.GetRightStickState(playerIndex).X > 0)
                xTilt += 0.01f;
            if (SystemCore.Input.GetRightStickState(playerIndex).X < 0)
                xTilt -= 0.01f;

            if (SystemCore.Input.GetRightStickState(playerIndex).Y > 0)
                yTilt += 0.01f;
            if (SystemCore.Input.GetRightStickState(playerIndex).Y < 0)
                yTilt -= 0.01f;

        }

        public void Activate()
        {
            SystemCore.SetActiveCamera(CameraObject.GetComponent<ComponentCamera>());
        }
    }

    public class DuneBuggy
    {

        public GameObject BuggyObject { get; set; }
        private Vehicle vehicle;
        private UprightSpring uprightSpring;
        private List<GameObject> wheels;
        public float BackwardSpeed = -13;
        public float ForwardSpeed = 30;
        private bool IsActive;
        public float MaximumTurnAngle = (float)Math.PI / 3;
        public float TurnSpeed = BEPUutilities.MathHelper.Pi;
        private PlayerIndex playerIndex;
        public UprightSpring uprightSpringConstraint;


        public DuneBuggy(PlayerIndex player, Color color, Microsoft.Xna.Framework.Vector3 position)
        {


            this.playerIndex = player;

            this.vehicle = VehicleFactory.Create(position.ToBepuVector());
            SystemCore.PhysicsSimulation.Add(vehicle);

            var shape = new ProceduralCuboid(2.5f / 2, 4.5f / 2, .75f / 2);
            shape.SetColor(color);
            BuggyObject = GameObjectFactory.CreateRenderableGameObjectFromShape(shape, EffectLoader.LoadSM5Effect("flatshaded"));
            BuggyObject.AddComponent(new ShadowCasterComponent());

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(BuggyObject);

            wheels = new List<GameObject>();
            foreach (Wheel w in vehicle.Wheels)
            {
                var cube = new ProceduralCube();
                cube.SetColor(Color.Maroon);
                var wheel = GameObjectFactory.CreateRenderableGameObjectFromShape(cube, EffectLoader.LoadSM5Effect("flatshaded"));
                wheel.AddComponent(new ShadowCasterComponent());

                var particles = new SquareParticleSystem();

                wheel.AddComponent(particles);

                SystemCore.GameObjectManager.AddAndInitialiseGameObject(wheel);

                particles.settings.Duration = TimeSpan.FromSeconds(2f);

                wheels.Add(wheel);
            }

            uprightSpringConstraint = new UprightSpring(vehicle.Body, BEPUutilities.Vector3.Up, 0.1f, (float)Math.PI, 1000f);
            SystemCore.PhysicsSimulation.Add(uprightSpringConstraint);
        }


        public void Update(GameTime gameTime)
        {



            BuggyObject.Transform.AbsoluteTransform = MonoMathHelper.GenerateMonoMatrixFromBepu(vehicle.Body.WorldTransform);

            for (int i = 0; i < vehicle.Wheels.Count; i++)
            {
                Wheel w = vehicle.Wheels[i];
                wheels[i].Transform.AbsoluteTransform = MonoMathHelper.GenerateMonoMatrixFromBepu(w.Shape.WorldTransform);


                var particleSystem = wheels[i].GetComponent<SquareParticleSystem>();

                if (w.HasSupport)
                {
                    if (particleSystem != null && (w.DrivingMotor.TargetSpeed == ForwardSpeed || w.DrivingMotor.TargetSpeed == BackwardSpeed))
                        particleSystem.AddParticle(wheels[i].Transform.AbsoluteTransform.Translation, Microsoft.Xna.Framework.Vector3.Zero);

                }

            }



            if (!IsActive)
                return;









            if (SystemCore.Input.IsKeyDown(Keys.E) || SystemCore.Input.GamePadButtonDown(Buttons.RightTrigger, playerIndex))
            {
                //Drive
                vehicle.Wheels[1].DrivingMotor.TargetSpeed = ForwardSpeed;
                vehicle.Wheels[3].DrivingMotor.TargetSpeed = ForwardSpeed;
                vehicle.Wheels[2].DrivingMotor.TargetSpeed = ForwardSpeed;
                vehicle.Wheels[0].DrivingMotor.TargetSpeed = ForwardSpeed;
            }
            else if (SystemCore.Input.IsKeyDown(Keys.D) || SystemCore.Input.GamePadButtonDown(Buttons.LeftTrigger, playerIndex))
            {
                //Reverse
                vehicle.Wheels[1].DrivingMotor.TargetSpeed = BackwardSpeed;
                vehicle.Wheels[3].DrivingMotor.TargetSpeed = BackwardSpeed;
                vehicle.Wheels[2].DrivingMotor.TargetSpeed = BackwardSpeed;
                vehicle.Wheels[0].DrivingMotor.TargetSpeed = BackwardSpeed;
            }
            else
            {
                //Idle
                vehicle.Wheels[1].DrivingMotor.TargetSpeed = 0;
                vehicle.Wheels[3].DrivingMotor.TargetSpeed = 0;
                vehicle.Wheels[2].DrivingMotor.TargetSpeed = 0;
                vehicle.Wheels[0].DrivingMotor.TargetSpeed = 0;
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
            if (SystemCore.Input.IsKeyDown(Keys.S) || SystemCore.Input.GetLeftStickState(playerIndex).X < 0)
            {
                steered = true;
                angle = Math.Max(vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, -MaximumTurnAngle);
                vehicle.Wheels[1].Shape.SteeringAngle = angle;
                vehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (SystemCore.Input.IsKeyDown(Keys.F) || SystemCore.Input.GetLeftStickState(playerIndex).X > 0)
            {
                steered = true;
                angle = Math.Min(vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, MaximumTurnAngle);
                vehicle.Wheels[1].Shape.SteeringAngle = angle;
                vehicle.Wheels[3].Shape.SteeringAngle = angle;
            }
            if (!steered)
            {
                //Neither key was pressed, so de-steer.
                if (vehicle.Wheels[1].Shape.SteeringAngle > 0)
                {
                    angle = Math.Max(vehicle.Wheels[1].Shape.SteeringAngle - TurnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
                    vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
                else
                {
                    angle = Math.Min(vehicle.Wheels[1].Shape.SteeringAngle + TurnSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
                    vehicle.Wheels[1].Shape.SteeringAngle = angle;
                    vehicle.Wheels[3].Shape.SteeringAngle = angle;
                }
            }
        }

        public void Flip()
        {
            vehicle.Body.AngularVelocity += BEPUutilities.Vector3.Right * 10f;
        }

        internal void Activate()
        {
            IsActive = true;

        }

        internal void Deactivate()
        {
            IsActive = false;
        }

        internal void Teleport(Microsoft.Xna.Framework.Vector3 vector3)
        {
            BEPUutilities.Vector3 v = vector3.ToBepuVector();

            foreach (Entity e in vehicle.InvolvedEntities)
            {
                e.Position = v;
            }
        }
    }



}
