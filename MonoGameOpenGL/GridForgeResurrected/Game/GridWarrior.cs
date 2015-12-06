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
    class GridWarrior
    {
        private GameObject playerGameObject;
        private PhysicsComponent physicsComponent;

        public GridWarrior(Vector3 startPos)
        {
            ProceduralCube playerShape = new ProceduralCube();
            playerShape.Scale(5f);
            playerGameObject = GameObjectFactory.CreateCollidableObject(playerShape,
                EffectLoader.LoadEffect("flatshaded"), PhysicsMeshType.box);

            playerGameObject.AddComponent(new TopDownMouseAndKeyboardController());

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(playerGameObject);

            playerGameObject.Transform.SetPosition(startPos);

            physicsComponent = playerGameObject.GetComponent<PhysicsComponent>();
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
                    playerGameObject.Transform.Translate(remove);
                    playerGameObject.Transform.Velocity += (remove / 20);
                    break;
                }

            }
        }
    }

  
}
