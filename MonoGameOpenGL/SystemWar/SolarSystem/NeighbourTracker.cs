using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.Procedural
{
    public class NeighbourTracker
    {

        public enum ConnectionDirection
        {
            north,
            south,
            east,
            west,
        }

        public struct ConnectionType
        {
            public ConnectionDirection dir;
            public int lodJump;

            public ConnectionType(ConnectionDirection direction, int lodJump)
            {
                dir = direction;
                this.lodJump = lodJump;
            }
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
        private Dictionary<Vector3, NeighbourTrackerNode> nodeDictionaryBuffer;
        private Dictionary<NeighbourTrackerNode, List<Connection>> connectionBuffer;
        private Dictionary<Vector3, List<ConnectionType>> adjustedEdges;

        public NeighbourTracker()
        {
            connections = new Dictionary<NeighbourTrackerNode, List<Connection>>();
            nodeDictionary = new Dictionary<Vector3, NeighbourTrackerNode>();
            nodeDictionaryBuffer = new Dictionary<Vector3, NeighbourTrackerNode>();
            connectionBuffer = new Dictionary<NeighbourTrackerNode, List<Connection>>();
            adjustedEdges = new Dictionary<Vector3, List<ConnectionType>>();
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
            if (a == null || b == null)
                return;

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
            if (a == null || b == null)
                return;

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


            HandleNorthConnections(nodeToReplace, nw, ne, northConnections);
            HandleSouthConnections(nodeToReplace, sw, se, southConnections);
            HandleWestConnections(nodeToReplace, nw, sw, westConnections);
            HandleEastConnections(nodeToReplace, ne, se, eastConnections);

            GC.KeepAlive(parentConnections);
            RemoveAllConnections(nodeToReplace);

        }

        private void HandleNorthConnections(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode nw, NeighbourTrackerNode ne, List<Connection> northConnections)
        {
            if (northConnections.Count == 1)
            {
                if (northConnections[0].node.side == nw.side)
                {

                    MakeConnection(nw, northConnections[0].node, ConnectionDirection.north);
                    MakeConnection(ne, northConnections[0].node, ConnectionDirection.north);
                }
                else
                {
                    //ConnectNorthSideEdges(nodeToReplace, nw, ne, northConnections);
                }
            }
            if (northConnections.Count == 2)
            {
                //connections on same side of cube
                if (northConnections[0].node.side == nw.side)
                {
                    MakeConnection(nw,
                        northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                        ConnectionDirection.north);
                    MakeConnection(ne,
                        northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                        ConnectionDirection.north);
                }
                else
                {
                    //ConnectMultipleNorthEdges(nodeToReplace, nw, ne, northConnections);
                }
            }
        }

        private void HandleWestConnections(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode nw, NeighbourTrackerNode sw, List<Connection> westConnections)
        {
            if (westConnections.Count == 1)
            {

                if (westConnections[0].node.side == nw.side)
                {
                    MakeConnection(sw, westConnections[0].node, ConnectionDirection.west);
                    MakeConnection(nw, westConnections[0].node, ConnectionDirection.west);
                }
                else
                {
                    //ConnectWestSideEdges(nodeToReplace, nw, sw, westConnections);
                }

            }
            if (westConnections.Count == 2)
            {

                if (westConnections[0].node.side == nw.side)
                {

                    MakeConnection(sw,
                        westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                        ConnectionDirection.west);
                    MakeConnection(nw,
                        westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                        ConnectionDirection.west);
                }
                else
                {
                    //ConnectMultipleWestSideEdges(nodeToReplace, nw, sw, westConnections);
                }
            }
        }

        private void HandleSouthConnections(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode sw, NeighbourTrackerNode se, List<Connection> southConnections)
        {
            if (southConnections.Count == 1)
            {
                if (southConnections[0].node.side == sw.side)
                {

                    MakeConnection(sw, southConnections[0].node, ConnectionDirection.south);
                    MakeConnection(se, southConnections[0].node, ConnectionDirection.south);
                }
                else
                {
                    //ConnectSouthSideEdges(nodeToReplace, sw, se, southConnections);
                }
            }
            if (southConnections.Count == 2)
            {
                if (southConnections[0].node.side == sw.side)
                {

                    MakeConnection(sw, southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node, ConnectionDirection.south);
                    MakeConnection(se, southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node, ConnectionDirection.south);
                }
                else
                {
                    //ConnectMultipleSouthEdges(nodeToReplace, sw, se, southConnections);
                }
            }

        }

        private void HandleEastConnections(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode ne, NeighbourTrackerNode se, List<Connection> eastConnections)
        {
            if (eastConnections.Count == 1)
            {
                if (eastConnections[0].node.side == ne.side)
                {
                    MakeConnection(se, eastConnections[0].node, ConnectionDirection.east);
                    MakeConnection(ne, eastConnections[0].node, ConnectionDirection.east);
                }
                else
                {
                    //ConnectEastSideEdges(nodeToReplace, ne, se, eastConnections);
                }

            }
            if (eastConnections.Count == 2)
            {

                if (eastConnections[0].node.side == ne.side)
                {

                    MakeConnection(se,
                        eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                        ConnectionDirection.east);
                    MakeConnection(ne,
                        eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                        ConnectionDirection.east);
                }
                else
                {
                    //ConnectMultipleEastSideEdges(nodeToReplace, ne, se, eastConnections);
                }
            }
        }

        private void ConnectMultipleWestSideEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode nw, NeighbourTrackerNode sw, List<Connection> westConnections)
        {
            //top connects on west to left's north
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.top)
            {
                MakeConnection(sw,
                westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                ConnectionDirection.west, ConnectionDirection.north);
                MakeConnection(nw,
                    westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                    ConnectionDirection.west, ConnectionDirection.north);
            }

            //bottom to right
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.bottom)
            {
                MakeConnection(sw,
                westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                ConnectionDirection.west, ConnectionDirection.south);
                MakeConnection(nw,
                    westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                    ConnectionDirection.west, ConnectionDirection.south);
            }

            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front
              || nodeToReplace.side == NeighbourTrackerNode.CubeSide.left
              || nodeToReplace.side == NeighbourTrackerNode.CubeSide.right
              || nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {

                MakeConnection(sw,
               westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
               ConnectionDirection.west, ConnectionDirection.east);
                MakeConnection(nw,
                    westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                    ConnectionDirection.west, ConnectionDirection.south);

            }
        }

        private void ConnectWestSideEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode nw, NeighbourTrackerNode sw, List<Connection> westConnections)
        {
            //top to left
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.top)
            {
                MakeConnection(sw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.north);
                MakeConnection(nw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.north);
            }

            //bottom to right
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.bottom)
            {
                MakeConnection(sw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.south);
                MakeConnection(nw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.south);
            }

            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front
                || nodeToReplace.side == NeighbourTrackerNode.CubeSide.left
                || nodeToReplace.side == NeighbourTrackerNode.CubeSide.right
                || nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {
                MakeConnection(sw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.east);
                MakeConnection(nw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.east);
            }
        }

        private void ConnectMultipleEastSideEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode ne, NeighbourTrackerNode se, List<Connection> eastConnections)
        {
            //top connects on east to right's north
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.top)
            {
                MakeConnection(se,
                eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                ConnectionDirection.east, ConnectionDirection.north);
                MakeConnection(ne,
                    eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                    ConnectionDirection.east, ConnectionDirection.north);
            }

            //bottom to left
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.bottom)
            {
                MakeConnection(se,
                eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                ConnectionDirection.east, ConnectionDirection.south);
                MakeConnection(ne,
                    eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                    ConnectionDirection.east, ConnectionDirection.south);
            }

            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front
              || nodeToReplace.side == NeighbourTrackerNode.CubeSide.left
              || nodeToReplace.side == NeighbourTrackerNode.CubeSide.right
              || nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {

                MakeConnection(se,
               eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
               ConnectionDirection.east, ConnectionDirection.west);
                MakeConnection(ne,
                    eastConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                    ConnectionDirection.east, ConnectionDirection.west);

            }
        }

        private void ConnectEastSideEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode ne, NeighbourTrackerNode se, List<Connection> eastConnections)
        {
            //top to right
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.top)
            {
                MakeConnection(se, eastConnections[0].node, ConnectionDirection.east, ConnectionDirection.north);
                MakeConnection(ne, eastConnections[0].node, ConnectionDirection.east, ConnectionDirection.north);
            }

            //bottom to left
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.bottom)
            {
                MakeConnection(se, eastConnections[0].node, ConnectionDirection.east, ConnectionDirection.south);
                MakeConnection(ne, eastConnections[0].node, ConnectionDirection.east, ConnectionDirection.south);
            }

            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front
                || nodeToReplace.side == NeighbourTrackerNode.CubeSide.left
                || nodeToReplace.side == NeighbourTrackerNode.CubeSide.right
                || nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {
                MakeConnection(se, eastConnections[0].node, ConnectionDirection.east, ConnectionDirection.west);
                MakeConnection(ne, eastConnections[0].node, ConnectionDirection.east, ConnectionDirection.west);
            }
        }

        private void ConnectMultipleSouthEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode sw, NeighbourTrackerNode se, List<Connection> southConnections)
        {
            ////left side connects to bottom's east edge.
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.left)
            {
                MakeConnection(sw,
                southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                ConnectionDirection.south, ConnectionDirection.east);

                MakeConnection(se,
                    southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                    ConnectionDirection.south, ConnectionDirection.east);
            }


            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.right)
            {
                MakeConnection(sw,
                southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                ConnectionDirection.south, ConnectionDirection.west);

                MakeConnection(se,
                    southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                    ConnectionDirection.south, ConnectionDirection.west);
            }

            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front)
            {
                MakeConnection(sw,
                southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                ConnectionDirection.south, ConnectionDirection.south);

                MakeConnection(se,
                    southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                    ConnectionDirection.south, ConnectionDirection.south);
            }

            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {
                MakeConnection(sw,
                southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                ConnectionDirection.south, ConnectionDirection.north);

                MakeConnection(se,
                    southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                    ConnectionDirection.south, ConnectionDirection.north);
            }

            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.top)
            {
                MakeConnection(sw,
                southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                ConnectionDirection.south, ConnectionDirection.north);

                MakeConnection(se,
                    southConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                    ConnectionDirection.south, ConnectionDirection.north);
            }


        }

        private void ConnectSouthSideEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode sw, NeighbourTrackerNode se, List<Connection> southConnections)
        {
            //left to bottom
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.left)
            {
                MakeConnection(sw, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.east);
                MakeConnection(se, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.east);
            }
            //right to bottom
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.right)
            {
                MakeConnection(sw, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.west);
                MakeConnection(se, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.west);
            }
            //forward to bottom
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front)
            {
                MakeConnection(sw, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.south);
                MakeConnection(se, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.south);
            }
            //backward to bottom
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {
                MakeConnection(sw, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.north);
                MakeConnection(se, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.north);
            }
            //top to front
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.bottom)
            {
                MakeConnection(sw, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.north);
                MakeConnection(se, southConnections[0].node, ConnectionDirection.south, ConnectionDirection.north);
            }
        }

        private void ConnectMultipleNorthEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode nw, NeighbourTrackerNode ne, List<Connection> northConnections)
        {
            //left side connects to top's west edge.
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.left)
            {
                MakeConnection(nw,
                northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                ConnectionDirection.north, ConnectionDirection.west);
                MakeConnection(ne,
                    northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                    ConnectionDirection.north, ConnectionDirection.west);
            }
            //right side connects to top's east edge.
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.right)
            {
                MakeConnection(nw,
                northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                ConnectionDirection.north, ConnectionDirection.east);
                MakeConnection(ne,
                    northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                    ConnectionDirection.north, ConnectionDirection.east);
            }
            //front side connects to top's south edge.
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front)
            {
                MakeConnection(nw,
                northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                ConnectionDirection.north, ConnectionDirection.south);
                MakeConnection(ne,
                    northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                    ConnectionDirection.north, ConnectionDirection.south);
            }

            //back side connects to top's north edge.
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {
                MakeConnection(nw,
                northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.ne).node,
                ConnectionDirection.north, ConnectionDirection.north);
                MakeConnection(ne,
                    northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                    ConnectionDirection.north, ConnectionDirection.north);
            }

            //bottom side connects to backs south edge.
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.bottom)
            {
                MakeConnection(nw,
                northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                ConnectionDirection.north, ConnectionDirection.south);
                MakeConnection(ne,
                    northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.se).node,
                    ConnectionDirection.north, ConnectionDirection.south);
            }
        }

        private void ConnectNorthSideEdges(NeighbourTrackerNode nodeToReplace, NeighbourTrackerNode nw, NeighbourTrackerNode ne, List<Connection> northConnections)
        {
            //left to top
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.left)
            {
                MakeConnection(nw, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.west);
                MakeConnection(ne, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.west);
            }
            //right to top
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.right)
            {
                MakeConnection(nw, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.east);
                MakeConnection(ne, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.east);
            }
            //forward to top
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.front)
            {
                MakeConnection(nw, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.south);
                MakeConnection(ne, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.south);
            }
            //backward to top
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.back)
            {
                MakeConnection(nw, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.north);
                MakeConnection(ne, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.north);
            }
            //bottom to back
            if (nodeToReplace.side == NeighbourTrackerNode.CubeSide.bottom)
            {
                MakeConnection(nw, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.south);
                MakeConnection(ne, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.south);
            }
        }

        public void RemoveAllConnections(NeighbourTrackerNode nodeToReplace)
        {
            List<Connection> nodeConnections = GetConnections(nodeToReplace);

            foreach (Connection conn in nodeConnections)
            {
                connections[conn.node].RemoveAll(x => x.node == nodeToReplace);
            }
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

        public List<Connection> GetConnectionsTo(NeighbourTrackerNode nodeOfInterest)
        {
            var nodeConnections = GetConnections(nodeOfInterest);
            var connectionsTo = new List<Connection>();

            foreach (Connection conn in nodeConnections)
            {
                connectionsTo.AddRange(connections[conn.node].FindAll(x => x.node == nodeOfInterest));
            }

            return connectionsTo;
        }

        internal void CopyConnectionDataToThreadSafeBuffer()
        {
            if (connections == null)
                return;


            //we've finished generating connectivity of the quadtrees, now copy data to a buffer that can 
            //be accessed safely from other threads.
            lock (connectionBuffer)
            {
                connectionBuffer.Clear();
                foreach (KeyValuePair<NeighbourTrackerNode, List<Connection>> keyValuePair in connections)
                {
                    connectionBuffer.Add(keyValuePair.Key, new List<Connection>(keyValuePair.Value));
                }
            }

            lock (nodeDictionaryBuffer)
            {
                nodeDictionaryBuffer.Clear();
                foreach (KeyValuePair<Vector3, NeighbourTrackerNode> keyValuePair in nodeDictionary)
                {
                    nodeDictionaryBuffer.Add(keyValuePair.Key, keyValuePair.Value);
                }
            }



        }

        public List<Connection> ThreadSafeGetConnections(PlanetNode node)
        {
            lock (connectionBuffer)
            {
                lock (nodeDictionaryBuffer)
                {
                    if (!nodeDictionaryBuffer.ContainsKey(node.GetKeyPoint()))
                        return null;

                    var key = nodeDictionaryBuffer[node.GetKeyPoint()];


                    if (!connectionBuffer.ContainsKey(key))
                        return null;

                    return connectionBuffer[key];
                }
            }
        }

        public void ClearAdjustedEdgeRecords(Vector3 keyPoint)
        {
            lock (adjustedEdges)
            {
                adjustedEdges.Remove(keyPoint);
            }
        }

        public void ThreadSafeNotifyOfAdjustedEdge(Vector3 keypoint, ConnectionDirection dir, int lodJump)
        {
            lock (adjustedEdges)
            {
                if (adjustedEdges.ContainsKey(keypoint))
                    adjustedEdges[keypoint].Add(new ConnectionType(dir, lodJump));
                else
                {
                    adjustedEdges.Add(keypoint, new List<ConnectionType>());
                    adjustedEdges[keypoint].Add(new ConnectionType(dir, lodJump));
                }
            }
        }

        /// <summary>
        /// Tells us whether the adjacent edge to our patch has been adjusted to a higher LOD, so we can compensate
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public ConnectionType ThreadSafeGetConnectingEdgeOfNeighbourPatch(Vector3 source, Vector3 target)
        {

            //find the connection to me in the buffer.
            var connectionToMe = connectionBuffer[nodeDictionaryBuffer[target]].Find(x => x.node.keyPoint == source);

            //return the edge connection info
            if (adjustedEdges.ContainsKey(target))
                return adjustedEdges[target].Find(x => x.dir == connectionToMe.direction);

            return new ConnectionType();

        }
    }
}