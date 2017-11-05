using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
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
            this.AddComponent(new PhysicsComponent(true, false, PhysicsMeshType.sphere));
            this.AddComponent(new ShadowCasterComponent());
        }
    }


    class PlayerControlComponent : IComponent, IUpdateable
    {


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
        float maxSpeed = 0.02f;

        public void Initialise()
        {

        }

        public void PostInitialise()
        {
            player = ParentObject as Player;
            this.physicsComponent = ParentObject.GetComponent<PhysicsComponent>();
        }

        public void Update(GameTime gameTime)
        {
            TurnToDesiredHeading();

            Vector3 currentPos = ParentObject.Transform.AbsoluteTransform.Translation;
            if (!MonoMathHelper.AlmostEquals(currentPos, player.DesiredPosition, 0.1f))
            {
                Vector3 toTarget = player.DesiredPosition - currentPos;
                var distance = toTarget.Length();
                toTarget.Normalize();
                var speed = MathHelper.Clamp((distance * 0.001f), 0, maxSpeed);
                player.Transform.Velocity = toTarget * speed;
            }
            else
            {
                //close enough!
                player.Transform.Velocity = Vector3.Zero;
                player.DesiredPosition = player.Transform.AbsoluteTransform.Translation;
            }

            HandleCollisions();

        }

        private void TurnToDesiredHeading()
        {
            //get current heading
            currentForward = ParentObject.Transform.AbsoluteTransform.Forward.ToVector2XZ();
            float heading = MathHelper.ToDegrees(MonoMathHelper.GetHeadingFromVector(currentForward));
            heading = (heading + 360) % 360;

            Vector2 desiredForward = MonoMathHelper.GetVectorFromHeading(MathHelper.ToRadians(player.DesiredHeading - 360));

            if (heading != player.DesiredHeading)
            {
                currentForward = Vector2.Lerp(currentForward, desiredForward, 0.1f);
                player.Transform.SetLookAndUp(new Vector3(currentForward.X, 0, currentForward.Y), Vector3.Up);
            }
        }

        private void HandleCollisions()
        {
            var pairs = physicsComponent.PhysicsEntity.CollisionInformation.Pairs;
            foreach (CollidablePairHandler pair in pairs)
            {

                var contacts = pair.Contacts;
                foreach (ContactInformation contact in contacts)
                {

                    var remove = (-pair.Contacts[0].Contact.Normal * pair.Contacts[0].Contact.PenetrationDepth).ToXNAVector();
                    remove.Y = 0;
                    ParentObject.Transform.Translate(remove);
                    ParentObject.Transform.Velocity = Vector3.Zero;
                    ((Player)ParentObject).DesiredPosition = ParentObject.Transform.AbsoluteTransform.Translation;
                }

            }
        }

    }
}
