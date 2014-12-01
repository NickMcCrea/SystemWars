using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.GUI.Controls
{
    public class UITransition
    {
        public int StartDelay { get; set; }
        public int TransitionDuration { get; set; }
        public BaseControl control;
        protected DateTime initialiseTime;
        protected double timeSinceInitialisation;

        public bool Done
        {
            get;
            protected set;

        }

        public UITransition(int startDelay, int transitionDuration)
        {
            StartDelay = startDelay;
            TransitionDuration = transitionDuration;
        }

        public virtual void SetStartState()
        {
            initialiseTime = DateTime.Now;
        }

        public void Update(GameTime gameTime)
        {
            if (!Done)
            {
                timeSinceInitialisation = (DateTime.Now - initialiseTime).TotalMilliseconds;

                if (timeSinceInitialisation > StartDelay)
                    ApplyTransition(gameTime);

            }
        }

        public virtual void ApplyTransition(GameTime gameTime)
        {

        }

        public virtual UITransition Clone()
        {
            return new UITransition(this.StartDelay, this.TransitionDuration);
        }

    }

    public class FadeTransition : UITransition
    {
        public float StartValue { get; set; }
        public float EndValue { get; set; }


        public FadeTransition(float startValue, float endValue, int startDelay, int transitionDuration)
            : base(startDelay, transitionDuration)
        {

            StartValue = startValue;
            EndValue = endValue;
        }

        public override void SetStartState()
        {
            control.Alpha = StartValue;
            base.SetStartState();
        }

        public override void ApplyTransition(GameTime gameTime)
        {
            if (control.Alpha != EndValue)
            {
                float timeProgression = (float)(timeSinceInitialisation - (float)StartDelay) /
                                        (float)TransitionDuration;

                control.Alpha = MathHelper.Lerp(StartValue, EndValue, timeProgression);

                if (timeProgression > TransitionDuration)
                    Done = true;
            }

            base.ApplyTransition(gameTime);
        }

        public override UITransition Clone()
        {
            return new FadeTransition(this.StartValue, this.EndValue, this.StartDelay, this.TransitionDuration);
        }
    }

    public class TranslateTransition : UITransition
    {
        public Vector2 Movement { get; set; }

        public TranslateTransition(Vector2 movement, int startDelay, int transitionDuration)
            : base(startDelay, transitionDuration)
        {
            Movement = movement;
        }

        public override void SetStartState()
        {
            base.SetStartState();
        }

        public override void ApplyTransition(GameTime gameTime)
        {
         
            base.ApplyTransition(gameTime);
        }

        public override UITransition Clone()
        {
            return new TranslateTransition(this.Movement, this.StartDelay, this.TransitionDuration);
        }
    }
}
