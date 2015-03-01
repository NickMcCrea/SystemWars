using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameEngineCore.Procedural
{
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

        public static void Enqueue(Effect effect, IModule module, Planet rootObject,int depth, Vector3 min, Vector3 max, float step, Vector3 normal, float sphereSize)
        {
            var node = new PlanetNode(effect, module, rootObject, depth, min, max, step, normal, sphereSize);
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
}