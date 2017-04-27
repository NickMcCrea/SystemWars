using System;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Helper;
using BEPUphysics.Entities;

namespace MonoGameEngineCore.GameObject.Components.Controllers
{
    public class TopDownMouseAndKeyboardController : IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        private InputManager inputManager;
        private float speed = 0.003f;
        private float bleed = 0.95f;

        public void PostInitialise()
        {

        }

        public void Initialise()
        {
            this.Enabled = true;
            this.inputManager = SystemCore.Input;
           
        }

        public bool Enabled { get; set; }
        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {


            if (inputManager.IsKeyDown(Keys.Left))
            {
                ParentObject.Transform.Velocity -= Vector3.Left * speed;
            }
            if (inputManager.IsKeyDown(Keys.Right))
            {
                ParentObject.Transform.Velocity += Vector3.Left * speed;
            }
            if (inputManager.IsKeyDown(Keys.Up))
            {
                ParentObject.Transform.Velocity -= Vector3.Forward * speed;
            }
            if (inputManager.IsKeyDown(Keys.Down))
            {
                ParentObject.Transform.Velocity += Vector3.Forward * speed;
            }
            ParentObject.Transform.Velocity *= 0.95f;

            RayCastResult result;
            if (inputManager.GetMouseImpact(out result))
            {
                Vector3 point = result.HitData.Location.ToXNAVector();
                point.Y = ParentObject.Transform.AbsoluteTransform.Translation.Y; //keeps the rotation clean
                Vector3 lookAt = ParentObject.Transform.AbsoluteTransform.Translation - point;
                lookAt.Normalize();
                ParentObject.Transform.SetLookAndUp(lookAt, Vector3.Up);
            }

            ParentObject.GetComponent<PhysicsComponent>().DoCollisionResponse(speed);
        }

        public int UpdateOrder { get; set; }
        public event EventHandler<EventArgs> UpdateOrderChanged;
    }
}
