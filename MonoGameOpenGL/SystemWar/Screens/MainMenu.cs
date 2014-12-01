using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemWar.Screens
{
    public class MainMenuScreen : Screen
    {
        Label titleLabel;
        Label startLabel;

        public MainMenuScreen()
            : base()
        {


            titleLabel = new Label(GUIFonts.Fonts["neuropolitical"], "System War");
          
            Vector2 titlePoint = GUIManager.ScreenMiddle();
            titlePoint -= new Vector2(0, SystemCore.GraphicsDevice.Viewport.Height / 3);
            titleLabel.SetPosition(titlePoint);
            FadeTransition fadeIn = new FadeTransition(0, 1, 1000, 50000);
            titleLabel.ApplyTransition(fadeIn);
            SystemCore.GUIManager.AddControl(titleLabel);

            startLabel = new Label(GUIFonts.Fonts["neuropolitical"], "Press Start");
            Vector2 startLabelPoint = GUIManager.ScreenMiddle();
            startLabel.SetPosition(startLabelPoint);
            FadeTransition fadeInStart = new FadeTransition(0, 1, 4000, 50000);
            startLabel.ApplyTransition(fadeInStart);
            SystemCore.GUIManager.AddControl(startLabel);

            GamePadButtonEvent gamePadButtonEvent = new GamePadButtonEvent(Buttons.Start);
            input.AddBinding(new InputBinding("startScreenActivate", gamePadButtonEvent));
            input.AddBinding(input.AddKeyPressBinding("startScreenActivate", Keys.Space));

        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();

            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (input.EvaluateInputBinding("startScreenActivate"))
            {
                SystemCore.ScreenManager.AddAndSetActive(new MainGameScreen());
            }

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            base.Render(gameTime);
        }
    }
}
