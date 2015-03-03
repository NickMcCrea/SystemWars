using System;
using System.Collections.Generic;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Rendering;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;
using SystemWar;
using System.Diagnostics;

namespace MonoGameEngineCore.Procedural
{
    //seamfix

    struct PatchMinMax
    {
        public Vector3 Min;
        public Vector3 Max;

        public PatchMinMax(Vector3 min, Vector3 max)
        {
            Max = max;
            Min = min;
        }
    }


    public class NeighbourTrackerNode
    {
        public enum Quadrant
        {
            none,
            se,
            nw,
            ne,
            sw
        }

        public int depth;
        public Vector3 keyPoint;
        public Quadrant quadrant;


        public NeighbourTrackerNode()
        {

        }

        public NeighbourTrackerNode(int depth, Vector3 keyPoint)
        {
            this.depth = depth;
            this.keyPoint = keyPoint;
        }
    }

    public class NeighbourTracker
    {

        public enum ConnectionDirection
        {
            north,
            south,
            east,
            west,
        }

        public class Connection
        {
            public ConnectionDirection direction;
            public NeighbourTrackerNode node;
            public Connection(ConnectionDirection dir, NeighbourTrackerNode n)
            {
                node = n;
                direction = dir;
            }
        }

        public Dictionary<NeighbourTrackerNode, List<Connection>> connections;
        public Dictionary<Vector3, NeighbourTrackerNode> nodeDictionary;

        public NeighbourTracker()
        {
            connections = new Dictionary<NeighbourTrackerNode, List<Connection>>();
            nodeDictionary = new Dictionary<Vector3, NeighbourTrackerNode>();
        }

        private ConnectionDirection GetOpposite(ConnectionDirection dir)
        {
            if (dir == ConnectionDirection.east)
                return ConnectionDirection.west;
            if (dir == ConnectionDirection.north)
                return ConnectionDirection.south;
            if (dir == ConnectionDirection.west)
                return ConnectionDirection.east;
            if (dir == ConnectionDirection.south)
                return ConnectionDirection.north;


            return ConnectionDirection.north;
        }

        public void ClearAllConnections()
        {
            connections.Clear();
            nodeDictionary.Clear();
        }

        public void MakeConnection(NeighbourTrackerNode a, NeighbourTrackerNode b, ConnectionDirection dir)
        {
            if (!connections.ContainsKey(a))
                connections.Add(a, new List<Connection>());

            connections[a].Add(new Connection(dir, b));

            if (!connections.ContainsKey(b))
                connections.Add(b, new List<Connection>());

            connections[b].Add(new Connection(GetOpposite(dir), a));

            if (!nodeDictionary.ContainsKey(a.keyPoint))
                nodeDictionary.Add(a.keyPoint, a);
            if (!nodeDictionary.ContainsKey(b.keyPoint))
                nodeDictionary.Add(b.keyPoint, b);

        }

        public void MakeConnection(NeighbourTrackerNode a, NeighbourTrackerNode b, ConnectionDirection dir, ConnectionDirection dir2)
        {
            if (!connections.ContainsKey(a))
                connections.Add(a, new List<Connection>());

            connections[a].Add(new Connection(dir, b));

            if (!connections.ContainsKey(b))
                connections.Add(b, new List<Connection>());

            connections[b].Add(new Connection(dir2, a));

            if (!nodeDictionary.ContainsKey(a.keyPoint))
                nodeDictionary.Add(a.keyPoint, a);
            if (!nodeDictionary.ContainsKey(b.keyPoint))
                nodeDictionary.Add(b.keyPoint, b);

        }

