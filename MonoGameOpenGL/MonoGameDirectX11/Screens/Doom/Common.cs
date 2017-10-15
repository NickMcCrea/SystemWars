using MonoGameEngineCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using System.IO;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Rendering.Camera;
using System;
using RestSharp;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.DoomLib;
using Kaitai;
using NickLib.Pathfinding;

namespace MonoGameDirectX11.Screens.Doom
{
    public class NavigationNode : IPartitionItem
    {

        public List<NavigationNode> Neighbours { get; set; }
        public string Type { get { return "NavigationNode"; } }
        public bool done;
        public NavigationNode()
        {
            Neighbours = new List<NavigationNode>();
            Cost = 0;
        }
        public int Cost { get; set; }

        public bool Navigable { get; set; }
        public Vector3 WorldPosition { get; set; }
    }


    public class DoomMapHandler
    {
        public Vector3 LevelEnd { get; set; }
        string wadPath;
        string startMarker, endMarker;
        private List<DoomLine> levelLines;
        DoomWad.Linedefs lineDefs;
        DoomWad.Things things;
        DoomWad.Sectors sectors;
        DoomWad.Blockmap blockMap;
        DoomWad.Vertexes vertices;
        DoomWad.Sidedefs sideDefs;
        public List<DoomLine> Doors;
        public List<DoomLine> Unknowns;
        public DoomFloodFill floodFiller;
        public float scale = 50f;
        public float offsetX = 0;
        public float offsetZ = 0;
        public bool FloodFillComplete { get { return floodFiller.complete; } }

        private List<int> blockingObjectIDs;

        public DoomMapHandler(string wadPath, string startLevelMarker, string endLevelMarker)
        {
            Doors = new List<DoomLine>();
            Unknowns = new List<DoomLine>();
            this.wadPath = wadPath;
            startMarker = startLevelMarker;
            endMarker = endLevelMarker;
            blockingObjectIDs = new List<int>();

            blockingObjectIDs.Add(55);
            blockingObjectIDs.Add(54);
            blockingObjectIDs.Add(56);
            blockingObjectIDs.Add(57);
            blockingObjectIDs.Add(46);
            blockingObjectIDs.Add(47);
            blockingObjectIDs.Add(48);
            blockingObjectIDs.Add(32);
            blockingObjectIDs.Add(33);


            blockingObjectIDs.Add(31);
            blockingObjectIDs.Add(35);
            blockingObjectIDs.Add(45);
            blockingObjectIDs.Add(43);
            blockingObjectIDs.Add(42);
            blockingObjectIDs.Add(41);
            blockingObjectIDs.Add(37);
            blockingObjectIDs.Add(36);
            blockingObjectIDs.Add(25);

            blockingObjectIDs.Add(26);
            blockingObjectIDs.Add(27);
            blockingObjectIDs.Add(28);
            blockingObjectIDs.Add(29);
            blockingObjectIDs.Add(30);
            blockingObjectIDs.Add(86);
            blockingObjectIDs.Add(85);
            blockingObjectIDs.Add(70);
            blockingObjectIDs.Add(2035);
            blockingObjectIDs.Add(2028);


        }

