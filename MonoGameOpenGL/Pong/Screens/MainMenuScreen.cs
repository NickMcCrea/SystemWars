using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.ScreenManagement;

namespace Pong
{
    class MainMenuScreen : Screen
    {

        private Button startButton;
        private Button quitButton;

        public MainMenuScreen()
            : base()
        {
            var gui = SystemCore.GetSubsystem<GUIManager>();
            gui.CreateDefaultMenuScreen("Pong", Color.Black, Color.White, Color.Black, Color.Black, "Start", "Quit");
            startButton = gui.GetControl("Start") as Button;
            quitButton = gui.GetControl("Quit") as Button;

            startButton.OnClick += StartClick;
            quitButton.OnClick += QuitClick;
        }

        private void QuitClick(object sender, EventArgs eventArgs)
        {
            SystemCore.Game.Exit();
        }

        private void StartClick(object sender, EventArgs eventArgs)
        {     
            SystemCore.GetSubsystem<ScreenManager>().AddAndSetActive(new GameScreen());
        }

        private void UnsubscribeEvents()
        {
            quitButton.OnClick -= QuitClick;
            startButton.OnClick -= StartClick;
        }

        public override void OnRemove()
        {
            UnsubscribeEvents();
            SystemCore.GetSubsystem<GUIManager>().ClearAllControls();
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
