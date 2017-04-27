using BEPUphysics;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GameObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGameEngineCore.GameObject.Components;

namespace GridForgeResurrected
{
    class SimpleEnemyAIController: IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        private InputManager inputManager;
        private GameObject playerObject;
        private float speed = 0.00005f;
        private float bleed = 0.95f;

        public void Initialise()
        {
            this.Enabled = true;
            this.inputManager = SystemCore.Input;
           
        }

        public void PostInitialise()
        {

        }

        public bool Enabled { get; set; }
        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
            if (playerObject == null)
            {
                playerObject = SystemCore.GameObjectManager.GetObject("player");
            }

            Vector3 toPlayer = playerObject.Transform.AbsoluteTransform.Translation - ParentObject.Transform.AbsoluteTransform.Translation;

            //look where we're heading.
            if (toPlayer != Vector3.Zero)
            {
                ParentObject.Transform.SetLookAndUp(Vector3.Normalize(toPlayer), Vector3.Up);
                ParentObject.Transform.Velocity += toPlayer * speed;
            }

            ParentObject.Transform.Velocity *= 0.95f;

           
            
        }

        public int UpdateOrder { get; set; }
        public event EventHandler<EventArgs> UpdateOrderChanged;
    }
}
