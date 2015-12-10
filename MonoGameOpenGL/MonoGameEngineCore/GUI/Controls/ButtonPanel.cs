using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.Helper;


namespace MonoGameEngineCore.GUI.Controls
{
    //panel containing lots of buttons / images in a grid
    public class ButtonGridPanel : Panel
    {
        public Dictionary<string, Button> Buttons { get; set; }
        public bool BackgroundPanelVisible { get; set; }
        public bool Draggable { get; set; }
        public bool SelectableItems { get; set; }
        public Button SelectedItem { get; set; }
        private bool dragMode = false;
        public Color SelectionBorder { get; set; }
        public Button dragButton;
        Rectangle dragRectangle;
        public EventHandler OnReleaseDrag;
        public EventHandler OnSelectionChanged;
        public void SetButtonBorder(int thickness)
        {
            foreach (Button b in Buttons.Values)
            {
                b.Border = true;
                b.BorderThickness = thickness;
            }
        }


        public Color ButtonBorderColor { get; set; }
        public int ButtonBorderThickness { get; set; }

        public ButtonGridPanel(Rectangle rec, Texture2D panelTexture)
            : base(rec, panelTexture)
        {
            Buttons = new Dictionary<string, Button>();
            BackgroundPanelVisible = true;
            SelectionBorder = Color.White;
        }

        public void AddButtons(Dictionary<string, Texture2D> buttons, int buttonWidth, int buttonHeight, int verticalSpacing, int horizontalSpacing, int buttonsInRow, int padding)
        {
            //top left
            int x = Rect.X;
            int y = Rect.Y;

            int buttonCount = buttons.Count;

            float rows = (float)buttonCount / (float)buttonsInRow;
            var rowsRequired = Math.Ceiling((double)rows);

            Rect = new Rectangle(Rect.X, Rect.Y, ((buttonWidth + horizontalSpacing) * buttonsInRow + (padding)),
                (buttonHeight + verticalSpacing) * (int)rowsRequired + (padding));



            int rowIndex = 0;
            int columnIndex = 0;
            int currentX = Rect.X + padding;
            int currentY = Rect.Y + padding;

            Rectangle buttonRect = new Rectangle(currentX, currentY, buttonWidth, buttonHeight);

            foreach (KeyValuePair<string, Texture2D> kvp in buttons)
            {
                buttonRect = new Rectangle(currentX, currentY, buttonWidth, buttonHeight);

                AddButtonToGrid(buttonRect, kvp.Key, kvp.Value);

                currentX += buttonWidth + horizontalSpacing;

                rowIndex++;
                if (rowIndex > (buttonsInRow - 1))
                {
                    rowIndex = 0;
                    columnIndex++;
                    currentY += buttonHeight + verticalSpacing;
                    currentX = Rect.X + padding;
                }

            }
        }

        public void AddColorPanel(List<Color> colors, int buttonWidth, int buttonHeight, int verticalSpacing, int horizontalSpacing, int buttonsInRow, int padding)
        {
            //top left
            int x = Rect.X;
            int y = Rect.Y;

            int buttonCount = colors.Count;

            float rows = (float)buttonCount / (float)buttonsInRow;
            var rowsRequired = Math.Ceiling((double)rows);

            Rect = new Rectangle(Rect.X, Rect.Y, ((buttonWidth + horizontalSpacing) * buttonsInRow + (padding)),
                (buttonHeight + verticalSpacing) * (int)rowsRequired + (padding));



            int rowIndex = 0;
            int columnIndex = 0;
            int currentX = Rect.X + padding;
            int currentY = Rect.Y + padding;

            Rectangle buttonRect = new Rectangle(currentX, currentY, buttonWidth, buttonHeight);

            int i = 0;
            foreach (Color col in colors)
            {
                buttonRect = new Rectangle(currentX, currentY, buttonWidth, buttonHeight);

                AddColorButton(buttonRect, i.ToString(), col);

                currentX += buttonWidth + horizontalSpacing;

                rowIndex++;
                if (rowIndex > (buttonsInRow - 1))
                {
                    rowIndex = 0;
                    columnIndex++;
                    currentY += buttonHeight + verticalSpacing;
                    currentX = Rect.X + padding;
                }
                i++;
            }
        }

