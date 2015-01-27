using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.ScreenManagement;

namespace Vector.Screens
{
    class MainMenuScreen : Screen
    {
        private Label titleLabel;
       
        public MainMenuScreen()
            : base()
        {
            var gui = SystemCore.GetSubsystem<GUIManager>();
           

            titleLabel = new Label(GUIFonts.Fonts["neuropolitical"], "Vector");
            titleLabel.SetPosition(new Vector2(GUIManager.ScreenMiddle().X, GUIManager.ScreenMiddle().Y));
            gui.AddControl(titleLabel);
           
        }

      
      

        private void UnsubscribeEvents()
        {
          
            
        }

        public override void OnRemove()
        {
            UnsubscribeEvents();
            SystemCore.GetSubsystem<GUIManager>().ClearAllControls();
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if(SystemCore.Input.KeyPress(Keys.Space))
                SystemCore.ScreenManager.AddAndSetActive(new GameScreen());

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            base.Render(gameTime);
        }
    }
}
