using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.Procedural
{
    public enum Direction
    {

        north,
        south,
        east,
        west
    }

    public class PlanetQuadTree
    {
        float splitDistance = 40f;
        float mergeDistance = 45f;
        private Vector3 centerPosition;

        public List<QuadTreeNode> rootNodes;

        public PlanetQuadTree(IModule module, Effect testEffect, Vector3 position, float scale)
        {
            rootNodes = new List<QuadTreeNode>();
            centerPosition = position;
            float vectorSpacing = 1f;
            float cubeVerts = 21;
            float sphereSize = scale;


            //top
            QuadTreeNode n1 = new QuadTreeNode(null, new Vector3(-cubeVerts / 2, cubeVerts / 2 - 1, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2 - 1, cubeVerts / 2), vectorSpacing, Vector3.Up, sphereSize, centerPosition);
            n1.GenerateGeometry(testEffect, module);


            //bottom
            QuadTreeNode n2 = new QuadTreeNode(null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Down, sphereSize, centerPosition);
            n2.GenerateGeometry(testEffect, module);


            //forward
            QuadTreeNode n3 = new QuadTreeNode(null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Forward, sphereSize, centerPosition);
            n3.GenerateGeometry(testEffect, module);


            //backward
            QuadTreeNode n4 = new QuadTreeNode(null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2 - 1), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2 - 1), vectorSpacing, Vector3.Backward, sphereSize, centerPosition);
            n4.GenerateGeometry(testEffect, module);

            //right
            QuadTreeNode n5 = new QuadTreeNode(null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(-cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Right, sphereSize, centerPosition);
            n5.GenerateGeometry(testEffect, module);

            //left
            QuadTreeNode n6 = new QuadTreeNode(null, new Vector3(cubeVerts / 2 - 1, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2 - 1, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Left, sphereSize, centerPosition);
            n6.GenerateGeometry(testEffect, module);

            rootNodes.Add(n1);
            rootNodes.Add(n2);
            rootNodes.Add(n3);
            rootNodes.Add(n4);
            rootNodes.Add(n5);
            rootNodes.Add(n6);

            n1.Planet = this;
            n2.Planet = this;
            n3.Planet = this;
            n4.Planet = this;
            n5.Planet = this;
            n6.Planet = this;
        }

        public void Update(Vector3 camPosition)
        {
            foreach (QuadTreeNode n in rootNodes)
            {
                n.SplitCheck(camPosition, splitDistance, mergeDistance);
            }
        }
    }

    public class QuadTreeNode
    {
        int depth;
        static int maximumDepth = 12;
        public QuadTreeNode Parent { get; set; }
        public PlanetQuadTree Planet { get; set; }
        public Vector3 normal { get; set; }
        public Vector3 max { get; set; }
        public Vector3 min { get; set; }
        public float step { get; set; }
        public List<QuadTreeNode> Children;
        public VertexPositionColorTextureNormal[] vertices;
        public short[] indices;
        public int heightMapSize;
        private GameObject.GameObject gameObject;
        private Effect effect;
        public bool isLeaf;
        private float sphereSize;
        private readonly Vector3 positionOffset;
        private IModule module;
        public Color NodeColor { get; set; }
        Vector3 se, sw, mid1, mid2, nw, ne, midBottom, midRight, midLeft, midTop;

        public QuadTreeNode(QuadTreeNode parent, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize, Vector3 positionOffset)
        {
            
            this.sphereSize = sphereSize;
            this.positionOffset = positionOffset;
            this.Parent = parent;
            this.min = min;
            this.max = max;
            this.step = step;
            this.normal = normal;
            heightMapSize = System.Math.Max((int)((max.X - min.X) / step), (int)((max.Z - min.Z) / step)); ;
            NodeColor = SystemCore.ActiveColorScheme.Color1;

            if (Parent == null)
                depth = 1;
            else
                depth = Parent.depth + 1;


            Children = new List<QuadTreeNode>();

            CalculatePatchBoundaries(out se, out sw, out mid1, out mid2, out nw, out ne, out midBottom, out midRight, out midLeft, out midTop);
        }

        public void GenerateGeometry(Effect testEffect, IModule module)
        {
            this.effect = testEffect;
            this.module = module;
            vertices = new VertexPositionColorTextureNormal[heightMapSize * heightMapSize];

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


            short[] ind = indices;
            var p = vertices.Select(x => x.Position).ToList();
            var s = BoundingSphere.CreateFromPoints(p);


            ProceduralShape spherePatch = new ProceduralShape(vertices, indices);
            spherePatch.Translate(positionOffset);
            gameObject = GameObjectFactory.CreateRenderableGameObjectFromShape(spherePatch, effect);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);

            isLeaf = true;


        }

        private void CalculateAxisStartPoints(out float axis1Start, out float axis2Start)
        {
            if (normal == Vector3.Up || normal == Vector3.Down)
            {
                axis1Start = min.X;
                axis2Start = min.Z;
                return;
            }
            if (normal == Vector3.Left || normal == Vector3.Right)
            {
                axis1Start = min.Z;
                axis2Start = min.Y;
                return;
            }
            if (normal == Vector3.Forward || normal == Vector3.Backward)
            {
                axis1Start = min.X;
                axis2Start = min.Y;
                return;
            }
            axis1Start = -1;
            axis2Start = -1;
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

        public void Split()
        {
            if (depth == maximumDepth)
                return;


            SystemCore.GameObjectManager.RemoveObject(gameObject);


            //need to add 4 new quadtree nodes
            QuadTreeNode a = new QuadTreeNode(this, se, mid1, step / 2, normal, sphereSize, positionOffset);
            a.GenerateGeometry(effect, module);

            QuadTreeNode b = new QuadTreeNode(this, mid2, nw, step / 2, normal, sphereSize, positionOffset);
            b.GenerateGeometry(effect, module);


            QuadTreeNode c = new QuadTreeNode(this, midBottom, midLeft, step / 2, normal, sphereSize, positionOffset);
            c.GenerateGeometry(effect, module);

            QuadTreeNode d = new QuadTreeNode(this, midRight, midTop, step / 2, normal, sphereSize, positionOffset);
            d.GenerateGeometry(effect, module);


            Children.Add(a);
            Children.Add(b);
            Children.Add(c);
            Children.Add(d);

            foreach (QuadTreeNode n in Children)
                n.Planet = Planet;

            isLeaf = false;
        }

        private void ClearChildNodes()
        {
            foreach (QuadTreeNode n in Children)
            {
                SystemCore.GameObjectManager.RemoveObject(n.gameObject);
                n.ClearChildNodes();
            }
            Children.Clear();
        }

        /// <summary>
        /// Generates helpful coordinates for the splitting of a node into four children, 
        /// including the four corner coordinates and various midpoints of the node about to be split.
        /// </summary>
        /// <param name="se"></param>
        /// <param name="sw"></param>
        /// <param name="mid1"></param>
        /// <param name="mid2"></param>
        /// <param name="nw"></param>
        /// <param name="ne"></param>
        /// <param name="midBottom"></param>
        /// <param name="midRight"></param>
        /// <param name="midLeft"></param>
        /// <param name="midTop"></param>
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

        public void MergeChildren()
        {


            if (!isLeaf)
            {
                //if your children are leaves, you're now a leaf node. Generate geometry.
                if (Children[0].isLeaf)
                {
                    ClearChildNodes();
                    GenerateGeometry(effect, module);
                    isLeaf = true;
                }
                else
                {

                    foreach (QuadTreeNode child in Children)
                    {
                        child.MergeChildren();
                    }
                }

            }


        }

        public void Sphereify(float radius)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].Position = (Vector3.Normalize(vertices[i].Position) * radius);
                double height = module.GetValue(vertices[i].Position.X, vertices[i].Position.Y, vertices[i].Position.Z);
                vertices[i].Position = (Vector3.Normalize(vertices[i].Position) * (radius + (float)height * 0.1f));
            }
        }

        internal void SplitCheck(Vector3 cameraPos, float splitDistance, float mergeDistance)
        {
            if (isLeaf)
            {

                float distanceToPatch = CalculateDistanceToPatch(ref cameraPos);
                if (distanceToPatch < splitDistance)
                {
                    Split();
                    return;
                }

            }
            else
            {
                float distanceToPatch = CalculateDistanceToPatch(ref cameraPos);
                if (distanceToPatch > mergeDistance)
                {
                    MergeChildren();
                }
                else
                {

                    foreach (QuadTreeNode child in Children)
                        child.SplitCheck(cameraPos, splitDistance / 2, mergeDistance / 2);
                }
            }
        }

        private float CalculateDistanceToPatch(ref Vector3 cameraPos)
        {
            //float distanceToPatch;
            //distanceToPatch = (((min + max) / 2) - cameraPos).Length();
            //return distanceToPatch;

            float distance;

            distance = (((Vector3.Normalize(mid1) * sphereSize) + positionOffset) - cameraPos).Length();

            return distance;
        }

        public int CountVertsInTree()
        {
            int verts = vertices.Length;
            foreach (QuadTreeNode n in Children)
                verts += n.CountVertsInTree();

            return verts;
        }

        public List<QuadTreeNode> FindNeighbours(Direction direction)
        {
            //the siblings of this node will comprise at least 2 of its neighbours.
            throw new NotImplementedException();

        }
    }

}
