using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Helper;
using ConversionHelper;

namespace MonoGameEngineCore.Procedural
{

    public class PlanetQuadTreeNode : GameObject.GameObject
    {
     
       

        readonly int depth;
        private const int maximumDepth = 8;
        public Planet Planet { get; set; }
        public Vector3 normal { get; set; }
        public Vector3 max { get; set; }
        public Vector3 min { get; set; }
        public float step { get; set; } 
        public VertexPositionColorTextureNormal[] vertices;
        public short[] indices;
        public int heightMapSize;
       
        private Effect effect;
        private float sphereSize;
        public BoundingSphere boundingSphere;
        private IModule module;
        public Color NodeColor { get; set; }
        public int quadTreeNodeID;
        private readonly int rootNodeId;

        Vector3 se, sw, mid1, mid2, nw, ne, midBottom, midRight, midLeft, midTop;

        public PlanetQuadTreeNode(Effect effect, IModule module, Planet rootObject, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize)
        {


            this.effect = effect;
            this.module = module;
            this.Planet = rootObject;
            this.sphereSize = sphereSize;
            this.min = min;
            this.max = max;
            this.step = step;
            this.normal = normal;
            heightMapSize = System.Math.Max((int)((max.X - min.X) / step), (int)((max.Z - min.Z) / step)); ;
            NodeColor = SystemCore.ActiveColorScheme.Color1;

      
            CalculatePatchBoundaries(out se, out sw, out mid1, out mid2, out nw, out ne, out midBottom, out midRight, out midLeft, out midTop);

            
        }

        public void BuildGeometry()
        {

            vertices = new VertexPositionColorTextureNormal[(heightMapSize * heightMapSize)];

            int vertIndex = 0;
           

            for (float i = 0; i < heightMapSize; i++)
            {
                for (float j = 0; j < heightMapSize; j++)
                {
                    var vert = new VertexPositionColorTextureNormal();

                    vert.Position = CalculateVertexPosition(i, j);
                    vert.Texture = new Vector2(i * 2f / heightMapSize, j * 2f / heightMapSize);
                    vert.Normal = normal;
                    vert.Color = NodeColor;
                    vertices[vertIndex] = vert;
                    vertIndex++;
                }
            }

            GenerateIndices();

            if (normal == Vector3.Down || normal == Vector3.Backward || normal == Vector3.Right)
                indices = indices.Reverse().ToArray();



            indices = indices.Reverse().ToArray();


            Sphereify(sphereSize);

            GenerateNormals(ref vertices);


            var p = vertices.Select(x => x.Position).ToList();
            boundingSphere = BoundingSphere.CreateFromPoints(p);

            ProceduralShape spherePatch = new ProceduralShape(vertices, indices);


            this.AddComponent(new RenderGeometryComponent(spherePatch));
            this.AddComponent(new EffectRenderComponent(effect));
           

            SetHighPrecisionPosition(this);

           
        }

        private void SetHighPrecisionPosition(GameObject.GameObject obj)
        {
            var highPrecision = new HighPrecisionPosition();
            highPrecision.Position = Planet.GetComponent<HighPrecisionPosition>().Position;
            obj.AddComponent(highPrecision);
        }

        private Vector3 CalculateVertexPosition(float i, float j)
        {
            var pos = Vector3.Zero;

            if (normal == Vector3.Up || normal == Vector3.Down)
                pos = new Vector3(i * step + min.X, min.Y, j * step + min.Z);

            if (normal == Vector3.Left || normal == Vector3.Right)
                pos = new Vector3(min.X, j * step + min.Y, i * step + min.Z);

            if (normal == Vector3.Forward || normal == Vector3.Backward)
                pos = new Vector3(i * step + min.X, j * step + min.Y, min.Z);


            return pos;
        }

        public void GenerateIndices()
        {

            // Construct the index array.
            indices = new short[(heightMapSize - 1) * (heightMapSize - 1) * 6];    // 2 triangles per grid square x 3 vertices per triangle

            int indicesIndex = 0;
            for (int y = 0; y < heightMapSize - 1; ++y)
            {
                for (int x = 0; x < heightMapSize - 1; ++x)
                {
                    int start = y * heightMapSize + x;
                    indices[indicesIndex++] = (short)start;
                    indices[indicesIndex++] = (short)(start + 1);
                    indices[indicesIndex++] = (short)(start + heightMapSize);
                    indices[indicesIndex++] = (short)(start + 1);
                    indices[indicesIndex++] = (short)(start + 1 + heightMapSize);
                    indices[indicesIndex++] = (short)(start + heightMapSize);
                }
            }
        }

