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

namespace MonoGameEngineCore.Procedural
{
    

    public class PlanetQuadTreeNode
    {
        public enum PatchState
        {
            initial,
            building,
            readyToAddGameObject,
            gameObjectBeingAdded,
            gameObjectBeingRemoved,
            awaitingChildGenerationBeforeRemoval,
            final
        }

        private PatchState patchState;
        readonly int depth;
        private const int maximumDepth = 12;
        public PlanetQuadTreeNode Parent { get; set; }
        public Planet Planet { get; set; }
        public Vector3 normal { get; set; }
        public Vector3 max { get; set; }
        public Vector3 min { get; set; }
        public float step { get; set; }
        public List<PlanetQuadTreeNode> Children;
        private Dictionary<Direction, List<PlanetQuadTreeNode>> neighbours; 
        public VertexPositionColorTextureNormal[] vertices;
        public short[] indices;
        public int heightMapSize;
        public GameObject.GameObject gameObject;
        private Effect effect;
        public bool isLeaf;
        private float sphereSize;
        EffectRenderComponent drawableComponent;
        private BoundingSphere boundingSphere;
        private IModule module;
        public Color NodeColor { get; set; }
        public int quadTreeNodeID;
        private readonly int rootNodeId;
        

        Vector3 se, sw, mid1, mid2, nw, ne, midBottom, midRight, midLeft, midTop;

        public PlanetQuadTreeNode(int sideId, int quadrant, Planet rootObject, PlanetQuadTreeNode parent, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize)
        {
            neighbours = new Dictionary<Direction, List<PlanetQuadTreeNode>>();
            neighbours.Add(Direction.north, new List<PlanetQuadTreeNode>());
            neighbours.Add(Direction.south, new List<PlanetQuadTreeNode>());
            neighbours.Add(Direction.east, new List<PlanetQuadTreeNode>());
            neighbours.Add(Direction.west, new List<PlanetQuadTreeNode>());

            patchState = PatchState.initial;
            this.Planet = rootObject;
            this.sphereSize = sphereSize;
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

            this.rootNodeId = sideId;

            //unique id per patch, composition of planet + side of cube + depth.
            string idString = Planet.planetId.ToString() + sideId.ToString() + depth.ToString() + quadrant.ToString();
            if (Parent != null)
                idString += Parent.quadTreeNodeID.ToString();

            quadTreeNodeID = idString.GetHashCode();

            Children = new List<PlanetQuadTreeNode>();

            CalculatePatchBoundaries(out se, out sw, out mid1, out mid2, out nw, out ne, out midBottom, out midRight, out midLeft, out midTop);
        }

        public void AddNeighbour(Direction dir, params PlanetQuadTreeNode [] nodes)
        {
            foreach (PlanetQuadTreeNode node in nodes)
            {
                neighbours[dir].Add(node);
               
            }
       }

        public void RemoveNeighbour(Direction dir, params PlanetQuadTreeNode[] nodes)
        {
            foreach (PlanetQuadTreeNode node in nodes)
            {
                neighbours[dir].Remove(node);

            }
        }

        public void StartGeometryGeneration(Effect testEffect, IModule module)
        {

            patchState = PatchState.building;
            Planet.BuildTally++;

            //background thread this
            Task.Factory.StartNew(() =>
            {
                BuildGeometry(testEffect, module);
            });



        }

        private void BuildGeometry(Effect testEffect, IModule module)
        {
            
            this.effect = testEffect;
            this.module = module;
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



            gameObject = SystemCore.GameObjectManager.GetObject(quadTreeNodeID);

            //if the object is null this is the first time we've seen it.

            gameObject = GameObjectFactory.CreateRenderableGameObjectFromShape(quadTreeNodeID, spherePatch, effect);

            gameObject.Name = Planet.ParentObject.Name + ": planetPatch : ";
            patchState = PatchState.readyToAddGameObject;

            SetHighPrecisionPosition();

            UpdatePosition();

            isLeaf = true;
        }

