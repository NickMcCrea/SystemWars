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
            nodesAwaitingBuilding.Enqueue(new PlanetNode(effect, module, rootObject, parent, min, max, step, normal, sphereSize));
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

            //right
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



            //to generate a node, we need...
            //min, max, normal, radius, depth of node in the tree, vectorspacing



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

        private bool ShouldMerge(Vector3 min, Vector3 max, float radius, int depth)
        {
            if (depth == 1)
                return false;

            float adjustedDistance = mergeDistance;
            for (int i = 1; i < depth; i++)
                adjustedDistance *= 0.5f;

            if (DistanceToPatch(min, max, radius) > (adjustedDistance))
                return true;
            return false;
        }

        private float DistanceToPatch(Vector3 min, Vector3 max, float radius)
        {
            Vector3 mid1 = (min + max) / 2;
            Vector3 surfaceMidPoint = Vector3.Transform(Vector3.Normalize(mid1) * radius, Transform.WorldMatrix);
            return surfaceMidPoint.Length();
        }

        public void Update(GameTime gameTime)
        {

            ICamera activeCamera = SystemCore.ActiveCamera;

            for (int i = 0; i < activePatches.Count; i++)
            {
                PlanetNode currentNode = activePatches[i];
                currentNode.UpdatePosition();


                if (ShouldSplit(currentNode.min, currentNode.max, radius, currentNode.depth))
                {

                    //todo - don't remove node until children generated.
                    FormChildNodes(currentNode);

                }

                if (ShouldMerge(currentNode.min, currentNode.max, radius, currentNode.depth))
                {
                    bool allChildrenPresent = true;
                    foreach (GameObject.GameObject obj in currentNode.Parent.Children)
                    {
                        PlanetNode node = obj as PlanetNode;
                        if (!node.built)
                            allChildrenPresent = false;
                    }

                    if (allChildrenPresent)
                    {
                        //PlanetBuilder.Enqueue(currentNode.Parent);
                        //foreach (GameObject.GameObject obj in currentNode.Parent.Children)
                        //{
                        //    PlanetNode node = obj as PlanetNode;
                        //    RemovePatch(node);
                        //}
                    }

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

            currentNode.Children.Add(a);
            currentNode.Children.Add(b);
            currentNode.Children.Add(c);
            currentNode.Children.Add(d);

            RemovePatch(currentNode);
        }

        private void FormParentNode(List<PlanetNode> children)
        {


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
