using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSRestMatch.GameServer
{
    public class TrainingDummyComponent : IComponent, IUpdateable
    {
        Player p;
        float timeSinceLastTrigger;
        float interval = 5;
        public bool Enabled
        {
            get; set;
        }
       
        public GameObject ParentObject
        {
            get; set;
        }

        public int UpdateOrder
        {
            get; set;
        }

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public void Initialise()
        {
            Enabled = true;
        }

        public void PostInitialise()
        {
            p = ParentObject as Player;
            p.Invulnurable = true;
        }

        public void Update(GameTime gameTime)
        {
            if((gameTime.TotalGameTime.TotalSeconds - timeSinceLastTrigger) > interval)
            {
                timeSinceLastTrigger = (float)gameTime.TotalGameTime.TotalSeconds;

                p.DesiredHeading = RandomHelper.GetRandomInt(0, 360);
                p.DesiredPosition = RandomHelper.GetRandomVector3(-50, 50).ZeroYComponent();
            }
        }
    }
}
