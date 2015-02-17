using System;
using SystemWar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEngineCore.GameObject.Components
{
    public class MouseKeyboardShipController : IComponent, IUpdateable
    {
        private Vector2 mouseSteer;

      
        private Ship ship;
        public GameObject ParentObject
        {
            get;
            set;
        }

        public MouseKeyboardShipController()
        {

        }

        public void Initialise()
        {
            Enabled = true;
            this.inputManager = SystemCore.Input;
            ship = ParentObject as Ship;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {


            float thrustChange = 0.1f;
            float rollPitchYawChange = 0.01f;
            if (inputManager.KeyPress(Keys.W))
                ship.Transform.Translate(ship.Transform.WorldMatrix.Forward * (float)gameTime.ElapsedGameTime.TotalMilliseconds);

                //ship.SetThrust(1000f);
            if (inputManager.KeyPress(Keys.S))
                ship.Transform.Translate(-ship.Transform.WorldMatrix.Forward * (float)gameTime.ElapsedGameTime.TotalMilliseconds);

            if (inputManager.IsKeyDown(Keys.A))
                ship.Roll(-rollPitchYawChange);
            if (inputManager.IsKeyDown(Keys.D))
                ship.Roll(rollPitchYawChange);
          

            if (inputManager.IsKeyDown(Keys.Up))
                ship.Pitch(rollPitchYawChange);
            if (inputManager.IsKeyDown(Keys.Down))
                ship.Pitch(-rollPitchYawChange);
            if (inputManager.IsKeyDown(Keys.Left))
                ship.Yaw(rollPitchYawChange);
            if (inputManager.IsKeyDown(Keys.Right))
                ship.Yaw(-rollPitchYawChange);




            
        

            

        }

        public int UpdateOrder
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> UpdateOrderChanged;
        private InputManager inputManager;
    }
}