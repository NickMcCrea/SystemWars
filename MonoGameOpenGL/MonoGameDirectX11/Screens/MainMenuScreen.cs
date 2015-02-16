using Microsoft.Xna.Framework;
using MonoGameDirectX11.Screens;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;

namespace MonoGameDirectX11
{
    class MainMenuScreen : Screen
    {
        public MainMenuScreen()
            : base()
        {

            string screenOne = "Render Test";
            string screenTwo = "Atmosphere Test";
            string screenThree = "Physics Test";
        

            SystemCore.GetSubsystem<GUIManager>().CreateDefaultMenuScreen("Main Menu", SystemCore.ActiveColorScheme, screenOne, screenTwo, screenThree);
            SystemCore.CursorVisible = true;

            Button b = SystemCore.GetSubsystem<GUIManager>().GetControl(screenOne) as Button;
            b.OnClick += (sender, args) =>
           { 
                SystemCore.ScreenManager.AddAndSetActive(new RenderTestScreen());
            };

            Button a = SystemCore.GetSubsystem<GUIManager>().GetControl(screenTwo) as Button;
            a.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new AtmosphereTest());
            };

            Button c = SystemCore.GetSubsystem<GUIManager>().GetControl(screenThree) as Button;
            c.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new PhysicsTestScreen());
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
