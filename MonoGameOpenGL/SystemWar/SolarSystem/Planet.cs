using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
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
        private static ConcurrentQueue<PlanetNode> finishedNodes;

        private static volatile bool quit = false;
        private static int numThreads = 2;

        static PlanetBuilder()
        {
            nodesAwaitingBuilding = new ConcurrentQueue<PlanetNode>();
            finishedNodes = new ConcurrentQueue<PlanetNode>();
            for (int i = 0; i < numThreads; i++)
            {
                Thread buildThread = new Thread(Update);
                buildThread.Start();
            }
            SystemCore.Game.Exiting += (x, y) => { quit = true; };

        }

        public static void Enqueue(PlanetNode nodeToBuild)
        {
            nodesAwaitingBuilding.Enqueue(nodeToBuild);
        }

        public static void Enqueue(Effect effect, IModule module, Planet rootObject, PlanetNode parent, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize)
        {
            var node = new PlanetNode(effect, module, rootObject, parent, min, max, step, normal, sphereSize);
            nodesAwaitingBuilding.Enqueue(node);
        }

        public static void Update()
        {
            while (!quit)
            {
                PlanetNode node;
                if (nodesAwaitingBuilding.TryDequeue(out node))
                {
                    node.BuildGeometry();
                    finishedNodes.Enqueue(node);
                }
                Thread.Sleep(10);
            }
        }

        public static bool GetBuiltNodes(out PlanetNode finishedNode)
        {
            return finishedNodes.TryDequeue(out finishedNode);
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
        public List<PlanetNode> activePatches;
        private List<PlanetNode> rootNodes;
        private List<PatchMinMax> nodesBeingBuilt;
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

        public Planet(string name, Vector3d position, IModule module, Effect testEffect, float radius, Color sea, Color land, Color mountains)
        {
            nodesBeingBuilt = new List<PatchMinMax>();
            siblingId = 1;
            this.Name = name;
            planetId = ++planetIdList;
            this.module = module;
            this.testEffect = testEffect;
            this.radius = radius;

            AddComponent(new HighPrecisionPosition());
            //AddComponent(new RotatorComponent(Vector3.Up));

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

        private void Initialise()
        {
            activePatches = new List<PlanetNode>();
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



            //left
            PlanetNode n6 = new PlanetNode(testEffect, module, this, null, new Vector3(cubeVerts / 2 - 1, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2 - 1, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Left, sphereSize);
            n6.BuildGeometry();


            //AddPatch(n1);
            //AddPatch(n2);
            AddPatch(n3);
            //AddPatch(n4);
            //AddPatch(n5);
           // AddPatch(n6);

            //rootNodes.Add(n1);
            //rootNodes.Add(n2);
            rootNodes.Add(n3);
            //rootNodes.Add(n4);
            //rootNodes.Add(n5);
           // rootNodes.Add(n6);


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

                //this node should be invisible. If it's in the active list, remove it.
                RemoveNodeIfPresent(normal, step, depth, min, max);


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
                    //ClearAnyChildNodes(normal, step, depth, min, max);
                }
            }
        }

    

        private void ClearAnyChildNodes(Vector3 normal, float step, int depth, Vector3 min, Vector3 max)
        {
            if (depth > maxDepth)
                return;

            Vector3 se, sw, mid1, mid2, nw, ne, midBottom, midRight, midLeft, midTop;
            PlanetNode.CalculatePatchBoundaries(normal, step, min, max, out se, out sw, out mid1, out mid2, out nw, out ne, out midBottom, out midRight, out midLeft, out midTop);

            RemoveNodeIfPresent(normal, step / 2, depth + 1, se, mid1);
            ClearAnyChildNodes(normal, step / 2, depth + 1, se, mid1);

            RemoveNodeIfPresent(normal, step / 2, depth + 1, mid2, nw);
            ClearAnyChildNodes(normal, step / 2, depth + 1, mid2, nw);

            RemoveNodeIfPresent(normal, step / 2, depth + 1, midBottom, midLeft);
            ClearAnyChildNodes(normal, step / 2, depth + 1, midBottom, midLeft);

            RemoveNodeIfPresent(normal, step / 2, depth + 1, midRight, midTop);
            ClearAnyChildNodes(normal, step / 2, depth + 1, midRight, midTop);

            
        }

        private void AddNodeIfNotPresent(Vector3 normal, float step, int depth, Vector3 min, Vector3 max)
        {
            //don't build if already under way.
            for (int i = 0; i < nodesBeingBuilt.Count; i++)
            {
                if (nodesBeingBuilt[i].Max == max)
                    if (nodesBeingBuilt[i].Min == min)
                        return;
            }

            for (int i = 0; i < activePatches.Count; i++)
            {
                PlanetNode node = activePatches[i];

                if (node.min == min && node.max == max)
                {

                    if (node.depth == 1)
                    {
                        node.GetComponent<EffectRenderComponent>().Visible = true;
                        return;
                    }
                    else
                    {
                        throw new Exception("Added twice");
                    }
                }
            }

            var patchBeingBuilt = new PatchMinMax(min, max);
            nodesBeingBuilt.Add(patchBeingBuilt);
            PlanetBuilder.Enqueue(testEffect, module, this, null, min, max, step, normal, radius);
        }

        private void RemoveNodeIfPresent(Vector3 normal, float step, int depth, Vector3 min, Vector3 max)
        {

            for (int i = 0; i < activePatches.Count; i++)
            {
                PlanetNode node = activePatches[i];
                if (node.min == min && node.max == max)
                {

                    if (node.depth == 1)
                    {
                        node.GetComponent<EffectRenderComponent>().Visible = false;
                        return;
                    }
                    else
                    {
                        RemovePatch(node);
                    }
                }
            }
        }

        private float DistanceToPatch(Vector3 min, Vector3 max, float radius)
        {
            Vector3 mid1 = (min + max) / 2;
            Vector3 surfaceMidPoint = Vector3.Transform(Vector3.Normalize(mid1) * radius, Transform.WorldMatrix);
            return surfaceMidPoint.Length();
        }

        DateTime lastUpdate = DateTime.Now;
        public void Update(GameTime gameTime)
        {

            ICamera activeCamera = SystemCore.ActiveCamera;

            for (int i = 0; i < activePatches.Count; i++)
            {
                PlanetNode currentNode = activePatches[i];
                currentNode.UpdatePosition();

            }

            TimeSpan t = DateTime.Now - lastUpdate;

            if (t.TotalSeconds > 5)
            {
                lastUpdate = DateTime.Now;
                for (int i = 0; i < rootNodes.Count; i++)
                {
                    PlanetNode root = rootNodes[i];
                    CalculatePatchLOD(root.normal, root.step, root.depth, root.min, root.max);
                }
            }



            PlanetNode finishedNode;
            if (PlanetBuilder.GetBuiltNodes(out finishedNode))
            {
                AddPatch(finishedNode);
            }
        }

        private void AddPatch(PlanetNode finishedNode)
        {
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(finishedNode);
            activePatches.Add(finishedNode);

            

            //don't build if already under way.
            for (int i = 0; i < nodesBeingBuilt.Count; i++)
            {
                if (nodesBeingBuilt[i].Max == finishedNode.max)
                    if (nodesBeingBuilt[i].Min == finishedNode.min)
                        nodesBeingBuilt.Remove(nodesBeingBuilt[i]);
            }
        }

        private void RemovePatch(PlanetNode currentNode)
        {
            activePatches.Remove(currentNode);
            SystemCore.GameObjectManager.RemoveObject(currentNode);
        }

        private void FormChildNodes(PlanetNode currentNode)
        {

            PlanetNode a = new PlanetNode(testEffect, module, this, currentNode, currentNode.se, currentNode.mid1,
                currentNode.step / 2, currentNode.normal, radius);

            PlanetNode b = new PlanetNode(testEffect, module, this, currentNode, currentNode.mid2, currentNode.nw,
                currentNode.step / 2, currentNode.normal, radius);

            PlanetNode c = new PlanetNode(testEffect, module, this, currentNode, currentNode.midBottom, currentNode.midLeft,
                currentNode.step / 2, currentNode.normal, radius);

            PlanetNode d = new PlanetNode(testEffect, module, this, currentNode, currentNode.midRight, currentNode.midTop,
                currentNode.step / 2, currentNode.normal, radius);

            PlanetBuilder.Enqueue(a);
            PlanetBuilder.Enqueue(b);
            PlanetBuilder.Enqueue(c);
            PlanetBuilder.Enqueue(d);

            RemovePatch(currentNode);
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
    }


}
