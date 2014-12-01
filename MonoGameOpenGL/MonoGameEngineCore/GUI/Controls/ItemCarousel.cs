using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;

namespace MonoGameEngineCore.GUI.Controls
{
    public class ItemCarousel : BaseControl
    {
        private List<Texture2D> items;
        private bool isHorizontal;
        private Vector2 anchorPoint;
        private int currentIndex;
        private int itemWidth;
        private int itemHeight;
        public int ItemSpacing { get; set; }
        private Rectangle drawRectangle;

        public ItemCarousel(Vector2 screenAnchor, bool horizontal, int itemWidth, int itemHeight)
            : base()
        {
            this.itemWidth = itemWidth;
            this.itemHeight = itemHeight;
            this.anchorPoint = screenAnchor;
            this.isHorizontal = horizontal;
            items = new List<Texture2D>();
            ItemSpacing = 20;
        }

        

        public override void RemoveEvents()
        {
            base.RemoveEvents();
        }

        public void Add(Texture2D texture, string name)
        {
            items.Add(texture);
        }

        public void MoveForward()
        {
            currentIndex++;
            if (currentIndex > items.Count - 1)
                currentIndex = 0;
        }

        public void MoveBackward()
        {
            currentIndex--;
            if (currentIndex < 0)
                currentIndex = items.Count - 1;
        }

        public override void Update(GameTime gameTime, InputManager input)
        {
            base.Update(gameTime, input);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice device)
        {

            drawRectangle = new Rectangle((int)anchorPoint.X - itemWidth / 2, (int)anchorPoint.Y - itemHeight / 2, itemHeight, itemHeight);
     
            spriteBatch.Draw(items[currentIndex], drawRectangle, Color.White);

            for (int i = 0; i < items.Count; i++)
            {
                if (i == currentIndex)
                    continue;

                int diffVersusCurrentIndex = currentIndex - i;
                Vector2 offset = Vector2.Zero;
                if (isHorizontal)
                    offset = new Vector2(i * diffVersusCurrentIndex * ItemSpacing, 0);
                else
                    offset = new Vector2(0, i * diffVersusCurrentIndex * ItemSpacing);
                Vector2 offsetPoint = anchorPoint + offset;
                drawRectangle = new Rectangle((int)offsetPoint.X - itemWidth / 2, (int)offsetPoint.Y - itemHeight / 2, itemHeight, itemHeight);
     
                spriteBatch.Draw(items[i], drawRectangle, Color.White);


            }
            base.Draw(gameTime, spriteBatch, device);
        }
    }
}
