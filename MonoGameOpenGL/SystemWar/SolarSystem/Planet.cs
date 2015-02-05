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
        private static ConcurrentQueue<PlanetQuadTreeNode> nodesAwaitingBuilding;
        private static ConcurrentQueue<PlanetQuadTreeNode> finishedNodes;

        private static volatile bool quit = false;
        private static int numThreads = 2;

        static PlanetBuilder()
        {
            nodesAwaitingBuilding = new ConcurrentQueue<PlanetQuadTreeNode>();
            finishedNodes = new ConcurrentQueue<PlanetQuadTreeNode>();
            for (int i = 0; i < numThreads; i++)
            {
                Thread buildThread = new Thread(Update);
                buildThread.Start();
            }
            SystemCore.Game.Exiting += (x, y) => { quit = true; };

        }

        public static void Enqueue(Effect effect, IModule module, Planet rootObject, PlanetQuadTreeNode parent, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize)
        {
            nodesAwaitingBuilding.Enqueue(new PlanetQuadTreeNode(effect, module, rootObject, parent, min, max, step, normal, sphereSize));
        }

        public static void Update()
        {
            while (!quit)
            {
                PlanetQuadTreeNode node;
                if (nodesAwaitingBuilding.TryDequeue(out node))
                {
                    node.BuildGeometry();
                    finishedNodes.Enqueue(node);
                }
                Thread.Sleep(10);
            }
        }

        public static bool GetBuiltNodes(out PlanetQuadTreeNode finishedNode)
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
        public List<PlanetQuadTreeNode> activeNodes;
        public Color SeaColor;
        public Color LandColor;
        public Color MountainColor;
        public int planetId;
        private static int planetIdList;
        public int BuildCountPerSecond;
        public int BuildTally;
        private TimeSpan lastClearTime;
        public bool visualisePatches = false;

        public Planet(string name, Vector3d position, IModule module, Effect testEffect, float radius, Color sea, Color land, Color mountains)
        {
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
            activeNodes = new List<PlanetQuadTreeNode>();

            float vectorSpacing = 1f;
            float cubeVerts = 21;
            float sphereSize = radius;


            //PlanetBuilder.Enqueue(testEffect, module, this, new Vector3(-cubeVerts/2, cubeVerts/2 - 1, -cubeVerts/2),
            //    new Vector3(cubeVerts/2, cubeVerts/2 - 1, cubeVerts/2), vectorSpacing, Vector3.Up, sphereSize);

            //PlanetBuilder.Enqueue(testEffect, module, this, new Vector3(-cubeVerts/2, -cubeVerts/2, -cubeVerts/2),
            //    new Vector3(cubeVerts/2, -cubeVerts/2, cubeVerts/2), vectorSpacing, Vector3.Down, sphereSize);

            //PlanetBuilder.Enqueue(testEffect, module, this, new Vector3(-cubeVerts/2, -cubeVerts/2, -cubeVerts/2),
            //    new Vector3(cubeVerts/2, cubeVerts/2, cubeVerts/2), vectorSpacing, Vector3.Forward, sphereSize);

            //PlanetBuilder.Enqueue(testEffect, module, this, new Vector3(-cubeVerts/2, -cubeVerts/2, cubeVerts/2 - 1),
            //    new Vector3(cubeVerts/2, cubeVerts/2, cubeVerts/2 - 1), vectorSpacing, Vector3.Backward, sphereSize);

            //PlanetBuilder.Enqueue(testEffect, module, this, new Vector3(-cubeVerts/2, -cubeVerts/2, -cubeVerts/2),
            //    new Vector3(-cubeVerts/2, cubeVerts/2, cubeVerts/2), vectorSpacing, Vector3.Right, sphereSize);

            //PlanetBuilder.Enqueue(testEffect, module, this, new Vector3(cubeVerts/2 - 1, -cubeVerts/2, -cubeVerts/2),
            //    new Vector3(cubeVerts/2 - 1, cubeVerts/2, cubeVerts/2), vectorSpacing, Vector3.Left, sphereSize);

            //top
            PlanetQuadTreeNode n1 = new PlanetQuadTreeNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, cubeVerts / 2 - 1, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2 - 1, cubeVerts / 2), vectorSpacing, Vector3.Up, sphereSize);
            n1.BuildGeometry();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n1);

            //bottom
            PlanetQuadTreeNode n2 = new PlanetQuadTreeNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Down, sphereSize);
            n2.BuildGeometry();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n2);


            //forward
            PlanetQuadTreeNode n3 = new PlanetQuadTreeNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Forward, sphereSize);
            n3.BuildGeometry();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n3);


            //backward
            PlanetQuadTreeNode n4 = new PlanetQuadTreeNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2 - 1), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2 - 1), vectorSpacing, Vector3.Backward, sphereSize);
            n4.BuildGeometry();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n4);

            //right
            PlanetQuadTreeNode n5 = new PlanetQuadTreeNode(testEffect, module, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(-cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Right, sphereSize);
            n5.BuildGeometry();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n5);

            //left
            PlanetQuadTreeNode n6 = new PlanetQuadTreeNode(testEffect, module, this, null, new Vector3(cubeVerts / 2 - 1, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2 - 1, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Left, sphereSize);
            n6.BuildGeometry();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n6);

            activeNodes.Add(n1);
            activeNodes.Add(n2);
            activeNodes.Add(n3);
            activeNodes.Add(n4);
            activeNodes.Add(n5);
            activeNodes.Add(n6);



            //to generate a node, we need...
            //min, max, normal, radius, depth of node in the tree, vectorspacing


        }

        private bool ShouldSplit(Vector3 min, Vector3 max, float radius, int depth)
        {
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

            for (int i = 0; i < activeNodes.Count; i++)
            {
                PlanetQuadTreeNode currentNode = activeNodes[i];
                currentNode.UpdatePosition();

                if (ShouldSplit(currentNode.min, currentNode.max, radius, currentNode.depth))
                {

                    //todo - don't remove node until children generated.
                    FormChildNodes(currentNode);
                  
                }

                if (ShouldMerge(currentNode.min, currentNode.max, radius, currentNode.depth))
                {
                 
                }

            }



            PlanetQuadTreeNode finishedNode;
            if (PlanetBuilder.GetBuiltNodes(out finishedNode))
            {
                SystemCore.GameObjectManager.AddAndInitialiseGameObject(finishedNode);
                activeNodes.Add(finishedNode);
            }

        }

        private void FormChildNodes(PlanetQuadTreeNode currentNode)
        {
            PlanetBuilder.Enqueue(testEffect, module, this, currentNode, currentNode.se, currentNode.mid1,
                currentNode.step / 2, currentNode.normal, radius);

            PlanetBuilder.Enqueue(testEffect, module, this, currentNode, currentNode.mid2, currentNode.nw,
                currentNode.step / 2, currentNode.normal, radius);

            PlanetBuilder.Enqueue(testEffect, module, this, currentNode, currentNode.midBottom, currentNode.midLeft,
                currentNode.step / 2, currentNode.normal, radius);

            PlanetBuilder.Enqueue(testEffect, module, this, currentNode, currentNode.midRight, currentNode.midTop,
                currentNode.step / 2, currentNode.normal, radius);

            activeNodes.Remove(currentNode);
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
    }


}
