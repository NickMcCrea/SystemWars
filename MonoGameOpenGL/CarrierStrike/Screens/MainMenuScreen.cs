using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using CarrierStrike.Screens;

namespace CarrierStrike
{
    class MainMenuScreen : Screen
    {
        public MainMenuScreen()
            : base()
        {

           

        }

        public override void OnInitialise()
        {
            string screenOne = "Test Island";


            SystemCore.GetSubsystem<GUIManager>().CreateDefaultMenuScreen("Carrier Strike Menu", SystemCore.ActiveColorScheme, screenOne);
            SystemCore.CursorVisible = true;

            Button b = SystemCore.GetSubsystem<GUIManager>().GetControl(screenOne) as Button;
            b.OnClick += (sender, args) =>
            {
                SystemCore.ScreenManager.AddAndSetActive(new TestIslandScreen());
            };

            input.AddKeyPressBinding("Escape", Microsoft.Xna.Framework.Input.Keys.Escape).InputEventActivated += (x, y) =>
            {
                SystemCore.Game.Exit();
            };
            base.OnInitialise();
        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            SystemCore.GameObjectManager.ClearAllObjects();
            input.ClearBindings();

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
