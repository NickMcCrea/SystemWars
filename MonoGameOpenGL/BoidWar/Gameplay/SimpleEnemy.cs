using BEPUphysics.Paths.PathFollowing;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidWar.Gameplay
{
    public class SimpleEnemy : GameObject, IEnemy
    {

        public Vector3 DesiredPosiiton { get; set; }

        public SimpleEnemy()
        {
     
            ProceduralCube shape = new ProceduralCube();
            AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
            AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            AddComponent(new ShadowCasterComponent());
            var physicsComponent = new PhysicsComponent(true, true, PhysicsMeshType.box);
            AddComponent(physicsComponent);

            AddComponent(new SimpleEnemyComponent(physicsComponent));

            
        }

        public void Destroy()
        {
            SystemCore.GameObjectManager.RemoveObject(this);
        }
    }

    public interface IEnemy
    {
        void Destroy();
    }

    public class SimpleEnemyComponent : IComponent, IUpdateable
    {
        EntityMover mover;
        EntityRotator rotator;
        SimpleEnemy e;
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
        PhysicsComponent physicsComponent;

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public SimpleEnemyComponent(PhysicsComponent component)
        {
            this.physicsComponent = component;
        }


        public void Initialise()
        {
            Enabled = true;

        }

        public void PostInitialise()
        {
            mover = new EntityMover(physicsComponent.PhysicsEntity);
            rotator = new EntityRotator(physicsComponent.PhysicsEntity);
            SystemCore.PhysicsSimulation.Add(mover);
            SystemCore.PhysicsSimulation.Add(rotator);

            mover.LinearMotor.Settings.Servo.SpringSettings.Stiffness /= 10000;
            mover.LinearMotor.Settings.Servo.SpringSettings.Damping /= 1000;
            
            e = ParentObject as SimpleEnemy;

        }

        public void Update(GameTime gameTime)
        {

            if (e.DesiredPosiiton != e.Transform.AbsoluteTransform.Translation)
            {
                mover.TargetPosition = e.DesiredPosiiton.ToBepuVector();

                Vector3 newForward = e.DesiredPosiiton - e.Transform.AbsoluteTransform.Translation;
                newForward.Normalize();
                Vector3 newRight = Vector3.Cross(newForward, Vector3.Up);
                Vector3 newUp = Vector3.Cross(newRight, newForward);

                var lookMatrix = Matrix.CreateWorld(ParentObject.Transform.AbsoluteTransform.Translation, newForward, newUp);
                var bepuMatrix = MonoMathHelper.GenerateBepuMatrixFromMono(lookMatrix);
                BEPUutilities.Quaternion desiredRot = BEPUutilities.Quaternion.CreateFromRotationMatrix(bepuMatrix);
                rotator.TargetOrientation = desiredRot;
            }

        }


    }
}
