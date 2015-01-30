using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace Vector.Screens.Entities
{
    class VectorEntity : GameObject
    {
        public int Health { get; set; }

        public VectorEntity(Vector3 startPos)
            : base()
        {
            this.Transform.SetPosition(startPos);
        }
    }

    class Projectile : VectorEntity
    {
        public int Damage { get; set; }
        public Vector3 Velocity { get; set; }

        public Projectile(Vector3 startPos)
            : base(startPos)
        {
            components.Add(new PhysicsComponent(true, true, PhysicsMeshType.sphere));
        }

    }

    class Player : VectorEntity
    {

        public Player(Vector3 startPos)
            : base(startPos)
        {
            AddComponent(new RenderGeometryComponent(new ProceduralCube()));
            AddComponent(new EffectRenderComponent(EffectLoader.LoadEffect("FlatShaded")));
            //AddComponent(new PhysicsComponent(false, false, PhysicsMeshType.box));
        }

        internal void Update(PlayerIndex playerIndex)
        {
            
        }
    }

    class Enemy : VectorEntity
    {
        public Enemy(Vector3 startPos)
            : base(startPos)
        {
            components.Add(new PhysicsComponent(true, true, PhysicsMeshType.box));
        }
    }

    class Structure : VectorEntity
    {

        public Structure(Vector3 startPos)
            : base(startPos)
        {
            components.Add(new PhysicsComponent(false, false, PhysicsMeshType.box));
        }
    }

}
