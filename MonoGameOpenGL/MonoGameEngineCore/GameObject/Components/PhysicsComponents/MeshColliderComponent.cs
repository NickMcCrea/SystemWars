using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using BepuRay = BEPUutilities.Ray;
using ConversionHelper;


namespace MonoGameEngineCore.GameObject.Components
{
    public class MobileMeshColliderComponent : IComponent, IUpdateable, IDisposable
    {
        public GameObject ParentObject { get; set; }
        public MobileMesh mobileMesh;
        Microsoft.Xna.Framework.Vector3 offset;

        public object Tag { get; set; }

        public MobileMeshColliderComponent(object tag, List<Microsoft.Xna.Framework.Vector3> verts, int[] indices)
        {
            if (tag != null)
                Tag = tag;

            ParentObject = tag as GameObject;

            List<BEPUutilities.Vector3> bepuVerts =
                MathConverter.Convert(verts.ToArray())
                    .ToList();

            mobileMesh = new MobileMesh(bepuVerts.ToArray(), indices, AffineTransform.Identity, MobileMeshSolidity.Counterclockwise);
            offset = mobileMesh.WorldTransform.Translation.ToXNAVector();
            
            mobileMesh.CollisionInformation.Tag = this.Tag;

            SystemCore.PhysicsSimulation.SpaceObjectBuffer.Add(mobileMesh);

        }

        public void Initialise()
        {
            Enabled = true;
        }

        public bool Enabled { get; set; }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
            if (!SystemCore.PhysicsOnBackgroundThread)
                mobileMesh.WorldTransform = MathConverter.Convert(Microsoft.Xna.Framework.Matrix.CreateTranslation(offset)) * MathConverter.Convert(ParentObject.Transform.AbsoluteTransform);
            else
            {
                mobileMesh.BufferedStates.States.WorldTransform = MathConverter.Convert(Microsoft.Xna.Framework.Matrix.CreateTranslation(offset)) * MathConverter.Convert(ParentObject.Transform.AbsoluteTransform);
            }
        }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public void Dispose()
        {
            if (!SystemCore.PhysicsOnBackgroundThread)
                SystemCore.PhysicsSimulation.Remove(mobileMesh);
            else
            {
                SystemCore.PhysicsSimulation.SpaceObjectBuffer.Remove(mobileMesh);
            }
        }
    }
}