        public void ReplaceNodeWithChildren(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode nw, NeighbourTrackerNode sw, NeighbourTrackerNode se, NeighbourTrackerNode ne)
        {
            //children are connected to each other on two edges, and inherit the parent connections on the others
            MakeConnection(nw, sw, ConnectionDirection.south);
            MakeConnection(nw, ne, ConnectionDirection.east);
            MakeConnection(sw, se, ConnectionDirection.east);
            MakeConnection(se, ne, ConnectionDirection.north);

            List<Connection> parentConnections = GetConnections(nodeToReplace);


            var northConnections = connections[nodeToReplace].FindAll(x => x.direction == ConnectionDirection.north);
            var southConnections = connections[nodeToReplace].FindAll(x => x.direction == ConnectionDirection.south);
            var westConnections = connections[nodeToReplace].FindAll(x => x.direction == ConnectionDirection.west);
            var eastConnections = connections[nodeToReplace].FindAll(x => x.direction == ConnectionDirection.east);


            if (northConnections.Count == 1)
            {
                MakeConnection(nw, northConnections[0].node, ConnectionDirection.north);
                MakeConnection(ne, northConnections[0].node, ConnectionDirection.north);
            }
            if (northConnections.Count == 2)
            {
                MakeConnection(nw, northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node, ConnectionDirection.north);
                MakeConnection(ne, northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node, ConnectionDirection.north);
            }

            if (southConnections.Count == 1)
            {
                MakeConnection(sw, southConnections[0].node, ConnectionDirection.south);
                MakeConnection(se, southConnections[0].node, ConnectionDirection.south);
            }
            if (southConnections.Count == 2)
            {
                MakeConnection(sw, southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node, ConnectionDirection.south);
                MakeConnection(se, southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node, ConnectionDirection.south);
            }


            if (westConnections.Count == 1)
            {
                MakeConnection(sw, westConnections[0].node, ConnectionDirection.west);
                MakeConnection(nw, westConnections[0].node, ConnectionDirection.west);
            }
            if (westConnections.Count == 2)
            {
                MakeConnection(sw, westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node, ConnectionDirection.west);
                MakeConnection(nw, westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node, ConnectionDirection.west);
            }

            if (eastConnections.Count == 1)
            {
                MakeConnection(se, eastConnections[0].node, ConnectionDirection.east);
                MakeConnection(ne, eastConnections[0].node, ConnectionDirection.east);
            }
            if (eastConnections.Count == 2)
            {
                MakeConnection(se, eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node, ConnectionDirection.east);
                MakeConnection(ne, eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node, ConnectionDirection.east);
            }

            GC.KeepAlive(parentConnections);
            RemoveAllConnections(nodeToReplace);

        }

        public void RemoveAllConnections(NeighbourTrackerNode nodeToReplace)
        {
            List<Connection> nodeConnections = GetConnections(nodeToReplace);

            foreach (Connection conn in nodeConnections)
            {
                connections[conn.node].RemoveAll(x => x.node == nodeToReplace);
            }

            //List<Connection> consToRemove = new List<Connection>();
            ////want to go through all connections featuring the node, and remove them.
            //foreach (NeighbourTrackerNode n in connections.Keys)
            //{
            //    if (n == nodeToReplace)
            //        continue;


            //    foreach (Connection c in connections[n])
            //    {
            //        if (c.node == nodeToReplace)
            //            consToRemove.Add(c);
            //    }

            //}

            //foreach (NeighbourTrackerNode n in connections.Keys)
            //{
            //    foreach(Connection c in consToRemove)
            //    {
            //        if (connections[n].Contains(c))
            //        {
            //            connections[n].Remove(c);
            //        }

            //    }
            //}

            //consToRemove.Clear();
            connections.Remove(nodeToReplace);
            nodeDictionary.Remove(nodeToReplace.keyPoint);
        }

        public List<Connection> GetConnections(NeighbourTrackerNode node)
        {
            if (connections.ContainsKey(node))
                return connections[node];
            return new List<Connection>();
        }

        public List<Connection> GetConnections(PlanetNode node)
        {
            if (nodeDictionary.ContainsKey(node.GetKeyPoint()))
                return GetConnections(nodeDictionary[node.GetKeyPoint()]);

            return new List<Connection>();
        }

    }

    public class Planet : GameObject.GameObject, IUpdateable
    {
        private NeighbourTracker neighbourTracker;

