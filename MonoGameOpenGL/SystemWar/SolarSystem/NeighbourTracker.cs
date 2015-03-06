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
                if (northConnections[0].node.side == nw.side)
                {

                    MakeConnection(nw, northConnections[0].node, ConnectionDirection.north);
                    MakeConnection(ne, northConnections[0].node, ConnectionDirection.north);
                }
                else
                {
                    //we're crossing over to another quad tree.
                    MakeConnection(nw, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.west);
                    MakeConnection(ne, northConnections[0].node, ConnectionDirection.north, ConnectionDirection.west);
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

                }
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

                if (westConnections[0].node.side == nw.side)
                {
                    MakeConnection(sw, westConnections[0].node, ConnectionDirection.west);
                    MakeConnection(nw, westConnections[0].node, ConnectionDirection.west);
                }
                else
                {
                    MakeConnection(sw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.north);
                    MakeConnection(nw, westConnections[0].node, ConnectionDirection.west, ConnectionDirection.north);
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
                }
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
            if(connections == null)
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

        public void ThreadSafeNotifyOfAdjustedEdge(Vector3 keypoint, ConnectionDirection dir, int lodJump)
        {
            lock (adjustedEdges)
            {
                if (adjustedEdges.ContainsKey(keypoint))
                    adjustedEdges[keypoint].Add(new ConnectionType(dir,lodJump));
                else
                {
                    adjustedEdges.Add(keypoint, new List<ConnectionType>());
                    adjustedEdges[keypoint].Add(new ConnectionType(dir,lodJump));
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