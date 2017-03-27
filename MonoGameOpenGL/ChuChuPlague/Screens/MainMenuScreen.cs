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

namespace ChuChuPlague
{
    class MainMenuScreen : Screen
    {
        public MainMenuScreen()
            : base()
        {

            string screenOne = "One";
            string screenTwo = "Two";
            string screenThree = "Three";
            string screenFour = "Four";



            SystemCore.GetSubsystem<GUIManager>().CreateDefaultMenuScreen("Chu Chu Plague", SystemCore.ActiveColorScheme, screenOne, screenTwo, screenThree, screenFour);
            SystemCore.CursorVisible = true;

            Button b = SystemCore.GetSubsystem<GUIManager>().GetControl(screenOne) as Button;
            b.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new RenderTestScreen());
            };

            Button a = SystemCore.GetSubsystem<GUIManager>().GetControl(screenTwo) as Button;
            a.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new RenderTestScreen());
            };

            Button c = SystemCore.GetSubsystem<GUIManager>().GetControl(screenThree) as Button;
            c.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new RenderTestScreen());
            };

            Button d = SystemCore.GetSubsystem<GUIManager>().GetControl(screenFour) as Button;
            c.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new RenderTestScreen());
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
