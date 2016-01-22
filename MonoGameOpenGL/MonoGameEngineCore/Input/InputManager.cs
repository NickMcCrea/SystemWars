using System.Collections.Generic;
using System.Resources;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore
{
    public interface InputEvent
    {
        bool Evaluate(InputManager input, GameTime gameTime);
    }
    public abstract class KeyEvent : InputEvent
    {
        public Keys key;

        public KeyEvent(Keys key)
        {
            this.key = key;
        }



        public abstract bool Evaluate(InputManager input, GameTime gameTime);

    }
    public class KeyPress : KeyEvent
    {
        public KeyPress(Keys key)
            : base(key)
        {
        }

        public override bool Evaluate(InputManager input, GameTime gameTime)
        {
            return input.KeyPress(key);
        }
    }
    public class KeyHold : KeyEvent
    {
        private TimeSpan time;
        private bool counting = false;
        private float timeInSeconds;

        public KeyHold(Keys key, float timeInSeconds)
            : base(key)
        {
            this.timeInSeconds = timeInSeconds;
            time = new TimeSpan(0, 0, 0, (int)(timeInSeconds * 100));
        }

        public override bool Evaluate(InputManager input, GameTime gameTime)
        {
            if (input.KeyPress(key))
                counting = true;

            if (counting)
            {
                if (input.IsKeyUp(key))
                {
                    Reset();
                    return false;
                }

                time -= new TimeSpan(0, 0, 0, (int)gameTime.ElapsedGameTime.TotalMilliseconds);

                if (time < TimeSpan.Zero)
                {
                    return true;
                }
            }

            return false;


        }

        private void Reset()
        {
            time = new TimeSpan(0, 0, 0, (int)(timeInSeconds * 100));
            counting = false;
        }
    }
    public class KeyDown : KeyEvent
    {
        public KeyDown(Keys key)
            : base(key)
        {
        }

        public override bool Evaluate(InputManager input, GameTime gameTime)
        {
            return input.IsKeyDown(key);
        }
    }
    public class KeyUp : KeyEvent
    {
        public KeyUp(Keys key)
            : base(key)
        {
        }

        public override bool Evaluate(InputManager input, GameTime gameTime)
        {
            return input.IsKeyUp(key);
        }
    }

    public class GamePadButtonEvent : InputEvent
    {
        private Buttons button;
        private PlayerIndex index;

        public GamePadButtonEvent(Buttons button)
        {
            this.button = button;        
        }

        public bool Evaluate(InputManager input, GameTime gameTime)
        {
            return input.GamePadButtonPress(button);
        }
    }





    public class InputBinding
    {
        public bool Active { get; set; }
        public InputEvent InputEvent;
        public string ControlName { get; set; }
       
        public EventHandler InputEventActivated;

        public InputBinding(string controlName, InputEvent inputEvent)
        {
            this.InputEvent = inputEvent;
            this.ControlName = controlName;
            Active = true;
        }

        public void NotifySubsribers()
        {
            if (InputEventActivated != null)
            {
                InputEventActivated(this, null);
            }
        }
    }

    public class InputManager : IGameSubSystem
    {

        PlayerIndex currentIndex = PlayerIndex.One;
        private MouseState currentMouseState;
        private MouseState oldMouseState;
        private KeyboardState currentKeyboardState;
        private KeyboardState oldKeyboardState;
        private GamePadState oldGamePadState;
        private GamePadState currentGamePadState;
        private Dictionary<string,List<InputBinding>> inputBindings;
        private Point screenMidPoint;
        public Point MouseDelta { get; private set; }
        public Point MouseOffsetFromCenter { get; private set; }
        public int ScrollDelta { get; private set; }
        public Point MousePosition
        {
            get;
            private set;
        }

        public void Initalise()
        {
            currentMouseState = Mouse.GetState();
            currentKeyboardState = Keyboard.GetState();
            currentGamePadState = GamePad.GetState(currentIndex);
            inputBindings =new Dictionary<string, List<InputBinding>>();
            screenMidPoint = new Point(SystemCore.GraphicsDevice.Viewport.Width/2,
                SystemCore.GraphicsDevice.Viewport.Height/2);




        }

        public void Update(GameTime gameTime)
        {
            RefreshInputStates();
            CalculateMouseDelta();
            MousePosition = currentMouseState.Position;
            EvaluateBindings(gameTime);
          
        }

        private void EvaluateBindings(GameTime gametime)
        {
            foreach (string key in inputBindings.Keys)
            {
                foreach (InputBinding binding in inputBindings[key])
                {

                    if (binding.InputEvent.Evaluate(this, gametime))
                    {
                        binding.Active = true;
                        binding.NotifySubsribers();
                    }
                    else
                    {
                        binding.Active = false;
                    }
                }
            }
        }

        private void RefreshInputStates()
        {
            oldGamePadState = currentGamePadState;
            currentGamePadState = GamePad.GetState(currentIndex); 
            oldMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
            oldKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();
        }

        public void Render(GameTime gameTime)
        {

        }

        public bool IsKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        public bool IsKeyUp(Keys key)
        {
            return currentKeyboardState.IsKeyUp(key);
        }

        public bool KeyPress(Keys key)
        {
            return oldKeyboardState.IsKeyUp(key) && currentKeyboardState.IsKeyDown(key);
        }

        public bool MouseLeftDown()
        {
            return currentMouseState.LeftButton == ButtonState.Pressed;
        }

        public bool MouseLeftPress()
        {
            return (currentMouseState.LeftButton == ButtonState.Pressed &&
                    oldMouseState.LeftButton == ButtonState.Released);
        }

        public bool MouseRightDown()
        {
            return currentMouseState.RightButton == ButtonState.Pressed;
        }

        public bool MouseRightPress()
        {
            return (currentMouseState.RightButton == ButtonState.Pressed &&
                    oldMouseState.RightButton == ButtonState.Released);
        }

        public void CalculateMouseDelta()
        {
            MouseDelta = currentMouseState.Position - oldMouseState.Position;

            MouseOffsetFromCenter = new Point(currentMouseState.Position.X - screenMidPoint.X,
                currentMouseState.Position.Y - screenMidPoint.Y);

            ScrollDelta = currentMouseState.ScrollWheelValue - oldMouseState.ScrollWheelValue;
        }

        public Vector2 GetLeftStickState()
        {
            return currentGamePadState.ThumbSticks.Left;
        }

        public Vector2 GetRightStickState()
        {
            return currentGamePadState.ThumbSticks.Right;
        }

        public Vector2 GetLeftStickDelta()
        {
            return currentGamePadState.ThumbSticks.Left - oldGamePadState.ThumbSticks.Left;
        }

        public Vector2 GetRightStickDelta()
        {
            return currentGamePadState.ThumbSticks.Right - oldGamePadState.ThumbSticks.Right;
        }

        public bool RightStickClick()
        {
            return currentGamePadState.Buttons.RightStick == ButtonState.Pressed && oldGamePadState.Buttons.RightStick == ButtonState.Released;
        }

        public bool LeftStickClick()
        {
            return currentGamePadState.Buttons.LeftStick == ButtonState.Pressed && oldGamePadState.Buttons.LeftStick == ButtonState.Released;
        }

        public float GetLeftTrigger()
        {
            return currentGamePadState.Triggers.Left;
        }

        public float GetRightTrigger()
        {
            return currentGamePadState.Triggers.Right;
        }

        internal bool MouseInRectangle(Rectangle Rect)
        {
            return Rect.Contains(currentMouseState.X, currentMouseState.Y);
        }

        internal bool MouseLeftUp()
        {
            return currentMouseState.LeftButton == ButtonState.Released;
        }

        public void AddBinding(InputBinding binding)
        {
            if (inputBindings.ContainsKey(binding.ControlName))
                inputBindings[binding.ControlName].Add(binding);
            else
            {
                inputBindings.Add(binding.ControlName, new List<InputBinding>());
                inputBindings[binding.ControlName].Add(binding);
            }
        }

        public InputBinding AddKeyPressBinding(string name, Keys key)
        {
            MonoGameEngineCore.KeyPress  kp = new KeyPress(key);
            InputBinding b = new InputBinding(name, kp);

            AddBinding(b);

            return b;
        }

        public InputBinding AddKeyDownBinding(string name, Keys key)
        {
            MonoGameEngineCore.KeyDown kp = new KeyDown(key);
            InputBinding b = new InputBinding(name, kp);

            AddBinding(b);

            return b;
        }

        public bool EvaluateInputBinding(string name)
        {
            if (inputBindings.ContainsKey(name))
            {
                foreach (InputBinding binding in inputBindings[name])
                    if (binding.Active)
                        return true;
            }
            return false;
        }

        public void CenterMouse()
        {
            if (MouseDelta.X == 0 && MouseDelta.Y == 0)
            {
                Mouse.SetPosition(SystemCore.GraphicsDevice.Viewport.Width / 2, SystemCore.GraphicsDevice.Viewport.Height / 2);
                MouseDelta = Point.Zero;
                currentMouseState = Mouse.GetState();
            }
        }

        public bool GamePadButtonPress(Buttons button)
        {
            if (currentGamePadState.IsButtonDown(button))
                if (oldGamePadState.IsButtonUp(button))
                    return true;
            return false;
        }

        public bool GamePadButtonDown(Buttons button)
        {
            return currentGamePadState.IsButtonDown(button);
        }

        public Ray GetProjectedMouseRay()
        {
            return MonoMathHelper.GetProjectedMouseRay(SystemCore.ActiveCamera.View,
                SystemCore.ActiveCamera.Projection, SystemCore.GraphicsDevice, (int) SystemCore.Input.MousePosition.X,
                (int) SystemCore.Input.MousePosition.Y);
        }

        public BEPUutilities.Ray GetBepuProjectedMouseRay()
        {
            return ConversionHelper.MathConverter.Convert(GetProjectedMouseRay());
        }

        public bool GetMouseImpact(out RayCastResult result)
        {
            return SystemCore.PhysicsSimulation.RayCast(SystemCore.Input.GetBepuProjectedMouseRay(), out result);
        }

        internal bool GetPlaneMouseRayCollision(Plane activePlane, out Vector3 collisionPoint)
        {
            Ray mouseRay = SystemCore.Input.GetProjectedMouseRay();
            float? result;
            mouseRay.Intersects(ref activePlane, out result);
            if (result.HasValue)
            {
                collisionPoint = mouseRay.Position + mouseRay.Direction * result.Value;
                return true;
            }
            collisionPoint = Vector3.Zero;
            return false;
        }
    }
}