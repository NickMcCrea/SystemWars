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
        private float step;
        private Vector3 max;
        private Vector3 min;


        public NeighbourTrackerNode()
        {

        }

        public NeighbourTrackerNode(int depth, Vector3 min, Vector3 max, float step)
        {
            this.depth = depth;
            this.keyPoint = (min + max)/2;
            this.min = min;
            this.max = max;
            this.step = step;
        }

        public override string ToString()
        {
            return depth + " , " + keyPoint.ToString() + " , " + quadrant.ToString() + " , " + side.ToString();
        }
    }
}