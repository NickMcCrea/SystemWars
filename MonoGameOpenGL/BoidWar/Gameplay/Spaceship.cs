using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.UpdateableSystems;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;

namespace BoidWar.Gameplay
{
    public class SpaceShip
    {

        private float forwardThrustInSpace = 30f;
        private float forwardThrustInAtmosphere = 3f;

        private PlayerIndex playerIndex;
        public GameObject ShipObject { get; set; }

        public CompoundBody PhysicsBody { get; set; }
        private List<CompoundShapeEntry> bodies;

        public bool IsActive = false;
        ChaseCamera chaseCamera;

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



            chaseCamera = new ChaseCamera();
            chaseCamera.DesiredPositionOffset = new Microsoft.Xna.Framework.Vector3(0.0f, 40f, 55f);
            chaseCamera.LookAtOffset = new Microsoft.Xna.Framework.Vector3(0.0f, 0.0f, 0);
            chaseCamera.Stiffness = 2000;
            chaseCamera.Damping = 600;
            chaseCamera.Mass = 50f;
            chaseCamera.NearZ = 0.5f;
            chaseCamera.FarZ = 10000.0f;
            SystemCore.SetActiveCamera(chaseCamera);
        }


        public void Update(GameTime gameTime, bool inAtmosphere)
        {

            float thrustToUse = forwardThrustInSpace;
            if (inAtmosphere)
                thrustToUse = forwardThrustInAtmosphere;

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
                    PhysicsBody.LinearVelocity += PhysicsBody.WorldTransform.Forward * (rightTrigger * (thrustToUse));
                }

                float speed = 0.05f;

                BEPUutilities.Vector3 left = currentLeft * leftStick.Y * speed;
                PhysicsBody.AngularVelocity += currentLeft * leftStick.Y * speed;
                PhysicsBody.AngularVelocity += currentUp * leftStick.X * speed;

                if (SystemCore.Input.GamePadButtonDown(Buttons.RightShoulder, playerIndex))
                {
                    PhysicsBody.AngularVelocity += currentForward * speed;
                }
                if (SystemCore.Input.GamePadButtonDown(Buttons.LeftShoulder, playerIndex))
                {
                    PhysicsBody.AngularVelocity -= currentForward * speed;
                }


                PhysicsBody.LinearVelocity += PhysicsBody.WorldTransform.Up * rightStick.Y;
                PhysicsBody.LinearVelocity -= PhysicsBody.WorldTransform.Left * rightStick.X;

                chaseCamera.DesiredPositionOffset = new Microsoft.Xna.Framework.Vector3(0.0f, 0, 200);
                chaseCamera.LookAtOffset = new Microsoft.Xna.Framework.Vector3(0.0f, 0.0f, 0);
                chaseCamera.ChasePosition = ShipObject.Transform.AbsoluteTransform.Translation;
                chaseCamera.ChaseDirection = ShipObject.Transform.AbsoluteTransform.Forward * 0.1f;
                chaseCamera.Up = ShipObject.Transform.AbsoluteTransform.Up;
                chaseCamera.Update(gameTime);
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
            SystemCore.SetActiveCamera(chaseCamera);

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
