using System;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GameObject.Components;

namespace OldGameTest.Screens
{
    class GameOfLifeComponent : IComponent, IDrawable, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        public bool active = false;
        private bool flipAtEndOfTick = false;
        public int DrawOrder { get; set; }

        public bool Visible { get; set; }

        public bool Enabled { get; set; }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;



        public void Draw(GameTime gameTime)
        {

        }

        public void ActivateCell()
        {
            ParentObject.GetComponent<RenderGeometryComponent>().SetColour(SimChalleneScreen.activeCellColour);
            active = true;
        }
        public void DeactivateCell()
        {

            ParentObject.GetComponent<RenderGeometryComponent>().SetColour(SimChalleneScreen.inactiveCellColour);
            active = false;
        }
        public void Initialise()
        {
            Enabled = true;
        }

        public void PostInitialise()
        {

        }

        public void ScheduleFlip()
        {
            flipAtEndOfTick = true;
        }

        public void FlipIfScheduled()
        {
            if (flipAtEndOfTick)
                FlipState();
        }


        internal void FlipState()
        {
            if (active)
                DeactivateCell();
            else
                ActivateCell();

            flipAtEndOfTick = false;
        }



        public void Update(GameTime gameTime)
        {
            if (active)
            {
                float newY = MathHelper.Lerp(ParentObject.Position.Y, 1.5f, 0.2f);
                ParentObject.Position = ParentObject.Position.ReplaceYComponent(newY);
            }
            else
            {
                ParentObject.Position = ParentObject.Position.ReplaceYComponent(MathHelper.Lerp(ParentObject.Position.Y, 0, 0.2f));
            }
        }
    }
}
