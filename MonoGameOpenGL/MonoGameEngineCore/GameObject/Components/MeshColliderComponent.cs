using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.BroadPhaseEntries;
using BEPUutilities;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using BepuRay = BEPUutilities.Ray;


namespace MonoGameEngineCore.GameObject.Components
{
    public class MeshColliderComponent : IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        private StaticMesh staticMesh;

        public void Initialise()
        {
            Enabled = true;
            var renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();

            var vertices = renderGeometry.GetVertices();
            List<BEPUutilities.Vector3> bepuVerts = MonoMathHelper.ConvertVertsToBepu(vertices);

            staticMesh = new StaticMesh(bepuVerts.ToArray(), MonoMathHelper.ConvertShortToInt(renderGeometry.GetIndices()));
        }

        public bool Enabled { get; set; }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
            var bepuTransform = MonoMathHelper.GenerateBepuMatrixFromMono(ParentObject.Transform.WorldMatrix);
            var quat = BEPUutilities.Quaternion.CreateFromRotationMatrix(bepuTransform);
            staticMesh.WorldTransform = new AffineTransform(quat, bepuTransform.Translation);
        }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> UpdateOrderChanged;

        public void RayCollision(Microsoft.Xna.Framework.Vector3 pos, Microsoft.Xna.Framework.Vector3 dir, float distance, out RayHit hit)
        {
            BepuRay ray = new BepuRay(pos.ToBepuVector(), dir.ToBepuVector());
            staticMesh.RayCast(ray, distance, out hit);
        }
    }
}
