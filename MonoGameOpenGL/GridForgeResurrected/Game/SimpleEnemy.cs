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
    class SimpleEnemy
    {
        private GameObject enemyGameObject;
        private PhysicsComponent physicsComponent;

        public SimpleEnemy(Vector3 startPos)
        {
            ProceduralSphereTwo playerShape = new ProceduralSphereTwo(10);
            playerShape.Scale(2f);
            enemyGameObject = GameObjectFactory.CreateCollidableObject(playerShape,
                EffectLoader.LoadEffect("flatshaded"), PhysicsMeshType.sphere);

            enemyGameObject.AddComponent(new SimpleEnemyAIController());

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(enemyGameObject);

            enemyGameObject.Transform.SetPosition(startPos);

            physicsComponent = enemyGameObject.GetComponent<PhysicsComponent>();
        }

        public void Update()
        {

        }

        internal void Update(GameTime gameTime)
        {

            CollisionResponse();
        }

        private void CollisionResponse()
        {
            var pairs = physicsComponent.PhysicsEntity.CollisionInformation.Pairs;
            foreach (CollidablePairHandler pair in pairs)
            {
                var contacts = pair.Contacts;
                foreach (ContactInformation contact in contacts)
                {
                    var remove = (-pair.Contacts[0].Contact.Normal * pair.Contacts[0].Contact.PenetrationDepth).ToXNAVector();
                    enemyGameObject.Transform.Translate(remove);
                    enemyGameObject.Transform.Velocity += (remove / 20);
                    break;
                }

            }
        }
    }
}