        private void AddButtonToGrid(Rectangle buttonRect, string p, Texture2D texture2D)
        {
            Button b = new Button(buttonRect, texture2D);
            Buttons.Add(p, b);
        }

        private void AddColorButton(Rectangle buttonRect, string key, Color col)
        {
            Button b = new Button(buttonRect, GUITexture.Textures["blank"]);
            b.MainColor = col;
            b.HighlightColor = col.ChangeTone(30);
            b.Border = true;
            b.BorderColor = Color.Black;
            b.BorderThickness = 1;
            Buttons.Add(key, b);
        }

        public void ReplaceButton(Button buttonToReplaceWith)
        {


            Button buttonToReplace = null;
            foreach (Button b in Buttons.Values)
                if (b.MouseOver)
                    buttonToReplace = b;

            if (buttonToReplace == null)
                return;

            buttonToReplace.Texture = buttonToReplaceWith.Texture;
            buttonToReplace.MainColor = buttonToReplaceWith.MainColor;
            buttonToReplace.HighlightColor = buttonToReplaceWith.HighlightColor;
        }

        public override void RemoveEvents()
        {
            foreach (Button b in Buttons.Values)
            {
                b.RemoveEvents();
            }
            base.RemoveEvents();
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            foreach (Button b in Buttons.Values)
            {
                b.Update(gameTime, input);
                b.MainAlpha = MainAlpha;

                foreach (BaseControl child in b.Children)
                    child.MainAlpha = MainAlpha;
            }

            if (SelectableItems && MouseOver)
            {
                if ( input.MouseLeftPress())
                {
                    foreach (Button b in Buttons.Values)
                        if (b.MouseOver)
                        {
                            SelectedItem = b;
                            if (OnSelectionChanged != null)
                                OnSelectionChanged(this, null);
                        }

                }
            }

            if (Draggable)
            {
                    if (input.MouseLeftPress())
                    {
                        //find the button we're trying to drag
                        dragMode = true;
                        foreach (Button b in Buttons.Values)
                            if (b.MouseOver)
                                dragButton = b;
                    }

                if (dragMode)
                {
                    if (input.MouseLeftUp())
                    {
                        dragMode = false;

                        if (OnReleaseDrag != null)
                        {
                            if (dragButton != null)
                                OnReleaseDrag(this, null);
                        }

                        dragButton = null;
                    }
                    else
                    {
                        if (dragButton != null)
                        {
                            dragRectangle = new Rectangle(input.MousePosition.X - dragButton.Rect.Width / 2,
                                input.MousePosition.Y - dragButton.Rect.Height / 2,
                                dragButton.Rect.Width,
                                dragButton.Rect.Height);
                        }

                    }
                }
            }
            

            base.Update(gameTime, input);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice device)
        {

            if (BackgroundPanelVisible)
                base.Draw(gameTime, spriteBatch, device);

            foreach (Button b in Buttons.Values)
            {
                if (b == SelectedItem)
                    b.BorderColor = SelectionBorder;
                else
                    b.BorderColor = BorderColor;

                b.Draw(gameTime, spriteBatch, device);
            }

            if (Draggable && dragMode)
            {
                if (dragButton != null)
                {
                    spriteBatch.Draw(dragButton.Texture, dragRectangle, dragButton.MainColor * 0.8f);
                }
            }
        }

        public override void SetPalette(Palette palette)
        {
            base.SetPalette(palette);

            foreach (Button b in Buttons.Values)
                b.SetPalette(palette);
        }

        public static Dictionary<string, Texture2D> GenerateButtonTextures(int count)
        {
            Dictionary<string, Texture2D> buttonTex = new Dictionary<string, Texture2D>();

            for (int i = 0; i < count; i++)
            {
                buttonTex.Add(i.ToString(), GUITexture.Textures["blank"]);
            }
            return buttonTex;
        }

    }
}