        public void ParseWad()
        {
            var file = new FileInfo(wadPath);
            using (var fs = file.OpenRead())
            {

                var doomWad = DoomWad.FromFile(wadPath);

                string desiredLevel = startMarker;

                int levelMarker = doomWad.Index.FindIndex(x => x.Name.Contains(desiredLevel));
                for (int i = levelMarker + 1; i < doomWad.NumIndexEntries; i++)
                {
                    var currentIndex = doomWad.Index[i];

                    if (currentIndex.Name.Contains(endMarker))
                        break;

                    if (currentIndex.Name == ("SECTORS\0"))
                        sectors = currentIndex.Contents as DoomWad.Sectors;

                    if (currentIndex.Name.Contains("BLOCKMAP"))
                        blockMap = currentIndex.Contents as DoomWad.Blockmap;

                    if (currentIndex.Name.Contains("SIDEDEF"))
                        sideDefs = currentIndex.Contents as DoomWad.Sidedefs;

                    if (currentIndex.Name.Contains("VERTEX"))
                        vertices = currentIndex.Contents as DoomWad.Vertexes;

                    if (currentIndex.Name.Contains("THING"))
                        things = currentIndex.Contents as DoomWad.Things;

                    if (currentIndex.Name.Contains("LINEDEF"))
                        lineDefs = currentIndex.Contents as DoomWad.Linedefs;
                }
            }

            var exitLine = lineDefs.Entries.Find(x => x.LineType == 11);

            DoomWad.Vertex startExit = vertices.Entries[exitLine.VertexStartIdx];
            DoomWad.Vertex endExit = vertices.Entries[exitLine.VertexEndIdx];
            var p1Exit = new Vector3((startExit.X) / scale + offsetX, 0,
                                  (startExit.Y) / scale + offsetZ);
            var p2Exit = new Vector3((endExit.X) / scale + offsetX, 0,
                               (endExit.Y) / scale + offsetZ);

            LevelEnd = (p1Exit + p2Exit) / 2;


            levelLines = new List<DoomLine>();
            foreach (DoomWad.Linedef lineDef in lineDefs.Entries)
            {
                //sector tag is useless - always 0
                //DoomWad.Sector sector = sectors.Entries[lineDef.SectorTag];

                DoomWad.Sidedef sideDefLeft, sideDefRight;
                sideDefLeft = null;

                //most lineDefs are not double sided. SideDef ID of 65535 means no side def for this direction
                if (lineDef.SidedefLeftIdx != 65535)
                {
                    sideDefLeft = sideDefs.Entries[lineDef.SidedefLeftIdx];

                }

                sideDefRight = sideDefs.Entries[lineDef.SidedefRightIdx];

                var sector = sectors.Entries[sideDefRight.SectorId];

                Color lineColor, lineColorMin, lineColorMax;


                float heightDiff = 0;
                if (sideDefLeft != null)
                {
                    var sec1 = sectors.Entries[sideDefRight.SectorId];
                    var sec2 = sectors.Entries[sideDefLeft.SectorId];
                    heightDiff = Math.Abs(sec1.FloorZ - sec2.FloorZ);
                }



                //convert the ceiling or floor height to a useful range.
                float convertedHeight = sector.CeilZ / 8;
                convertedHeight += 17;
                convertedHeight /= 34;

                lineColorMin = Color.Blue;
                lineColorMax = Color.Red;

                lineColor = Color.Lerp(lineColorMin, lineColorMax, convertedHeight);


                DoomWad.Vertex start = vertices.Entries[lineDef.VertexStartIdx];
                DoomWad.Vertex end = vertices.Entries[lineDef.VertexEndIdx];



                var p1 = new Vector3((start.X) / scale + offsetX, 0,
                                      (start.Y) / scale + offsetZ);

                var p2 = new Vector3((end.X) / scale + offsetX, 0,
                                   (end.Y) / scale + offsetZ);

                if (sideDefLeft == null)
                    levelLines.Add(new DoomLine() { start = p1, end = p2, color = lineColor, BlocksLineOfSight = true });

                if (sideDefLeft != null && heightDiff > 24)
                    levelLines.Add(new DoomLine() { start = p1, end = p2, color = lineColor, BlocksLineOfSight = false });

                if (sideDefLeft != null && heightDiff <= 24)
                {
                    Console.WriteLine("Potential door");
                    if (lineDef.LineType == 1)
                    {
                        Doors.Add(new DoomLine() { start = p1, end = p2, color = Color.OrangeRed, BlocksLineOfSight = true });
                    }
                    else
                    {
                        int value = lineDef.Flags;
                        string binary = Convert.ToString(value, 2);
                        int blockFlag = 1;
                        var mystring = binary.Substring(binary.Length-1, 1);
                        if (mystring == blockFlag.ToString())
                            levelLines.Add(new DoomLine() { start = p1, end = p2, color = Color.Aquamarine, BlocksLineOfSight = true });

                    }
                }


            }


            var blockingThings = things.Entries.Where(x => blockingObjectIDs.Contains(x.Type));

            foreach (DoomWad.Thing t in blockingThings)
            {
                var pos = new Vector3((t.X) / scale + offsetX, 0,
                               (t.Y) / scale + offsetZ);

                float size = 0.2f;
                Vector3 topLeft = pos + new Vector3(-size, 0, size);
                Vector3 topRight = pos + new Vector3(size, 0, size);
                Vector3 bottomRight = pos + new Vector3(size, 0, -size);
                Vector3 bottomLeft = pos + new Vector3(-size, 0, -size);
                levelLines.Add(new DoomLine() { start = topLeft, end = topRight, color = Color.Red, BlocksLineOfSight = true });
                levelLines.Add(new DoomLine() { start = topRight, end = bottomRight, color = Color.Red, BlocksLineOfSight = true });
                levelLines.Add(new DoomLine() { start = bottomRight, end = bottomLeft, color = Color.Red, BlocksLineOfSight = true });
                levelLines.Add(new DoomLine() { start = bottomLeft, end = topLeft, color = Color.Red, BlocksLineOfSight = true });

            }



        }

