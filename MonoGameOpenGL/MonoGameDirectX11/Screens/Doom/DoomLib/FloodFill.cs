using Kaitai;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using NickLib.Pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameDirectX11.Screens.Doom.DoomLib
{
    public class DoomFloodFill
    {
        public bool pause = true;
        public bool complete = false;
        public BucketPartition partition;

        float stepSize = 1f;
        public List<NavigationNode> positions;
        private List<DoomLine> levelLineGeometry;
        public Queue<NavigationNode> positionsToSearch = new Queue<NavigationNode>();
        public NavigationNode next;
        public List<NavigationNode> justAdded = new List<NavigationNode>();
        private DoomMapHandler mapHandler;

        public DoomFloodFill(DoomMapHandler mapHandler, List<DoomLine> levelLines, Vector3 startPoint, DoomWad.Vertexes vertexes, float scale, float xOffset, float zOffset)
        {
            positions = new List<NavigationNode>();
            levelLineGeometry = levelLines;
            this.mapHandler = mapHandler;


            float startX = vertexes.Entries.Min(x => x.X - 10) / scale + xOffset;
            float startZ = vertexes.Entries.Min(x => x.Y - 10) / scale + zOffset;

            float maxX = vertexes.Entries.Max(x => x.X - 10) / scale + xOffset;
            float maxZ = vertexes.Entries.Max(x => x.Y - 10) / scale + zOffset;

            float width = maxX - startX;
            float height = maxZ - startZ;

            int bucketSize = 5;
            int numofBucketsX = (int)width / bucketSize + 2;
            int numofBucketsZ = (int)height / bucketSize + 2;

            partition = new BucketPartition(startX - 1, startZ - 1, bucketSize, numofBucketsX, numofBucketsZ);

            NavigationNode p = new NavigationNode() { WorldPosition = startPoint };
            p.Navigable = true;
            positions.Add(p);
            positionsToSearch.Enqueue(p);

            partition.AddNavigationNode(p);

            foreach (DoomLine l in levelLines)
                partition.AddDoomLine(l);

        }


        public void ConnectPoints(NavigationNode a, NavigationNode b)
        {
            a.Neighbours.Add(b);
            b.Neighbours.Add(a);
        }

        public void Update()
        {
            if (pause)
                return;

            Step();
        }

        public void Step()
        {
            if (complete)
                return;

            justAdded.Clear();

            if (positionsToSearch.Count == 0)
            {
                complete = true;
                return;
            }

            NavigationNode current = positionsToSearch.Dequeue();

            if (positionsToSearch.Count > 0)
                next = positionsToSearch.Peek();

            //search north, south east and west, then intersect each line with the map.
            NavigationNode north = GeneratePoint(current, new Vector3(stepSize, 0, 0));
            ConnectIfNotIntersect(current, north);

            NavigationNode south = GeneratePoint(current, new Vector3(-stepSize, 0, 0));
            ConnectIfNotIntersect(current, south);

            NavigationNode east = GeneratePoint(current, new Vector3(0, 0, stepSize));
            ConnectIfNotIntersect(current, east);

            NavigationNode west = GeneratePoint(current, new Vector3(0, 0, -stepSize));
            ConnectIfNotIntersect(current, west);

            current.done = true;


        }

        private NavigationNode GeneratePoint(NavigationNode current, Vector3 offset)
        {
            Vector3 newPos = current.WorldPosition + offset;

            //does this already exist? if so return it
            NavigationNode test = positions.Find(x => x.WorldPosition == newPos);

            if (test != null)
                return test;

            //make a new point and add it to the queue
            NavigationNode newPoint = new NavigationNode() { WorldPosition = newPos };
            newPoint.Navigable = true;
            return newPoint;
        }

        private void ConnectIfNotIntersect(NavigationNode current, NavigationNode adjacent)
        {


            bool exists = false;

            //already done
            if (current.Neighbours.Contains(adjacent))
                return;





            //if there's no intersection, , and there isn't a link already, 
            //add a link, add the new position, and add to the queue for future search
            if (!partition.IntersectsALevelSegment(current.WorldPosition, adjacent.WorldPosition, false))
            {

                //check if we cross a hazard line
                if (CrossesHazardLine(current, adjacent))
                {
                    return;
                    if (current.Cost == 0)
                        adjacent.Cost = 100;
                    if (current.Cost == 100)
                        adjacent.Cost = 0;
                }
                else
                {
                    adjacent.Cost = current.Cost;
                }

                //don't place points too close to a wall, then we can't navigate them
                if (partition.DistanceFromClosestWall(adjacent.WorldPosition) < 0.2f)
                    return;

                ConnectPoints(current, adjacent);

                if (!adjacent.done)
                {
                    //add the point if it doesn't already exist
                    NavigationNode test = positions.Find(x => MonoMathHelper.AlmostEquals(x.WorldPosition, adjacent.WorldPosition, 0.01f));
                    if (test == null)
                    {
                        positions.Add(adjacent);
                        positionsToSearch.Enqueue(adjacent);
                        justAdded.Add(adjacent);
                        partition.AddNavigationNode(adjacent);
                    }
                    else
                    {
                        return;
                    }

                }
            }




        }

        private bool CrossesHazardLine(NavigationNode current, NavigationNode adjacent)
        {
            if (mapHandler == null)
                return false;


            foreach (DoomLine hazardLine in mapHandler.HazardLines)
            {
                if (MonoMathHelper.LineIntersection(current.WorldPosition.ToVector2XZ(), adjacent.WorldPosition.ToVector2XZ(),
                    hazardLine.start.ToVector2XZ(), hazardLine.end.ToVector2XZ()))
                {
                    return true;
                }
            }
            return false;
        }

        internal NavigationNode FindNearestPoint(Vector3 mouseLeftPoint)
        {
            return partition.GetNearestNode(mouseLeftPoint);

        }

        internal void OptimiseNode(NavigationNode node)
        {
            //for each neighbour, if we have LOS to each of ITS neighbours, then
            //eliminate them.


            foreach (NavigationNode neighbour in node.Neighbours)
            {

            }

        }
    }


}
