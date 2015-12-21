using Microsoft.Xna.Framework;
using MonoGameEngineCore.Procedural;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MonoGameEngineCore.Editor
{
    public class SimpleModelEditor
    {

        ProceduralShapeBuilder shapeBuilder;
        private string shapeFolderPath = "//Editor//Shapes//";
        public SimpleModelEditor()
        {
            shapeBuilder = new ProceduralShapeBuilder();
        }


        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            shapeBuilder.AddTriangle(a, b, c);
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

    }
}
