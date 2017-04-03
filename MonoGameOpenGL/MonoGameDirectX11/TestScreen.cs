using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering.Camera;
using MonoGameDirectX11;

namespace MonoGameEngineCore.ScreenManagement
{
    public class TestScreen : Screen
    {
        protected MouseFreeCamera mouseCamera;
        protected bool releaseMouse;
        public TestScreen()
            : base()
        {
           


        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = false;
            fpsLabel.Visible = true;

            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);



            AddInputBindings();
            base.OnInitialise();
        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            SystemCore.GameObjectManager.ClearAllObjects();
            input.ClearBindings();
            base.OnRemove();
        }

        private void AddInputBindings()
        {
            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("CameraForward", Keys.Up);
            input.AddKeyDownBinding("CameraBackward", Keys.Down);
            input.AddKeyDownBinding("CameraLeft", Keys.Left);
            input.AddKeyDownBinding("CameraRight", Keys.Right);
            input.AddKeyDownBinding("SlowCamera", Keys.RightShift);
            var binding = input.AddKeyPressBinding("WireframeToggle", Keys.Space);

            input.AddKeyPressBinding("MainMenu", Keys.Escape);

            var releaseMouseBinding = input.AddKeyPressBinding("MouseRelease", Keys.M);
            releaseMouseBinding.InputEventActivated += (x, y) => {
                releaseMouse = !releaseMouse;
                SystemCore.CursorVisible = releaseMouse;
            };


            binding.InputEventActivated += (x, y) => { SystemCore.Wireframe = !SystemCore.Wireframe; };
        }

        public override void Update(GameTime gameTime)
        {
            if (!SystemCore.CursorVisible)
            {

                mouseCamera.Slow = input.EvaluateInputBinding("SlowCamera");

                if (input.EvaluateInputBinding("CameraForward"))
                    mouseCamera.MoveForward();
                if (input.EvaluateInputBinding("CameraBackward"))
                    mouseCamera.MoveBackward();
                if (input.EvaluateInputBinding("CameraLeft"))
                    mouseCamera.MoveLeft();
                if (input.EvaluateInputBinding("CameraRight"))
                    mouseCamera.MoveRight();

                if (!releaseMouse)
                {
                    mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);
                    input.CenterMouse();
                }
            }

            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            
            base.Render(gameTime);


        }

        protected void SetCameraMovement(bool active)
        {
            SystemCore.CursorVisible = !active;
        }
    }
}
