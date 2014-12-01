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

namespace MonoGameEngineCore.ScreenManagement
{
    public class MouseCamScreen : Screen
    {
        protected MouseFreeCamera mouseCamera;

        public MouseCamScreen()
            : base()
        {
            SystemCore.CursorVisible = false;
            fpsLabel.Visible = true;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            var basicEffect = new BasicEffect(SystemCore.GraphicsDevice);
            basicEffect.VertexColorEnabled = true;
            basicEffect.EnableDefaultLighting();
            basicEffect.PreferPerPixelLighting = true;


            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);



            AddInputBindings();


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
                    mouseCamera.Left();
                if (input.EvaluateInputBinding("CameraRight"))
                    mouseCamera.Right();


                mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);

                input.CenterMouse();
            }


            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(SystemCore.ActiveColorScheme.Color2);

            base.Render(gameTime);


        }

        protected void SetCameraMovement(bool active)
        {
            SystemCore.CursorVisible = !active;
        }
    }
}
