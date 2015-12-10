using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MonoGameEngineCore;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GUI.Controls;
using Microsoft.Xna.Framework;
using NicksLib.Rendering;

namespace MonoGameEngineCore.GUI
{
    public class GUIManager : IGameSubSystem
    {

        public Dictionary<string, Palette> Palettes = new Dictionary<string, Palette>();
        SpriteBatch guiBatch;
        public List<BaseControl> controlsToAdd = new List<BaseControl>();
        public List<BaseControl> controls;
        private List<BaseControl> controlsToRemove;
        public bool MouseOverGUIElement { get; set; }
        static bool initialised;
        private Vector2 screenMidPoint;
        private Vector2 bottomLeft;
        private Vector2 bottomRight;
        private Vector2 topRight;
        private Vector2 topLeft;
        public enum ScreenPoint
        {
            midpoint,
            middleLeft,
            topleft,
            topright,
            bottomleft,
            bottomright,
            bottomMiddle,
            topMiddle

        }

        public GUIManager()
        {


        }

        public void Initalise()
        {
            guiBatch = new SpriteBatch(SystemCore.GraphicsDevice);
            controls = new List<BaseControl>();
            controlsToRemove = new List<BaseControl>();

            if (!initialised)
            {
                LoadTexturesAndFonts(SystemCore.ContentManager);
                initialised = true;
            }

            screenMidPoint = new Vector2(SystemCore.GraphicsDevice.Viewport.Width / 2, SystemCore.GraphicsDevice.Viewport.Height / 2);
            topLeft = Vector2.Zero;
            topRight = new Vector2(SystemCore.GraphicsDevice.Viewport.Width, 0);
            bottomLeft = new Vector2(0, SystemCore.GraphicsDevice.Viewport.Height);
            bottomRight = new Vector2(SystemCore.GraphicsDevice.Viewport.Width, SystemCore.GraphicsDevice.Viewport.Height);


            Palette a = new Palette();
            a.BorderColor = Color.Black;
            a.MainColor = Color.DimGray.SetAlpha(100);
            a.HighlightColor = Color.DarkOrange;
            a.SecondaryColor = Color.Black;
            a.Border = true;
            a.BorderThickness = 1;
            a.TextColor = Color.DarkOrange;
            a.TextOutlineColor = Color.Black;
            a.TextOutline = true;
            Palettes.Add("Dwarf", a);
        }

        private static void LoadTexturesAndFonts(ContentManager content)
        {
            GUIFonts.Fonts.Add("test", content.Load<SpriteFont>("Fonts/SimpleSansSerif"));
            GUIFonts.Fonts.Add("neuropolitical", content.Load<SpriteFont>("Fonts/neuropolitical"));
            GUIFonts.Fonts.Add("sansserif", content.Load<SpriteFont>("Fonts/SimpleSansSerif"));
            GUITexture.Textures.Add("blank", content.Load<Texture2D>("Textures/blank"));
        }

        public void Update(GameTime gameTime)
        {
            InputManager input = SystemCore.GetSubsystem<InputManager>();

            bool mouseOver = false;
            foreach (BaseControl control in controls)
            {

                control.Update(gameTime, input);

                if (control.MouseOver)
                    mouseOver = true;
            }
            MouseOverGUIElement = mouseOver;


            //remove old controls
            foreach (BaseControl c in controlsToRemove)
                controls.Remove(c);

            controlsToRemove.Clear();

            foreach (BaseControl c in controlsToAdd)
                controls.Add(c);

            controlsToAdd.Clear();
        }

        public void Render(GameTime gameTime)
        {
            guiBatch.Begin();

            foreach (BaseControl control in controls)
            {
                if (control.Visible)
                    control.Draw(gameTime, guiBatch, SystemCore.GraphicsDevice);
            }
            guiBatch.End();
        }

        public void AddControl(BaseControl control)
        {
            controlsToAdd.Add(control);
        }

        public Vector2 GetScreenPoint(ScreenPoint point)
        {
            if (point == ScreenPoint.midpoint)
                return new Vector2(SystemCore.GraphicsDevice.Viewport.Width / 2, SystemCore.GraphicsDevice.Viewport.Height / 2);
            if (point == ScreenPoint.topleft)
                return Vector2.Zero;
            if (point == ScreenPoint.topright)
                return new Vector2(SystemCore.GraphicsDevice.Viewport.Width, 0);
            if (point == ScreenPoint.bottomright)
                return new Vector2(SystemCore.GraphicsDevice.Viewport.Width, SystemCore.GraphicsDevice.Viewport.Height);
            if (point == ScreenPoint.bottomleft)
                return new Vector2(0, SystemCore.GraphicsDevice.Viewport.Height);
            if (point == ScreenPoint.bottomMiddle)
                return new Vector2(SystemCore.GraphicsDevice.Viewport.Width / 2, SystemCore.GraphicsDevice.Viewport.Height);
            if (point == ScreenPoint.topMiddle)
                return new Vector2(SystemCore.GraphicsDevice.Viewport.Width / 2, 0);
            if (point == ScreenPoint.middleLeft)
                return new Vector2(0, SystemCore.GraphicsDevice.Viewport.Height / 2);
            return Vector2.Zero;
        }

        public static int ScreenRatioX(float x)
        {
            float width = SystemCore.GraphicsDevice.Viewport.Width;
            return (int) (width*x);
        }

        public static int ScreenRatioY(float y)
        {
            float width = SystemCore.GraphicsDevice.Viewport.Height;
            return (int)(width * y);
        }

        public void RemoveControl(BaseControl control)
        {
            control.RemoveEvents();
            controlsToRemove.Add(control);

        }

