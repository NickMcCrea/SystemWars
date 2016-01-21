using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MonoGameEngineCore.Editor
{
    public class SimpleModelEditor : IGameSubSystem
    {

        ProceduralShapeBuilder shapeBuilder;
        private string shapeFolderPath = "//Editor//Shapes//";
        private float modellingAreaSize = 8;
        public bool RenderGrid { get; set; }

        public SimpleModelEditor()
        {
            RenderGrid = true;
        }


        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color col)
        {
            shapeBuilder.AddTriangleWithColor(col, a, b, c);
        }

        public void AddFace(Color col, params Vector3[] points)
        {
            shapeBuilder.AddFaceWithColor(col, points);
        }

        public void Clear()
        {
            shapeBuilder = new ProceduralShapeBuilder();
        }

        public void SaveCurrentShape(string name)
        {
            ProceduralShape s = shapeBuilder.BakeShape();

            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = new FileStream(name + ".shape", FileMode.Create))
            {
                bf.Serialize(fs, s);
            }

        }

        public ProceduralShape LoadShape(string name)
        {
            BinaryFormatter bf = new BinaryFormatter();
            ProceduralShape s = null;
            using (FileStream fs = new FileStream(name + ".shape", FileMode.Open))
            {
                s = bf.Deserialize(fs) as ProceduralShape;
            }

            return s;
        }


        public void Initalise()
        {
            shapeBuilder = new ProceduralShapeBuilder();


           


        }

        public void Update(GameTime gameTime)
        {
           
        }

        public void Render(GameTime gameTime)
        {
            if (RenderGrid)
            {
                for (float i = -modellingAreaSize / 2; i <= modellingAreaSize / 2; i++)
                {
                    DebugShapeRenderer.AddXYGrid(new Vector3(-modellingAreaSize / 2, -modellingAreaSize / 2, i),
                        modellingAreaSize,
                        modellingAreaSize, 1, Color.DarkGray);

                    DebugShapeRenderer.AddXZGrid(new Vector3(-modellingAreaSize / 2, i, -modellingAreaSize / 2),
                        modellingAreaSize,
                        modellingAreaSize, 1, Color.DarkGray);

                    DebugShapeRenderer.AddYZGrid(new Vector3(i, -modellingAreaSize / 2, -modellingAreaSize / 2),
                        modellingAreaSize, modellingAreaSize, 1, Color.DarkGray);

                }
            }
        }
    }
}
