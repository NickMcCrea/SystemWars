using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using SystemWar;

namespace MonoGameEngineCore.GameObject.Components
{
    public class ShipController : IComponent, IUpdateable
    {
        float mainThrust;

        float pitch = 0;
        float roll = 0;
        float yaw = 0;
        Vector3 velocity;
        private Ship ship;

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
        

            float bleedRate = 0.95f;
            float turnRate = 0.00004f;
          
            float rightTrigger = inputManager.GetRightTrigger();
            float leftTrigger = inputManager.GetLeftTrigger();
            float leftStickHorizontal = inputManager.GetLeftStickState().X;
            float leftStickVertical = inputManager.GetLeftStickState().Y;
            float rightStickHorizontal = inputManager.GetRightStickState().X;
            float rightStickVertical = inputManager.GetRightStickState().Y;
          



            ship.SetThrust(rightTrigger);
            ship.SetSuperThrust(leftTrigger);
            //ship.SetThrust(-leftTrigger);
          
            ship.LateralThrust(-leftStickHorizontal, leftStickVertical);
            ship.Yaw(-rightStickHorizontal * 0.01f);
            ship.Pitch(-rightStickVertical * 0.01f);


            if (inputManager.GamePadButtonDown(Buttons.RightShoulder))
                ship.Roll(0.01f);
            if (inputManager.GamePadButtonDown(Buttons.LeftShoulder))
                ship.Roll(-0.01f);
            else
            {
                if (ship.InAtmosphere)
                    ship.RealignShip();
            }
           

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