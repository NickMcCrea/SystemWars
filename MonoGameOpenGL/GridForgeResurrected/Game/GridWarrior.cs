using BEPUphysics.Entities;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GameObject.Components.Controllers;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MonoGameEngineCore.Helper;

namespace GridForgeResurrected.Game
{
    class GridWarrior : GameEntity
    {
   
        public int Health { get; private set; }
        public int Score { get; private set; }

        public GridWarrior(Vector3 startPos)
        {
            Transform.SetPosition(startPos);
            Health = 100;
        }

        protected override void Initialise()
        {
            ProceduralCube playerShape = new ProceduralCube();
            playerShape.Scale(5f);

            AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(playerShape),
                BufferBuilder.IndexBufferBuild(playerShape), playerShape.PrimitiveCount));

            AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));

            AddComponent(new PhysicsComponent(true, false, PhysicsMeshType.box));
            
            Name = "player";


            AddComponent(new TopDownMouseAndKeyboardController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(this);
            physicsComponent = GetComponent<PhysicsComponent>();
            base.Initialise();
        }

        internal void Update(GameTime gameTime)
        {
            var pairs = physicsComponent.PhysicsEntity.CollisionInformation.Pairs;
            foreach (CollidablePairHandler pair in pairs)
            {
                var contacts = pair.Contacts;
                foreach (ContactInformation contact in contacts)
                {
                    var remove = (-pair.Contacts[0].Contact.Normal * pair.Contacts[0].Contact.PenetrationDepth).ToXNAVector();
                    remove.Y = 0;
                    Transform.Translate(remove);
                    Transform.Velocity += (remove * 0.01f);
                    break;
                }

            }


        }

        internal void Damage(int damage)
        {
            Health -= damage;
            if (Health < damage)
                Health = 0;
        }
       
    }

  
}
