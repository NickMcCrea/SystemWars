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
using MSRestMatch.Screens;

namespace MSRestMatch
{
    class MainMenuScreen : Screen
    {
        public MainMenuScreen()
            : base()
        {

            string screenOne = "Training Arena";
            string screenTwo = "Death Match Arena";


            SystemCore.GetSubsystem<GUIManager>().CreateDefaultMenuScreen("Main Menu", SystemCore.ActiveColorScheme, screenOne, screenTwo);
            SystemCore.CursorVisible = true;

            Button b = SystemCore.GetSubsystem<GUIManager>().GetControl(screenOne) as Button;
            b.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new TrainingSimScreen());
            };
            b = SystemCore.GetSubsystem<GUIManager>().GetControl(screenTwo) as Button;
            b.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new DeathMatchArenaScreen());
            };



        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();

            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.Game.Exit();

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            base.Render(gameTime);
        }
    }
}
