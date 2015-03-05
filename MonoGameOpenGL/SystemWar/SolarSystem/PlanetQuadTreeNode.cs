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

    public class PlanetNode : GameObject.GameObject
    {


        public int depth;
        public Planet Planet { get; set; }
        public Vector3 normal { get; set; }
        public int SiblingId { get; set; }
        public Vector3 max { get; set; }
        public Vector3 min { get; set; }
        public float step { get; set; }
        public bool remove;
        public volatile bool built;
        public int heightMapSize;

        private Effect effect;
        private float sphereSize;
        public BoundingSphere boundingSphere;
        private IModule module;
        public Color NodeColor { get; set; }
        public int quadTreeNodeID;
        private readonly int rootNodeId;
        public EffectRenderComponent renderComponent;
        public MeshColliderComponent meshCollider;

        public Vector3 se, sw, mid1, mid2, nw, ne, midBottom, midRight, midLeft, midTop;

        public PlanetNode(Effect effect, IModule module, Planet rootObject, int depth, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize)
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

            this.depth = depth;

            CalculatePatchBoundaries(normal, step, min, max, out se, out sw, out mid1, out mid2, out nw, out ne, out midBottom, out midRight, out midLeft, out midTop);


        }

        public Vector3 GetKeyPoint()
        {
            return (min + max) / 2;
        }

        public void BuildGeometry()
        {

            var vertices = new VertexPositionColorTextureNormal[(heightMapSize * heightMapSize)];

            int vertIndex = 0;



            List<int> topEdges = new List<int>();
            List<int> bottomEdges = new List<int>();
            List<int> leftEdges = new List<int>();
            List<int> rightEdges = new List<int>();

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


                    //if (i == 0)
                    //    bottomEdges.Add(vertIndex);
                    //if (i == heightMapSize - 1)
                    //    topEdges.Add(vertIndex);
                    if (j == 0)
                        bottomEdges.Add(vertIndex);
                    if (j == heightMapSize - 1)
                        topEdges.Add(vertIndex);



                    vertIndex++;


                }
            }

            var indices = GenerateIndices();

            if (normal == Vector3.Up || normal == Vector3.Forward || normal == Vector3.Left)
                indices = indices.Reverse().ToArray();

            bool adjustTop = false;
            bool adjustBottom = false;
            bool adjustLeft = false;
            bool adjustRight = false;

            List<NeighbourTracker.Connection> connections = this.Planet.GetNeighbours(this);
            if (connections != null)
            {
                var northConn = connections.Find(x => x.direction == NeighbourTracker.ConnectionDirection.north);
                var southConn = connections.Find(x => x.direction == NeighbourTracker.ConnectionDirection.south);
                var westConn = connections.Find(x => x.direction == NeighbourTracker.ConnectionDirection.west);
                var eastConn = connections.Find(x => x.direction == NeighbourTracker.ConnectionDirection.east);


                if (northConn != null)
                    if (northConn.node.depth < depth)
                        adjustTop = true;
                //if (southConn != null && southConn.node.depth <= depth)
                //    adjustBottom = true;
                //if (westConn != null && westConn.node.depth <= depth)
                //    adjustLeft = true;
                //if (eastConn != null && southConn.node.depth <= depth)
                //    adjustRight = true;
                //if (NeighbourIsLowerOrSameLod(connections, ref vertices, ref topEdges))
                //    adjustTop = true;

                //if (NeighbourIsLowerOrSameLod(connections, ref vertices, ref bottomEdges))
                //    adjustBottom = true;

                //if (NeighbourIsLowerOrSameLod(connections, ref vertices, ref leftEdges))
                //    adjustLeft = true;

                //if (NeighbourIsLowerOrSameLod(connections, ref vertices, ref rightEdges))
                //    adjustRight = true;
            }

            Sphereify(sphereSize, ref vertices);

            if (adjustTop)
                AdjustEdges(ref vertices, ref topEdges);
            //if (adjustBottom)
            //    AdjustEdges(ref vertices, ref bottomEdges);
            //if (adjustLeft)
            //    AdjustEdges(ref vertices, ref leftEdges);
            //if (adjustRight)
            //    AdjustEdges(ref vertices, ref rightEdges);


            ////should see no seams between LOD 8 + 7, but remain elsewhere.
            //if (depth == Planet.maxDepth)
            //{
            //    AdjustEdges(ref vertices, ref topEdges);
            //    AdjustEdges(ref vertices, ref bottomEdges);
            //    AdjustEdges(ref vertices, ref leftEdges);
            //    AdjustEdges(ref vertices, ref rightEdges);
            //}


            GenerateNormals(ref vertices, ref indices);


            var p = vertices.Select(x => x.Position).ToList();
            boundingSphere = BoundingSphere.CreateFromPoints(p);

            ProceduralShape spherePatch = new ProceduralShape(vertices, indices);



            this.AddComponent(new RenderGeometryComponent(spherePatch));

            meshCollider = new MeshColliderComponent(this, spherePatch.GetVertices(), spherePatch.GetIndicesAsInt().ToArray());
            AddComponent(meshCollider);

            if (this.effect is BasicEffect)
                this.AddComponent(new BasicEffectRenderComponent(effect as BasicEffect));
            else
            {
                renderComponent = new EffectRenderComponent(effect);
                renderComponent.DrawOrder = Planet.DrawOrder;
                this.AddComponent(renderComponent);
            }


            SetHighPrecisionPosition(this);

            built = true;


        }

        private bool NeighbourIsLowerOrSameLod(List<NeighbourTracker.Connection> connections, ref VertexPositionColorTextureNormal[] vertices, ref List<int> topEdges)
        {


            Vector3 cornerA = vertices[topEdges[0]].Position;
            Vector3 cornerB = vertices[topEdges[20]].Position;

            foreach (NeighbourTracker.Connection connection in connections)
            {

                //if we share two corners, this is our adjoining patch.
                if (SharesTwoCorners(cornerA, cornerB, connection.node.min, connection.node.max, connection.node.normal,
                    connection.node.step))
                {
                    return connection.node.depth <= depth;
                }


            }

            return false;

        }

        //Determines whether an edge is shared by a neighbouring patch.
        private bool SharesTwoCorners(Vector3 cornerA, Vector3 cornerB, Vector3 thisMin, Vector3 thisMax, Vector3 thisNormal, float thisStep)
        {
            //calculate info about this neighbour patch.
            Vector3 se1, sw1, mid11, mid21, nw1, ne1, midBottom1, midRight1, midLeft1, midTop1;
            CalculatePatchBoundaries(thisNormal, thisStep, thisMin, thisMax, out se1, out sw1, out mid11, out mid21, out nw1, out ne1, out midBottom1, out midRight1, out midLeft1, out midTop1);

            List<Vector3> first = new List<Vector3>();
            first.Add(cornerA);
            first.Add(cornerB);

            List<Vector3> second = new List<Vector3>();
            second.Add(se1);
            second.Add(sw1);
            second.Add(nw1);
            second.Add(ne1);
            second.Add(midBottom1);
            second.Add(midRight1);
            second.Add(midLeft1);
            second.Add(midTop1);


            int match = 0;
            foreach (Vector3 corner in first)
            {
                foreach (Vector3 neighbourVec in second)
                {
                    if (MonoMathHelper.Vector3ComponentAlmostEquals(corner, neighbourVec, step / 2))
                        match++;
                }
            }
            if (match == 2)
                return true;
            return false;

        }

        private Vector3 VecTransform(Vector3 min)
        {
            return Vector3.Normalize(min) * sphereSize;
        }

        private ProceduralShape AddSkirt(ref VertexPositionColorTextureNormal[] vertices, ref List<int> topEdges, ProceduralShape spherePatch, bool insideOut)
        {
            ProceduralShapeBuilder builder = new ProceduralShapeBuilder();
            float skirtSize = 10f;
            float offset = 0;
            for (int i = 0; i < topEdges.Count - 1; i++)
            {

                Vector3 point = vertices[topEdges[i]].Position;
                Vector3 toCenter = Vector3.Normalize(-point);
                Vector3 lowPoint = point + toCenter * skirtSize;
                point += toCenter * offset;
                lowPoint += toCenter * offset;

                Vector3 point2 = vertices[topEdges[i + 1]].Position;
                Vector3 toCenter2 = Vector3.Normalize(-point2);
                Vector3 lowPoint2 = point2 + toCenter2 * skirtSize;
                point2 += toCenter2 * offset;
                lowPoint2 += toCenter2 * offset;

                builder.AddFaceWithColor(vertices[topEdges[i]].Color, point, point2,
                    lowPoint2, lowPoint);
            }

            var skirt = builder.BakeShape();
            if (insideOut)
                skirt.InsideOut();

            spherePatch = ProceduralShape.Combine(spherePatch, skirt);
            return spherePatch;
        }

        private void AdjustEdges(ref VertexPositionColorTextureNormal[] vertices, ref List<int> edgeIndexes)
        {
            for (int i = 1; i < edgeIndexes.Count - 1; i += 2)
            {
                Vector3 neighbourA = vertices[edgeIndexes[i - 1]].Position;
                Vector3 neighbourB = vertices[edgeIndexes[i + 1]].Position;
                float aLength = neighbourA.Length();
                float bLength = neighbourB.Length();
                float avgHeight = (aLength + bLength) / 2;
                vertices[edgeIndexes[i]].Position = Vector3.Normalize(vertices[edgeIndexes[i]].Position) * avgHeight;

            }
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

        private short[] GenerateIndices()
        {

            // Construct the index array.
            var indices = new short[(heightMapSize - 1) * (heightMapSize - 1) * 6];    // 2 triangles per grid square x 3 vertices per triangle

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
            return indices;
        }

        private void GenerateNormals(ref VertexPositionColorTextureNormal[] vertArray, ref short[] indices)
        {


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

        public static void CalculatePatchBoundaries(Vector3 normal, float step, Vector3 min, Vector3 max, out Vector3 se, out Vector3 sw, out Vector3 mid1, out Vector3 mid2, out Vector3 nw, out Vector3 ne, out Vector3 midBottom, out Vector3 midRight, out Vector3 midLeft, out Vector3 midTop)
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

        private void Sphereify(float radius, ref VertexPositionColorTextureNormal[] vertices)
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
                    Color col = Color.White;

                    if (depth == 1)
                        col = Color.White;
                    if (depth == 2)
                        col = Color.Blue;
                    if (depth == 3)
                        col = Color.Green;
                    if (depth == 4)
                        col = Color.Red;
                    if (depth == 5)
                        col = Color.Pink;
                    if (depth == 6)
                        col = Color.Yellow;
                    if (depth == 7)
                        col = Color.Orange;
                    if (depth == 8)
                        col = Color.Purple;


                    vertices[i].Color = col;
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

        public void Update()
        {


            Transform.WorldMatrix = Planet.Transform.WorldMatrix;

            //midpoint of the patch, transformed to the right scale and location
            boundingSphere.Center = Vector3.Transform(Vector3.Normalize(mid1) * sphereSize,
                Planet.Transform.WorldMatrix);


            this.GetComponent<EffectRenderComponent>().DrawOrder = Planet.DrawOrder;


        }

        internal void DetermineIntersection(Ray ray, Vector3 hitPos)
        {
            //hitPos is the position on the planet surface 
        }

        public void Disable()
        {
            renderComponent.Visible = false;
            if (meshCollider != null)
                meshCollider.Enabled = false;
        }

        public void Enable()
        {
            renderComponent.Visible = true;
            if (meshCollider != null)
                meshCollider.Enabled = true;
        }
    }
}