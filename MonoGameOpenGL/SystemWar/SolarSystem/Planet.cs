using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using SystemWar.SolarSystem;
using BEPUutilities;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Ray = Microsoft.Xna.Framework.Ray;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace MonoGameEngineCore.Procedural
{

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

    public static class PlanetBuilder
    {
        private static ConcurrentQueue<PlanetNode> nodesAwaitingBuilding;
        private static Dictionary<string, ConcurrentQueue<PlanetNode>> finishedNodes;

        public static int GetQueueSize()
        {
            return nodesAwaitingBuilding.Count;
        }
        public static int GetBuiltNodesQueueSize(string name)
        {
            if (finishedNodes.ContainsKey(name))
                return finishedNodes[name].Count;
            return 0;
        }

        private static volatile bool quit = false;
        private static int numThreads = 3;

        static PlanetBuilder()
        {
            nodesAwaitingBuilding = new ConcurrentQueue<PlanetNode>();
            finishedNodes = new Dictionary<string, ConcurrentQueue<PlanetNode>>();

            for (int i = 0; i < numThreads; i++)
            {
                Thread buildThread = new Thread(Update);
                buildThread.Start();
            }
            SystemCore.Game.Exiting += (x, y) => { quit = true; };

        }

        public static void Enqueue(PlanetNode nodeToBuild)
        {
            if (!finishedNodes.ContainsKey(nodeToBuild.Planet.Name))
                finishedNodes.Add(nodeToBuild.Planet.Name, new ConcurrentQueue<PlanetNode>());

            nodesAwaitingBuilding.Enqueue(nodeToBuild);
        }

        public static void Enqueue(Effect effect, IModule module, Planet rootObject, PlanetNode parent, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize)
        {
            var node = new PlanetNode(effect, module, rootObject, parent, min, max, step, normal, sphereSize);
            Enqueue(node);
        }

        public static void Update()
        {
            while (!quit)
            {
                PlanetNode node;
                if (nodesAwaitingBuilding.TryDequeue(out node))
                {
                    node.BuildGeometry();
                    finishedNodes[node.Planet.Name].Enqueue(node);
                }
                Thread.Sleep(10);
            }
        }

        public static bool GetBuiltNodes(string planetName, out PlanetNode finishedNode)
        {
            if (finishedNodes.ContainsKey(planetName))
                return finishedNodes[planetName].TryDequeue(out finishedNode);

            finishedNode = null;
            return false;
        }
    }

    public class Planet : GameObject.GameObject, IUpdateable
    {
        private readonly IModule module;
        private readonly Effect testEffect;
        private readonly float radius;
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
        public bool visualisePatches = false;
        private decimal maxDepth = 8;
        private int siblingId;
        private Planet orbitBody;
        private Vector3d positionToOrbit;
        private float orbitSpeed;
        private bool orbitEnabled;

        private float angle;
        public Planet(string name, Vector3d position, IModule module, Effect testEffect, float radius, Color sea, Color land, Color mountains)
        {
            nodesBeingBuilt = new Dictionary<Vector3, PatchMinMax>();
            siblingId = 1;
            this.Name = name;
            planetId = ++planetIdList;
            this.module = module;
            this.testEffect = testEffect;
            this.radius = radius;

            AddComponent(new HighPrecisionPosition());
            AddComponent(new RotatorComponent(Vector3.Up, 0.00001f));
            Transform.SetPosition(position);

            splitDistance = radius * 4;
            mergeDistance = radius * 4.5f;

            customProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, this.radius * 10);

            this.SeaColor = sea;
            this.LandColor = land;
            this.MountainColor = mountains;

            Initialise();
        }

        private void GenerateCustomProjectionMatrix(float far)
        {
            if (far <= 0)
                far = 2;

            customProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, far);
        }

        public void Orbit(Vector3d positionToOrbit, float orbitSpeed)
        {
            this.positionToOrbit = positionToOrbit;
            this.orbitSpeed = orbitSpeed;
            orbitEnabled = true;
        }

        public void Orbit(Planet planetToOrbit, float orbitSpeed)
        {
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
            PlanetNode n1 = new PlanetNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, cubeVerts / 2 - 1, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2 - 1, cubeVerts / 2), vectorSpacing, Vector3.Up, sphereSize);
            n1.BuildGeometry();

            ////bottom
            PlanetNode n2 = new PlanetNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Down, sphereSize);
            n2.BuildGeometry();


            //forward
            PlanetNode n3 = new PlanetNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Forward, sphereSize);
            n3.BuildGeometry();


            //backward
            PlanetNode n4 = new PlanetNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2 - 1), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2 - 1), vectorSpacing, Vector3.Backward, sphereSize);
            n4.BuildGeometry();

            PlanetNode n5 = new PlanetNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(-cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Right, sphereSize);
            n5.BuildGeometry();

            //left
            PlanetNode n6 = new PlanetNode(testEffect, module, this, null, new Vector3(cubeVerts / 2 - 1, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2 - 1, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Left, sphereSize);
            n6.BuildGeometry();


            AddPatch(n1);
            AddPatch(n2);
            AddPatch(n3);
            AddPatch(n4);
            AddPatch(n5);
            AddPatch(n6);

            rootNodes.Add(n1);
            rootNodes.Add(n2);
            rootNodes.Add(n3);
            rootNodes.Add(n4);
            rootNodes.Add(n5);
            rootNodes.Add(n6);


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

        private void CalculatePatchLOD(Vector3 normal, float step, int depth, Vector3 min, Vector3 max)
        {


            //recurse down through the tree. For each node on the way down, we decide if it should split or not.
            //if it should, calculate the split and move down. Remove the node if it's currently visible.
            if (ShouldSplit(min, max, radius, depth))
            {
                Vector3 se, sw, mid1, mid2, nw, ne, midBottom, midRight, midLeft, midTop;
                PlanetNode.CalculatePatchBoundaries(normal, step, min, max, out se, out sw, out mid1, out mid2, out nw, out ne, out midBottom, out midRight, out midLeft, out midTop);


                //se mid1
                CalculatePatchLOD(normal, step / 2, depth + 1, se, mid1);

                //mid2 nw
                CalculatePatchLOD(normal, step / 2, depth + 1, mid2, nw);

                //midbottom midleft
                CalculatePatchLOD(normal, step / 2, depth + 1, midBottom, midLeft);

                //midright midtop
                CalculatePatchLOD(normal, step / 2, depth + 1, midRight, midTop);

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
                node.GetComponent<EffectRenderComponent>().Visible = true;
                node.remove = false;
                return;
            }


            var patchBeingBuilt = new PatchMinMax(min, max);
            nodesBeingBuilt.Add(mid, patchBeingBuilt);
            PlanetBuilder.Enqueue(testEffect, module, this, null, min, max, step, normal, radius);


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

            ICamera activeCamera = SystemCore.ActiveCamera;

            Vector3 toCenterOfPlanet = Transform.WorldMatrix.Translation;
            float distanceToCenterOfPlanet = toCenterOfPlanet.Length();
            float surfaceDistance = distanceToCenterOfPlanet - radius;
            float farPlaneMultiplier = MonoMathHelper.MapFloatRange(radius, radius * 2, 0.3f, 1f, surfaceDistance);
            GenerateCustomProjectionMatrix(distanceToCenterOfPlanet * farPlaneMultiplier);
            var frustrum = new BoundingFrustum(activeCamera.View * customProjection);


            foreach (PlanetNode node in activePatches.Values)
            {
                node.UpdatePosition();

                node.remove = true;

                if (node.depth == 1)
                    continue;

                //if (!frustrum.Intersects(node.boundingSphere))
                //    node.GetComponent<EffectRenderComponent>().Visible = false;
                //else
                //    node.GetComponent<EffectRenderComponent>().Visible = true;


            }

       

            for (int i = 0; i < rootNodes.Count; i++)
            {
                PlanetNode root = rootNodes[i];
                CalculatePatchLOD(root.normal, root.step, root.depth, root.min, root.max);
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
         
            var positionComponent = GetComponent<HighPrecisionPosition>();   
            Vector3d toOrbitTarget = posToOrbit - positionComponent.Position;
            double orbitRadius = toOrbitTarget.Length;

            Vector3d newPos = new Vector3d(posToOrbit.X + (orbitRadius*System.Math.Sin(angle)), posToOrbit.Y,
                positionToOrbit.Z + (orbitRadius*System.Math.Cos(angle)));

            angle += orbitSpeed;
            if (angle > 360)
                angle = 0;

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

        internal void AddToOrbit(GameObject.GameObject ship)
        {
            if (!Children.Contains(ship))
                Children.Add(ship);
        }

        internal void RemoveFromOrbit(GameObject.GameObject ship)
        {
            if (Children.Contains(ship))
                Children.Remove(ship);
        }
    }


}
