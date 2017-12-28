using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.Procedural
{
    public class NeighbourTrackerNode
    {
        public enum Quadrant
        {
            none,
            se,
            nw,
            ne,
            sw
        }
        public enum CubeSide
        {
            none,
            top,
            bottom,
            front,
            back,
            left,
            right
        }

        public int depth;
        public Vector3 keyPoint;
        public Quadrant quadrant;
        public CubeSide side;
        public float step;
        public Vector3 max;
        public Vector3 min;
        public Vector3 normal;


        public NeighbourTrackerNode(int depth, Vector3 min, Vector3 max, float step, Vector3 normal)
        {
            this.depth = depth;
            this.keyPoint = (min + max)/2;
            this.min = min;
            this.max = max;
            this.step = step;
            this.normal = normal;
        }

        public override string ToString()
        {
            return depth + " , " + keyPoint.ToString() + " , " + quadrant.ToString() + " , " + side.ToString();
        }
    }
}