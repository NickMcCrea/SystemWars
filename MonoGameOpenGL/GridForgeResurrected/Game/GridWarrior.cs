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
   

        public GridWarrior(Vector3 startPos)
        {
            gameObject.Transform.SetPosition(startPos);
        }

        protected override void Initialise()
        {
            ProceduralCube playerShape = new ProceduralCube();
            playerShape.Scale(5f);
            gameObject = GameObjectFactory.CreateCollidableObject(playerShape,
                EffectLoader.LoadEffect("flatshaded"), PhysicsMeshType.box);
            gameObject.Name = "player";
            gameObject.AddComponent(new TopDownMouseAndKeyboardController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);
            physicsComponent = gameObject.GetComponent<PhysicsComponent>();
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
                    gameObject.Transform.Translate(remove);
                    gameObject.Transform.Velocity += (remove * 0.01f);
                    break;
                }

            }
        }

       
    }

  
}
