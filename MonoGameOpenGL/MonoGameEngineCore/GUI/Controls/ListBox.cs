using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;

namespace MonoGameEngineCore.GUI.Controls
{
    public class ListBox : BaseControl
    {

        public TextBox CurrentSelectedItem { get; private set; }
        public EventHandler OnItemSelectedEvent;
        public Panel MainPanel { get; set; }
        private ScrollBar scrollBar;
        public Color TextBoxColor { get; set; }
        public Color TextBoxHighlightColor { get; set; }
        public bool TextBoxBorder { get; set; }
        public int TextBoxBorderThickness { get; set; }
        public Color TextboxBorderColor { get; set; }
        private List<TextBox> textBoxes;

        public ListBox(Rectangle rec, Texture2D tex)
            : base()
        {


            MainPanel = new Panel(rec, tex);
            textBoxes = new List<TextBox>();

            scrollBar = new ScrollBar(ScrollBar.ScrollBarType.vertical, new Rectangle(MainPanel.Rect.Right - 5, MainPanel.Rect.Top, 5, MainPanel.Rect.Height),
                new Rectangle(MainPanel.Rect.Right - 5, MainPanel.Rect.Top, 5, 10),
                GUITexture.Textures["blank"], GUITexture.Textures["blank"]);
        }

        public void AddItem(string item)
        {
            var tBox = new TextBox(CalculateTextBoxRec(textBoxes.Count), GUITexture.Textures["blank"], GUIFonts.Fonts["test"]);
            tBox.MainColor = TextBoxColor;
            tBox.HighlightColor = TextBoxHighlightColor;
            textBoxes.Add(tBox);
            tBox.Text = item;
            tBox.ReadOnly = true;
            tBox.Border = TextBoxBorder;
            tBox.BorderThickness = TextBoxBorderThickness;
            tBox.BorderColor = TextboxBorderColor;
        }

        private Rectangle CalculateTextBoxRec(int pos)
        {
            int x = MainPanel.Rect.X + 2;
            int y = MainPanel.Rect.Y + 2 + (12 * pos);
            return new Rectangle(x, y, MainPanel.Rect.Width - 20, 10);
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            MainPanel.Update(gameTime, input);
            scrollBar.Update(gameTime, input);
            MouseOver = MainPanel.MouseOver;

            foreach (TextBox t in textBoxes)
            {
                t.Update(gameTime, input);
                if (t.HasFocus)
                    if (OnItemSelectedEvent != null)
                    {
                        CurrentSelectedItem = t;
                        OnItemSelectedEvent(this, null);
                    }

                if (t.MouseOver)
                    t.MainColor = Color.DarkGray;
                else
                    t.MainColor = Color.LightGray;
            }


            int top = textBoxes[0].Rect.Top;
            int bottom = textBoxes[textBoxes.Count - 1].Rect.Bottom;

            int movementRange = bottom - top - MainPanel.Rect.Height;
            if (movementRange > 0)
            {
                scrollBar.Visible = true;
                for (int i = 0; i < textBoxes.Count; i++)
                {
                    Rectangle r = CalculateTextBoxRec(i);
                    var pos = new Vector2(r.X, r.Y);
                    var offset = new Vector2(0, -movementRange * scrollBar.ScrollValue);
                    textBoxes[i].SetPosition(pos + offset);
                }
            }
            else
                scrollBar.Visible = false;


        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice device)
        {
            //end the old spritebatch
            spriteBatch.End();

            //new one has ScissorTestEnable set to true, and has the ScissorRectangle set
            //to the main panel
            var rastState = new RasterizerState() { ScissorTestEnable = true };
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend,
                  null, null, rastState);


            spriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(MainPanel.Rect.X - MainPanel.BorderThickness,
                MainPanel.Rect.Y - MainPanel.BorderThickness,
                MainPanel.Rect.Width + MainPanel.BorderThickness * 2,
                 MainPanel.Rect.Height + MainPanel.BorderThickness * 2);


            MainPanel.Draw(gameTime, spriteBatch, device);
            if (scrollBar.Visible)
                scrollBar.Draw(gameTime, spriteBatch, device);
            foreach (TextBox t in textBoxes)
            {
                t.Draw(gameTime, spriteBatch, device);
            }

            //draw the mainpanel border last, so it renders over the items in the box.
            if (MainPanel.Border)
                MainPanel.DrawBorder(spriteBatch);

            //end the clipped batch, restart the old
            spriteBatch.End();
            spriteBatch.Begin();
        }

        public override void RemoveEvents()
        {
            if (OnItemSelectedEvent != null)
            {
                var invocList = OnItemSelectedEvent.GetInvocationList();
                foreach (Delegate d in invocList)
                {
                    OnItemSelectedEvent -= (EventHandler)d;
                }
            }
            base.RemoveEvents();
        }

        public override void SetPalette(Palette palette)
        {
            MainPanel.SetPalette(palette);
            scrollBar.SetPalette(palette);
            foreach (TextBox t in textBoxes)
                t.SetPalette(palette);
        }
    }
}
