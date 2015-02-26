using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.PositionUpdating;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.Procedural
{
    public class Edge
    {
        public Vector3 a;
        public Vector3 b;
        public override bool Equals(Object obj)
        {
            Edge other = obj as Edge;
            if (other.a == a)
                if (other.b == b)
                    return true;
            if (other.b == a)
                if (other.a == b)
                    return true;

            return false;
        }
    }

    public class ProceduralShapeBuilder
    {
        private List<VertexPositionColorTextureNormal> vertices;
        private List<short> indices;
        private int primCount;
        private short currentIndex = 0;

        public ProceduralShapeBuilder()
        {
            vertices = new List<VertexPositionColorTextureNormal>();
            indices = new List<short>();
        }

        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Plane p = new Plane(a, b, c);

            VertexPositionColorTextureNormal v1 = new VertexPositionColorTextureNormal();
            v1.Position = a;
            v1.Normal = p.Normal;
            v1.Color = Color.White;
            v1.Texture = new Vector2(0, 0);

            VertexPositionColorTextureNormal v2 = new VertexPositionColorTextureNormal();
            v2.Position = b;
            v2.Normal = p.Normal;
            v2.Color = Color.White;
            v2.Texture = new Vector2(0, 0);


            VertexPositionColorTextureNormal v3 = new VertexPositionColorTextureNormal();
            v3.Position = c;
            v3.Normal = p.Normal;
            v3.Color = Color.White;
            v3.Texture = new Vector2(0, 0);

            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);

            indices.Add(currentIndex);
            currentIndex++;
            indices.Add(currentIndex);
            currentIndex++;
            indices.Add(currentIndex);
            currentIndex++;

            primCount++;

        }

        public void AddSquareFace(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight)
        {
            AddTriangle(topLeft, topRight, bottomLeft);
            AddTriangle(bottomLeft, topRight, bottomRight);
        }

        public void AddBevelledSquareFace(Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft, Vector3 bottomRight,
           float bevelDepth, float bevelSize)
        {

            Vector3 midPoint = (topLeft + topRight + bottomLeft + bottomRight) / 4;
            Plane p = new Plane(topLeft, topRight, bottomLeft);
            Vector3 bevelMidPoint = midPoint + (p.Normal * bevelDepth);


            Vector3 bevelTopLeft = GetBevelPoint(topLeft, bevelMidPoint, bevelSize, bevelDepth, p);
            Vector3 bevelTopRight = GetBevelPoint(topRight, bevelMidPoint, bevelSize, bevelDepth, p);
            Vector3 bevelbottomLeft = GetBevelPoint(bottomLeft, bevelMidPoint, bevelSize, bevelDepth, p);
            Vector3 bevelBottomRight = GetBevelPoint(bottomRight, bevelMidPoint, bevelSize, bevelDepth, p);

            //inner panel
            AddSquareFace(bevelTopLeft, bevelTopRight, bevelbottomLeft, bevelBottomRight);


            //bevel panels
            AddSquareFace(topLeft, topRight, bevelTopLeft, bevelTopRight);
            AddSquareFace(topRight, bottomRight, bevelTopRight, bevelBottomRight);
            AddSquareFace(bottomRight, bottomLeft, bevelBottomRight, bevelbottomLeft);
            AddSquareFace(bottomLeft, topLeft, bevelbottomLeft, bevelTopLeft);


        }


        /// <summary>
        /// Adds convex polygon. Expects clockwise entry.
        /// </summary>
        /// <param name="points"></param>
        public void AddFace(params Vector3[] points)
        {
            Vector3 midPoint = GetMidPoint(points);


            for (int i = 0; i < points.Length - 1; i++)
            {
                AddTriangle(midPoint, points[i], points[i + 1]);
            }
            AddTriangle(midPoint, points[points.Length - 1], points[0]);
        }

        private Vector3 GetMidPoint(Vector3[] points)
        {
            Vector3 midPoint = Vector3.Zero;
            for (int i = 0; i < points.Length; i++)
            {
                midPoint += points[i];
            }
            midPoint /= points.Length;
            return midPoint;
        }

        public void AddBevelledFace(float bevelSize, float bevelDepth, params Vector3[] points)
        {
            Vector3 midPoint = GetMidPoint(points);
            Plane p = new Plane(points[0], points[1], points[2]);
       
            Vector3 bevelMidPoint = midPoint + (p.Normal*bevelDepth);

            for (int i = 0; i < points.Length - 1; i++)
            {
                Vector3 bevelPoint = GetBevelPoint(points[i], bevelMidPoint, bevelSize, bevelDepth, p);
                Vector3 bevelNextPoint = GetBevelPoint(points[i + 1], bevelMidPoint, bevelSize, bevelDepth, p);

                //middle panel
                AddTriangle(bevelMidPoint, bevelPoint, bevelNextPoint);

                //bevel panel tris                
                AddTriangle(bevelPoint,points[i],points[i+1]);
                AddTriangle(bevelPoint, points[i + 1], bevelNextPoint);



            }


            AddTriangle(bevelMidPoint, GetBevelPoint(points[points.Length - 1], bevelMidPoint, bevelSize, bevelDepth, p),
                GetBevelPoint(points[0], bevelMidPoint, bevelSize, bevelDepth, p));


            AddTriangle(points[points.Length - 1], points[0], GetBevelPoint(points[0], bevelMidPoint, bevelSize, bevelDepth, p));

            AddTriangle(points[points.Length - 1], GetBevelPoint(points[0], bevelMidPoint, bevelSize, bevelDepth, p),
                GetBevelPoint(points[points.Length - 1], bevelMidPoint, bevelSize, bevelDepth, p));


        }

        private Vector3 GetBevelPoint(Vector3 originalPoint, Vector3 bevelMidPoint, float bevelSize, float bevelDepth, Plane p)
        {
            Vector3 translatedOriginalPoint = originalPoint + (p.Normal * bevelDepth);
            Vector3 bevelPoint = translatedOriginalPoint + ((bevelMidPoint - translatedOriginalPoint) * bevelSize);
            return bevelPoint;

        }

        public ProceduralShape BakeShape()
        {
            ProceduralShape shape = new ProceduralShape();
            shape.Vertices = vertices.ToArray();
            shape.Indices = indices.ToArray();
            shape.PrimitiveCount = primCount;

            shape.FlipNormals();

            return shape;
        }
    }

    public class ProceduralShape
    {

        public VertexPositionColorTextureNormal[] Vertices { get; set; }
        public short[] Indices { get; set; }

        public ProceduralShape()
        {

        }

        public ProceduralShape(VertexPositionColorTextureNormal[] vertices, short[] indices)
        {
            Vertices = new VertexPositionColorTextureNormal[vertices.Length];
            Indices = new short[indices.Length];
            Array.Copy(vertices, Vertices, vertices.Length);
            Array.Copy(indices, Indices, indices.Length);
            PrimitiveCount = Indices.Count()/3;
        }


        public int PrimitiveCount { get; set; }

        public void Translate(Vector3 translate)
        {
            for (int i = 0; i < Vertices.Length; i++)
                Vertices[i].Position += translate;
        }

        public void Transform(Matrix transform)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position = Vector3.Transform(Vertices[i].Position, transform);
                Vertices[i].Normal = Vector3.Transform(Vertices[i].Normal, transform);
            }
        }

        /// <summary>
        /// Performs traditional "box" uv mapping based on the vertex position and normal.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public Vector2 GenerateBoxMap(Vector3 position, Vector3 normal)
        {
            if (Math.Abs(normal.X) > Math.Abs(normal.Y))
            {
                if (Math.Abs(normal.X) > Math.Abs(normal.Z))
                    return new Vector2(position.Y, position.Z);//X
                else
                    return new Vector2(position.X, position.Y);//Z
            }
            else
            {
                if (Math.Abs(normal.Y) > Math.Abs(normal.Z))
                    return new Vector2(position.X, position.Z);//Y
                else
                    return new Vector2(position.X, position.Y);//Z
            }
        }

        public ProceduralShape Clone()
        {
            ProceduralShape clone = new ProceduralShape();
            clone.Indices = new short[Indices.Length];
            clone.Vertices = new VertexPositionColorTextureNormal[Vertices.Length];
            Indices.CopyTo(clone.Indices, 0);
            Vertices.CopyTo(clone.Vertices, 0);
            clone.PrimitiveCount = this.PrimitiveCount;
            return clone;
        }

        internal void FlipNormals()
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Normal = -Vertices[i].Normal;
            }
        }

        internal void ReverseVertices()
        {
            Vertices = Vertices.Reverse().ToArray();
        }

        internal void ReverseIndices()
        {
            Indices = Indices.Reverse().ToArray();
        }

        public List<Vector3> GetVertices()
        {
         
            var vertices = new List<Vector3>();
            for (int i = 0; i < Vertices.Length; i++)
            {
                vertices.Add(Vertices[i].Position);
            }

            return vertices;
        } 

        public void InsideOut()
        {
            Indices = Indices.Reverse().ToArray();
            FlipNormals();
        }

        public List<Edge> GetEdges()
        {
            List<Edge> edges = new List<Edge>();

            for (int i = 0; i < Indices.Length - 3; i += 3)
            {
                Vector3 a = Vertices[Indices[i]].Position;
                Vector3 b = Vertices[Indices[i + 1]].Position;
                Edge e = new Edge();
                e.a = a;
                e.b = b;
                edges.Add(e);

                a = Vertices[Indices[i + 1]].Position;
                b = Vertices[Indices[i + 2]].Position;
                e = new Edge();
                e.a = a;
                e.b = b;
                edges.Add(e);

                a = Vertices[Indices[i]].Position;
                b = Vertices[Indices[i + 2]].Position;
                e = new Edge();
                e.a = a;
                e.b = b;
                edges.Add(e);
            }


            Vector3 afinal = Vertices[Indices[Indices.Length - 3]].Position;
            Vector3 bfinal = Vertices[Indices[Indices.Length - 2]].Position;
            Edge efinal = new Edge();
            efinal.a = afinal;
            efinal.b = bfinal;
            edges.Add(efinal);

            afinal = Vertices[Indices[Indices.Length - 2]].Position;
            bfinal = Vertices[Indices[Indices.Length - 1]].Position;
            efinal = new Edge();
            efinal.a = afinal;
            efinal.b = bfinal;
            edges.Add(efinal);

            afinal = Vertices[Indices[Indices.Length - 3]].Position;
            bfinal = Vertices[Indices[Indices.Length - 1]].Position;
            efinal = new Edge();
            efinal.a = afinal;
            efinal.b = bfinal;
            edges.Add(efinal);
            edges.Add(efinal);

            return edges;
        }

        public List<Edge> GetExternalEdges()
        {
            List<Edge> allEdges = GetEdges();

            List<Edge> shared = new List<Edge>();
            foreach (Edge e in allEdges)
            {

                foreach (Edge f in allEdges)
                {
                    if (e == f)
                        continue;
                    if (e.Equals(f))
                    {
                        shared.Add(e);
                    }
                }
            }

            List<Edge> toRemove = new List<Edge>();
            foreach (Edge e in allEdges)
            {
                foreach (Edge dupe in shared)
                    if (e.Equals(dupe))
                        toRemove.Add(e);
            }

            foreach (Edge e in toRemove)
                allEdges.Remove(e);

            return allEdges.Distinct().ToList();
        }

        public Vector3 GetMidPoint()
        {
            Vector3 avg = Vector3.Zero;

            foreach (VertexPositionColorTextureNormal v in Vertices)
            {
                avg += v.Position;
            }
            avg /= Vertices.Length;
            return avg;
        }

        internal List<Vector3> GetEdgeVertices()
        {
            var edges = GetExternalEdges();

            var vectors = new List<Vector3>();

            for (int i = 0; i < edges.Count; i++)
                vectors.Add(edges[i].a);

            return vectors;

        }

        public void Scale(float scaleFactor)
        {
            Vector3 midPoint = GetMidPoint();

            //translate each point by the midpoint, scale, then re-translate
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Position -= midPoint;
                Vertices[i].Position *= scaleFactor;
                Vertices[i].Position += midPoint;
            }

        }

        public void RegenerateNormals()
        {

            //If you have three vertices, V1, V2 and V3, ordered in counterclockwise order, you can obtain the direction of the normal by computing
            //(V2 - V1) x (V3 - V1), where x is the cross product of the two vectors.

            for (int i = 0; i < Indices.Length - 3; i += 3)
            {
                VertexPositionColorTextureNormal v1 = Vertices[i];
                VertexPositionColorTextureNormal v2 = Vertices[i + 1];
                VertexPositionColorTextureNormal v3 = Vertices[i + 2];

                Vector3 v1v2 = v2.Position - v1.Position;
                Vector3 v1v3 = v3.Position - v1.Position;

                // Vector3 triMidPoint = (v1.Position + v2.Position + v3.Position) / 3;

                Vector3 potentialNormal = Vector3.Cross(v1v2, v1v3);
                potentialNormal.Normalize();
                Vertices[i].Normal = potentialNormal;
                Vertices[i + 1].Normal = potentialNormal;
                Vertices[i + 2].Normal = potentialNormal;


            }
        }

        public void SetColor(Color col)
        {
            for (int i = 0; i < Vertices.Length; i++)
            {
                Vertices[i].Color = col;
            }
        }

        public static ProceduralShape Combine(params ProceduralShape[] shapes)
        {
            ProceduralShape newShape = new ProceduralShape();

            newShape.Vertices = new VertexPositionColorTextureNormal[shapes.Sum(x => x.Vertices.Length)];
            newShape.Indices = new short[shapes.Sum(x => x.Indices.Length)];



            int VertexOffset = 0;
            int IndexOffset = 0;

            foreach (ProceduralShape s in shapes)
            {
                for (int v = 0; v < s.Vertices.Length; v++)
                {
                    newShape.Vertices[VertexOffset + v] = s.Vertices[v];
                }

                for (int i = 0; i < s.Indices.Length; i++)
                {
                    newShape.Indices[IndexOffset + i] = (short)(s.Indices[i] + VertexOffset);
                }

                VertexOffset += s.Vertices.Length;
                IndexOffset += s.Indices.Length;
            }

            newShape.PrimitiveCount = shapes.Sum(x => x.PrimitiveCount);
            return newShape;
        }

        public static ProceduralShape Mirror(ProceduralShape shapeToMirror, Plane mirrorAxis)
        {
            ProceduralShape newShape = shapeToMirror.Clone();
           

           for(int i = 0;i<shapeToMirror.Vertices.Length;i++)
           {
               //newShape.Vertices[i].Normal = -shapeToMirror.Vertices[i].Normal;
               newShape.Vertices[i].Position = MonoMathHelper.MirrorInPlane(shapeToMirror.Vertices[i].Position,
                   mirrorAxis);
           }

            newShape.ReverseIndices();
            newShape.FlipNormals();
            return newShape;
        }

    }

    public class ProceduralCuboid : ProceduralShape
    {
        // Used to procedurally create box meshes.
        private Vector3[] _BoxCorners;

        // Used to procedurally create box meshes.
        static private int[] _BoxIndices = {
            1, 0, 3, 2, 4, 5, 6, 7,
            0, 4, 2, 6, 5, 1, 7, 3,
            0, 1, 4, 5, 3, 2, 7, 6,
        };

        /// <summary>
        /// Creates a new BoxInstanceSourceData instance.
        /// </summary>
        public ProceduralCuboid(float width, float depth, float height)
        {

            _BoxCorners = new Vector3[]{
            new Vector3(width, height, depth), new Vector3(-width, height, depth),
            new Vector3(width, -height, depth), new Vector3(-width, -height, depth),
            new Vector3(width, height, -depth), new Vector3(-width, height, -depth),
            new Vector3(width, -height, -depth), new Vector3(-width, -height, -depth),
        };
            // Create the box data buffers.
            Indices = new short[36];
            Vertices = new VertexPositionColorTextureNormal[24];

            // Create and fill the box data.
            int vertindex = 0;
            int indexoffset = 0;

            for (int f = 0; f < 6; f++)
            {
                // Get the static box builder data.
                Vector3 vert0 = _BoxCorners[_BoxIndices[vertindex]];
                Vector3 vert1 = _BoxCorners[_BoxIndices[vertindex + 1]];
                Vector3 vert2 = _BoxCorners[_BoxIndices[vertindex + 2]];
                Vector3 vert3 = _BoxCorners[_BoxIndices[vertindex + 3]];

                // Create and set the vertex positions, normals, uvs, and indices.
                Vertices[vertindex].Position = vert0;
                Vertices[vertindex + 1].Position = vert1;
                Vertices[vertindex + 2].Position = vert2;
                Vertices[vertindex + 3].Position = vert3;

                Plane plane = new Plane(vert0, vert2, vert1);

                Vertices[vertindex].Normal = plane.Normal;
                Vertices[vertindex + 1].Normal = plane.Normal;
                Vertices[vertindex + 2].Normal = plane.Normal;
                Vertices[vertindex + 3].Normal = plane.Normal;

                Vertices[vertindex].Texture = GenerateBoxMap(vert0, plane.Normal);
                Vertices[vertindex + 1].Texture = GenerateBoxMap(vert1, plane.Normal);
                Vertices[vertindex + 2].Texture = GenerateBoxMap(vert2, plane.Normal);
                Vertices[vertindex + 3].Texture = GenerateBoxMap(vert3, plane.Normal);

                Indices[indexoffset++] = (byte)(vertindex);
                Indices[indexoffset++] = (byte)(vertindex + 1);
                Indices[indexoffset++] = (byte)(vertindex + 2);
                Indices[indexoffset++] = (byte)(vertindex + 3);
                Indices[indexoffset++] = (byte)(vertindex + 2);
                Indices[indexoffset++] = (byte)(vertindex + 1);

                vertindex += 4;
            }


            //Indices = Indices.Reverse().ToArray();


            PrimitiveCount = 12;
        }


    }

    public class ProceduralCube : ProceduralShape
    {



        // Used to procedurally create box meshes.
        static private Vector3[] _BoxCorners = {
            new Vector3(0.5f, 0.5f, 0.5f), new Vector3(-0.5f, 0.5f, 0.5f),
            new Vector3(0.5f, -0.5f, 0.5f), new Vector3(-0.5f, -0.5f, 0.5f),
            new Vector3(0.5f, 0.5f, -0.5f), new Vector3(-0.5f, 0.5f, -0.5f),
            new Vector3(0.5f, -0.5f, -0.5f), new Vector3(-0.5f, -0.5f, -0.5f),
        };

        // Used to procedurally create box meshes.
        static private int[] _BoxIndices = {
            1, 0, 3, 2, 4, 5, 6, 7,
            0, 4, 2, 6, 5, 1, 7, 3,
            0, 1, 4, 5, 3, 2, 7, 6,
        };

        /// <summary>
        /// Creates a new BoxInstanceSourceData instance.
        /// </summary>
        public ProceduralCube()
        {
            // Create the box data buffers.
            Indices = new short[36];
            Vertices = new VertexPositionColorTextureNormal[24];

            // Create and fill the box data.
            int vertindex = 0;
            int indexoffset = 0;

            for (int f = 0; f < 6; f++)
            {
                // Get the static box builder data.
                Vector3 vert0 = _BoxCorners[_BoxIndices[vertindex]];
                Vector3 vert1 = _BoxCorners[_BoxIndices[vertindex + 1]];
                Vector3 vert2 = _BoxCorners[_BoxIndices[vertindex + 2]];
                Vector3 vert3 = _BoxCorners[_BoxIndices[vertindex + 3]];

                // Create and set the vertex positions, normals, uvs, and indices.
                Vertices[vertindex].Position = vert0;
                Vertices[vertindex + 1].Position = vert1;
                Vertices[vertindex + 2].Position = vert2;
                Vertices[vertindex + 3].Position = vert3;

                Plane plane = new Plane(vert0, vert2, vert1);

                Vertices[vertindex].Normal = plane.Normal;
                Vertices[vertindex + 1].Normal = plane.Normal;
                Vertices[vertindex + 2].Normal = plane.Normal;
                Vertices[vertindex + 3].Normal = plane.Normal;

                Vertices[vertindex].Texture = GenerateBoxMap(vert0, plane.Normal);
                Vertices[vertindex + 1].Texture = GenerateBoxMap(vert1, plane.Normal);
                Vertices[vertindex + 2].Texture = GenerateBoxMap(vert2, plane.Normal);
                Vertices[vertindex + 3].Texture = GenerateBoxMap(vert3, plane.Normal);

                Indices[indexoffset++] = (byte)(vertindex);
                Indices[indexoffset++] = (byte)(vertindex + 1);
                Indices[indexoffset++] = (byte)(vertindex + 2);
                Indices[indexoffset++] = (byte)(vertindex + 3);
                Indices[indexoffset++] = (byte)(vertindex + 2);
                Indices[indexoffset++] = (byte)(vertindex + 1);

                vertindex += 4;
            }





            PrimitiveCount = 12;
        }






    }

    public class ProceduralSphere : ProceduralShape
    {
        private readonly int slices;
        private readonly int stacks;



        public ProceduralSphere(int slices, int stacks)
            : base()
        {
            this.slices = slices;
            this.stacks = stacks;
            GenerateVertexArray();
            GenerateIndices();
        }

        private void GenerateVertexArray()
        {
            Vertices = new VertexPositionColorTextureNormal[(slices + 1) * (stacks + 1)];
            int currentIndex = 0;
            for (int slice = 0; slice <= slices; ++slice)
            {
                for (int stack = 0; stack <= stacks; ++stack)
                {
                    VertexPositionColorTextureNormal v = new VertexPositionColorTextureNormal();
                    v.Position.Y = (float)System.Math.Sin(((double)stack / stacks - 0.5) * System.Math.PI);
                    if (v.Position.Y < -1) v.Position.Y = -1;
                    else if (v.Position.Y > 1) v.Position.Y = 1;
                    float l = (float)System.Math.Sqrt(1 - v.Position.Y * v.Position.Y);
                    v.Position.X = l * (float)System.Math.Sin((double)slice / slices * System.Math.PI * 2);
                    v.Position.Z = l * (float)System.Math.Cos((double)slice / slices * System.Math.PI * 2);
                    v.Normal = v.Position;
                    v.Texture = new Vector2((float)slice / slices, (float)stack / stacks);
                    Vertices[currentIndex] = v;
                    currentIndex++;
                }

            }


        }

        public void GenerateIndices()
        {
            Indices = new short[slices * stacks * 6];

            int six = 0;
            for (int slice = 0; slice < slices; ++slice)
            {
                for (short stack = 0; stack < stacks; ++stack)
                {
                    short v = (short)(slice * (stacks + 1) + stack);
                    Indices[six++] = v;
                    Indices[six++] = (short)(v + 1);
                    Indices[six++] = (short)(v + (stacks + 1));
                    Indices[six++] = (short)(v + (stacks + 1));
                    Indices[six++] = (short)(v + 1);
                    Indices[six++] = (short)(v + (stacks + 1) + 1);
                }
            }
            PrimitiveCount = Indices.Length / 3;

        }


    }

    public class ProceduralPlane : ProceduralShape
    {


        public ProceduralPlane()
        {

            // Create the box data buffers.
            Indices = new short[6];
            Vertices = new VertexPositionColorTextureNormal[4];

            Vertices[0].Position = new Vector3(-0.5f, 0, -0.5f);
            Vertices[1].Position = new Vector3(-0.5f, 0, 0.5f);
            Vertices[2].Position = new Vector3(0.5f, 0, 0.5f);
            Vertices[3].Position = new Vector3(0.5f, 0, -0.5f);

            Vertices[0].Normal = Vector3.Up;
            Vertices[1].Normal = Vector3.Up;
            Vertices[2].Normal = Vector3.Up;
            Vertices[3].Normal = Vector3.Up;

            Vertices[0].Texture = GenerateBoxMap(Vertices[0].Position, Vertices[0].Normal);
            Vertices[1].Texture = GenerateBoxMap(Vertices[1].Position, Vertices[1].Normal);
            Vertices[2].Texture = GenerateBoxMap(Vertices[2].Position, Vertices[2].Normal);
            Vertices[3].Texture = GenerateBoxMap(Vertices[3].Position, Vertices[3].Normal);

            Indices[0] = (byte)(0);
            Indices[1] = (byte)(3);
            Indices[2] = (byte)(1);
            Indices[3] = (byte)(1);
            Indices[4] = (byte)(3);
            Indices[5] = (byte)(2);




            PrimitiveCount = 2;
        }

    }

    public class ProceduralDiamond : ProceduralShape
    {
        public ProceduralDiamond()
        {
            //simple diamond (from center)
            float height = 0.35f;
            float width = 0.75f;


            //four points temp store
            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(new Vector3(height, 0f, 0f));
            vertices.Add(new Vector3(0f, 0f, width));
            vertices.Add(new Vector3(-height, 0f, 0f));
            vertices.Add(new Vector3(0f, 0f, -width));

            // Create the box data buffers.
            Indices = new short[6];
            Vertices = new VertexPositionColorTextureNormal[4];

            //triangle vertices

            for (int i = 0; i < vertices.Count; i++)
            {
                Vertices[i].Position = vertices[i];
                Vertices[i].Normal = Vector3.Up;
                Vertices[i].Texture = GenerateBoxMap(Vertices[i].Position, Vertices[i].Normal);
            }


            Indices[0] = (byte)(0);
            Indices[1] = (byte)(3);
            Indices[2] = (byte)(1);
            Indices[3] = (byte)(1);
            Indices[4] = (byte)(3);
            Indices[5] = (byte)(2);

            Indices = Indices.Reverse().ToArray();




            PrimitiveCount = 2;
        }
    }

    public class ProceduralTriangle : ProceduralShape
    {
        public ProceduralTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 normal, Vector4 col)
        {
            Indices = new short[3];
            Vertices = new VertexPositionColorTextureNormal[3];

            Indices[0] = 0;
            Indices[1] = 1;
            Indices[2] = 2;

            Vertices[0].Position = a;
            Vertices[0].Normal = normal;
            Vertices[0].Texture = GenerateBoxMap(Vertices[0].Position, normal);


            Vertices[1].Position = b;
            Vertices[1].Normal = normal;
            Vertices[1].Texture = GenerateBoxMap(Vertices[1].Position, normal);

            Vertices[2].Position = c;
            Vertices[2].Normal = normal;
            Vertices[2].Texture = GenerateBoxMap(Vertices[0].Position, normal);



            PrimitiveCount = 1;
        }

    }

    public class ProceduralCylinder : ProceduralShape
    {


        public ProceduralCylinder(float bottomRadius, float topRadius, float length, int slices, int stacks)
        {



            float sliceStep = MathHelper.TwoPi / slices;
            float heightStep = length / stacks;
            float radiusStep = (topRadius - bottomRadius) / stacks;
            float currentHeight = -length / 2;
            int vertexCount = (stacks + 1) * slices + 2;   //cone = stacks * slices + 1
            int triangleCount = (stacks + 1) * slices * 2; //cone = stacks * slices * 2 + slices
            int indexCount = triangleCount * 3;
            float currentRadius = bottomRadius;

            Vertices = new VertexPositionColorTextureNormal[vertexCount];

            // Start at the bottom of the cylinder            
            int currentVertex = 0;
            Vertices[currentVertex++] = new VertexPositionColorTextureNormal(new Vector3(0, currentHeight, 0), Color.White, Vector2.Zero, Vector3.Down);
            for (int i = 0; i <= stacks; i++)
            {
                float sliceAngle = 0;
                for (int j = 0; j < slices; j++)
                {
                    float x = currentRadius * (float)Math.Cos(sliceAngle);
                    float y = currentHeight;
                    float z = currentRadius * (float)Math.Sin(sliceAngle);

                    Vector3 position = new Vector3(x, y, z);
                    Vertices[currentVertex++] = new VertexPositionColorTextureNormal(position, Color.White, Vector2.Zero, Vector3.Normalize(position));

                    sliceAngle += sliceStep;
                }
                currentHeight += heightStep;
                currentRadius += radiusStep;
            }
            Vertices[currentVertex++] = new VertexPositionColorTextureNormal(new Vector3(0, length / 2, 0), Color.White, Vector2.Zero, Vector3.Up);

            // Create the actual vertex buffer object

            CreateIndexBuffer(vertexCount, indexCount, slices);


        }

        /// <summary>
        /// Creates an <see cref="IndexBuffer"/> for spherical shapes like Spheres, Cylinders, and Cones.
        /// </summary>
        /// <param name="device">The <see cref="GraphicsDevice"/> that is associated with the created cylinder.</param>
        /// <param name="vertexCount">The total number of vertices making up the shape.</param>
        /// <param name="indexCount">The total number of indices making up the shape.</param>
        /// <param name="slices">The number of slices about the Y axis.</param>
        /// <returns>The index buffer containing the index data for the shape.</returns>
        private void CreateIndexBuffer(int vertexCount, int indexCount, int slices)
        {
            Indices = new short[indexCount];
            short currentIndex = 0;

            // Bottom circle/cone of shape
            for (short i = 1; i <= slices; i++)
            {
                Indices[currentIndex++] = 0;
                Indices[currentIndex++] = i;
                if (i - 1 == 0)
                    Indices[currentIndex++] = (short)(i + (short)slices - 1);
                else
                    Indices[currentIndex++] = (short)(i - 1);
            }

            // Middle sides of shape
            for (short i = 1; i < vertexCount - (short)slices - 1; i++)
            {
                Indices[currentIndex++] = i;
                Indices[currentIndex++] = (short)(i + (short)slices);
                if ((i - 1) % slices == 0)
                    Indices[currentIndex++] = (short)(i + (short)slices + (short)slices - 1);
                else
                    Indices[currentIndex++] = (short)(i + (short)slices - 1);

                if ((i - 1) % slices == 0)
                    Indices[currentIndex++] = (short)(i + (short)slices - 1);
                else
                    Indices[currentIndex++] = (short)(i - 1);
                Indices[currentIndex++] = i;
                if ((i - 1) % slices == 0)
                    Indices[currentIndex++] = (short)(i + (short)slices + (short)slices - 1);
                else
                    Indices[currentIndex++] = (short)(i + (short)slices - 1);
            }

            // Top circle/cone of shape
            for (short i = (short)(vertexCount - (short)slices - 1); i < vertexCount - 1; i++)
            {
                Indices[currentIndex++] = (short)(vertexCount - 1);
                if ((i - 1) % slices == 0)
                    Indices[currentIndex++] = (short)(i + (short)slices - 1);
                else
                    Indices[currentIndex++] = (short)(i - 1);
                Indices[currentIndex++] = i;
            }

            PrimitiveCount = Indices.Length / 3;

        }
    }

}
