using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.CollisionShapes;
using BEPUphysics.Entities.Prefabs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using BepuRay = BEPUutilities.Ray;
using ConversionHelper;


namespace MonoGameEngineCore.GameObject.Components
{
    public class MeshColliderComponent : IComponent, IUpdateable, IDisposable
    {
        public GameObject ParentObject { get; set; }
        public MobileMesh mobileMesh;
        Microsoft.Xna.Framework.Vector3 offset;
        public object Tag { get; set; }
    
        public MeshColliderComponent(object tag)
        {
            if (tag != null)
                Tag = tag;

        }

        public void Initialise()
        {
      ;

            Enabled = true;
            var renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();

            var vertices = renderGeometry.GetVertices();
            List<BEPUutilities.Vector3> bepuVerts = MathConverter.Convert(vertices.ToArray()).ToList();
            mobileMesh = new MobileMesh(bepuVerts.ToArray(), MonoMathHelper.ConvertShortToInt(renderGeometry.GetIndices()), AffineTransform.Identity, MobileMeshSolidity.Counterclockwise);
            offset = mobileMesh.WorldTransform.Translation.ToXNAVector();
            mobileMesh.CollisionInformation.Tag = this.Tag;
            SystemCore.PhysicsSimulation.Add(mobileMesh);
        }

        public bool Enabled { get; set; }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {

            mobileMesh.WorldTransform = MathConverter.Convert(Microsoft.Xna.Framework.Matrix.CreateTranslation(offset)) * MathConverter.Convert(ParentObject.Transform.WorldMatrix);
        }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> UpdateOrderChanged;


        public void Dispose()
        {
            SystemCore.PhysicsSimulation.Remove(mobileMesh);
        }
    }
}
