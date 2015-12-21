using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace MonoGameEngineCore.Rendering
{
    [Serializable]
    public struct VertexPositionColorTextureNormal : IVertexType, ISerializable
    {
       
        public Vector3 Position;
        public Color Color;
        public Vector2 Texture;
        public Vector3 Normal;

        public readonly static VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(24, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );

        VertexDeclaration IVertexType.VertexDeclaration
        {
            get { return VertexDeclaration; }
        }

       
        public VertexPositionColorTextureNormal(Vector3 position, Color color, Vector2 texture, Vector3 normal)
        {
            Position = position;
            Color = color;
            Texture = texture;
            Normal = normal;
        }

        private VertexPositionColorTextureNormal(SerializationInfo info, StreamingContext context)
        {
           
            Position.X = info.GetSingle("x");
            Position.Y = info.GetSingle("y");
            Position.Z = info.GetSingle("z");

            Color = Color.Black;
            Color.R = info.GetByte("r");
            Color.G = info.GetByte("g");
            Color.B = info.GetByte("b");
            Color.A = info.GetByte("a");

            Texture.X = info.GetSingle("u");
            Texture.Y = info.GetSingle("v");

            Normal.X = info.GetSingle("xn");
            Normal.Y = info.GetSingle("yn");
            Normal.Z = info.GetSingle("zn");

        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("x", Position.X);
            info.AddValue("y", Position.Y);
            info.AddValue("z", Position.Z);

            info.AddValue("r", Color.R);
            info.AddValue("b", Color.B);
            info.AddValue("g", Color.G);
            info.AddValue("a", Color.A);

            info.AddValue("u", Texture.X);
            info.AddValue("v", Texture.Y);

            info.AddValue("xn", Normal.X);
            info.AddValue("yn", Normal.Y);
            info.AddValue("zn", Normal.Z);

        }
    } 
}
