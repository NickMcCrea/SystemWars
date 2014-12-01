using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.Procedural
{
    public static class BufferBuilder
    {
        public static VertexBuffer VertexBufferBuild(VertexPositionColorTextureNormal [] vertices)
        {
            var testBuffer = new VertexBuffer(SystemCore.GraphicsDevice, VertexPositionColorTextureNormal.VertexDeclaration, vertices.Count(), BufferUsage.None);
            testBuffer.SetData(vertices);
            return testBuffer;
        }

        public static IndexBuffer IndexBufferBuild(short[] indices)
        {
            var testIndexBuffer = new IndexBuffer(SystemCore.GraphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.None);
            testIndexBuffer.SetData(indices);
            return testIndexBuffer;
        }

        public static VertexBuffer VertexBufferBuild(ProceduralShape shape)
        {
            var testBuffer = new VertexBuffer(SystemCore.GraphicsDevice, VertexPositionColorTextureNormal.VertexDeclaration, shape.Vertices.Count(), BufferUsage.None);
            testBuffer.SetData(shape.Vertices.ToArray());
            return testBuffer;
        }

        public static IndexBuffer IndexBufferBuild(ProceduralShape shape)
        {
            var testIndexBuffer = new IndexBuffer(SystemCore.GraphicsDevice, IndexElementSize.SixteenBits, shape.Indices.Count(), BufferUsage.None);
            testIndexBuffer.SetData(shape.Indices.ToArray());
            return testIndexBuffer;
        }
    }
}