        private void SetHighPrecisionPosition()
        {
            var highPrecision = new HighPrecisionPosition();
            highPrecision.Position = Planet.ParentObject.GetComponent<HighPrecisionPosition>().Position;
            gameObject.AddComponent(highPrecision);
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

        private void ClearChildNodes()
        {
            foreach (PlanetQuadTreeNode n in Children)
            {
                n.RemoveGameObjectFromScene();
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

        internal void Update(GameTime gameTime, float splitDistance, float mergeDistance)
        {

            UpdatePosition();

            DetermineVisibility();

            UpdateStates();

            foreach (PlanetQuadTreeNode child in Children)
                child.Update(gameTime, splitDistance / 2f, mergeDistance / 2f);

            //add collision data when we get close
            float distanceFromSurface = CalculateDistanceToPatch();

            if (distanceFromSurface < 100 && patchState == PatchState.final)
            {
                //if (!gameObject.ContainsComponent<MeshColliderComponent>())
                //    gameObject.AddAndInitialise(new MeshColliderComponent());
            }



            if (isLeaf)
            {

                if (ShouldSplit(splitDistance))
                {
                    //onlt split if fully generated
                    if (patchState == PatchState.final && ChildrenHaveCleared())
                        Split();
                }

            }
            else
            {
                if (ShouldMerge(mergeDistance))
                {
                    if (patchState == PatchState.initial && ChildrenHaveGenerated())
                        MergeChildren();
                }

            }


        }

        private void Split()
        {
            if (depth == maximumDepth)
                return;

            patchState = PatchState.awaitingChildGenerationBeforeRemoval;


            //need to add 4 new quadtree nodes, and do neighbours bookkeeping.

            //bottom right, inherits east and south neighbours
            PlanetQuadTreeNode a = new PlanetQuadTreeNode(rootNodeId, 1, Planet, this, se, mid1, step / 2, normal, sphereSize);
            a.StartGeometryGeneration(effect, module);
        
            //top left, inherits north and west
            PlanetQuadTreeNode b = new PlanetQuadTreeNode(rootNodeId, 2, Planet, this, mid2, nw, step / 2, normal, sphereSize);
            b.StartGeometryGeneration(effect, module);

            //bottom left, inherits south and west
            PlanetQuadTreeNode c = new PlanetQuadTreeNode(rootNodeId, 3, Planet, this, midBottom, midLeft, step / 2, normal, sphereSize);
            c.StartGeometryGeneration(effect, module);

            //top right inherits north and east
            PlanetQuadTreeNode d = new PlanetQuadTreeNode(rootNodeId, 4, Planet, this, midRight, midTop, step / 2, normal, sphereSize);
            d.StartGeometryGeneration(effect, module);

            //bottom right
            a.AddNeighbour(Direction.west, c);
            a.AddNeighbour(Direction.north,d);
            a.FixNeighbours(neighbours, Direction.east, Direction.south);

            //top left
            b.AddNeighbour(Direction.south, c);
            b.AddNeighbour(Direction.east, d);
            b.FixNeighbours(neighbours, Direction.north, Direction.west);

            //bottom left
            c.AddNeighbour(Direction.north, b);
            c.AddNeighbour(Direction.east, a);
            c.FixNeighbours(neighbours, Direction.south, Direction.west);

            //top right
            d.AddNeighbour(Direction.south, a);
            d.AddNeighbour(Direction.west, b);
            d.FixNeighbours(neighbours, Direction.north, Direction.east);

            if (Children.Count > 0)
                throw new Exception("Um...");

            Children.Add(a);
            Children.Add(b);
            Children.Add(c);
            Children.Add(d);

            foreach (PlanetQuadTreeNode n in Children)
            {
                n.Planet = Planet;
            }

            isLeaf = false;
        }

        private void FixNeighbours(Dictionary<Direction, List<PlanetQuadTreeNode>> parentNeighbours, params Direction[] fixDirections)
        {
            //when a new node is made, 2 of its neighbours will be siblings from the same parent.
            //the other two will be neighbouring patches of possibly different LOD, inherited from parent
            foreach (Direction direction in fixDirections)
            {
                neighbours[direction] = parentNeighbours[direction];
            }

        }

        public void MergeChildren()
        {

            //covers the case when moving at high speed, 
            //and the merge is needed before the split is finished.
            if (Children.Count == 0)
                return;

            if (!isLeaf)
            {
                //if your children are leaves, you're now a leaf node. Generate geometry to replace the child nodes
                if (Children[0].isLeaf)
                {
                    StartGeometryGeneration(effect, module);
                }
                else //or else go further down the heirarchy until you reach the leaves of the tree
                {

                    foreach (PlanetQuadTreeNode child in Children)
                    {
                        child.MergeChildren();
                    }
                }

            }


        }

        private void UpdateStates()
        {
            if (patchState == PatchState.gameObjectBeingRemoved)
            {
                if (!SystemCore.GameObjectManager.ObjectInManager(gameObject.ID))
                {
                    //back to initial state.
                    patchState = PatchState.initial;
                }
            }

            if (patchState == PatchState.gameObjectBeingAdded)
            {
                if (SystemCore.GameObjectManager.ObjectInManager(gameObject.ID))
                {
                    //we're done.
                    patchState = PatchState.final;
                }
            }

            if (patchState == PatchState.final)
            {
                //covers the post-merge case, if this has visible children, clear them.
                if (ChildrenHaveGenerated())
                    ClearChildNodes();
            }

            if (patchState == PatchState.awaitingChildGenerationBeforeRemoval)
            {
                //can now remove itself
                if (ChildrenHaveGenerated())
                    RemoveGameObjectFromScene();

            }

            if (patchState == PatchState.readyToAddGameObject)
            {
                
                AddGameObjectToScene();
            }
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
            Vector3 surfacePoint = Vector3.Transform(Vector3.Normalize(mid1) * sphereSize, Planet.ParentObject.Transform.WorldMatrix);
            return surfacePoint.Length();
        }

        public int CountVertsInTree()
        {
            int verts = vertices.Length;
            foreach (PlanetQuadTreeNode n in Children)
                verts += n.CountVertsInTree();

            return verts;
        }

        public List<PlanetQuadTreeNode> FindNeighbours(Direction direction)
        {
            //the siblings of this node will comprise at least 2 of its neighbours.
            throw new NotImplementedException();

        }

        private bool ShouldSplit(float splitDistance)
        {
            if (patchState != PatchState.final)
                return false;

            if (!drawableComponent.Visible)
                return false;

            float distanceToPatch = CalculateDistanceToPatch();
            if (distanceToPatch < splitDistance)
                return true;

            return false;
        }

        private bool ShouldMerge(float mergeDistance)
        {
            if (patchState != PatchState.initial)
                return false;

            float distanceToPatch = CalculateDistanceToPatch();
            if (distanceToPatch > mergeDistance)
                return true;

            return false;
        }

        private void AddGameObjectToScene()
        {

            SystemCore.GameObjectManager.AddAndInitialiseObjectOnNextFrame(gameObject);
            drawableComponent = gameObject.GetComponent<EffectRenderComponent>();
            patchState = PatchState.gameObjectBeingAdded;
        }

        private void RemoveGameObjectFromScene()
        {
            SystemCore.GameObjectManager.RemoveGameObjectOnNextFrame(gameObject);
            patchState = PatchState.gameObjectBeingRemoved;
        }

        private bool ChildrenHaveGenerated()
        {
            if (Children.Count == 0)
                return false;

            foreach (PlanetQuadTreeNode child in Children)
            {
                if (child.patchState != PatchState.final)
                    return false;
            }
            return true;
        }

        private bool ChildrenHaveCleared()
        {
            if (Children.Count == 0)
                return true;

            foreach (PlanetQuadTreeNode child in Children)
            {
                if (child.patchState != PatchState.initial)
                    return false;
            }
            return true;
        }

        private void DetermineVisibility()
        {
            if (patchState != PatchState.final)
                return;

            if (!isLeaf)
            {
                drawableComponent.Visible = false;
                return;
            }


            Matrix view = SystemCore.GetCamera("main").View;

            //shift the view matrix back slightly to ensure we are rendering patches just behind us.
            view.Translation += Planet.ParentObject.Transform.WorldMatrix.Forward * 50;

            BoundingFrustum planetCullFrustrum =
                new BoundingFrustum(view * Planet.customProjection);


            bool intersetcts = planetCullFrustrum.Intersects(boundingSphere);

            if (intersetcts)
            {

                drawableComponent.Visible = true;
                
                foreach (PlanetQuadTreeNode quadTreeNode in Children)
                {
                    quadTreeNode.DetermineVisibility();
                }
            }
            else
            {
                drawableComponent.Visible = false;
                SetAllChildrenVisible(false);
            }
        }

        private void SetAllChildrenVisible(bool visible)
        {
            foreach (PlanetQuadTreeNode node in Children)
            {
                node.drawableComponent.Visible = visible;
                node.SetAllChildrenVisible(visible);
            }
        }

        private void UpdatePosition()
        {
            //only null whilst waiting for first-time generation.
            if (gameObject == null)
                return;

            gameObject.Transform.WorldMatrix = Planet.ParentObject.Transform.WorldMatrix;

            //midpoint of the patch, transformed to the right scale and location
            boundingSphere.Center = Vector3.Transform(Vector3.Normalize(mid1) * sphereSize,
                Planet.ParentObject.Transform.WorldMatrix);

            //if (drawableComponent.Visible)
            //    DebugShapeRenderer.AddBoundingSphere(boundingSphere, Color.Red);

        }

    }
}