        private void GenerateNormals(ref VertexPositionColorTextureNormal[] vertArray)
        {

            for (int i = 0; i < vertArray.Length; i++)
                vertArray[i].Normal = new Vector3(0, 0, 0);

            for (int i = 0; i < indices.Length / 3; i++)
            {
                Vector3 firstvec = vertArray[indices[i * 3 + 1]].Position - vertArray[indices[i * 3]].Position;
                Vector3 secondvec = vertArray[indices[i * 3]].Position - vertArray[indices[i * 3 + 2]].Position;
                Vector3 normal = Vector3.Cross(firstvec, secondvec);
                normal.Normalize();
                vertArray[indices[i * 3]].Normal += normal;
                vertArray[indices[i * 3 + 1]].Normal += normal;
                vertArray[indices[i * 3 + 2]].Normal += normal;
            }

            for (int i = 0; i < vertArray.Length; i++)
            {
                vertArray[i].Normal.Normalize();
                vertArray[i].Normal = -vertArray[i].Normal;
            }
        }
     
        private void CalculatePatchBoundaries(out Vector3 se, out Vector3 sw, out Vector3 mid1, out Vector3 mid2, out Vector3 nw, out Vector3 ne, out Vector3 midBottom, out Vector3 midRight, out Vector3 midLeft, out Vector3 midTop)
        {
            if (normal == Vector3.Forward || normal == Vector3.Backward)
            {

                se = min;
                sw = new Vector3(max.X, min.Y, min.Z);
                ne = new Vector3(min.X, max.Y, min.Z);
                nw = new Vector3(max.X - step / 2, max.Y - step / 2, min.Z);
                mid1 = (min + max) / 2;
                mid1.Z = min.Z;
                mid2 = ((min + max) / 2);
                mid2 -= new Vector3(step / 2, step / 2, 0);
                mid2.Z = min.Z;

                midBottom = new Vector3(((min.X + max.X) / 2) - step / 2, min.Y, min.Z);
                midRight = new Vector3(min.X, ((min.Y + max.Y) / 2) - step / 2, min.Z);
                midLeft = new Vector3(max.X - step / 2, ((min.Y + max.Y) / 2), min.Z);
                midTop = new Vector3(((min.X + max.X) / 2), max.Y - step / 2, min.Z);
                return;
            }

            if (normal == Vector3.Right || normal == Vector3.Left)
            {

                se = min;
                sw = new Vector3(max.X, min.Y, max.Z);
                ne = new Vector3(max.X, max.Y, min.Z);
                nw = new Vector3(max.X, max.Y - step / 2, max.Z - step / 2);
                mid1 = (min + max) / 2;
                mid2 = ((min + max) / 2);
                mid2 -= new Vector3(0, step / 2, step / 2);
                mid1.X = max.X;
                mid2.X = max.X;
                midBottom = new Vector3(max.X, min.Y, ((min.Z + max.Z) / 2) - step / 2);
                midRight = new Vector3(max.X, ((min.Y + max.Y) / 2) - step / 2, min.Z);
                midLeft = new Vector3(max.X, ((min.Y + max.Y) / 2), max.Z - step / 2);
                midTop = new Vector3(max.X, max.Y - step / 2, ((min.Z + max.Z) / 2));
                return;
            }

            if (normal == Vector3.Up || normal == Vector3.Down)
            {
                se = min;
                sw = new Vector3(max.X, min.Y, min.Z);
                ne = new Vector3(min.X, min.Y, max.Z);
                nw = new Vector3(max.X - step / 2, min.Y, max.Z - step / 2);
                mid1 = (min + max) / 2;
                mid2 = ((min + max) / 2);
                mid2 -= new Vector3(step / 2, 0, step / 2);
                mid1.Y = min.Y;
                mid2.Y = min.Y;
                midBottom = new Vector3(((min.X + max.X) / 2) - step / 2, min.Y, min.Z);
                midRight = new Vector3(min.X, min.Y, ((min.Z + max.Z) / 2) - step / 2);
                midLeft = new Vector3(max.X - step / 2, min.Y, ((min.Z + max.Z) / 2));
                midTop = new Vector3(((min.X + max.X) / 2), min.Y, max.Z - step / 2);
                return;
            }

            throw new Exception();
        }

