using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Paths.PathFollowing;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;

namespace MSRestMatch.GameServer
{
    class Player : GameObject
    {
        public Color PlayerColor { get; set; }
        public float DesiredHeading { get; set; }
        public Vector3 DesiredPosition { get; set; }
        public Player()
        {
            this.AddComponent(new PlayerControlComponent());
            this.AddComponent(new RenderGeometryComponent(new ProceduralSphere(10, 10)));
            this.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            this.AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.sphere));
            this.AddComponent(new ShadowCasterComponent());
        }
    }


    class PlayerControlComponent : IComponent, IUpdateable
    {

        EntityMover mover;
        EntityRotator rotator;

        public PlayerControlComponent()
        {
            Enabled = true;
        }
        public bool Enabled
        {
            get; set;
        }
        public GameObject ParentObject
        {
            get; set;
        }
        public int UpdateOrder
        {
            get; set;
        }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        Player player;
        Vector2 currentForward;
        PhysicsComponent physicsComponent;

        public void Initialise()
        {

        }

        public void PostInitialise()
        {
            player = ParentObject as Player;
            this.physicsComponent = ParentObject.GetComponent<PhysicsComponent>();

            mover = new EntityMover(physicsComponent.PhysicsEntity);
            rotator = new EntityRotator(physicsComponent.PhysicsEntity);
            SystemCore.PhysicsSimulation.Add(mover);
            SystemCore.PhysicsSimulation.Add(rotator);

            mover.LinearMotor.Settings.Servo.SpringSettings.Stiffness /= 10000;
            mover.LinearMotor.Settings.Servo.SpringSettings.Damping /= 10000;

        }

        public void Update(GameTime gameTime)
        {
            TurnToDesiredHeading();

            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.O))
            {
                player.DesiredPosition += new Vector3(10, 0, 0);
                //physicsComponent.PhysicsEntity.LinearVelocity = new BEPUutilities.Vector3(-50, 0, 0);
            }
            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.P))
            {
                player.DesiredPosition -= new Vector3(10, 0, 0);

            }
            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Q))
            {
                player.DesiredPosition += new Vector3(0, 0, 10);
            }
            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.A))
            {
                player.DesiredPosition -= new Vector3(0, 0, 10);

            }

            mover.TargetPosition = player.DesiredPosition.ToBepuVector();

            if (physicsComponent.InCollision())
            {

                return;
            }
        }

        private void TurnToDesiredHeading()
        {
            //get current heading
            currentForward = ParentObject.Transform.AbsoluteTransform.Forward.ToVector2XZ();
            float heading = MathHelper.ToDegrees(MonoMathHelper.GetHeadingFromVector(currentForward));
            heading = (heading + 360) % 360;

            Vector2 desiredForward = MonoMathHelper.GetVectorFromHeading(MathHelper.ToRadians(player.DesiredHeading - 360));

            //rotator.TargetOrientation = BEPUutilities.Quaternion.

            //if (heading != player.DesiredHeading)
            //{
            //    currentForward = Vector2.Lerp(currentForward, desiredForward, 0.1f);
            //    player.Transform.SetLookAndUp(new Vector3(currentForward.X, 0, currentForward.Y), Vector3.Up);
            //}
        }



    }
}
