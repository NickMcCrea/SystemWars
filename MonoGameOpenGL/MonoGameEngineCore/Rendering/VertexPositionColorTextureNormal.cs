using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameEngineCore.Rendering
{
    public struct VertexPositionColorTextureNormal : IVertexType
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Texture;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(28, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

        public VertexPositionColorTextureNormal(Vector3 position, Color color, Vector3 texture, Vector3 normal)
        {
            Position = position;
            Color = color;
            Texture = texture;
            Normal = normal;
        }
    } 
}