        private void Split()
        {
            //if (depth == maximumDepth)
            //    return;

            //SetState(PatchState.splitting);

            //Children.Clear();

            ////need to add 4 new quadtree nodes, and do neighbours bookkeeping.

            ////bottom right, inherits east and south neighbours
            //PlanetQuadTreeNode a = new PlanetQuadTreeNode(rootNodeId, 1, Planet, this, se, mid1, step / 2, normal, sphereSize);
            //a.QueueGeometryGeneration(effect, module);

            ////top left, inherits north and west
            //PlanetQuadTreeNode b = new PlanetQuadTreeNode(rootNodeId, 2, Planet, this, mid2, nw, step / 2, normal, sphereSize);
            //b.QueueGeometryGeneration(effect, module);

            ////bottom left, inherits south and west
            //PlanetQuadTreeNode c = new PlanetQuadTreeNode(rootNodeId, 3, Planet, this, midBottom, midLeft, step / 2, normal, sphereSize);
            //c.QueueGeometryGeneration(effect, module);

            ////top right inherits north and east
            //PlanetQuadTreeNode d = new PlanetQuadTreeNode(rootNodeId, 4, Planet, this, midRight, midTop, step / 2, normal, sphereSize);
            //d.QueueGeometryGeneration(effect, module);

            ////bottom right
            //a.AddNeighbour(Direction.west, c);
            //a.AddNeighbour(Direction.north, d);
            //a.AddNeighbour(Direction.east, neighbours[Direction.east]);
            //a.AddNeighbour(Direction.south, neighbours[Direction.south]);

            ////top left
            //b.AddNeighbour(Direction.south, c);
            //b.AddNeighbour(Direction.east, d);
            //b.AddNeighbour(Direction.north, neighbours[Direction.north]);
            //b.AddNeighbour(Direction.west, neighbours[Direction.west]);

            ////bottom left
            //c.AddNeighbour(Direction.north, b);
            //c.AddNeighbour(Direction.east, a);
            //c.AddNeighbour(Direction.south, neighbours[Direction.south]);
            //c.AddNeighbour(Direction.west, neighbours[Direction.west]);


            ////top right
            //d.AddNeighbour(Direction.south, a);
            //d.AddNeighbour(Direction.west, b);
            //d.AddNeighbour(Direction.north, neighbours[Direction.north]);
            //d.AddNeighbour(Direction.east, neighbours[Direction.east]);


            //if (Children.Count > 0)
            //    throw new Exception("Um...");

            //Children.Add(a);
            //Children.Add(b);
            //Children.Add(c);
            //Children.Add(d);

            //foreach (PlanetQuadTreeNode n in Children)
            //{
            //    n.Planet = Planet;
            //}

            //isLeaf = false;
        }

        private void Sphereify(float radius)
        {
            Color randomColor = RandomHelper.RandomColor;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = (Vector3.Normalize(vertices[i].Position)) * radius;
                double height = module.GetValue(vertices[i].Position.X, vertices[i].Position.Y, vertices[i].Position.Z);


                vertices[i].Position += (Vector3.Normalize(vertices[i].Position) * ((float)height));


                float length = vertices[i].Position.Length();
                if (length < radius)
                    vertices[i].Color = Planet.SeaColor;
                if (length >= radius && length <= radius + 5)
                    vertices[i].Color = Planet.LandColor;
                if (length > radius + 5)
                    vertices[i].Color = Planet.MountainColor;

                if (Planet.visualisePatches)
                {
                    vertices[i].Color = randomColor;
                }

            }
        }

        private float CalculateDistanceToPatch()
        {
            Vector3 surfacePoint = GetSurfaceMidPoint();
            return surfacePoint.Length();
        }

        public Vector3 GetSurfaceMidPoint()
        {
            return Vector3.Transform(Vector3.Normalize(mid1) * sphereSize, Planet.Transform.WorldMatrix);
        }

        public int CountVertsInTree()
        {
            int verts = vertices.Length;
            foreach (PlanetQuadTreeNode n in Children)
                verts += n.CountVertsInTree();

            return verts;
        }
  
        public void UpdatePosition()
        {
           

            Transform.WorldMatrix = Planet.Transform.WorldMatrix;

            //midpoint of the patch, transformed to the right scale and location
            boundingSphere.Center = Vector3.Transform(Vector3.Normalize(mid1) * sphereSize,
                Planet.Transform.WorldMatrix);

            

        }

       
    }
}