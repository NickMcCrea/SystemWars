using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GridForgeResurrected.Game
{
    class GameEntity
    {
        protected GameObject gameObject;
        protected PhysicsComponent physicsComponent;

        public GameEntity()
        {
            Initialise();
        }

        protected virtual void Initialise()
        {

        }

        public virtual void Update(GameTime gameTime)
        {

        }

    }
}
