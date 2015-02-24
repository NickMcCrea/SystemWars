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

            ParentObject = tag as GameObject;

            List<BEPUutilities.Vector3> bepuVerts =
                MathConverter.Convert(ParentObject.GetComponent<RenderGeometryComponent>().GetVertices().ToArray())
                    .ToList();

            mobileMesh = new MobileMesh(bepuVerts.ToArray(),
                MonoMathHelper.ConvertShortToInt(ParentObject.GetComponent<RenderGeometryComponent>().GetIndices()),
                AffineTransform.Identity, MobileMeshSolidity.Counterclockwise);
            offset = mobileMesh.WorldTransform.Translation.ToXNAVector();
            mobileMesh.CollisionInformation.Tag = this.Tag;

        }

        public void Initialise()
        {
            Enabled = true;
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
