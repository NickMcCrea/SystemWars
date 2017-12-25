using BEPUphysics.Entities;
using BEPUphysics.Vehicle;
using BEPUphysicsDemos.SampleCode;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using Particle3DSample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidWar.Gameplay
{

    public class DuneBuggy
    {

        public Microsoft.Xna.Framework.Vector3 smoothedPosition;
        public Microsoft.Xna.Framework.Vector3 smoothedForward;
        private List<Microsoft.Xna.Framework.Vector3> positions;
        private List<Microsoft.Xna.Framework.Vector3> forwards;

        private Vehicle vehicle;
        public GameObject body;
        UprightSpring uprightSpring;

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
        public float TurnSpeed = BEPUutilities.MathHelper.Pi;
        private PlayerIndex playerIndex;
        public UprightSpring uprightSpringConstraint;


        public DuneBuggy(PlayerIndex player, Color color, Microsoft.Xna.Framework.Vector3 position)
        {
            positions = new List<Microsoft.Xna.Framework.Vector3>();
            forwards = new List<Microsoft.Xna.Framework.Vector3>();

            this.playerIndex = player;

            this.vehicle = VehicleFactory.Create(position.ToBepuVector());
            SystemCore.PhysicsSimulation.Add(vehicle);

            var shape = new ProceduralCuboid(2.5f / 2, 4.5f / 2, .75f / 2);
            shape.SetColor(color);
            body = GameObjectFactory.CreateRenderableGameObjectFromShape(shape, EffectLoader.LoadSM5Effect("flatshaded"));
            body.AddComponent(new ShadowCasterComponent());

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(body);

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



                wheels.Add(wheel);
            }



            //uprightSpringConstraint = new UprightSpring(vehicle.Body, BEPUutilities.Vector3.Up, 0.1f, (float)Math.PI, 10000f);
            // SystemCore.PhysicsSimulation.Add(uprightSpringConstraint);
        }


        public void Update(GameTime gameTime)
        {
            body.Transform.AbsoluteTransform = MonoMathHelper.GenerateMonoMatrixFromBepu(vehicle.Body.WorldTransform);
            for (int i = 0; i < vehicle.Wheels.Count; i++)
            {
                Wheel w = vehicle.Wheels[i];
                wheels[i].Transform.AbsoluteTransform = MonoMathHelper.GenerateMonoMatrixFromBepu(w.Shape.WorldTransform);

                var particleSystem = wheels[i].GetComponent<SquareParticleSystem>();

                if (particleSystem != null && (w.DrivingMotor.TargetSpeed == ForwardSpeed || w.DrivingMotor.TargetSpeed == BackwardSpeed))
                    particleSystem.AddParticle(wheels[i].Transform.AbsoluteTransform.Translation, Microsoft.Xna.Framework.Vector3.Zero);

            }

            positions.Add(body.Transform.AbsoluteTransform.Translation);
            forwards.Add(body.Transform.AbsoluteTransform.Forward);



            if (positions.Count > 120)
            {
                positions.RemoveRange(0, 60);
                forwards.RemoveRange(0, 60);
            }



            if (positions.Count > 60)
            {
                smoothedPosition = Microsoft.Xna.Framework.Vector3.Zero;
                smoothedForward = Microsoft.Xna.Framework.Vector3.Zero;
                int count = 0;
                for (int i = positions.Count - 1; i > 0; i--)
                {
                    smoothedPosition += positions[i];
                    smoothedForward += forwards[i];
                    count++;
                    if (count >= 60)
                        break;

                }
                smoothedPosition /= count;
                smoothedForward /= count;
            }





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
