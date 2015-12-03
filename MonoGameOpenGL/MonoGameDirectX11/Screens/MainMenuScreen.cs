using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            SystemCore.CursorVisible = true;

            var typesInNameSpace =
                Assembly.GetAssembly(this.GetType()).GetTypes().Where(t => typeof(Screen).IsAssignableFrom(t) && t != typeof(MainMenuScreen));

            List<string> names = new List<string>();

            foreach (Type type in typesInNameSpace)
            {
                names.Add(type.Name);
            }


            SystemCore.GetSubsystem<GUIManager>()
                .CreateDefaultMenuScreen("Main Menu", SystemCore.ActiveColorScheme, names);


            foreach (Type type in typesInNameSpace)
            {
                Button b = SystemCore.GetSubsystem<GUIManager>().GetControl(type.Name) as Button;
                b.OnClick += (sender, args) =>
                {
                    Screen screen = Activator.CreateInstance(type) as Screen;
                    SystemCore.ScreenManager.AddAndSetActive(screen);
                };
            }


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