        private readonly IModule module;
        private readonly Effect testEffect;
        public readonly float radius;
        private float splitDistance;
        private float mergeDistance;
        public Matrix customProjection;
        public Dictionary<Vector3, PlanetNode> activePatches;
        private List<PlanetNode> rootNodes;
        private Dictionary<Vector3, PatchMinMax> nodesBeingBuilt;
        public Color SeaColor;
        public Color LandColor;
        public Color MountainColor;
        public int planetId;
        private static int planetIdList;
        public int BuildCountPerSecond;
        public int BuildTally;
        private TimeSpan lastClearTime;
        public bool visualisePatches = true;
        public int maxDepth = 8;

        private Planet orbitBody;
        private Vector3d positionToOrbit;
        private float orbitSpeed;
        private double orbitRadius;
        private bool orbitEnabled;
        public float orbitAngle;
        private Vector3d positionLastFrame;
        public bool HasAtmosphere { get; private set; }

        public Atmosphere atmosphere;
        private GroundScatteringHelper atmosphericScatteringHelper;
        private SpaceScatteringHelper spaceScatteringHelper;
        public HighPrecisionPosition Position { get; private set; }
        public int DrawOrder { get; set; }

        private NeighbourTrackerNode topNode, bottomNode, leftNode, rightNode, forwardNode, backwardNode;

        public Planet(string name, Vector3d position, IModule module, Effect testEffect, float radius, Color sea, Color land, Color mountains)
        {
            nodesBeingBuilt = new Dictionary<Vector3, PatchMinMax>();
            neighbourTracker = new NeighbourTracker();
            this.Name = name;
            planetId = ++planetIdList;
            this.module = module;
            this.testEffect = testEffect;
            this.radius = radius;

            Position = new HighPrecisionPosition();
            AddComponent(Position);

            Transform.SetPosition(position);

            splitDistance = radius * 4;
            mergeDistance = radius * 4.5f;

            customProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, this.radius * 10);

            SeaColor = sea;
            LandColor = land;
            MountainColor = mountains;


            Initialise();
        }

        public Planet(string name, Vector3d position, IModule module, Effect testEffect, float radius, Color sea,
            Color land, Color mountains, float rotation)
            : this(name, position, module, testEffect, radius, sea,
                land, mountains)
        {
            AddComponent(new RotatorComponent(Vector3.Up, rotation));
        }

        //this is a test

        public void AddAtmosphere()
        {

            HasAtmosphere = true;
            atmosphere = new Atmosphere(this.radius * 1.05f, this.radius);
            atmosphere.AddComponent(new HighPrecisionPosition());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphere);

