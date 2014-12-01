using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;

namespace MonoGameEngineCore.GUI.Controls
{
    public class ScrollBar : BaseControl
    {
        public Panel Background { get; set; }
        public Panel Bar { get; set; }
        bool clamped;
        public enum ScrollBarType
        {
            horizontal,
            vertical
        }
        public ScrollBarType Type { get; set; }
        public float ScrollValue { get; private set; }

        public ScrollBar(ScrollBarType type, Rectangle backgroundRec, Rectangle foregroundRec, Texture2D backgroundTex, Texture2D foregroundTex)
            : base()
        {
            Type = type;
            Background = new Panel(backgroundRec, backgroundTex);
            Bar = new Panel(foregroundRec, foregroundTex);
            Background.MainColor = Color.LightGray;
            Bar.MainColor = Color.DarkGray;
        }


        public override void Update(GameTime gameTime, InputManager input)
        {
            Background.Update(gameTime, input);
            Bar.Update(gameTime, input);

            MouseOver = Background.MouseOver;

            if (Bar.MouseOver && input.MouseLeftDown())
            {
                if (!clamped)
                    clamped = true;
            }

            if (Type == ScrollBarType.vertical)
            {
                if (clamped && input.MouseLeftDown())
                {
                    Bar.Rect = new Rectangle(Bar.Rect.X, Bar.Rect.Y + input.MouseDelta.Y, Bar.Rect.Width, Bar.Rect.Height);

                    if (Bar.Rect.Top < Background.Rect.Top)
                        Bar.Rect = new Rectangle(Bar.Rect.X, Background.Rect.Top, Bar.Rect.Width, Bar.Rect.Height);

                    if (Bar.Rect.Bottom > Background.Rect.Bottom)
                        Bar.Rect = new Rectangle(Bar.Rect.X, Background.Rect.Bottom - Bar.Rect.Height, Bar.Rect.Width, Bar.Rect.Height);

                    ScrollValue = (float)(-Background.Rect.Top + Bar.Rect.Top) / (float)(Background.Rect.Height - Bar.Rect.Height);

                }
            }

            if (Type == ScrollBarType.horizontal)
            {
                if (clamped && input.MouseLeftDown())
                {
                    Bar.Rect = new Rectangle(Bar.Rect.X+input.MouseDelta.X, Bar.Rect.Y, Bar.Rect.Width, Bar.Rect.Height);

                    if (Bar.Rect.Left < Background.Rect.Left)
                        Bar.Rect = new Rectangle(Background.Rect.X, Bar.Rect.Top, Bar.Rect.Width, Bar.Rect.Height);

                    if (Bar.Rect.Right > Background.Rect.Bottom)
                        Bar.Rect = new Rectangle(Background.Rect.Right - Bar.Rect.Width, Bar.Rect.Y, Bar.Rect.Width, Bar.Rect.Height);

                    ScrollValue = (float)(-Background.Rect.Left + Bar.Rect.Left) / (float)(Background.Rect.Width - Bar.Rect.Width);

                }

            }

            if (clamped && input.MouseLeftDown())
            {
                clamped = false;
            }


            base.Update(gameTime, input);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice device)
        {
            Background.Draw(gameTime, spriteBatch, device);
            Bar.Draw(gameTime, spriteBatch, device);
            base.Draw(gameTime, spriteBatch, device);
        }


        public override void SetPalette(Palette palette)
        {
            Background.SetPalette(palette);
            Bar.SetPaletteSecondary(palette);
        }

    }
}
