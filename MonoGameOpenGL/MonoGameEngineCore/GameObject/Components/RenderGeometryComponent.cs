using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject.Components
{
    public class RenderGeometryComponent : IComponent
    {
        public GameObject ParentObject { get; set; }
        public VertexBuffer VertexBuffer { get; set; }
        public IndexBuffer IndexBuffer { get; set; }
        public int PrimitiveCount { get; set; }

        public RenderGeometryComponent(VertexBuffer vBuffer, IndexBuffer iBuffer, int primCount)
        {
            this.VertexBuffer = vBuffer;
            this.IndexBuffer = iBuffer;
            this.PrimitiveCount = primCount;
        }


        public RenderGeometryComponent(ProceduralShape shape)
        {
            this.VertexBuffer = BufferBuilder.VertexBufferBuild(shape);
            this.IndexBuffer = BufferBuilder.IndexBufferBuild(shape);
            this.PrimitiveCount = shape.PrimitiveCount;
        }


        public void Initialise()
        {
        }

        internal List<Vector3> GetVertices()
        {

            var list = new VertexPositionColorTextureNormal[VertexBuffer.VertexCount];
            VertexBuffer.GetData(list);

            List<Vector3> vertices = new List<Vector3>(VertexBuffer.VertexCount);

            for (int i = 0; i < VertexBuffer.VertexCount; i++)
            {
                vertices.Add(list[i].Position);
            }

            return vertices;
        }

        internal short[] GetIndices()
        {
            var list = new short[IndexBuffer.IndexCount];
            IndexBuffer.GetData(list);
            return list;
        }
    }
}