using MonoGameEngineCore.GUI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.GUI
{
    public class GUITransition
    {
        protected BaseControl control;

        public GUITransition(BaseControl control)
        {
            this.control = control;
        }

        public virtual void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {

        }
    }

    class FadeOutTransition : GUITransition
    {
        private float duration;

        public FadeOutTransition(BaseControl control, float durationInMilliseconds)
            : base(control)
        {
            this.duration = durationInMilliseconds;

        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {


            base.Update(gameTime);
        }


    }
}