        public void ClearAllControls()
        {
            foreach (BaseControl b in controls)
            {
                b.RemoveEvents();
                controlsToRemove.Add(b);
            }



        }

        public Panel AddBackground(string presetTexture, Color color)
        {
            Panel p = new Panel(new Rectangle(0, 0, SystemCore.GraphicsDevice.Viewport.Width, SystemCore.GraphicsDevice.Viewport.Height), GUITexture.Textures[presetTexture]);
            p.MainColor = color;
            AddControl(p);
            return p;


        }

        public Button AddDefaultLabelledButton(Vector2 midPoint, string labelText, int width, int height, Color mainButtonColor, Color buttonHighlight, Color labelTextColor)
        {


           

            var paintButton = new Button(new Rectangle((int)midPoint.X - width / 2, (int)midPoint.Y - height / 2, width,height),
               GUITexture.Textures["blank"]);
            paintButton.HighlightColor = buttonHighlight;
            paintButton.MainColor = mainButtonColor;

            var label = new Label(GUIFonts.Fonts["neuropolitical"], labelText);
            label.Position = new Vector2(100, 100);
            label.TextColor = labelTextColor;
            label.TextOutline = false;
            label.OutlineColor = labelTextColor;
            paintButton.AttachLabel(label);
            AddControl(paintButton);
            return paintButton;
        }

        public void CreateDefaultMenuScreen(string mainmenuName, ColorScheme activeColorScheme, List<string> labels )
        {

            ColorScheme scheme = activeColorScheme;
            Color background = scheme.Color1;
            Color button1Col = scheme.Color3;
            Color button2Col = scheme.Color5;

            AddBackground("blank", background);

            Panel leftPanel =
                new Panel(
                    new Rectangle(GUIManager.GetFractionOfWidth(0.1f), 0, GUIManager.GetFractionOfWidth(0.3f),
                        SystemCore.GraphicsDevice.Viewport.Height), GUITexture.Textures["blank"]);

            leftPanel.MainColor = scheme.Color3;
            leftPanel.MainAlpha = 0.1f;
            AddControl(leftPanel);


            Vector2 buttonSpace = new Vector2(0, GUIManager.GetFractionOfHeight(0.02f));
            float spacing = 50;


            int maxWidth = 0;
            int maxHeight = 0;

            foreach (string l in labels)
            {
                Vector2 labelSize = GUIFonts.Fonts["neuropolitical"].MeasureString(l);
                if (labelSize.X > maxWidth)
                    maxWidth = (int)labelSize.X;
                if (labelSize.Y > maxHeight)
                    maxHeight = (int)labelSize.Y;

            }


            foreach (string label in labels)
            {

                var button1 = AddDefaultLabelledButton(new Vector2(GUIManager.GetFractionOfWidth(0.25f), screenMidPoint.Y) + buttonSpace, label,maxWidth,maxHeight, button1Col, button2Col, Color.Black);            
                button1.Name = label;
                button1.MainAlpha = 0f;
                button1.HighlightAlpha = 0.1f;
                buttonSpace.Y += spacing;
            }



            var lab = new Label(GUIFonts.Fonts["neuropolitical"], mainmenuName);
            lab.Position = new Vector2(GUIManager.GetFractionOfWidth(0.25f), GUIManager.GetFractionOfHeight(0.25f));
            lab.TextColor = Color.Black;
            AddControl(lab);
            lab.Name = "mainMenuLabel";

        }

        public void CreateDefaultMenuScreen(string mainmenuName, ColorScheme activeColorScheme, params string[] labels)
        {

            CreateDefaultMenuScreen(mainmenuName, activeColorScheme, labels.ToList());

        }

        public Panel AddPanel(int x, int y, int width, int height, string availableTextures, Color color)
        {
            Panel p = new Panel(new Rectangle(x, y, width, height), GUITexture.Textures[availableTextures]);
            p.MainColor = color;
            AddControl(p);
            return p;
        }

     
        public BaseControl GetControl(string name)
        {
            foreach (BaseControl control in controls)
                if (control.Name == name)
                    return control;

            foreach (BaseControl control in controlsToAdd)
                if (control.Name == name)
                    return control;

            return null;
        }

        public TextBox AddDefaultTextbox(Rectangle rectangle)
        {
            TextBox t = new TextBox(new Rectangle(100, 100, 100, 50), GUITexture.Textures["blank"], GUIFonts.Fonts["test"]);
            controls.Add(t);
            return t;
        }

        public void ApplyPalette(Palette palette)
        {
            foreach (BaseControl control in controls)
            {
                control.SetPalette(palette);
            }
            foreach (BaseControl control in controlsToAdd)
            {
                control.SetPalette(palette);
            }
        }

        public void ApplyPalette(string name)
        {
            if (Palettes.ContainsKey(name))
                ApplyPalette(Palettes[name]);
        }

        public static int TopMidLeft()
        {
            return SystemCore.GraphicsDevice.Viewport.Width / 4;
        }

        public static Vector2 ScreenMiddle()
        {
            return new Vector2(SystemCore.GraphicsDevice.Viewport.Width / 2, SystemCore.GraphicsDevice.Viewport.Height / 2);
        }

        public static int TopMidRight()
        {
            return SystemCore.GraphicsDevice.Viewport.Width - (SystemCore.GraphicsDevice.Viewport.Width / 4);
        }

        public static int GetFractionOfWidth(float fraction)
        {
            return (int) ((float) SystemCore.GraphicsDevice.Viewport.Width*fraction);
        }

        public static int GetFractionOfHeight(float fraction)
        {
            return (int)((float)SystemCore.GraphicsDevice.Viewport.Height * fraction);
        }
    }
}
