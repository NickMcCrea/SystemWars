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

namespace BoidWar.Gameplay
{

    public class MainBase : Building
    {


        public MainBase()
        {
            Health = 100;
            Size = 8;

            ProceduralCube shape = new ProceduralCube();
            shape.Scale(Size);
            shape.SetColor(Color.Blue);
            AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
            AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            AddComponent(new ShadowCasterComponent());
            var physicsComponent = new PhysicsComponent(false, false, PhysicsMeshType.box);
            AddComponent(physicsComponent);

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(this);
            GetComponent<PhysicsComponent>().PhysicsEntity.CollisionInformation.Events.DetectingInitialCollision += Events_DetectingInitialCollision;

           
        }

        private void Events_DetectingInitialCollision(BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable sender, BEPUphysics.BroadPhaseEntries.Collidable other, BEPUphysics.NarrowPhaseSystems.Pairs.CollidablePairHandler pair)
        {
            Health--;  
            if(other.Tag is IEnemy)
            {
                ((IEnemy)other.Tag).Destroy();
            }
        }

        public void Update(GameTime gameTime)
        {




        }

    }


    public class Building : GameObject
    {
        public int Health { get; set; }
        public int Size { get; set; }

    }



}
