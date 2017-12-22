using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameDirectX11;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.ScreenManagement;
using BoidWar.Screens;

namespace BoidWar
{
    class MainMenuScreen : Screen
    {
        public MainMenuScreen()
            : base()
        {

            string screenOne = "Survival";
            string screenTwo = "Options";


            SystemCore.GetSubsystem<GUIManager>().CreateDefaultMenuScreen("Boid War", SystemCore.ActiveColorScheme, screenOne, screenTwo);
            SystemCore.CursorVisible = true;

            Button b = SystemCore.GetSubsystem<GUIManager>().GetControl(screenOne) as Button;
            b.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new SurvivalModeScreen());
            };

            Button a = SystemCore.GetSubsystem<GUIManager>().GetControl(screenTwo) as Button;
            a.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new RenderTestScreen());
            };
            input.AddKeyPressBinding("Escape", Microsoft.Xna.Framework.Input.Keys.Escape).InputEventActivated += (x, y) =>
            {
                SystemCore.Game.Exit();
            };


        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();

            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            base.Render(gameTime);
        }
    }
}
