using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUphysics.Paths.PathFollowing;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;

namespace MSRestMatch.GameServer
{





    class Player : GameObject, IUpdateable
    {
        public bool Invulnurable { get; set; }
        public Color PlayerColor { get; set; }
        public float DesiredHeading { get; set; }
        public Vector3 DesiredPosition { get; set; }
        public int Health { get; set; }
        public Label PlayerLabel { get; set; }
        public List<Weapon> Weapons;

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public Weapon CurrentWeapon { get; set; }

        public bool Enabled
        {
            get; set;
        }

        public int UpdateOrder
        {
            get; set;
        }

        public Player()
        {
            this.AddComponent(new PlayerControlComponent());
            this.AddComponent(new RenderGeometryComponent(new ProceduralSphere(10, 10)));
            this.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            this.AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.sphere));
            this.AddComponent(new ShadowCasterComponent());
            Health = 100;
            Weapons = new List<Weapon>();
            Weapons.Add(WeaponFactory.CreatePistol());
            CurrentWeapon = Weapons[0];


            PlayerLabel = new Label(GUIFonts.Fonts["neurosmall"], "");
            SystemCore.GUIManager.AddControl(PlayerLabel);
        }

        public void FireCurrentWeapon()
        {
            Projectile p = new Projectile(Transform.AbsoluteTransform.Translation, Transform.AbsoluteTransform.Forward * CurrentWeapon.ProjectileSpeed, CurrentWeapon);
        }

        internal void DamagePlayer(Weapon firingWeapon)
        {
            
            Health -= firingWeapon.Damage;
            if (Health < 0)
                Health = 0;
        }

        internal float GetHeading()
        {
            var currentForward = Transform.AbsoluteTransform.Forward.ToVector2XZ();
            float heading = MathHelper.ToDegrees(MonoMathHelper.GetHeadingFromVector(currentForward));
            heading = (heading + 360) % 360;
            return heading;
        }

        internal float GetX()
        {
            return Transform.AbsoluteTransform.Translation.X;
        }
        internal float GetY()
        {
            return Transform.AbsoluteTransform.Translation.Z;
        }

        public void Update(GameTime gameTime)
        {
            PlayerLabel.Text = Name;
            PlayerLabel.SetPosition(MonoMathHelper.ScreenProject(Transform.AbsoluteTransform.Translation - Vector3.Forward * 5, SystemCore.Viewport, 
                SystemCore.ActiveCamera.View, 
                SystemCore.ActiveCamera.Projection, 
                Matrix.Identity).ToVector2XY());
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
            mover.LinearMotor.Settings.Servo.SpringSettings.Damping /= 1000;

        }

        public void Update(GameTime gameTime)
        {
            TurnToDesiredHeading();

            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.O))
            {
                player.DesiredPosition += new Vector3(10, 0, 0);
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
            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.M))
            {
                player.FireCurrentWeapon();
            }

            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.K))
            {
                player.DesiredHeading += 10;
                player.DesiredHeading = player.DesiredHeading % 360;
            }
            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.L))
            {
                player.DesiredHeading -= 10;
                player.DesiredHeading = player.DesiredHeading % 360;
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
            var heading = player.GetHeading();

            Vector2 desiredForward = MonoMathHelper.GetVectorFromHeading(MathHelper.ToRadians(player.DesiredHeading - 360));

            var lookMatrix = Matrix.CreateWorld(ParentObject.Transform.AbsoluteTransform.Translation,
                new Vector3(desiredForward.X, 0, desiredForward.Y), Vector3.Up);

            var bepuMatrix = MonoMathHelper.GenerateBepuMatrixFromMono(lookMatrix);

            BEPUutilities.Quaternion desiredRot = BEPUutilities.Quaternion.CreateFromRotationMatrix(bepuMatrix);
            rotator.TargetOrientation = desiredRot;


        }



    }
}
