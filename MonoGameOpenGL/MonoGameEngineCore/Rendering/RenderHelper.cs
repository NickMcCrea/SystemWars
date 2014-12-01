using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.Rendering
{
    public class RenderHelper
    {

        public static void DrawIndexedPrimitive(BasicEffect effect, Matrix view, Matrix projection, Matrix world, VertexBuffer vBuffer, IndexBuffer iBuffer, int primitiveCount)
        {
           
            effect.World = Matrix.Identity;
            effect.View = view;
            effect.Projection = projection;

            SystemCore.GraphicsDevice.SetVertexBuffer(vBuffer);
            SystemCore.GraphicsDevice.Indices = iBuffer;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                SystemCore.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vBuffer.VertexCount, 0, primitiveCount);
            }
        }
    }
}
