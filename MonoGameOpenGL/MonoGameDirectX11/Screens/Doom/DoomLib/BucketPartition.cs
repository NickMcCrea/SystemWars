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
}
