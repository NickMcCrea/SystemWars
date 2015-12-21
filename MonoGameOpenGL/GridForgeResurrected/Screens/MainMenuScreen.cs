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
using GridForgeResurrected.Screens;

namespace GridForgeResurrected
{
    class MainMenuScreen : Screen
    {
        public MainMenuScreen()
            : base()
        {

            string screenOne = "Combat Test";
            string screenTwo = "Procedural Test";
            string screenThree = "Editor Test";


            SystemCore.GetSubsystem<GUIManager>().CreateDefaultMenuScreen("Grid Forge", SystemCore.ActiveColorScheme, screenOne, screenTwo, screenThree);
            SystemCore.CursorVisible = true;

            Button b = SystemCore.GetSubsystem<GUIManager>().GetControl(screenOne) as Button;
            b.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new CombatArenaTest());
            };

            Button a = SystemCore.GetSubsystem<GUIManager>().GetControl(screenTwo) as Button;
            a.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new ProceduralTerrainTestScreen());
            };


            Button c = SystemCore.GetSubsystem<GUIManager>().GetControl(screenThree) as Button;
            c.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new EditorTest());
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
