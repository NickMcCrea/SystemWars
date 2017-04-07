using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.ScreenManagement;

namespace MonoGameDirectX11.Screens
{
    class UITest : TestScreen
    {
        Panel p;

        public UITest()
            : base()
        {
         

         
        }

        public override void OnInitialise()
        {
            base.OnInitialise();

            GUIManager guiManager = SystemCore.GetSubsystem<GUIManager>();

            //panel
            p = new Panel(new Rectangle(0, 0, GUIManager.ScreenRatioX(0.1f), GUIManager.ScreenRatioY(0.1f)), GUITexture.Textures["blank"]);
            p.HighlightAlpha = 0.3f;
            p.MainColor = SystemCore.ActiveColorScheme.Color4;
            p.Anchor(AnchorPoint.middle, GUIManager.ScreenPoint.midpoint, new Vector2(0, 0));
            guiManager.AddControl(p);



            //button
            Button b = new Button(new Rectangle(0, 0, GUIManager.ScreenRatioX(0.1f), GUIManager.ScreenRatioY(0.05f)), GUITexture.Textures["blank"]);
            b.HighlightAlpha = 0.4f;
            b.MainColor = SystemCore.ActiveColorScheme.Color3;
            b.HighlightColor = SystemCore.ActiveColorScheme.Color4;
            b.Anchor(AnchorPoint.topMid, GUIManager.ScreenPoint.topMiddle, Vector2.Zero);
            guiManager.AddControl(b);



            //label attached to button
            Label l = new Label(GUIFonts.Fonts["test"], "Button");
            l.TextOutline = false;
            l.TextColor = SystemCore.ActiveColorScheme.Color1;
            b.AttachLabel(l);



            //bordered button
            Button c = new Button(new Rectangle(700, 10, 100, 40), GUITexture.Textures["blank"]);
            c.MainAlpha = 0.4f;
            c.MainColor = SystemCore.ActiveColorScheme.Color3;
            c.HighlightColor = SystemCore.ActiveColorScheme.Color4;
            c.Border = true;
            c.BorderColor = SystemCore.ActiveColorScheme.Color5;
            c.BorderThickness = 1;
            guiManager.AddControl(c);


            FadeTransition fadeTest = new FadeTransition(1, 0, 500, 5000);
            p.ApplyTransition(fadeTest);

            //needs work
            ListBox listBox = new ListBox(new Rectangle(200, 200, 200, 200), GUITexture.Textures["blank"]);
            for (int i = 0; i < 100; i++)
            {
                listBox.AddItem(i.ToString());
            }
            guiManager.AddControl(listBox);

            //fucked
            ItemCarousel itemCarousel = new ItemCarousel(new Vector2(700, 500), true, 50, 50);
            for (int i = 0; i < 5; i++)
            {
                itemCarousel.Add(GUITexture.Textures["blank"], i.ToString());
            }
            guiManager.AddControl(itemCarousel);




            SetCameraMovement(false);
            
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
           
    

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(SystemCore.ActiveColorScheme.Color1);
           
            base.Render(gameTime);


        }

    }
}
