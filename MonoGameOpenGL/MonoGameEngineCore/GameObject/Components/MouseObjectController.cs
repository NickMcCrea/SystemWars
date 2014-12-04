using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEngineCore.GameObject.Components
{
    public class MouseObjectController : IComponent, IUpdateable
    {
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
            float movementSpeed = 1f;

            Vector3 translation = Vector3.Zero;
            if (inputManager.IsKeyDown(Keys.Left))
                translation += (ParentObject.Transform.WorldMatrix.Left);
            if (inputManager.IsKeyDown(Keys.Right))
                translation -= (ParentObject.Transform.WorldMatrix.Left);

            if (inputManager.IsKeyDown(Keys.Up))
                translation += (ParentObject.Transform.WorldMatrix.Forward);
            if (inputManager.IsKeyDown(Keys.Down))
                translation -= (ParentObject.Transform.WorldMatrix.Forward);

            if (inputManager.IsKeyDown(Keys.RightShift))
                movementSpeed = 1000000f;

            if (inputManager.IsKeyDown(Keys.RightControl))
                movementSpeed = 100f;


            if (inputManager.IsKeyDown(Keys.NumPad0))
                movementSpeed = 0.1f;



            ParentObject.Transform.Translate(translation * movementSpeed);

            ParentObject.Transform.Rotate(Vector3.Up, -inputManager.MouseDelta.X * gameTime.ElapsedGameTime.Milliseconds * 0.001f);
            ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Left, inputManager.MouseDelta.Y * gameTime.ElapsedGameTime.Milliseconds * 0.001f);


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