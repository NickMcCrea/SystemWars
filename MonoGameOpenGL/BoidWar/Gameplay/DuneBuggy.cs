using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.UpdateableSystems;
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

                particles.settings.Duration = TimeSpan.FromSeconds(2f);

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


    public class SpaceShip
    {

        private float forwardThrust = 30f;

        private PlayerIndex playerIndex;
        public GameObject ShipObject { get; set; }

        public CompoundBody PhysicsBody { get; set; }
        private List<CompoundShapeEntry> bodies;

        public bool IsActive = false;


        public SpaceShip(PlayerIndex player, Color color, Microsoft.Xna.Framework.Vector3 position)
        {


            this.playerIndex = player;



            var shape = new ProceduralCuboid(1, 1, 1);
            shape.SetColor(color);

            ShipObject = new GameObject();

            ShipObject.AddComponent(new RenderGeometryComponent(shape));
            ShipObject.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            ShipObject.AddComponent(new ShadowCasterComponent());

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(ShipObject);

            //Build the ship
            var shipFuselage = new CompoundShapeEntry(new BoxShape(1, 1f, 1), new BEPUutilities.Vector3(0, 0, 0), 4);

            bodies = new List<CompoundShapeEntry>();
            bodies.Add(shipFuselage);


            PhysicsBody = new CompoundBody(bodies, 10);





            PhysicsBody.Orientation = BEPUutilities.Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Right, (float)Math.PI / 2) * BEPUutilities.Quaternion.CreateFromAxisAngle(BEPUutilities.Vector3.Forward, (float)Math.PI / 2);
            PhysicsBody.Position = position.ToBepuVector();
            SystemCore.PhysicsSimulation.Add(PhysicsBody);
            PhysicsBody.IsAffectedByGravity = false;
            PhysicsBody.Tag = "spaceship";



            PhysicsBody.AngularDamping = 0.99f;
            PhysicsBody.LinearDamping = 0.99f;
        }


        public void Update(GameTime gameTime)
        {

            ShipObject.Transform.AbsoluteTransform = MonoMathHelper.GenerateMonoMatrixFromBepu(PhysicsBody.WorldTransform);

            if (IsActive)
            {

                var currentLeft = PhysicsBody.WorldTransform.Left;
                var currentForward = PhysicsBody.WorldTransform.Forward;
                var currentUp = PhysicsBody.WorldTransform.Up;

                Microsoft.Xna.Framework.Vector2 leftStick = SystemCore.Input.GetLeftStickState(playerIndex);
                leftStick.X = -leftStick.X;
                Microsoft.Xna.Framework.Vector2 rightStick = SystemCore.Input.GetRightStickState(playerIndex);

                float rightTrigger = SystemCore.Input.GetRightTrigger(playerIndex);

                if (rightTrigger > 0)
                {
                    PhysicsBody.LinearVelocity += PhysicsBody.WorldTransform.Forward * (rightTrigger * (forwardThrust));
                }

                float speed = 0.1f;
                PhysicsBody.AngularVelocity += currentLeft * leftStick.Y * speed;
                PhysicsBody.AngularVelocity += currentForward * rightStick.X * speed;
                PhysicsBody.AngularVelocity += currentUp * leftStick.X * speed;


            }


        }

        internal void Teleport(Microsoft.Xna.Framework.Vector3 vector3)
        {
            BEPUutilities.Vector3 v = vector3.ToBepuVector();
            PhysicsBody.Position = v;

        }

        internal void Activate()
        {
            IsActive = true;
           
        }

        internal void Deactivate()
        {
            IsActive = false;
        }
    }


    public class Thruster : Updateable, IDuringForcesUpdateable
    {
        private float age;
        private float lifeSpan;

        /// <summary>
        /// Constructs a thruster originating at the given position, pushing in the given direction.
        /// </summary>
        /// <param name="targetEntity">Entity that the force will be applied to.</param>
        /// <param name="pos">Origin of the force.</param>
        /// <param name="dir">Direction of the force.</param>
        /// <param name="time">Total lifespan of the force.  A lifespan of zero is infinite.</param>
        public Thruster(Entity targetEntity, BEPUutilities.Vector3 pos, BEPUutilities.Vector3 dir, float time)
        {
            Target = targetEntity;
            Position = pos;
            Direction = dir;
            LifeSpan = time;
        }

        /// <summary>
        /// Gets or sets the position of the thruster in the local space of the target entity.
        /// </summary>
        public BEPUutilities.Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the direction of the force in the local space of the target entity.  Magnitude of the force is equal to the magnitude of the direction.
        /// </summary>
        public BEPUutilities.Vector3 Direction { get; set; }

        /// <summary>
        /// Gets or sets the entity to apply force to.
        /// </summary>
        public Entity Target { get; set; }

        /// <summary>
        /// Gets or sets the length of time that the thruster has been firing.
        /// This can be reset to 'refresh' the life of the force.
        /// </summary>
        public float Age
        {
            get { return age; }
            set
            {
                age = value;
                if (age < LifeSpan)
                    IsUpdating = true; //IsUpdating is a property of the Updateable class.  The updateDuringForces method won't be called if IsUpdating is false.
            }
        }

        /// <summary>
        /// Maximum life span of the force, after which the thruster will deactivate.
        /// Set to 0 for infinite lifespan.
        /// </summary>
        public float LifeSpan
        {
            get { return lifeSpan; }
            set
            {
                lifeSpan = value;
                if (lifeSpan > Age || lifeSpan == 0)
                    IsUpdating = true; //Wake the thruster up if it's young again.
            }
        }


        /// <summary>
        /// Applies the thruster forces.
        /// Called automatically by the owning space during a space update.
        /// </summary>
        /// <param name="dt">Simulation timestep.</param>
        void IDuringForcesUpdateable.Update(float dt)
        {
            //Transform the local position and direction into world space.
            BEPUutilities.Vector3 worldPosition = Target.Position + Matrix3x3.Transform(Position, Target.OrientationMatrix);
            BEPUutilities.Vector3 worldDirection = Matrix3x3.Transform(Direction, Target.OrientationMatrix);
            //Apply the force.
            Target.ApplyImpulse(worldPosition, worldDirection * dt);


            Age += dt;
            if (LifeSpan > 0 && Age > LifeSpan)
            {
                IsUpdating = false; //The thruster has finished firing.
            }
        }
    }
}
