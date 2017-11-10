using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSRestMatch.GameServer
{
    public class ProjectileComponent : IComponent, IUpdateable
    {
        private Weapon firingWeapon;
        private PhysicsComponent physicsComponent;
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

        public ProjectileComponent(Weapon firingWeapon)
        {
            this.firingWeapon = firingWeapon;
        }

        public void Initialise()
        {
            Enabled = true;
        }

        public void PostInitialise()
        {
            this.physicsComponent = ParentObject.GetComponent<PhysicsComponent>();
            physicsComponent.PhysicsEntity.CollisionInformation.CollisionRules.Personal = BEPUphysics.CollisionRuleManagement.CollisionRule.NoSolver;

        }

        public void Update(GameTime gameTime)
        {
            var pairs = physicsComponent.PhysicsEntity.CollisionInformation.Pairs;
            foreach (CollidablePairHandler pair in pairs)
            {
                var contacts = pair.Contacts;
                foreach (ContactInformation contact in contacts)
                {
                    if ((pair.EntityB != null && pair.EntityB.Tag is Player) || (pair.EntityA != null && pair.EntityA.Tag is Player))
                    {

                        Player p = null;
                        if (pair.EntityB.Tag is Player)
                            p = pair.EntityB.Tag as Player;


                        if (pair.EntityA.Tag is Player)
                            p = pair.EntityA.Tag as Player;

                        //this was projectile is owned by the current player, ignore.
                        if (p.CurrentWeapon == firingWeapon)
                            continue;

                        p.DamagePlayer(firingWeapon);
                        Remove();

                    }
                    else
                    {
                        //wall collision
                        Remove();
                    }
                }
            }

            //projectile is miles away. Remove it.
            if (ParentObject.Transform.AbsoluteTransform.Translation.Length() > 500)
                Remove();
        }

        void Remove()
        {
            SystemCore.GameObjectManager.RemoveObject(ParentObject);
        }

    }

    public class Projectile : GameObject
    {



        public Projectile(Vector3 position, Vector3 velocity, Weapon firingWeapon)
        {

            ProceduralSphereTwo projectileShape = new ProceduralSphereTwo(10);
            projectileShape.Scale(0.05f);

            AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(projectileShape),
                BufferBuilder.IndexBufferBuild(projectileShape), projectileShape.PrimitiveCount));

            AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            AddComponent(new PhysicsComponent(true, false, PhysicsMeshType.sphere));

            AddComponent(new ProjectileComponent(firingWeapon));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(this);

            Transform.SetPosition(position);
            Transform.Velocity = velocity;

        }




    }
}
