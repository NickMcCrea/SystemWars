using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using System;

namespace MSRestMatch.GameServer
{
    internal class CombatDummyComponent : IComponent, IUpdateable
    {
        Player p;
        float timeSinceLastTrigger;
        float interval = 5;
        Player target;
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

            if (target == null)
            {
                var players = GameSimulation.GetLivingPlayers();

                if (players.Count > 0 && players[0] != ParentObject)
                {
                    target = players[0] as Player;
                }
            }

            if (target != null)
            {

                if ((gameTime.TotalGameTime.TotalSeconds - timeSinceLastTrigger) > interval)
                {

                    timeSinceLastTrigger = (float)gameTime.TotalGameTime.TotalSeconds;
                    p.DesiredPosition = target.Transform.AbsoluteTransform.Translation + RandomHelper.GetRandomVector3(-10, 10).ZeroYComponent();

                }
                p.GetComponent<PlayerControlComponent>().SetHeadingToPointToVector(target.Transform.AbsoluteTransform.Translation);

                p.FireCurrentWeapon();

            }

            if (target != null && target.Dead)
                target = null;

        }
    }
}