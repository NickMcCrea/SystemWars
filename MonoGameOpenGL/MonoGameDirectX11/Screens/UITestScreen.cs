using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;

namespace MonoGameDirectX11.Screens
{
    class UITestScreen : RenderTestScreen
    {
        readonly Panel p;

        public UITestScreen()
            : base()
        {
         

            GUIManager guiManager = SystemCore.GetSubsystem<GUIManager>();

            //panel
            p = new Panel(new Rectangle(10, 10, 100, 100), GUITexture.Textures["blank"]);
            p.Alpha = 0.1f;
            p.MainColor = SystemCore.ActiveColorScheme.Color4;
            guiManager.AddControl(p);


        
            //button
            Button b = new Button(new Rectangle(500, 10, 100, 40), GUITexture.Textures["blank"]);
            b.Alpha = 0.4f;
            b.MainColor = SystemCore.ActiveColorScheme.Color3;
            b.HighlightColor = SystemCore.ActiveColorScheme.Color4;
            guiManager.AddControl(b);

            p.Children.Add(b);

            //label attached to button
            Label l = new Label(GUIFonts.Fonts["test"], "Button");
            l.TextOutline = false;
            l.TextColor = SystemCore.ActiveColorScheme.Color1;
            b.AttachLabel(l);

          

            //bordered button
            Button c = new Button(new Rectangle(700, 10, 100, 40), GUITexture.Textures["blank"]);
            c.Alpha = 0.4f;
            c.MainColor = SystemCore.ActiveColorScheme.Color3;
            c.HighlightColor = SystemCore.ActiveColorScheme.Color4;
            c.Border = true;
            c.BorderColor = SystemCore.ActiveColorScheme.Color5;
            c.BorderThickness = 1;
            guiManager.AddControl(c);


            FadeTransition fadeTest = new FadeTransition(1, 0, 500, 5000);
            p.ApplyTransition(fadeTest);
          

            SetCameraMovement(false);
        }

     
        public override void Update(GameTime gameTime)
        {
            p.Translate(new Vector2(0, 1f));
           // b.Translate(new Vector2(0, 1f));
          

           

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
           
            base.Render(gameTime);


        }

    }
}
