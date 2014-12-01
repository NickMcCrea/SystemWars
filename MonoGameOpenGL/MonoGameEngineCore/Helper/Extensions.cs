using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.Helper
{
    public static class Extensions
    {
        public static Vector2 ToVector2XY(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Y);
        }

        public static Vector2 ToVector2XZ(this Vector3 vec)
        {
            return new Vector2(vec.X, vec.Z);
        }

        public static Vector3 FlipX(this Vector3 vec)
        {
            return new Vector3(-vec.X, vec.Y, vec.Z);
        }
        public static Vector3 FlipY(this Vector3 vec)
        {
            return new Vector3(vec.X, -vec.Y, vec.Z);
        }
        public static Vector3 FlipZ(this Vector3 vec)
        {
            return new Vector3(vec.X, vec.Y, -vec.Z);
        }

        public static Vector2 ToVector2YZ(this Vector3 vec)
        {
            return new Vector2(vec.Z, vec.Y);
        }

        public static bool IsNan(this Vector3 vec)
        {
            return float.IsNaN(vec.X);
        }

        public static Vector3 ZeroYComponent(this Vector3 vec)
        {
            return new Vector3(vec.X, 0, vec.Z);
        }

        public static Vector3 ZeroZComponent(this Vector3 vec)
        {
            return new Vector3(vec.X, vec.Y, 0);
        }

        public static bool WithinRange(this int f, int range, int target)
        {
            return (f >= (target - range)) && (f <= (target + range));
        }

        public static Color SetAlpha(this Color col, int alpha)
        {
            return new Color((int)col.R, (int)col.G, (int)col.B, (int)alpha);
        }

        public static double Hue(this Color col)
        {
            return Math.Atan2(Math.Sqrt(3) * (col.G - col.B), 2 * col.R - col.G - col.B);
        }

        public static Color ChangeTone(this Color col, int changeAmount)
        {
            return new Color((int)col.R+changeAmount, (int)col.G+changeAmount, (int)col.B+changeAmount, (int)col.A);
        }


        public static Vector3 ReplaceXComponent(this Vector3 vec, float replacement)
        {
            return new Vector3(replacement, vec.Y, vec.Z);
        }
        public static Vector3 ReplaceYComponent(this Vector3 vec, float replacement)
        {
            return new Vector3(vec.X, replacement, vec.Z);
        }
        public static Vector3 ReplaceZComponent(this Vector3 vec, float replacement)
        {
            return new Vector3(vec.X, vec.Y, replacement);
        }
    }
}
