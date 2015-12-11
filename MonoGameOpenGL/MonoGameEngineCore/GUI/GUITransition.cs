using Microsoft.Xna.Framework;
using MonoGameEngineCore.GUI.Controls;
using System;

namespace MonoGameEngineCore.GUI
{
    public abstract class GUITransition
    {
        protected BaseControl control;
        public bool IsComplete { get; protected set; }

        public GUITransition(BaseControl control)
        {
            this.control = control;
            IsComplete = false;
        }

        public abstract void Update(Microsoft.Xna.Framework.GameTime gameTime);

    }

    public class FadeOutTransition : GUITransition
    {
        private float duration;
        private DateTime startTime;
        private float startMainAlpha;
        private float startHighlightAlpha;
        private float mainAlphaChangePerLoop;
        private float highlightAlphaChangePerLoop;

        public FadeOutTransition(BaseControl control, float durationInMilliseconds)
            : base(control)
        {
            this.duration = durationInMilliseconds;
            startTime = DateTime.Now;

            startMainAlpha = control.MainAlpha;
            startHighlightAlpha = control.HighlightAlpha;

        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if(IsComplete)
                return;


            float timeSinceStart =(float)(DateTime.Now - startTime).TotalMilliseconds;

          

            float fractionOfDuration = timeSinceStart/duration;
            control.MainAlpha = startMainAlpha * (1 - fractionOfDuration);
            control.HighlightAlpha = startHighlightAlpha * (1 - fractionOfDuration);


            if (timeSinceStart > duration)
                IsComplete = true;
        }


    }

    public class FadeInTransition : GUITransition
    {
        private float duration;
        private float highlightAlpha;
        private float mainAlpha;
        private DateTime startTime;

        public FadeInTransition(BaseControl control, float durationInMilliseconds)
            : base(control)
        {
            startTime = DateTime.Now;
            this.duration = durationInMilliseconds;
            this.mainAlpha = control.MainAlpha;
            this.highlightAlpha = control.HighlightAlpha;

            //now set to zero so we can fade in.
            control.MainAlpha = 0;
            control.HighlightAlpha = 0;

        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (IsComplete)
                return;

            float timeSinceStart = (float)(DateTime.Now - startTime).TotalMilliseconds;



            float fractionOfDuration = timeSinceStart / duration;
            control.MainAlpha = mainAlpha *fractionOfDuration;
            control.HighlightAlpha = highlightAlpha*fractionOfDuration;


            if (timeSinceStart > duration)
                IsComplete = true;

        }
    }

    public class MoveTransition : GUITransition
    {
        private DateTime startTime;
        private Vector2 movementPerMillisecond;
        private float duration;

        public MoveTransition(BaseControl control, Vector2 movement, float durationInMilliseconds)
            : base(control)
        {

            this.duration = durationInMilliseconds;
            movementPerMillisecond = movement/durationInMilliseconds;
            startTime = DateTime.Now;

        }

        public override void Update(GameTime gameTime)
        {
            if (IsComplete)
                return;

            float timeSinceStart = (float)(DateTime.Now - startTime).TotalMilliseconds;

            double millisecondsSinceLastUpdate = gameTime.ElapsedGameTime.TotalMilliseconds;
            control.Translate(movementPerMillisecond*(float) millisecondsSinceLastUpdate);


            if (timeSinceStart > duration)
                IsComplete = true;

        }
    }
}
