using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.GameObject.Components
{
    public class HighPrecisionPosition : IComponent, IUpdateable
    {
        public Vector3d Position { get; set; }
        

        public HighPrecisionPosition()
        {
            Enabled = true;
        }

        public void PostInitialise()
        {

        }

        public void Update(GameTime gameTime)
        {

        }

        public GameObject ParentObject
        {
            get;
            set;
        }

        public void Initialise()
        {

        }

        public bool Enabled
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public int UpdateOrder
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> UpdateOrderChanged;

       
    }
}
