using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;

namespace MonoGameEngineCore.GUI.Controls
{
    public class TextBox : Panel
    {
        public bool ReadOnly { get; set; }
        public SpriteFont Font { get; set; }
        public string Text { get; set; }
        public Color TextColour { get; set; }
        public EventHandler OnReturnEvent;
        bool atMaxLength;

        public TextBox(Rectangle rec, Texture2D texture, SpriteFont font)
            : base(rec, texture)
        {
            TextColour = Color.Black;
            Font = font;
            Text = "";
            Focusable = true;

        }


        public override void Update(GameTime gameTime, InputManager input)
        {
            if (HasFocus)
            {
                DetectKeyboardInput(input);

            }

           

            base.Update(gameTime, input);
        }

        private void DetectKeyboardInput(InputManager input)
        {
            if (ReadOnly) return;

            if (input.KeyPress(Keys.Delete) || input.KeyPress(Keys.Back))
            {
                if (!String.IsNullOrEmpty(Text))
                    Text = Text.Remove(Text.Length - 1);
            }

            if (input.KeyPress(Keys.Enter))
            {
                if (HasFocus)
                {
                    if (OnReturnEvent != null)
                        OnReturnEvent(this, null);
                }
            }


            int textLength = (int)Font.MeasureString(Text).X;
            if (textLength > (Rect.X - (Rect.X / 5)))
                atMaxLength = true;
            else
                atMaxLength = false;

            if (atMaxLength)
                return;

            for (Keys k = Keys.A; k <= Keys.Z; k++)
            {
                if (input.KeyPress(k))
                {
                    if (input.IsKeyDown(Keys.LeftShift) || input.IsKeyDown(Keys.RightShift)) //if shift is held down
                        Text += k.ToString().ToUpper(); //convert the Key enum member to uppercase string
                    else
                        Text += k.ToString().ToLower(); //convert the Key enum member to lowercase string
                }
            }
            if (input.KeyPress(Keys.OemMinus))
                Text += "_";


        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice device)
        {
            //draw the panel
            base.Draw(gameTime, spriteBatch, device);

            string finalString = Text;
            if (HasFocus && !ReadOnly)
            {
                finalString += "<";
            }

            spriteBatch.DrawString(Font, finalString, CalculateStringPosition(), TextColour);
        }

        private Vector2 CalculateStringPosition()
        {
            //string should begin just to the right of the panel
            Vector2 pos = Vector2.Zero;
            pos.X = Rect.Left + 2;
            pos.Y = Rect.Center.Y - Font.MeasureString("Blach").Y / 2;
            return pos;

        }

        public override void RemoveEvents()
        {
            if (OnReturnEvent != null)
            {
                var invocList = OnReturnEvent.GetInvocationList();
                foreach (Delegate d in invocList)
                {
                    OnReturnEvent -= (EventHandler)d;
                }
            }

            base.RemoveEvents();
        }

        public override void Translate(Vector2 offset)
        {
            base.Translate(offset);
        }

    }
}
