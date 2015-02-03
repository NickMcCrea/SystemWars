using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using BEPUutilities;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private static volatile bool quit = false;
        private static int numThreads = 2;

        static PlanetBuilder()
        {
            nodesAwaitingBuilding = new ConcurrentQueue<PlanetQuadTreeNode>();

            for (int i = 0; i < numThreads; i++)
            {
                Thread buildThread = new Thread(Update);
                buildThread.Start();
            }
            SystemCore.Game.Exiting += (x,y) => { quit = true; };

        }

        public static void Enqueue(PlanetQuadTreeNode node)
        {
            nodesAwaitingBuilding.Enqueue(node);
        }

        public static void Update()
        {
            while (!quit)
            {  
                PlanetQuadTreeNode node;
                if (nodesAwaitingBuilding.TryDequeue(out node))
                {
                    node.BuildGeometry();
                }
                Thread.Sleep(10);
            }
        }
    }

    public class Planet : GameObject.GameObject
    {
        private readonly IModule module;
        private readonly Effect testEffect;
        private readonly float radius;
        private float splitDistance;
        private float mergeDistance;
        public Matrix customProjection;
        public List<PlanetQuadTreeNode> rootNodes;
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

            this.AddComponent(new HighPrecisionPosition());
            this.AddComponent(new RotatorComponent(Vector3.Up));

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
            rootNodes = new List<PlanetQuadTreeNode>();

            float vectorSpacing = 1f;
            float cubeVerts = 21;
            float sphereSize = radius;


            //top
            PlanetQuadTreeNode n1 = new PlanetQuadTreeNode(testEffect,module, 1, 1, this, null, new Vector3(-cubeVerts / 2, cubeVerts / 2 - 1, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2 - 1, cubeVerts / 2), vectorSpacing, Vector3.Up, sphereSize);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n1);

            //bottom
            PlanetQuadTreeNode n2 = new PlanetQuadTreeNode(testEffect, module, 2, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Down, sphereSize);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n2);


            //forward
            PlanetQuadTreeNode n3 = new PlanetQuadTreeNode(testEffect, module, 3, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Forward, sphereSize);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n3);


            //backward
            PlanetQuadTreeNode n4 = new PlanetQuadTreeNode(testEffect, module, 4, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2 - 1), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2 - 1), vectorSpacing, Vector3.Backward, sphereSize);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n4);

            //right
            PlanetQuadTreeNode n5 = new PlanetQuadTreeNode(testEffect, module, 5, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(-cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Right, sphereSize);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n5);

            //left
            PlanetQuadTreeNode n6 = new PlanetQuadTreeNode(testEffect, module, 6, 1, this, null, new Vector3(cubeVerts / 2 - 1, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2 - 1, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Left, sphereSize);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(n6);

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
 

        public void Update(GameTime gameTime)
        {
            foreach (PlanetQuadTreeNode n in rootNodes)
            {
                //n.UpdatePosition();
            }

        }

 
    }


}
