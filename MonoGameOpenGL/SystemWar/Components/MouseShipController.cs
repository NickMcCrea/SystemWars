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

            if (inputManager.IsKeyDown(Keys.W))
                ship.AlterThrust(0.1f);
            if (inputManager.IsKeyDown(Keys.S))
                ship.AlterThrust(-0.1f);
            if (inputManager.IsKeyDown(Keys.A))
                ship.Roll(-0.01f);
            if (inputManager.IsKeyDown(Keys.D))
                ship.Roll(0.01f);
          

            if (inputManager.IsKeyDown(Keys.Up))
               ship.Pitch(0.01f);
            if (inputManager.IsKeyDown(Keys.Down))
               ship.Pitch(-0.01f);
            if (inputManager.IsKeyDown(Keys.Left))
                ship.Yaw(0.01f);
            if (inputManager.IsKeyDown(Keys.Right))
                ship.Yaw(-0.01f);




            int deadZoneSize = 30;
            //if (inputManager.MouseOffsetFromCenter.X > deadZoneSize || inputManager.MouseOffsetFromCenter.X < -deadZoneSize
            //    || inputManager.MouseOffsetFromCenter.Y < -deadZoneSize || inputManager.MouseOffsetFromCenter.Y > deadZoneSize)
            //    mouseSteer = new Vector2(inputManager.MouseOffsetFromCenter.X, inputManager.MouseOffsetFromCenter.Y);


            //mouseSteer *= 0.000005f;

            //ship.Yaw(-mouseSteer.X);
            //ship.Pitch(mouseSteer.Y);

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