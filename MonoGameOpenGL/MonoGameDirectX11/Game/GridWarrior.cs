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
        private List<Projectile> projectiles; 

        public GridWarrior(Vector3 startPos)
        {
            Transform.SetPosition(startPos);
            Health = 100;

            projectiles = new List<Projectile>();
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

                    if (pair.EntityB != null && pair.EntityB.Tag is Projectile)
                        continue;
                    if (pair.EntityA != null && pair.EntityA.Tag is Projectile)
                        continue;

                    var remove = (-pair.Contacts[0].Contact.Normal * pair.Contacts[0].Contact.PenetrationDepth).ToXNAVector();
                    remove.Y = 0;
                    Transform.Translate(remove);
                    Transform.Velocity += (remove * 0.01f);
                    break;
                }

            }

            if (SystemCore.Input.MouseLeftPress())
            {
                Projectile p = new Projectile(Transform.AbsoluteTransform.Translation, -Transform.AbsoluteTransform.Forward*0.3f);
                projectiles.Add(p);
            }

            foreach (Projectile projectile in projectiles)
            {
                projectile.Update(gameTime);
            }

            int killed = projectiles.RemoveAll(x => x.Killed);
            Score += killed;

            projectiles.RemoveAll(x => x.Collided);
        }

        internal void Damage(int damage)
        {
            Health -= damage;
            if (Health < damage)
                Health = 0;
        }
       
    }

    public class Projectile : GameEntity
    {
        public bool Killed { get; set; }
        public bool Collided { get; set; }

        public Projectile(Vector3 position, Vector3 velocity)
        {
            ProceduralSphereTwo projectileShape = new ProceduralSphereTwo(10);
            projectileShape.Scale(1f);

            AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(projectileShape),
                BufferBuilder.IndexBufferBuild(projectileShape), projectileShape.PrimitiveCount));

            AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            AddComponent(new PhysicsComponent(true, false, PhysicsMeshType.box));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(this);
            physicsComponent = GetComponent<PhysicsComponent>();

            base.Initialise();

            Transform.SetPosition(position);
            Transform.Velocity = velocity;
        }

        public override void Update(GameTime gameTime)
        {
            
            var pairs = physicsComponent.PhysicsEntity.CollisionInformation.Pairs;
            foreach (CollidablePairHandler pair in pairs)
            {
                var contacts = pair.Contacts;
                foreach (ContactInformation contact in contacts)
                {
                    if ((pair.EntityB != null && pair.EntityB.Tag is SimpleEnemy )||(pair.EntityA != null && pair.EntityA.Tag is SimpleEnemy))
                    {
                        Remove();
                        Killed = true;
                    }
                    else
                    {
                        Collided = true;
                        Remove();
                    }
             
                 
                }
            }

            base.Update(gameTime);
        }
    }

  
}
