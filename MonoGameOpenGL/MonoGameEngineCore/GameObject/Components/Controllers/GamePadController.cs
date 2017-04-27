using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.GameObject.Components
{
    public class GamePadObjectController : IComponent, IUpdateable
    {
        public GameObject ParentObject
        {
            get;
            set;
        }

        float speed;
        float speedMultiplier = 100;

        public GamePadObjectController()
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
            if (inputManager.GetRightTrigger() > 0 || inputManager.GetLeftTrigger() > 0)
                speed = (inputManager.GetRightTrigger() * speedMultiplier);
            else
                speed = 0;




            Vector3 translation = -(ParentObject.Transform.AbsoluteTransform.Left * inputManager.GetLeftStickState().X);
            translation += (ParentObject.Transform.AbsoluteTransform.Forward * inputManager.GetLeftStickState().Y);

            if (speed > 1)
                translation *= speed;

            if (inputManager.GetLeftTrigger() > 0.5f)
                translation /= 10f;

            ParentObject.Transform.Translate(translation);
            ParentObject.Transform.Rotate(ParentObject.Transform.AbsoluteTransform.Up, -inputManager.GetRightStickState().X * gameTime.ElapsedGameTime.Milliseconds * 0.01f);
            ParentObject.Transform.Rotate(ParentObject.Transform.AbsoluteTransform.Left, -inputManager.GetRightStickState().Y * gameTime.ElapsedGameTime.Milliseconds * 0.01f);

            if (inputManager.GamePadButtonDown(Buttons.RightShoulder))
                ParentObject.Transform.Rotate(ParentObject.Transform.AbsoluteTransform.Forward, 0.005f);

            if (inputManager.GamePadButtonDown(Buttons.LeftShoulder))
                ParentObject.Transform.Rotate(ParentObject.Transform.AbsoluteTransform.Forward, -0.005f);

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
