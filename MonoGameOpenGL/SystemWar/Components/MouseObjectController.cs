using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEngineCore.GameObject.Components
{
    public class MouseObjectController : IComponent, IUpdateable
    {
        private Vector2 mouseSteer;

        float movementSpeed = 1f;

        public GameObject ParentObject
        {
            get;
            set;
        }

        public MouseObjectController()
        {

        }

        public void Initialise()
        {
            Enabled = true;
            this.inputManager = SystemCore.Input;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {

            if (inputManager.ScrollDelta > 0)
                movementSpeed *= 2;
            if (inputManager.ScrollDelta < 0)
                movementSpeed /= 2;

            Vector3 translation = Vector3.Zero;
            if (inputManager.IsKeyDown(Keys.Left))
                ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Forward, -0.01f);
            if (inputManager.IsKeyDown(Keys.Right))
                ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Forward, 0.01f);

            if (inputManager.IsKeyDown(Keys.Up))
                translation += (ParentObject.Transform.WorldMatrix.Forward);
            if (inputManager.IsKeyDown(Keys.Down))
                translation -= (ParentObject.Transform.WorldMatrix.Forward);

            if (inputManager.IsKeyDown(Keys.RightShift))
                movementSpeed = 1000f;

            if (inputManager.IsKeyDown(Keys.RightControl))
                movementSpeed = 100f;


            if (inputManager.IsKeyDown(Keys.NumPad0))
                movementSpeed = 0.1f;

            int deadZoneSize = 30;
            if (inputManager.MouseOffsetFromCenter.X > deadZoneSize || inputManager.MouseOffsetFromCenter.X < -deadZoneSize
                || inputManager.MouseOffsetFromCenter.Y < -deadZoneSize || inputManager.MouseOffsetFromCenter.Y > deadZoneSize)
                mouseSteer = new Vector2(inputManager.MouseOffsetFromCenter.X, inputManager.MouseOffsetFromCenter.Y);


            mouseSteer *= 0.000005f;


            ParentObject.Transform.Translate(translation * movementSpeed);
            ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Up, -mouseSteer.X * gameTime.ElapsedGameTime.Milliseconds);
            ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Left, mouseSteer.Y * gameTime.ElapsedGameTime.Milliseconds);


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