using Microsoft.Xna.Framework;
using MonoGameEngineCore.Procedural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemWar.Shapes
{
    public class SystemWarShapes
    {

        public static ProceduralShape CockpitBar()
        {
            ProceduralShapeBuilder builder = new ProceduralShapeBuilder();

            //bottom bar
            builder.AddFace(Vector3.Left, Vector3.Left + new Vector3(0, 0.1f, 0), Vector3.Right + new Vector3(0, 0.1f, 0), Vector3.Right);
            builder.AddFace(Vector3.Left * 2 + Vector3.Backward,
                Vector3.Left * 2 + Vector3.Backward + new Vector3(0, 0.1f, 0),
                Vector3.Left + new Vector3(0, 0.1f, 0),
                Vector3.Left);
            builder.AddFace(Vector3.Right,
                Vector3.Right + new Vector3(0, 0.1f, 0),
                Vector3.Right * 2 + Vector3.Backward + new Vector3(0, 0.1f, 0),
                Vector3.Right * 2 + Vector3.Backward);



            return builder.BakeShape();
        }

        public static ProceduralShape CockpitPanel()
        {
            ProceduralShapeBuilder builder = new ProceduralShapeBuilder();
            builder.AddBevelledSquareFace(Vector3.Left + Vector3.Up, Vector3.Right + Vector3.Up, Vector3.Left + Vector3.Down, Vector3.Right + Vector3.Down, 0.1f, 0.1f);
            return builder.BakeShape();
        }
    }
}