        internal void InitialiseFloodFill()
        {

            var playerStartThing = things.Entries.Find(x => x.Type == 1);
            Vector3 pos = ConvertPosition(playerStartThing.X, playerStartThing.Y);
            floodFiller = new DoomFloodFill(levelLines, pos, vertices, scale, offsetX, offsetZ);

        }

        public void FloodFillStep()
        {
            floodFiller.Step();
        }

        private Vector3 ConvertPosition(float x, float y)
        {
            return new Vector3(x / scale + offsetX, 0, y / scale + offsetZ);
        }

        public List<DoomLine> GetLevelOutline()
        {
            return levelLines;
        }

        internal NavigationNode FindNavPoint(Vector3 target)
        {
            return floodFiller.FindNearestPoint(target);
        }

        internal bool IntersectsLevel(Vector3 start, Vector3 end, bool onlyCareAboutVisibility = false)
        {
            return floodFiller.partition.IntersectsALevelSegment(start, end, onlyCareAboutVisibility);
        }
    }

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

        public DoomFloodFill(List<DoomLine> levelLines, Vector3 startPoint, DoomWad.Vertexes vertexes, float scale, float xOffset, float zOffset)
        {
            positions = new List<NavigationNode>();
            levelLineGeometry = levelLines;


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

    public interface IPartitionItem
    {
        string Type { get; }
    }

    public class Bucket
    {
        public Vector3 bottomLeft;
        public Vector3 topRight;
        public Vector3 topLeft;
        public Vector3 bottomRight;
        public List<IPartitionItem> itemsInBucket;

        public Bucket(Vector3 min, Vector3 max)
        {
            bottomLeft = min;
            topRight = max;
            topLeft = new Vector3(bottomLeft.X, 0, topRight.Z);
            bottomRight = new Vector3(topRight.X, 0, bottomLeft.Z);

            itemsInBucket = new List<IPartitionItem>();
        }

        public bool PointIn(Vector3 point)
        {
            if (point.X >= bottomLeft.X)
                if (point.X <= topRight.X)
                    if (point.Z >= bottomLeft.Z)
                        if (point.Z <= topRight.Z)
                            return true;

            return false;
        }

        public bool LineIntersects(Vector3 a, Vector3 b)
        {


            if (MonoMathHelper.LineIntersection(a.ToVector2XZ(), b.ToVector2XZ(), topLeft.ToVector2XZ(), topRight.ToVector2XZ()))
                return true;
            if (MonoMathHelper.LineIntersection(a.ToVector2XZ(), b.ToVector2XZ(), topLeft.ToVector2XZ(), bottomLeft.ToVector2XZ()))
                return true;
            if (MonoMathHelper.LineIntersection(a.ToVector2XZ(), b.ToVector2XZ(), bottomLeft.ToVector2XZ(), bottomRight.ToVector2XZ()))
                return true;
            if (MonoMathHelper.LineIntersection(a.ToVector2XZ(), b.ToVector2XZ(), bottomRight.ToVector2XZ(), topRight.ToVector2XZ()))
                return true;

            return false;

        }
    }

    public class BucketPartition
    {
        public List<Bucket> buckets;



        public BucketPartition(float startX, float startZ, float bucketSize, int numofBucketsX, int numofBucketsZ)
        {
            buckets = new List<Bucket>();

            for (float x = startX; x < (startX + (numofBucketsX * bucketSize)); x += bucketSize)
            {
                for (float z = startZ; z < (startZ + (numofBucketsZ * bucketSize)); z += bucketSize)
                {
                    buckets.Add(new Bucket(new Vector3(x, 0, z), new Vector3(x + bucketSize, 0, z + bucketSize)));
                }
            }
        }

        public NavigationNode GetNearestNode(Vector3 v)
        {
            NavigationNode closest = null;
            float distance = float.MaxValue;

            foreach (Bucket b in buckets)
            {
                List<IPartitionItem> nodesInBucket = b.itemsInBucket.FindAll(x => x.Type == "NavigationNode");

                foreach (NavigationNode node in nodesInBucket)
                {
                    float d = (node.WorldPosition - v).Length();

                    if (d < distance)
                    {
                        closest = node;
                        distance = d;
                    }
                }
            }

            return closest;
        }

        public void AddNavigationNode(NavigationNode n)
        {
            foreach (Bucket b in buckets)
                if (b.PointIn(n.WorldPosition))
                    b.itemsInBucket.Add(n as IPartitionItem);
        }

        public void AddDoomLine(DoomLine doomLine)
        {
            foreach (Bucket b in buckets)
            {
                if (b.LineIntersects(doomLine.start, doomLine.end))
                    b.itemsInBucket.Add(doomLine as IPartitionItem);

                //add lines that are completely contained to the bucket also
                if (b.PointIn(doomLine.start) && b.PointIn(doomLine.end))
                    b.itemsInBucket.Add(doomLine as IPartitionItem);
            }
        }

        public bool IntersectsALevelSegment(Vector3 a, Vector3 b, bool onlyCareAboutVisibility)
        {

            foreach (Bucket buck in buckets)
            {
                if (buck.LineIntersects(a, b) || (buck.PointIn(a) && buck.PointIn(b)))
                {
                    List<IPartitionItem> linesInBucket = buck.itemsInBucket.FindAll(x => x.Type == "DoomLine");
                    foreach (DoomLine line in linesInBucket)
                    {
                        //this is a test of LOS only, so ignore lines that prevent traverse but allow LOS
                        //e.g. testing for LOS for a monster on a high pillar
                        if (onlyCareAboutVisibility)
                            if (!line.BlocksLineOfSight)
                                continue;

                        if (MonoMathHelper.LineIntersection(line.start.ToVector2XZ(), line.end.ToVector2XZ(), a.ToVector2XZ(), b.ToVector2XZ()))
                            return true;

                    }
                }
            }



            return false;

        }

        internal float DistanceFromClosestWall(Vector3 p)
        {
            float closest = float.MaxValue;
            foreach (Bucket b in buckets)
            {
                List<IPartitionItem> linesInBucket = b.itemsInBucket.FindAll(x => x.Type == "DoomLine");
                foreach (DoomLine line in linesInBucket)
                {
                    float dist = MonoMathHelper.DistanceLineSegmentToPoint(line.start.ToVector2XZ(), line.end.ToVector2XZ(), p.ToVector2XZ());
                    if (dist < closest)
                        closest = dist;
                }

            }
            return closest;
        }
    }

    public struct DoomLine : IPartitionItem
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public bool BlocksLineOfSight;
        public string Type { get { return "DoomLine"; } }
        public Vector3 MidPoint()
        {
            return (start + end) / 2;
        }



    }


}
