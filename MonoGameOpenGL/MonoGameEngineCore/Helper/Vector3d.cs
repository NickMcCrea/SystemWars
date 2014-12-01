using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.Helper
{
    public struct Vector3d
    {
        public double X;
        public double Y;
        public double Z;

        public Vector3d(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3d(Vector3 vec)
        {
            X = vec.X;
            Y = vec.Y;
            Z = vec.Z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public static Vector3d operator +(Vector3d a, Vector3d b)
        {
            Vector3d newPos = new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            return newPos;
        }

        public static Vector3d operator +(Vector3d a, Vector3 b)
        {
            Vector3d newPos = new Vector3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
            return newPos;
        }

        public static Vector3d operator -(Vector3d a, Vector3d b)
        {
            Vector3d newPos = new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            return newPos;
        }

        public static Vector3d operator -(Vector3d a, Vector3 b)
        {
            Vector3d newPos = new Vector3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            return newPos;
        }

        public static Vector3d operator *(Vector3d a, double b)
        {
            return new Vector3d(a.X * b, a.Y * b, a.Z * b);
        }

        public static Vector3d operator /(Vector3d a, double b)
        {
            return new Vector3d(a.X / b, a.Y / b, a.Z / b);

        }

        public static Vector3d Zero { get { return new Vector3d(0, 0, 0); } }

        public string ToString()
        {
            return X.ToString() + ":" + Y.ToString() + ":" + Z.ToString();
        }

        public double Length
        {
            get { return Math.Sqrt((this.X * this.X) + (this.Y * this.Y) + (this.Z * this.Z)); }
        }
    }
}