            atmosphericScatteringHelper = new GroundScatteringHelper(this.testEffect, radius * 1.05f, radius);



        }

        private void GenerateCustomProjectionMatrix(float far)
        {
            if (far <= 0)
                far = 2;

            customProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, far);
        }

        public void Orbit(Vector3d positionToOrbit, double orbitRadius, float orbitSpeed)
        {
            this.positionToOrbit = positionToOrbit;
            this.orbitSpeed = orbitSpeed;
            orbitEnabled = true;
            this.orbitRadius = orbitRadius;
        }

        public void Orbit(Planet planetToOrbit, double orbitRadius, float orbitSpeed)
        {
            this.orbitRadius = orbitRadius;
            orbitBody = planetToOrbit;
            this.orbitSpeed = orbitSpeed;
            orbitEnabled = true;
        }

        private void Initialise()
        {
            activePatches = new Dictionary<Vector3, PlanetNode>();
            rootNodes = new List<PlanetNode>();
            float vectorSpacing = 1f;
            float cubeVerts = 21;
            float sphereSize = radius;




            //top
            //PlanetNode top = new PlanetNode(testEffect, module, this, 1, new Vector3(-cubeVerts / 2, cubeVerts / 2 - 1, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2 - 1, cubeVerts / 2), vectorSpacing, Vector3.Up, sphereSize);
            //top.BuildGeometry();
            //AddPatch(top);
            //rootNodes.Add(top);

            //////bottom
            //PlanetNode bottom = new PlanetNode(testEffect, module, this, 1, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Down, sphereSize);
            //bottom.BuildGeometry();
            //AddPatch(bottom);
            //rootNodes.Add(bottom);

            //forward
            //PlanetNode forward = new PlanetNode(testEffect, module, this, 1, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Forward, sphereSize);
            //forward.BuildGeometry();
            //AddPatch(forward);
            //rootNodes.Add(forward);

            ////backward
            //PlanetNode backward = new PlanetNode(testEffect, module, this, 1, new Vector3(-cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2 - 1), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2 - 1), vectorSpacing, Vector3.Backward, sphereSize);
            //backward.BuildGeometry();
            //AddPatch(backward);
            //rootNodes.Add(backward);

            //right
            //PlanetNode right = new PlanetNode(testEffect, module, this, 1, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(-cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Right, sphereSize);
            //right.BuildGeometry();
            //AddPatch(right);
            //rootNodes.Add(right);

            ////left
            PlanetNode left = new PlanetNode(testEffect, module, this, 1, new Vector3(cubeVerts / 2 - 1, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2 - 1, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Left, sphereSize);
            left.BuildGeometry();
            AddPatch(left);
            rootNodes.Add(left);

            leftNode = new NeighbourTrackerNode(1, left.GetKeyPoint());









            //topNode = new NeighbourTrackerNode(1, top.GetKeyPoint());
            //bottomNode = new NeighbourTrackerNode(1, bottom.GetKeyPoint());
            //forwardNode = new NeighbourTrackerNode(1, forward.GetKeyPoint());
            //leftNode = new NeighbourTrackerNode(1, left.GetKeyPoint());
            //rightNode = new NeighbourTrackerNode(1, right.GetKeyPoint());
            //backwardNode = new NeighbourTrackerNode(1, backward.GetKeyPoint());

            ReinitialiseTracker();

        }

        private void ReinitialiseTracker()
        {
            //neighbourTracker.MakeConnection(topNode, leftNode, NeighbourTracker.ConnectionDirection.west, NeighbourTracker.ConnectionDirection.north);
            //neighbourTracker.MakeConnection(topNode, rightNode, NeighbourTracker.ConnectionDirection.east, NeighbourTracker.ConnectionDirection.north);
            //neighbourTracker.MakeConnection(topNode, forwardNode, NeighbourTracker.ConnectionDirection.south, NeighbourTracker.ConnectionDirection.north);
            //neighbourTracker.MakeConnection(topNode, backwardNode, NeighbourTracker.ConnectionDirection.north, NeighbourTracker.ConnectionDirection.north);

            //neighbourTracker.MakeConnection(bottomNode, leftNode, NeighbourTracker.ConnectionDirection.west, NeighbourTracker.ConnectionDirection.south);
            //neighbourTracker.MakeConnection(bottomNode, rightNode, NeighbourTracker.ConnectionDirection.east, NeighbourTracker.ConnectionDirection.south);
            //neighbourTracker.MakeConnection(bottomNode, forwardNode, NeighbourTracker.ConnectionDirection.north, NeighbourTracker.ConnectionDirection.south);
            //neighbourTracker.MakeConnection(bottomNode, backwardNode, NeighbourTracker.ConnectionDirection.south, NeighbourTracker.ConnectionDirection.south);

            //neighbourTracker.MakeConnection(leftNode, forwardNode, NeighbourTracker.ConnectionDirection.east);
            //neighbourTracker.MakeConnection(leftNode, backwardNode, NeighbourTracker.ConnectionDirection.west);
            //neighbourTracker.MakeConnection(rightNode, forwardNode, NeighbourTracker.ConnectionDirection.west);
            //neighbourTracker.MakeConnection(rightNode, backwardNode, NeighbourTracker.ConnectionDirection.east);


            neighbourTracker.connections.Add(leftNode, new List<NeighbourTracker.Connection>());
            neighbourTracker.nodeDictionary.Add(leftNode.keyPoint, leftNode);
        }

        private bool ShouldSplit(Vector3 min, Vector3 max, float radius, int depth)
        {
            if (depth >= maxDepth)
                return false;

            float adjustedDistance = splitDistance;
            for (int i = 1; i < depth; i++)
                adjustedDistance *= 0.5f;

            float distanceToPatch = DistanceToPatch(min, max, radius);
            if (distanceToPatch < (adjustedDistance))
                return true;
            return false;
        }

        private void CalculatePatchLOD(Vector3 normal, float step, int depth, Vector3 min, Vector3 max, NeighbourTrackerNode parent)
        {

            //NeighbourTrackerNode node = neighbourTracker.connections

            //recurse down through the tree. For each node on the way down, we decide if it should split or not.
            //if it should, calculate the split and move down. Remove the node if it's currently visible.
            if (ShouldSplit(min, max, radius, depth))
            {
                Vector3 se, sw, mid1, mid2, nw, ne, midBottom, midRight, midLeft, midTop;
                PlanetNode.CalculatePatchBoundaries(normal, step, min, max, out se, out sw, out mid1, out mid2, out nw, out ne, out midBottom, out midRight, out midLeft, out midTop);

                //remove this node in the neighbour tracker, generate and connect children
                NeighbourTrackerNode southEast = new NeighbourTrackerNode(depth + 1, (se + mid1) / 2);
                southEast.quadrant = NeighbourTrackerNode.Quadrant.se;
                NeighbourTrackerNode northWest = new NeighbourTrackerNode(depth + 1, (mid2 + nw) / 2);
                northWest.quadrant = NeighbourTrackerNode.Quadrant.nw;
                NeighbourTrackerNode southWest = new NeighbourTrackerNode(depth + 1, (midBottom + midLeft) / 2);
                southWest.quadrant = NeighbourTrackerNode.Quadrant.sw;
                NeighbourTrackerNode northEast = new NeighbourTrackerNode(depth + 1, (midRight + midTop) / 2);
                northEast.quadrant = NeighbourTrackerNode.Quadrant.ne;

                if (depth == 2 && parent.quadrant == NeighbourTrackerNode.Quadrant.sw)
                {
                    GC.KeepAlive(northEast);
                }

                neighbourTracker.ReplaceNodeWithChildren(parent, northWest, southWest, southEast, northEast);

                CalculatePatchLOD(normal, step / 2, depth + 1, se, mid1, southEast);
                CalculatePatchLOD(normal, step / 2, depth + 1, mid2, nw, northWest);
                CalculatePatchLOD(normal, step / 2, depth + 1, midBottom, midLeft, southWest);
                CalculatePatchLOD(normal, step / 2, depth + 1, midRight, midTop, northEast);



            }
            else
            {
                if (depth <= maxDepth)
                {
                    AddNodeIfNotPresent(normal, step, depth, min, max);
                }
            }
        }

        private void AddNodeIfNotPresent(Vector3 normal, float step, int depth, Vector3 min, Vector3 max)
        {
            //don't build if already under way.
            Vector3 mid = (min + max) / 2;

            if (nodesBeingBuilt.ContainsKey(mid))
                return;

            if (activePatches.ContainsKey(mid))
            {
                PlanetNode node = activePatches[mid];
                node.remove = false;
                return;
            }


            var patchBeingBuilt = new PatchMinMax(min, max);
            nodesBeingBuilt.Add(mid, patchBeingBuilt);
            PlanetBuilder.Enqueue(testEffect, module, this, depth, min, max, step, normal, radius);


        }

        private float DistanceToPatch(Vector3 min, Vector3 max, float radius)
        {
            Vector3 mid1 = (min + max) / 2;

            if (mid1 == Vector3.Zero)
                mid1.Z = min.Z;

            Vector3 surfaceMidPoint = Vector3.Transform(Vector3.Normalize(mid1) * radius, Transform.WorldMatrix);
            return surfaceMidPoint.Length();
        }

        public void Update(GameTime gameTime)
        {
            if (orbitEnabled)
                CalculateOrbit(gameTime);

            Vector3d planetCenter = GetComponent<HighPrecisionPosition>().Position;

            if (HasAtmosphere)
            {
                atmosphere.GetComponent<HighPrecisionPosition>().Position = planetCenter;
                atmosphere.Update(SolarSystem.GetSun().LightDirection, Vector3.Zero);

                atmosphericScatteringHelper.Update((Vector3.Zero - Transform.WorldMatrix.Translation).Length(),
                SolarSystem.GetSun().LightDirection, Vector3.Zero - Transform.WorldMatrix.Translation);

            }




            foreach (GameObject.GameObject child in Children)
            {
                CalculateChildMovement(gameTime, child, planetCenter);
            }


            Vector3 toCenterOfPlanet = Transform.WorldMatrix.Translation;
            float distanceToCenterOfPlanet = toCenterOfPlanet.Length();
            float surfaceDistance = distanceToCenterOfPlanet - radius;
            float farPlaneMultiplier = MonoMathHelper.MapFloatRange(radius, radius * 2, 0.3f, 1f, surfaceDistance);
            GenerateCustomProjectionMatrix(distanceToCenterOfPlanet * farPlaneMultiplier);
            var frustrum = new BoundingFrustum(SystemCore.ActiveCamera.View * customProjection);

            int activeCount = 0;


            foreach (PlanetNode node in activePatches.Values)
            {
                node.Update();

                List<NeighbourTracker.Connection> connections = neighbourTracker.GetConnections(node);
                Color nodeQuadrantColor = Color.Red;

                if (neighbourTracker.nodeDictionary.ContainsKey(node.GetKeyPoint()))
                {
                    NeighbourTrackerNode trackerNode = neighbourTracker.nodeDictionary[node.GetKeyPoint()];

                    if (trackerNode.quadrant == NeighbourTrackerNode.Quadrant.ne)
                        nodeQuadrantColor = Color.White;
                    if (trackerNode.quadrant == NeighbourTrackerNode.Quadrant.nw)
                        nodeQuadrantColor = Color.Green;
                    if (trackerNode.quadrant == NeighbourTrackerNode.Quadrant.se)
                        nodeQuadrantColor = Color.Pink;
                    if (trackerNode.quadrant == NeighbourTrackerNode.Quadrant.sw)
                        nodeQuadrantColor = Color.Yellow;

                    DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(node.GetSurfaceMidPoint(), 200f), nodeQuadrantColor);
                }



                foreach (NeighbourTracker.Connection conn in connections)
                {
                    DebugShapeRenderer.AddLine(node.GetSurfaceMidPoint(), Vector3.Transform(Vector3.Normalize(conn.node.keyPoint) * radius, Transform.WorldMatrix), Color.Blue);
                }

                //all nodes are flagged for removal every frame. 
                //The LOD calculation will unflag if nodes should be kept.
                node.remove = true;

                if (!frustrum.Intersects(node.boundingSphere))
                {
                    node.Disable();
                }
                else
                {
                    node.Enable();
                    activeCount++;
                }

            }



            neighbourTracker.ClearAllConnections();
            ReinitialiseTracker();

            for (int i = 0; i < rootNodes.Count; i++)
            {
                PlanetNode root = rootNodes[i];

                NeighbourTrackerNode nodeTracker = null;
                if (neighbourTracker.nodeDictionary.ContainsKey(root.GetKeyPoint()))
                    nodeTracker = neighbourTracker.nodeDictionary[root.GetKeyPoint()];

                CalculatePatchLOD(root.normal, root.step, root.depth, root.min, root.max, nodeTracker);
            }




            //removes nodes that have not had their flags refreshed by the LOD pass
            RemoveStaleNodes();

            int patchCountPerFrame = 10;
            for (int i = 0; i < patchCountPerFrame; i++)
            {
                PlanetNode finishedNode;
                if (PlanetBuilder.GetBuiltNodes(Name, out finishedNode))
                {
                    AddPatch(finishedNode);
                }
            }

            positionLastFrame = GetComponent<HighPrecisionPosition>().Position;


        }

        private void CalculateChildMovement(GameTime gameTime, GameObject.GameObject child, Vector3d planetCenter)
        {
            var highPrecisionComponent = child.GetComponent<HighPrecisionPosition>();
            Vector3d movementLastFrame = planetCenter - positionLastFrame;
            highPrecisionComponent.Position += movementLastFrame;

            //we want to rotate the high precision component around the up vector, around the planet center.
            if (GetComponent<RotatorComponent>() != null)
            {
                double angleRotatedLastFrame = GetComponent<RotatorComponent>().RotationSpeed *
                                               gameTime.ElapsedGameTime.TotalMilliseconds;


                double s = System.Math.Sin(angleRotatedLastFrame);
                double c = System.Math.Cos(angleRotatedLastFrame);
                Vector3d shipPos = highPrecisionComponent.Position;
                shipPos.X -= planetCenter.X;
                shipPos.Z -= planetCenter.Z;

                double xNew = shipPos.X * c + shipPos.Z * s;
                double zNew = -shipPos.X * s + shipPos.Z * c;

                shipPos.X = xNew + planetCenter.X;
                shipPos.Z = zNew + planetCenter.Z;

                highPrecisionComponent.Position = shipPos;

                child.Transform.Rotate(Vector3.Up, (float)angleRotatedLastFrame);
            }
        }

        private void CalculateOrbit(GameTime gameTime)
        {
            //we're orbiting another body.
            if (orbitBody != null)
            {
                CalcOrbit(gameTime, orbitBody.GetComponent<HighPrecisionPosition>().Position);
            }
            else //we're orbiting an arbitrary point.
            {
                CalcOrbit(gameTime, positionToOrbit);
            }
        }

        private void CalcOrbit(GameTime gameTime, Vector3d posToOrbit)
        {

            Vector3d newPos = new Vector3d(posToOrbit.X + (orbitRadius * System.Math.Sin(orbitAngle)), posToOrbit.Y,
                posToOrbit.Z + (orbitRadius * System.Math.Cos(orbitAngle)));

            orbitAngle += orbitSpeed;

            //move us in the direction of the orbit.
            Transform.SetPosition(newPos);
        }

        private void RemoveStaleNodes()
        {
            List<PlanetNode> nodesToRemove = new List<PlanetNode>();
            foreach (PlanetNode n in activePatches.Values)
            {
                if (n.remove)
                    nodesToRemove.Add(n);
            }

            if (PlanetBuilder.GetBuiltNodesQueueSize(Name) == 0 && PlanetBuilder.GetQueueSize() == 0)
            {
                foreach (PlanetNode n in nodesToRemove)
                    RemovePatch(n);

                nodesToRemove.Clear();
            }
        }

        private void AddPatch(PlanetNode finishedNode)
        {
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(finishedNode);
            Vector3 mid = (finishedNode.min + finishedNode.max) / 2;
            activePatches.Add(mid, finishedNode);
            nodesBeingBuilt.Remove(mid);


        }

        private void RemovePatch(PlanetNode currentNode)
        {
            Vector3 mid = (currentNode.min + currentNode.max) / 2;
            activePatches.Remove(mid);
            SystemCore.GameObjectManager.RemoveObject(currentNode);
        }

        public bool Enabled
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public int UpdateOrder
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> UpdateOrderChanged;

        internal void AddToInfluence(GameObject.GameObject gameObject)
        {
            if (!Children.Contains(gameObject))
            {

                Children.Add(gameObject);
                if (gameObject is Ship)
                {
                    ((Ship)gameObject).SetInOrbit(this);
                }
            }
        }

        internal void RemoveFromInfluence(GameObject.GameObject gameObject)
        {
            if (Children.Contains(gameObject))
            {
                Children.Remove(gameObject);
                if (gameObject is Ship)
                {
                    ((Ship)gameObject).ExitedOrbit();
                }
            }
        }

        internal PlanetNode DetermineHitNode(Ray ray)
        {
            List<PlanetNode> hitNodes = new List<PlanetNode>();
            foreach (var activePatch in activePatches.Values)
            {
                BoundingSphere sphere = activePatch.boundingSphere;
                if (ray.Intersects(sphere).HasValue)
                    hitNodes.Add(activePatch);
            }

            PlanetNode closest = null;
            float closestDistance = float.MaxValue;
            foreach (PlanetNode n in hitNodes)
            {

                float distanceToNode = (n.boundingSphere.Center - ray.Position).Length();
                if (distanceToNode < closestDistance)
                {
                    closest = n;
                    closestDistance = distanceToNode;
                }
            }


            return closest;

        }
    }


}
