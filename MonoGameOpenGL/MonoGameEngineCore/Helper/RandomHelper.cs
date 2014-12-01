using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.Helper
{
    /// <summary>
    /// Useful random number helpers.
    /// </summary>
    public static class RandomHelper
    {
        #region Variables
        /// <summary>
        /// Global random generator
        /// </summary>
        public static Random globalRandomGenerator =
            GenerateNewRandomGenerator();
        #endregion

        #region Generate a new random generator
        /// <summary>
        /// Generate a new random generator with help of
        /// WindowsHelper.GetPerformanceCounter.
        /// Also used for all GetRandom methods here.
        /// </summary>
        /// <returns>Random</returns>
        public static Random GenerateNewRandomGenerator()
        {
            globalRandomGenerator =
                new Random((int)DateTime.Now.Ticks);
            //needs Interop: (int)WindowsHelper.GetPerformanceCounter());
            return globalRandomGenerator;
        }
        #endregion

        #region Get random float and byte methods
        /// <summary>
        /// Get random int
        /// </summary>
        /// <param name="max">Maximum</param>
        /// <returns>Int</returns>
        public static int GetRandomInt(int max)
        {
            return globalRandomGenerator.Next(max);
        }

        public static int GetRandomInt(int min, int max)
        {
            return globalRandomGenerator.Next(min, max);
        }

        /// <summary>
        /// Get random float between min and max
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        /// <returns>Float</returns>
        public static float GetRandomFloat(float min, float max)
        {
            return (float)globalRandomGenerator.NextDouble() * (max - min) + min;
        }

        //returns a random true or false
        public static bool CoinToss()
        {
            int result = GetRandomInt(1000);
            if (result > 500)
                return true;
            return false;
        }

        /// <summary>
        /// Get random byte between min and max
        /// </summary>
        /// <param name="min">Min</param>
        /// <param name="max">Max</param>
        /// <returns>Byte</returns>
        public static byte GetRandomByte(byte min, byte max)
        {
            return (byte)(globalRandomGenerator.Next(min, max));
        }

        /// <summary>
        /// Get random Vector2
        /// </summary>
        /// <param name="min">Minimum for each component</param>
        /// <param name="max">Maximum for each component</param>
        /// <returns>Vector2</returns>
        public static Vector2 GetRandomVector2(float min, float max)
        {
            return new Vector2(
                GetRandomFloat(min, max),
                GetRandomFloat(min, max));
        }

        /// <summary>
        /// Get random Vector3
        /// </summary>
        /// <param name="min">Minimum for each component</param>
        /// <param name="max">Maximum for each component</param>
        /// <returns>Vector3</returns>
        public static Vector3 GetRandomVector3(float min, float max)
        {
            return new Vector3(
                GetRandomFloat(min, max),
                GetRandomFloat(min, max),
                GetRandomFloat(min, max));
        }

        /// <summary>
        /// Get random vector within a volume.
        /// </summary>
        /// <param name="_min"></param>
        /// <param name="_max"></param>
        /// <returns></returns>
        public static Vector3 GetRandomVector3(Vector3 _min, Vector3 _max)
        {
            return new Vector3(
                GetRandomFloat(_min.X, _max.X),
                GetRandomFloat(_min.Y, _max.Y),
                GetRandomFloat(_min.Z, _max.Z));
        }

        /// <summary>
        /// Get random color
        /// </summary>
        /// <returns>Color</returns>
        public static Color RandomColor
        {
            get
            {
                return new Color(new Vector3(
                                     GetRandomFloat(0.25f, 1.0f),
                                     GetRandomFloat(0.25f, 1.0f),
                                     GetRandomFloat(0.25f, 1.0f)));
            }
        }

        /// <summary>
        /// Get random normal Vector3
        /// </summary>
        /// <returns>Vector3</returns>
        public static Vector3 RandomNormalVector3
        {
            get
            {
                Vector3 randomNormalVector = new Vector3(
                    GetRandomFloat(-1.0f, 1.0f),
                    GetRandomFloat(-1.0f, 1.0f),
                    GetRandomFloat(-1.0f, 1.0f));
                randomNormalVector.Normalize();
                return randomNormalVector;
            }
        }

        /// <summary>
        /// return random true or false
        /// </summary>
        /// <returns></returns>
        public static bool GetRandomBoolean()
        {
            int result = GetRandomInt(1000);
            if (result > 500)
                return true;
            return false;
        }


        /// <summary>
        /// Random 4 char upper string
        /// </summary>
        /// <returns></returns>
        public static string GetRandomString()
        {
            StringBuilder builder = new StringBuilder();

            char ch;
            for (int i = 0; i < 4; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * globalRandomGenerator.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }

        
        #endregion

        public static Color GetRandomColor()
        {
            return new Color(RandomHelper.GetRandomByte(0, 255), RandomHelper.GetRandomByte(0, 255), RandomHelper.GetRandomByte(0, 255));
        }

        internal static Vector3 RandomVectorNearPosition(Vector3 vector3, float p)
        {
            return vector3 + (RandomNormalVector3 * p);
        }
    }
}