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
    

    class SimpleEnemy : GameEntity
    {

        public SimpleEnemy(Vector3 startPos)
            : base()
        {
            Transform.SetPosition(startPos);
        }

        protected override void Initialise()
        {
            ProceduralSphereTwo playerShape = new ProceduralSphereTwo(10);
            playerShape.Scale(2f);

            AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(playerShape),
                BufferBuilder.IndexBufferBuild(playerShape), playerShape.PrimitiveCount));

            AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));

            AddComponent(new PhysicsComponent(true, false, PhysicsMeshType.sphere));

            AddComponent(new SimpleEnemyAIController());
            Name = "simpleenemy";
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(this);
            physicsComponent = GetComponent<PhysicsComponent>();
           
        }

        public override void Update(GameTime gameTime)
        {
            var pairs = physicsComponent.PhysicsEntity.CollisionInformation.Pairs;
            foreach (CollidablePairHandler pair in pairs)
            {
                var contacts = pair.Contacts;
                foreach (ContactInformation contact in contacts)
                {
                    var remove = (-pair.Contacts[0].Contact.Normal * pair.Contacts[0].Contact.PenetrationDepth).ToXNAVector();

                    if (pair.EntityB == physicsComponent.PhysicsEntity)
                        remove *= -1;

                    remove.Y = 0;
                    Transform.Translate(remove);
                    Transform.Velocity += (remove * 0.001f);
                    break;
                }

            }

        }


    }
}
