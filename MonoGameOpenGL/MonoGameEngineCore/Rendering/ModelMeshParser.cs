using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Procedural;

namespace MonoGameEngineCore.Rendering
{
    public static class ModelMeshParser
    {
        public static ProceduralShape GetShapeFromModel(Model model)
        {
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                ProceduralShape shape = null;
                foreach (ModelMeshPart part in modelMesh.MeshParts)
                {
                   
                    VertexPositionNormal[] array =
                        new VertexPositionNormal[part.VertexBuffer.VertexCount];

                    VertexPositionColorTextureNormal[] newArray = new VertexPositionColorTextureNormal[part.VertexBuffer.VertexCount];

                    short[] indices =new short[part.IndexBuffer.IndexCount];
                    part.IndexBuffer.GetData<short>(indices);

                    part.VertexBuffer.GetData<VertexPositionNormal>(array);


                    for (int i = 0; i < array.Length; i++)
                    {
                        newArray[i] = new VertexPositionColorTextureNormal(array[i].Position, Color.DarkGray,
                            Vector2.Zero, array[i].Normal);
                    }



                    if (shape == null)
                    {
                        shape = new ProceduralShape(newArray, indices);
                    }
                    else
                    {
                        shape = ProceduralShape.Combine(shape, new ProceduralShape(newArray, indices));
                    }

                    return shape;
                    
                }
            }
            return null;
        }
    }
}
