using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;

namespace MonoGameEngineCore.GUI.Controls
{
    public enum LabelAlignment
    {
        left,
        middle,
        right
    }

    public class Label : BaseControl
    {
        public Vector2 Position { get; set; }
        public SpriteFont Font { get; set; }
        public string Text { get; set; }
        public Color TextColor { get; set; }
        public bool TextOutline { get; set; }
        public Color OutlineColor { get; set; }
        public LabelAlignment Alignment { get; set; }
        public int OutlineSize { get; set; }

        public Label(SpriteFont font, string text)
        {
            Font = font;
            Text = text;
            TextColor = Color.White;
            OutlineColor = Color.White;
            Alignment = LabelAlignment.middle;
            OutlineSize = 1;
            Alpha = 1;
        }

        public override void Update(GameTime gameTime, InputManager input)
        {

            base.Update(gameTime, input);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice device)
        {
            if (TextOutline)
            {
                spriteBatch.DrawString(Font, Text, CalculateStringPosition(Alignment) - new Vector2(OutlineSize, 0), OutlineColor * Alpha);
                spriteBatch.DrawString(Font, Text, CalculateStringPosition(Alignment) + new Vector2(OutlineSize, 0), OutlineColor * Alpha);
                spriteBatch.DrawString(Font, Text, CalculateStringPosition(Alignment) - new Vector2(0, OutlineSize), OutlineColor * Alpha);
                spriteBatch.DrawString(Font, Text, CalculateStringPosition(Alignment) + new Vector2(0, OutlineSize), OutlineColor * Alpha);
            }
            Color finalTextColor;
            if (Parent != null && Parent.MouseOver)
            {
                if (TextOutline)
                    finalTextColor = OutlineColor * Alpha;
                else
                    finalTextColor = TextColor * Alpha;

            }
            else
                finalTextColor = TextColor * Alpha;

            spriteBatch.DrawString(Font, Text, CalculateStringPosition(Alignment), finalTextColor);

        }

        private Vector2 CalculateStringPosition(LabelAlignment alignment)
        {
            Vector2 pos = Vector2.Zero;
            if (alignment == LabelAlignment.middle)
            {
                pos = Position;
                pos -= Font.MeasureString(Text) / 2;

            }
            if (alignment == LabelAlignment.left)
            {
                pos = Position;

            }
            if (alignment == LabelAlignment.right)
            {
                pos = Position;
                pos -= Font.MeasureString(Text);

            }
            return pos;
        }

        public override void SetPosition(Vector2 pos)
        {
            Position = pos;
        }

        public override void Translate(Vector2 offset)
        {
            Position += offset;
        }

        public override void SetPalette(Palette palette)
        {
            OutlineColor = palette.TextOutlineColor;
            TextColor = palette.TextColor;
            TextOutline = palette.TextOutline;
            base.SetPalette(palette);
        }



    }
}
