using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using NicksLib.Rendering;

namespace MonoGameEngineCore.GUI.Controls
{
    public abstract class BaseControl
    {
        public BaseControl Parent { get; set; }
        public List<BaseControl> Children;
        public string Name { get; set; }
        public bool Visible { get; set; }
        public float Alpha { get; set; }
        public EventHandler OnMouseEnterEvent;
        public EventHandler OnMouseLeaveEvent;
        public EventHandler OnFocusEvent;
        public EventHandler OnFocusLostEvent;
        public bool MouseOver { get; set; }
        public bool HasFocus { get; set; }
        public bool Focusable { get; set; }
        private List<UITransition> activeTransitions;
       
        public BaseControl()
        {
            activeTransitions = new List<UITransition>();
            Children = new List<BaseControl>();
            Visible = true;
        }

        public virtual void Update(GameTime gameTime, InputManager input)
        {
            foreach (BaseControl child in Children)
                child.Update(gameTime, input);

            if (activeTransitions.Count > 0)
            {
                foreach (UITransition transition in activeTransitions)
                {
                    transition.Update(gameTime);
                }

                activeTransitions.RemoveAll(x => x.Done == true);
            }

        }
        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch, GraphicsDevice device)
        {
            foreach (BaseControl child in Children)
                child.Draw(gameTime, spriteBatch, device);
        }
        public virtual void RemoveEvents()
        {

            if (OnFocusEvent != null)
            {
                var invocList = OnFocusEvent.GetInvocationList();
                foreach (Delegate d in invocList)
                {
                    OnFocusEvent -= (EventHandler)d;
                }
            }

            if (OnFocusLostEvent != null)
            {
                var invocList = OnFocusLostEvent.GetInvocationList();
                foreach (Delegate d in invocList)
                {
                    OnFocusLostEvent -= (EventHandler)d;
                }
            }

            if (OnMouseEnterEvent != null)
            {
                var invocList = OnMouseEnterEvent.GetInvocationList();
                foreach (Delegate d in invocList)
                {
                    OnMouseEnterEvent -= (EventHandler)d;
                }
            }

            if (OnMouseLeaveEvent != null)
            {
                var invocList = OnMouseLeaveEvent.GetInvocationList();
                foreach (Delegate d in invocList)
                {
                    OnMouseLeaveEvent -= (EventHandler)d;
                }
            }


        }
        public virtual void Translate(Vector2 offset)
        {
            foreach (BaseControl child in Children)
                child.Translate(offset);
        }
        public virtual void SetPosition(Vector2 pos) { }
        public virtual void SetPalette(Palette palette)
        {
           
        }
        public virtual void SetPaletteSecondary(Palette palette)
        {

        }
        public virtual void ApplyTransition(UITransition transition)
        {
            activeTransitions.Add(transition);
            transition.control = this;
            transition.SetStartState();

            foreach (BaseControl baseControl in Children)
            {
                baseControl.ApplyTransition(transition.Clone());
            }
        }

        public virtual void Anchor(AnchorPoint anchorPoint, GUIManager.ScreenPoint screenPoint, Vector2 offset)
        {
           

        }

        public virtual void Anchor(AnchorPoint anchorPoint, GUIManager.ScreenPoint screenPoint, float xScreenRatioOffset, float yScreenRatioOffset)
        {


        }

        public virtual Vector2 GetAnchorOffset(AnchorPoint anchorPoint)
        {
            return Vector2.Zero;

        }
       

    }
}