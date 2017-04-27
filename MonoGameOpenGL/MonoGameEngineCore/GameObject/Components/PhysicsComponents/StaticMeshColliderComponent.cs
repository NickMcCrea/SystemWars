using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using ConversionHelper;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.GameObject.Components
{
    public class StaticMeshColliderComponent : IComponent, IUpdateable, IDisposable
    {
        public GameObject ParentObject { get; set; }
        public StaticMesh staticMesh;
        private Microsoft.Xna.Framework.Vector3 offset;

        public object Tag { get; set; }

        public StaticMeshColliderComponent(object tag, List<Microsoft.Xna.Framework.Vector3> verts, int[] indices)
        {
            if (tag != null)
                Tag = tag;

            ParentObject = tag as GameObject;

            List<BEPUutilities.Vector3> bepuVerts =
                MathConverter.Convert(verts.ToArray())
                    .ToList();

            staticMesh = new StaticMesh(bepuVerts.ToArray(), indices, AffineTransform.Identity);
            offset = staticMesh.WorldTransform.Translation.ToXNAVector();
            
            staticMesh.Tag = this.Tag;

            SystemCore.PhysicsSimulation.SpaceObjectBuffer.Add(staticMesh);

        }

        public void Initialise()
        {
            Enabled = true;
        }

        public void PostInitialise()
        {

        }

        public bool Enabled { get; set; }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {

        }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public void Dispose()
        {
            if (!SystemCore.PhysicsOnBackgroundThread)
                SystemCore.PhysicsSimulation.Remove(staticMesh);
            else
            {
                SystemCore.PhysicsSimulation.SpaceObjectBuffer.Remove(staticMesh);
            }
        }
    }
}