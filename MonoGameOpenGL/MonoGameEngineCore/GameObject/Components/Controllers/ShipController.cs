using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace MonoGameEngineCore.GameObject.Components
{
    public class ShipController : IComponent, IUpdateable
    {
        float mainThrust;

        float pitch = 0;
        float roll = 0;
        float yaw = 0;
        Vector3 velocity;
        public GameObject ParentObject
        {
            get;
            set;
        }



        public ShipController()
        {
            mainThrust = 0;
        

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
        

            float bleedRate = 0.95f;
            float turnRate = 0.00005f;
            float thrustRate = 0.0005f;
            float rightTrigger = inputManager.GetRightTrigger();
            float leftTrigger = inputManager.GetLeftTrigger();
            float leftStickHorizontal = inputManager.GetLeftStickState().X;
            float leftStickVertical = inputManager.GetLeftStickState().Y;
            float rightStickHorizontal = inputManager.GetRightStickState().X;
            float rightStickVertical = inputManager.GetRightStickState().Y;

            if (inputManager.GamePadButtonDown(Buttons.A))
                thrustRate *= 10;

            if (inputManager.GamePadButtonDown(Buttons.B))
                thrustRate *= 100;

            if (inputManager.GamePadButtonDown(Buttons.Y))
                thrustRate *= 1000;

            if (inputManager.GamePadButtonDown(Buttons.X))
                thrustRate /= 10;


            float currentForwardThrustValue = MathHelper.SmoothStep(0, 1, inputManager.GetRightTrigger());
            float currentReverseThrustValue = MathHelper.SmoothStep(0, 1, inputManager.GetLeftTrigger());
            mainThrust += currentForwardThrustValue;
            mainThrust -= currentReverseThrustValue;
            mainThrust = MathHelper.Clamp(mainThrust, 0, 5);



            Vector3 mainThrustVelocity = (ParentObject.Transform.WorldMatrix.Forward * mainThrust * gameTime.ElapsedGameTime.Milliseconds);
            velocity += mainThrustVelocity * thrustRate;

            ParentObject.Transform.Translate(velocity);

         
            if (inputManager.GamePadButtonDown(Buttons.LeftShoulder))
                yaw += turnRate * gameTime.ElapsedGameTime.Milliseconds;

            if (inputManager.GamePadButtonDown(Buttons.RightShoulder))
                yaw -= turnRate * gameTime.ElapsedGameTime.Milliseconds;

            pitch +=  leftStickVertical * gameTime.ElapsedGameTime.Milliseconds * turnRate;
            roll += leftStickHorizontal * gameTime.ElapsedGameTime.Milliseconds * turnRate;
           
            ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Left, pitch);
            ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Forward, roll);
            ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Up, yaw);

          
            pitch *= bleedRate;
            roll *= bleedRate;
            yaw *= bleedRate;
            velocity *= bleedRate;
           

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