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
            gameObject.Transform.SetPosition(startPos);
        }

        protected override void Initialise()
        {
            ProceduralSphereTwo playerShape = new ProceduralSphereTwo(10);
            playerShape.Scale(2f);
            gameObject = GameObjectFactory.CreateCollidableObject(playerShape,
                EffectLoader.LoadSM5Effect("flatshaded"), PhysicsMeshType.sphere);

            gameObject.AddComponent(new SimpleEnemyAIController());
            
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);
            physicsComponent = gameObject.GetComponent<PhysicsComponent>();
           
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
                    gameObject.Transform.Translate(remove);
                    gameObject.Transform.Velocity += (remove * 0.001f);
                    break;
                }

            }

        }


    }
}
