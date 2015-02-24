using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using Matrix = BEPUutilities.Matrix;
using BEPUphysics.BroadPhaseEntries;

namespace MonoGameEngineCore.GameObject.Components
{

    public enum PhysicsMeshType
    {
        sphere,
        box
    }

    public class PhysicsComponent : IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        public Entity PhysicsEntity { get; private set; }
        public bool Simulated { get; set; }
        public PhysicsMeshType PhysicsMeshType { get; private set; }
        private bool movable;

        public PhysicsComponent(Entity physicsEntity, bool movable, bool simulated)
        {
            Enabled = true;
            Simulated = simulated;
            this.PhysicsEntity = physicsEntity;
            if (!movable)
                PhysicsEntity.BecomeKinematic();

        }

        public PhysicsComponent(bool movable, bool simulated, PhysicsMeshType type)
        {
            this.PhysicsMeshType = type;
            Enabled = true;
            Simulated = simulated;
            this.movable = movable;

        }

        public void Initialise()
        {
            if (PhysicsEntity == null)
            {
                if (PhysicsMeshType == PhysicsMeshType.box)
                    GenerateBoxCollider();
                if (PhysicsMeshType == PhysicsMeshType.sphere)
                    GenerateSphereCollider();

                if (!movable)
                    PhysicsEntity.BecomeKinematic();
            }

            PhysicsEntity.Tag = ParentObject.ID;
            PhysicsEntity.CollisionInformation.Tag = ParentObject.ID;

            PhysicsEntity.WorldTransform = MonoMathHelper.GenerateBepuMatrixFromMono(ParentObject.Transform.WorldMatrix);
            SystemCore.PhysicsSimulation.Add(PhysicsEntity);
        }

        private void GenerateSphereCollider()
        {
            RenderGeometryComponent geometry = ParentObject.GetComponent<RenderGeometryComponent>();
            List<Vector3> verts = geometry.GetVertices();
            BoundingSphere sphere = BoundingSphere.CreateFromPoints(verts);

            PhysicsEntity = new Sphere(MonoMathHelper.Translate(ParentObject.Transform.WorldMatrix.Translation),
                sphere.Radius * 2, 1);
        }

        private void GenerateBoxCollider()
        {
            RenderGeometryComponent geometry = ParentObject.GetComponent<RenderGeometryComponent>();
            List<Vector3> verts = geometry.GetVertices();
            BoundingBox testBox = BoundingBox.CreateFromPoints(verts);
            float width = testBox.Max.X - testBox.Min.X;
            float height = testBox.Max.Y - testBox.Min.Y;
            float depth = testBox.Max.Z - testBox.Min.Z;

            PhysicsEntity = new Box(new BEPUutilities.Vector3(ParentObject.Transform.WorldMatrix.Translation.X,
                ParentObject.Transform.WorldMatrix.Translation.X,
                ParentObject.Transform.WorldMatrix.Translation.X), width, height, depth, 1);
        }

        public bool Enabled { get; set; }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
            if (Simulated)
                ParentObject.Transform.WorldMatrix = MonoMathHelper.GenerateMonoMatrixFromBepu(PhysicsEntity.WorldTransform);
            else
            {
                PhysicsEntity.WorldTransform = MonoMathHelper.GenerateBepuMatrixFromMono(ParentObject.Transform.WorldMatrix);
            }
        }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public bool InCollision()
        {
            return PhysicsEntity.CollisionInformation.Pairs.Count > 0;
        }

        public bool CollidedWithEntity(int entityID)
        {
            for (int i = 0; i < PhysicsEntity.CollisionInformation.Pairs.Count; i++)
            {
                if ((int)PhysicsEntity.CollisionInformation.Pairs[i].EntityA.Tag == entityID)
                    return true;

                if ((int)PhysicsEntity.CollisionInformation.Pairs[i].EntityB.Tag == entityID)
                    return true;
            }
            return false;
        }

        
        
    }


}
