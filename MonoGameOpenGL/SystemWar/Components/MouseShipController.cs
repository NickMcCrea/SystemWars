using System;
using SystemWar;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEngineCore.GameObject.Components
{
    public class MouseKeyboardShipController : IComponent, IUpdateable
    {
        private Vector2 mouseSteer;
        private float thrustInput = 0;

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



            if (inputManager.IsKeyDown(Keys.RightShift))
                thrustInput += thrustChange;

            if (inputManager.IsKeyDown(Keys.RightControl))
                thrustInput -= thrustChange;

            if (thrustInput > 1)
                thrustInput = 1;

            ship.SetThrust(thrustInput);

            if(inputManager.IsKeyDown(Keys.LeftShift))
                ship.Transform.HighPrecisionTranslate(ship.Transform.AbsoluteTransform.Forward * 100f);

            float horizontal = 0;
            float vertical = 0;

            if (inputManager.IsKeyDown(Keys.A))
                horizontal = 1;
            if (inputManager.IsKeyDown(Keys.D))
                horizontal = -1;

            if (inputManager.IsKeyDown(Keys.W))
                vertical = 1;
            if (inputManager.IsKeyDown(Keys.S))
                vertical = -1;

            ship.LateralThrust(horizontal,vertical);

         

            if (inputManager.IsKeyDown(Keys.Up))
                ship.Pitch(rollPitchYawChange);
            if (inputManager.IsKeyDown(Keys.Down))
                ship.Pitch(-rollPitchYawChange);
            if (inputManager.IsKeyDown(Keys.Left))
                ship.Yaw(rollPitchYawChange);
            if (inputManager.IsKeyDown(Keys.Right))
                ship.Yaw(-rollPitchYawChange);



        }

        public void PostInitialise()
        {
          
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