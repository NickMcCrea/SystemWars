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
                        ConnectionDirection.north);
                        MakeConnection(ne,
                            northConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.sw).node,
                            ConnectionDirection.north);
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


                MakeConnection(sw, westConnections[0].node, ConnectionDirection.west);
                MakeConnection(nw, westConnections[0].node, ConnectionDirection.west);

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
                        ConnectionDirection.west);
                        MakeConnection(nw,
                            westConnections.Find(x => x.node.quadrant == NeighbourTrackerNode.Quadrant.nw).node,
                            ConnectionDirection.west);
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

        internal void AddUnconnectedNode(NeighbourTrackerNode node)
        {
            connections.Add(node, new List<Connection>());
        }
    }
}