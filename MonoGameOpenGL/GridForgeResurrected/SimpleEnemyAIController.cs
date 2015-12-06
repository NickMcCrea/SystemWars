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

namespace GridForgeResurrected
{
    class SimpleEnemyAIController: IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        private InputManager inputManager;
        private float speed = 0.003f;
        private float bleed = 0.95f;

        public void Initialise()
        {
            this.Enabled = true;
            this.inputManager = SystemCore.Input;
           
        }

        public bool Enabled { get; set; }
        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {

            //look where we're heading.
            if (ParentObject.Transform.Velocity != Vector3.Zero)
                ParentObject.Transform.SetLookAndUp(Vector3.Normalize(ParentObject.Transform.Velocity), Vector3.Up);
            
        }

        public int UpdateOrder { get; set; }
        public event EventHandler<EventArgs> UpdateOrderChanged;
    }
}
