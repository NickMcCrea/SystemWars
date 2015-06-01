using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;

namespace MonoGameEngineCore.GUI.Controls
{
    public class Button : Panel
    {

        public EventHandler OnClick;
        
        public Button(Rectangle rec, Texture2D tex)
            : base(rec, tex)
        {
            HighlightOnMouseOver = true;
            HighlightColor = Color.LightBlue;
        }

      

        public override void Update(GameTime gameTime, InputManager input)
        {
            base.Update(gameTime, input);
            if (MouseOver && input.MouseLeftPress())
            {
                if (OnClick != null)
                    OnClick(this, null);
            }
            
        }

        public override void RemoveEvents()
        {
            if (OnClick != null)
            {
                var invocList = OnClick.GetInvocationList();
                foreach (Delegate d in invocList)
                {
                    OnClick -= (EventHandler)d;
                }
            }
            base.RemoveEvents();
        }

        public void AttachLabel(Label label)
        {
            label.Position = GetMidPoint();
            Children.Add(label);
            label.Parent = this;
        }

        private Vector2 GetMidPoint()
        {
            return new Vector2(Rect.Center.X, Rect.Center.Y);
        }

        public override void SetPosition(Vector2 pos)
        {
           
            base.SetPosition(pos);

            foreach (BaseControl b in Children)
                b.SetPosition(GetMidPoint());
        }

        public void SetLabelText(string p)
        {
            ((Label)Children[0]).Text = p;
        }
    }
}
