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


namespace MonoGameEngineCore.GameObject.Components
{
    public class MeshColliderComponent : IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        private MobileMesh mobileMesh;
      

        public void Initialise()
        {
            Enabled = true;
            var renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();

            var vertices = renderGeometry.GetVertices();
            List<BEPUutilities.Vector3> bepuVerts = MonoMathHelper.ConvertVertsToBepu(vertices);
       
            mobileMesh = new MobileMesh(bepuVerts.ToArray(), MonoMathHelper.ConvertShortToInt(renderGeometry.GetIndices()), AffineTransform.Identity, MobileMeshSolidity.Counterclockwise);
        }

        public bool Enabled { get; set; }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {

        }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public bool RayCollision(Microsoft.Xna.Framework.Vector3 pos, Microsoft.Xna.Framework.Vector3 dir, float distance, out RayHit hit)
        {
            PositionMesh();
            BepuRay ray = new BepuRay(pos.ToBepuVector(), dir.ToBepuVector());
            return mobileMesh.CollisionInformation.RayCast(ray, distance, out hit);
        }

        private void PositionMesh()
        {
            var bepuTransform = MonoMathHelper.GenerateBepuMatrixFromMono(ParentObject.Transform.WorldMatrix);
            mobileMesh.WorldTransform = bepuTransform;
        }
    }
}
