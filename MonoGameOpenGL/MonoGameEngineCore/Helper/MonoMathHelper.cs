using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameEngineCore.Helper
{
    /// <summary>
    /// Variety of useful maths static helper methods.
    /// </summary>
    public static class MonoMathHelper
    {
        /// <summary>
        /// Remaps an input range into a desired output. Doesn't like negative numbers...
        /// </summary>
        /// <param name="lowIn"></param>
        /// <param name="highIn"></param>
        /// <param name="lowOut"></param>
        /// <param name="highOut"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public static float MapFloatRange(float lowIn, float highIn, float lowOut, float highOut, float input)
        {
            float inputRange = highIn - lowIn;
            float ratio = input/inputRange;

            float transformedOutputRange = highOut - lowOut;

            transformedOutputRange *= ratio;
            transformedOutputRange += lowOut;

            return transformedOutputRange;
        }

        /// <summary>
        /// Simple class for sorting vectors on their X component.
        /// </summary>
        public class XComparison : IComparer<Vector3>
        {
            public int Compare(Vector3 x, Vector3 y)
            {
                if (x.X < y.X)
                    return 1;
                if (x.X > y.X)
                    return -1;
                return 0;
            }
        }

        /// <summary>
        /// Simple class for sorting vectors on their Z component.
        /// </summary>
        public class ZComparison : IComparer<Vector3>
        {
            public int Compare(Vector3 x, Vector3 y)
            {
                if (x.Z < y.Z)
                    return 1;
                if (x.Z > y.Z)
                    return -1;
                return 0;
            }
        }

        /// <summary>
        /// Returns perpendicular vector to a 2d vector
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 CrossProduct(Vector2 v)
        {
            return new Vector2(v.Y, -v.X);
        }

        /// <summary>
        /// Return angle between two vectors. 
        /// </summary>
        /// <param name="vec1">Vector 1</param>
        /// <param name="vec2">Vector 2</param>
        /// <returns>Float</returns>
        public static float GetAngleBetweenVectors(Vector3 vec1, Vector3 vec2)
        {
            // See http://en.wikipedia.org/wiki/Vector_(spatial)
            // for help and check out the Dot Product section ^^
            // Both vectors are normalized so we can save deviding through the
            // lengths.
            return (float) Math.Acos(Vector3.Dot(vec1, vec2));
        }

        /// <summary>
        /// Returns a float from 0-360 representing heading.
        /// </summary>
        /// <param name="heading"></param>
        /// <returns></returns>
        public static float GetHeadingFromVector(Vector2 heading)
        {
            return (float) Math.Atan2(heading.Y, heading.X);
        }

        /// <summary>
        /// Distance from our point to the line described by linePos1 and linePos2.
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="linePos1">Line position 1</param>
        /// <param name="linePos2">Line position 2</param>
        /// <returns>Float</returns>
        public static float DistanceToLine(Vector3 point,
            Vector3 linePos1, Vector3 linePos2)
        {
            // For help check out this article:
            // http://mathworld.wolfram.com/Point-LineDistance3-Dimensional.html
            Vector3 lineVec = linePos2 - linePos1;
            Vector3 pointVec = linePos1 - point;
            return Vector3.Cross(lineVec, pointVec).Length()/lineVec.Length();
        }

        public static float DistanceLineSegmentToPoint(Vector2 A, Vector2 B, Vector2 p)
        {
            //get the normalized line segment vector
            Vector2 v = B - A;
            v.Normalize();

            //determine the point on the line segment nearest to the point p
            float distanceAlongLine = Vector2.Dot(p, v) - Vector2.Dot(A, v);
            Vector2 nearestPoint;
            if (distanceAlongLine < 0)
            {
                //closest point is A
                nearestPoint = A;
            }
            else if (distanceAlongLine > Vector2.Distance(A, B))
            {
                //closest point is B
                nearestPoint = B;
            }
            else
            {
                //closest point is between A and B... A + d  * ( ||B-A|| )
                nearestPoint = A + distanceAlongLine*v;
            }

            //Calculate the distance between the two points
            float actualDistance = Vector2.Distance(nearestPoint, p);
            return actualDistance;
        }

        public static float DistanceBetween(Vector3 a, Vector3 b)
        {
            return (a - b).Length();
        }

        public static Vector3 ClosestPointOnLineSegment(
            Vector3 lineA,
            Vector3 lineB,
            Vector3 point)
        {
            Vector3 v = lineB - lineA;
            v.Normalize();
            float t = Vector3.Dot(v, point - lineA);
            if (t < 0) return lineA;
            float d = (lineB - lineA).Length();
            if (t > d) return lineB;
            return lineA + v*t;
        }

        /// <summary>
        /// Returns the number of digits in an integer - useful for placing scores on screen with leading zeros etc
        /// </summary>
        /// <param name="valueInt"></param>
        /// <returns></returns>
        public static int GetIntegerDigitCount(int valueInt)
        {
            double value = valueInt;
            int sign = 0;
            if (value < 0)
            {
                value = -value;
                sign = 1;
            }
            if (value <= 9)
            {
                return sign + 1;
            }
            if (value <= 99)
            {
                return sign + 2;
            }
            if (value <= 999)
            {
                return sign + 3;
            }
            if (value <= 9999)
            {
                return sign + 4;
            }
            if (value <= 99999)
            {
                return sign + 5;
            }
            if (value <= 999999)
            {
                return sign + 6;
            }
            if (value <= 9999999)
            {
                return sign + 7;
            }
            if (value <= 99999999)
            {
                return sign + 8;
            }
            if (value <= 999999999)
            {
                return sign + 9;
            }
            return sign + 10;
        }

        public static bool AlmostEquals(int int1, int int2, int precision)
        {
            return (Math.Abs(int1 - int2) <= precision);
        }

        public static bool AlmostEquals(float float1, float float2, float precision)
        {
            return (Math.Abs(float1 - float2) <= precision);
        }

        public static bool AlmostEquals(double double1, double double2, float precision)
        {
            return (Math.Abs(double1 - double2) <= precision);
        }

        public static bool AlmostEquals(Vector3 vec1, Vector3 vec2, float permissableDistance)
        {
            return (vec1 - vec2).Length() <= permissableDistance;
        }

        public static Vector3 DistanceFromPointToLineSegment(Vector3 a, Vector3 b, Vector3 point)
        {
            Vector3 c = point - a;
            Vector3 v = Vector3.Normalize(b - a);
            float d = (b - a).Length();
            float t = Vector3.Dot(v, c);

            // Check to see if the point is on the line
            // if not then return the endpoint
            if (t < 0) return a;
            if (t > d) return b;

            // get the distance to move from point a
            v *= t;

            // move from point a to the nearest point on the segment
            return a + v;
        }

        /// <summary>
        /// Very useful method. Returns a valid world matrix from a position and a target position which determines facing.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Matrix GenerateWorldMatrixFromPositionAndTarget(Vector3 pos, Vector3 target)
        {
            Matrix world = Matrix.Identity;

            Vector3 forward = target - pos;
            forward.Normalize();
            Vector3 right = Vector3.Cross(forward, Vector3.Backward);
            right.Normalize();
            Vector3 up = Vector3.Cross(right, forward);

            world.Forward = forward;
            world.Up = up;
            world.Right = right;
            world.Translation = pos;
            return world;
        }


        /// <summary>
        /// Very useful method. Returns a valid world matrix from a position and a target position which determines facing.
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static Matrix GenerateWorldMatrixFromPositionAndTarget(Vector3 pos, Vector3 target, Vector3 customUp)
        {
            Matrix world = Matrix.Identity;

            Vector3 forward = target - pos;
            forward.Normalize();
            Vector3 right = Vector3.Cross(forward, customUp);
            right.Normalize();

            world.Forward = forward;
            world.Up = customUp;
            world.Right = right;
            world.Translation = pos;
            return world;
        }


        /// <summary>
        /// Signed distance to plane
        /// </summary>
        /// <param name="point">Point</param>
        /// <param name="planePosition">Plane position</param>
        /// <param name="planeNormal">Plane normal</param>
        /// <returns>Float</returns>
        public static float SignedDistanceToPlane(Vector3 point,
            Vector3 planePosition, Vector3 planeNormal)
        {
            Vector3 pointVec = planePosition - point;
            return Vector3.Dot(planeNormal, pointVec);
        }

        public static Vector3 ClosestPointOnPlane(Vector3 point, Plane plane)
        {
            Ray r = new Ray(point, -plane.Normal);

            float? distance;
            r.Intersects(ref plane, out distance);


            return point + r.Direction*distance.Value;
        }

        public static Vector3 MirrorInPlane(Vector3 point, Plane p)
        {
            Ray r = new Ray(point, -p.Normal);

            float? distance;
            r.Intersects(ref p, out distance);


            return point + r.Direction*(distance.Value*2);
        }


        /// <summary>
        /// Returns velocity of deflection with elasticity.
        /// </summary>
        /// <param name="currentVelocity">Velocity vector if item to be bounced.</param>
        /// <param name="elasticity">Elasticity of item to be bounced.</param>
        /// <param name="collisionNormal">Normal vector of plane the item is bouncing off of.</param>
        /// <returns>Velocity vector of deflection.</returns>
        public static Vector3 CalculateDeflectionEx(Vector3 currentVelocity, float elasticity, Vector3 collisionNormal)
        {
            Vector3 newDirection = elasticity*
                                   (-2*Vector3.Dot(currentVelocity, collisionNormal)*collisionNormal + currentVelocity);
            return newDirection;
        }

        /// <summary>
        /// Returns velocity of deflection with elasticity.
        /// </summary>
        /// <param name="currentVelocity">Velocity vector if item to be bounced.</param>
        /// <param name="elasticity">Elasticity of item to be bounced.</param>
        /// <param name="collisionNormal">Normal vector of plane the item is bouncing off of.</param>
        /// <returns>Velocity vector of deflection.</returns>
        public static Vector3 CalculateDeflection(Vector3 currentVelocity, float elasticity, Vector3 collisionNormal)
        {
            return Vector3.Reflect(currentVelocity, collisionNormal)*elasticity;
        }

        /// <summary>
        /// Translates a point around an origin.
        /// </summary>
        /// <param name="point">Point that is going to be translated.</param>
        /// <param name="originPoint">Origin of rotation.</param>
        /// <param name="rotationAxis">Axis to rotate around, this Vector should be a unit vector (normalized).</param>
        /// <param name="radiansToRotate">Radians to rotate.</param>
        /// <returns>Translated point.</returns>
        public static Vector3 RotateAroundPoint(Vector3 point, Vector3 originPoint, Vector3 rotationAxis,
            float radiansToRotate)
        {
            Vector3 diffVect = point - originPoint;
            Vector3 rotatedVect = Vector3.Transform(diffVect, Matrix.CreateFromAxisAngle(rotationAxis, radiansToRotate));
            rotatedVect += originPoint;
            return rotatedVect;
        }

        public static Vector2 Rotate2DUnitVector(Vector2 unitVector, float angleInRadians)
        {
            if (angleInRadians == 0) return unitVector;

            double x = unitVector.X*Math.Cos(angleInRadians) - unitVector.Y*Math.Sin(angleInRadians);
            double y = unitVector.X*Math.Sin(angleInRadians) - unitVector.Y*Math.Cos(angleInRadians);
            return new Vector2((float) x, -(float) y);
        }

        public static Vector2 Rotate(float angle, Vector2 currentPos, Vector2 centre)
        {
            double distance = Math.Sqrt(Math.Pow(currentPos.X - centre.X, 2) + Math.Pow(currentPos.Y - centre.Y, 2));
            return new Vector2((float) (distance*Math.Cos(angle)), (float) (distance*Math.Sin(angle))) + centre;
        }

        /// <summary>
        /// Convert an orientation matrix to an eular angle.
        /// </summary>
        public static Vector3 MatrixToEular(ref Matrix orientation)
        {
            float x, y, z;

            // extract pitch from m23, being careful for domain errors with asin(). We could have
            // values slightly out of range due to floating point arithmetic.
            float sp = -orientation.M23;

            if (sp <= -1.0f) x = -MathHelper.PiOver2;
            else if (sp >= 1.0) x = MathHelper.PiOver2;
            else x = (float) Math.Asin(sp);

            // check for the Gimbal lock case, giving a slight tolerance for numerical imprecision
            if (sp > 0.9999f)
            {
                // we are looking straight up or down. just set heading and slam bank to zero
                y = (float) Math.Atan2(-orientation.M31, orientation.M11);
                z = 0.0f;
            }
            else
            {
                y = (float) Math.Atan2(orientation.M13, orientation.M33);
                z = (float) Math.Atan2(orientation.M21, orientation.M22);
            }

            return new Vector3(-x, -y, -z);
        }

        public static bool LineIntersection(Vector2 point1, Vector2 point2, Vector2 point3, Vector2 point4,
            out Vector2 intersectOut)
        {
            intersectOut = Vector2.Zero;

            float ua = (point4.X - point3.X)*(point1.Y - point3.Y) - (point4.Y - point3.Y)*(point1.X - point3.X);
            float ub = (point2.X - point1.X)*(point1.Y - point3.Y) - (point2.Y - point1.Y)*(point1.X - point3.X);
            float denominator = (point4.Y - point3.Y)*(point2.X - point1.X) -
                                (point4.X - point3.X)*(point2.Y - point1.Y);
            bool intersection = false;

            if (Math.Abs(denominator) <= 0.00001f)
            {
                if (Math.Abs(ua) <= 0.00001f && Math.Abs(ub) <= 0.00001f)
                {
                    intersection = true;
                    intersectOut = (point1 + point2)/2;
                }
            }
            else
            {
                ua /= denominator;
                ub /= denominator;

                if (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1)
                {
                    intersection = true;
                    intersectOut.X = point1.X + ua*(point2.X - point1.X);
                    intersectOut.Y = point1.Y + ua*(point2.Y - point1.Y);
                }
            }
            return intersection;
        }

        public static bool LineSegmentIntersection(Vector3 a1, Vector3 a2, Vector3 b1, Vector3 b2,
            out Vector3 intersectionPoint)
        {
            float firstLineSlopeX, firstLineSlopeY, secondLineSlopeX, secondLineSlopeY;

            firstLineSlopeX = a2.X - a1.X;
            firstLineSlopeY = a2.Y - a1.Y;

            secondLineSlopeX = b2.X - b1.X;
            secondLineSlopeY = b2.Y - b1.Y;

            float s, t;
            s = (-firstLineSlopeY*(a1.X - b1.X) + firstLineSlopeX*(a1.Y - b1.Y))/
                (-secondLineSlopeX*firstLineSlopeY + firstLineSlopeX*secondLineSlopeY);
            t = (secondLineSlopeX*(a1.Y - b1.Y) - secondLineSlopeY*(a1.X - b1.X))/
                (-secondLineSlopeX*firstLineSlopeY + firstLineSlopeX*secondLineSlopeY);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                float intersectionPointX = a1.X + (t*firstLineSlopeX);
                float intersectionPointY = a1.Y + (t*firstLineSlopeY);

                // Collision detected
                intersectionPoint = new Vector3(intersectionPointX, intersectionPointY, 0);

                return true;
            }

            intersectionPoint = Vector3.Zero;
            return false; // No collision
        }

        public static Vector2[] Vector3ToVector2UsingXZ(Vector3[] vec3Array)
        {
            Vector2[] vecArray = new Vector2[vec3Array.Length];

            for (int i = 0; i < vec3Array.Length; i++)
            {
                vecArray[i] = new Vector2(vec3Array[i].X, vec3Array[i].Z);
            }

            return vecArray;
        }

        public static Vector2[] Vector3ToVector2UsingZY(Vector3[] vec3Array)
        {
            Vector2[] vecArray = new Vector2[vec3Array.Length];

            for (int i = 0; i < vec3Array.Length; i++)
            {
                vecArray[i] = new Vector2(vec3Array[i].Z, vec3Array[i].Y);
            }

            return vecArray;
        }

        public static Vector2[] Vector3ToVector2UsingXY(Vector3[] vec3Array)
        {
            Vector2[] vecArray = new Vector2[vec3Array.Length];

            for (int i = 0; i < vec3Array.Length; i++)
            {
                vecArray[i] = new Vector2(vec3Array[i].X, vec3Array[i].Y);
            }

            return vecArray;
        }

        public static double GetSignedAngleBetween2DVectors(Vector3 FromVector, Vector3 DestVector)
        {
            Vector3 DestVectorsRight = Vector3.Cross(DestVector, Vector3.UnitZ);
            FromVector.Normalize();
            DestVector.Normalize();
            DestVectorsRight.Normalize();

            float forwardDot = Vector3.Dot(FromVector, DestVector);
            float rightDot = Vector3.Dot(FromVector, DestVectorsRight);

            // Keep dot in range to prevent rounding errors
            forwardDot = MathHelper.Clamp(forwardDot, -1.0f, 1.0f);

            double angleBetween = Math.Acos(forwardDot);

            if (rightDot < 0.0f)
                angleBetween *= -1.0f;

            return angleBetween;
        }

        public static Vector3[] Vector2ToVector3UsingXZ(Vector2[] vec2Array)
        {
            Vector3[] vecArray = new Vector3[vec2Array.Length];

            for (int i = 0; i < vec2Array.Length; i++)
            {
                vecArray[i] = new Vector3(vec2Array[i].X, 0, vec2Array[i].Y);
            }

            return vecArray;
        }

        /// <summary>
        /// 2D Check if a point lies withing a polygon.
        /// </summary>
        /// <param name="polygonVertex">The points of the polygon.</param>
        /// <param name="testVertex">The point to check.</param>
        /// <returns>
        /// A boolean flag indicating if the test vertex
        /// is inside the polygon.
        /// </returns>
        public static bool PointInPolygon(Vector2[] polygonVertex, Vector2 testVertex)
        {
            bool c = false;
            int nvert = polygonVertex.Length;
            if (nvert > 2)
            {
                int i, j;
                for (i = 0, j = nvert - 1; i < nvert; j = i++)
                {
                    if (((polygonVertex[i].Y > testVertex.Y) != (polygonVertex[j].Y > testVertex.Y)) &&
                        (testVertex.X < (polygonVertex[j].X - polygonVertex[i].X)*
                         (testVertex.Y - polygonVertex[i].Y)/
                         (polygonVertex[j].Y - polygonVertex[i].Y) + polygonVertex[i].X))
                    {
                        c = !c;
                    }
                }
            }

            return c;
        }

        public static bool IsInPolygon(Vector2[] poly, Vector2 p)
        {
            Vector2 p1, p2;


            bool inside = false;


            if (poly.Length < 3)
            {
                return inside;
            }


            var oldPoint = new Vector2(
                poly[poly.Length - 1].X, poly[poly.Length - 1].Y);


            for (int i = 0; i < poly.Length; i++)
            {
                var newPoint = new Vector2(poly[i].X, poly[i].Y);


                if (newPoint.X > oldPoint.X)
                {
                    p1 = oldPoint;

                    p2 = newPoint;
                }

                else
                {
                    p1 = newPoint;

                    p2 = oldPoint;
                }


                if ((newPoint.X < p.X) == (p.X <= oldPoint.X)
                    && (p.Y - p1.Y)*(p2.X - p1.X)
                    < (p2.Y - p1.Y)*(p.X - p1.X))
                {
                    inside = !inside;
                }


                oldPoint = newPoint;
            }


            return inside;
        }

        /// <summary>
        /// 2D Check if a point lies withing a polygon.
        /// </summary>
        /// <param name="polygonVertex">The points of the polygon.</param>
        /// <param name="testVertex">The point to check.</param>
        /// <returns>
        /// A boolean flag indicating if the test vertex
        /// is inside the polygon.
        /// </returns>
        public static bool PointInPolygonUsingZeroY(ref Vector3[] polygonVertex, ref Vector3 testVertex)
        {
            bool c = false;
            int nvert = polygonVertex.Length;
            if (nvert > 2)
            {
                int i, j;
                for (i = 0, j = nvert - 1; i < nvert; j = i++)
                {
                    if (((polygonVertex[i].Z > testVertex.Z) != (polygonVertex[j].Z > testVertex.Z)) &&
                        (testVertex.X < (polygonVertex[j].X - polygonVertex[i].X)*
                         (testVertex.Z - polygonVertex[i].Z)/
                         (polygonVertex[j].Z - polygonVertex[i].Z) + polygonVertex[i].X))
                    {
                        c = !c;
                    }
                }
            }

            return c;
        }

        /// <summary>
        /// Returns screen space position of a point in the world.
        /// </summary>
        /// <param name="point"></param>
        /// <param name="wholeViewport"></param>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="world"></param>
        /// <returns></returns>
        public static Vector3 ScreenProject(Vector3 point, Viewport wholeViewport, Matrix view, Matrix projection,
            Matrix world)
        {
            Vector4 mp = Vector4.Transform(new Vector4(point, 1.0f), Matrix.Invert(world));
            Vector3 pt = wholeViewport.Project(new Vector3(mp.X, mp.Y, mp.Z), projection, view, world);
            return pt;
        }

        /// <summary>
        /// Returns a ray projected into the world from the mouse position, for use in
        /// mouse picking.
        /// </summary>
        /// <param name="projectionMatrix"></param>
        /// <param name="viewMatrix"></param>
        /// <returns></returns>
        public static Ray GetProjectedMouseRay(Matrix _cameraView, Matrix _cameraProjection, GraphicsDevice _device,
            int mouseX, int mouseY)
        {
            // create 2 positions in screenspace using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3((float) mouseX, (float) mouseY, 0f);
            Vector3 farSource = new Vector3((float) mouseX, (float) mouseY, 0.99f);

            Matrix world = Matrix.CreateTranslation(Vector3.Zero);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = _device.Viewport.Unproject(nearSource,
                _cameraProjection, _cameraView, world);

            Vector3 farPoint = _device.Viewport.Unproject(farSource,
                _cameraProjection, _cameraView, world);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        internal static float DistanceBetweenPositions(Vector3 pos1, Vector3 pos2)
        {
            return (pos1 - pos2).Length();
        }

        public static bool GetRayPlaneIntersectionPoint(Ray r, Plane p, out Vector3 result)
        {
            float? dist = r.Intersects(p);

            if (dist.HasValue)
            {
                result = r.Position + (r.Direction*dist.Value);
                return true;
            }
            result = Vector3.Zero;
            return false;
        }


        // O is your object's position
        // P is the position of the object to face
        // U is the nominal "up" vector (typically Vector3.Y)
        // Note: this does not work when O is straight below or straight above P
        public static Matrix RotateToFace(Vector3 O, Vector3 P, Vector3 U)
        {
            Vector3 D = (O - P);
            Vector3 Right = Vector3.Cross(U, D);
            Vector3.Normalize(ref Right, out Right);
            Vector3 Backwards = Vector3.Cross(Right, U);
            Vector3.Normalize(ref Backwards, out Backwards);
            Vector3 Up = Vector3.Cross(Backwards, Right);
            Matrix rot = new Matrix(Right.X, Right.Y, Right.Z, 0, Up.X, Up.Y, Up.Z, 0, Backwards.X, Backwards.Y,
                Backwards.Z, 0, 0, 0, 0, 1);
            return rot;
        }

        /// <summary>
        /// Returns the vector required to push the sphere out of the box, if aligned on a XY plane
        /// </summary>
        /// <param name="CollisionSphere"></param>
        /// <param name="boundingBox"></param>
        /// <returns></returns>
        public static Vector3 CalculateRepulsionVector(BoundingSphere CollisionSphere, BoundingBox boundingBox)
        {
            Vector3 boxCenter = (boundingBox.Min + boundingBox.Max)/2;

            float boxMidLength;

            //sphere above or below box. Align X coord.
            bool vertical = false;
            bool horizontal = false;
            float vertPen = 0;
            float horizPen = 0;
            Vector3 horizontalDirVector = Vector3.Zero;
            Vector3 verticalDirVector = Vector3.Zero;

            //vert collision
            if (CollisionSphere.Center.Y < boundingBox.Min.Y || CollisionSphere.Center.Y > boundingBox.Max.Y)
            {
                boxMidLength =
                    (boxCenter - new Vector3((boundingBox.Min.X + boundingBox.Max.X)/2, boundingBox.Min.Y, boxCenter.Z))
                        .Length();

                vertical = true;
                Vector3 sphereCenter = CollisionSphere.Center;
                sphereCenter.X = boxCenter.X;
                verticalDirVector = sphereCenter - boxCenter;
                float distanceBetweenCenters = verticalDirVector.Length();
                verticalDirVector.Normalize();

                vertPen = distanceBetweenCenters - boxMidLength;
                vertPen = CollisionSphere.Radius - vertPen;
            }

            //horizontal collision
            if (CollisionSphere.Center.X > boundingBox.Max.X || CollisionSphere.Center.X < boundingBox.Min.X)
            {
                boxMidLength =
                    (boxCenter - new Vector3(boundingBox.Min.X, (boundingBox.Min.Y + boundingBox.Max.Y)/2, boxCenter.Z))
                        .Length();


                horizontal = true;
                Vector3 sphereCenter = CollisionSphere.Center;
                sphereCenter.Y = boxCenter.Y;
                horizontalDirVector = sphereCenter - boxCenter;
                float distanceBetweenCenters = horizontalDirVector.Length();
                horizontalDirVector.Normalize();

                horizPen = distanceBetweenCenters - boxMidLength;
                horizPen = CollisionSphere.Radius - horizPen;
            }

            //we're colliding from a corner region
            if (horizontal && vertical)
            {
                //determine which distance is shorter, and deal with that.
                if (horizPen < vertPen)
                    return new Vector3(horizontalDirVector.X*horizPen/2, 0, 0);
                else
                    return new Vector3(0, verticalDirVector.Y*vertPen/2, 0);
            }


            if (vertical && !horizontal)
                return new Vector3(0, verticalDirVector.Y*vertPen, 0);

            if (horizontal && !vertical)
                return new Vector3(horizontalDirVector.X*horizPen, 0, 0);

            return Vector3.Zero;
        }

        public static Vector3 CalculateRepulsionVector(BoundingSphere collisionSphere, BoundingSphere boundingSphere)
        {
            //simply need to return the negative of the direction vector between the two, to a magnitude
            //equal to the (two radii - current penetration distance)
            Vector3 toCollision = boundingSphere.Center - collisionSphere.Center;

            float distanceBetweenCenters = toCollision.Length();
            float desiredDistance = collisionSphere.Radius + boundingSphere.Radius;
            float pushDistance = desiredDistance - distanceBetweenCenters;

            toCollision.Normalize();

            return -toCollision*pushDistance;
        }

        public static int RoundToNearest(int x, int roundToNearest)
        {
            int remainder = x%roundToNearest;

            if (remainder < (roundToNearest/2))
                return x - remainder;
            else
                return x + (roundToNearest - remainder);
        }

        public static double Round(double i, int v)
        {
            return Math.Round(i/v)*v;
        }

        public static float Round(float i, int v)
        {
            return (float) Math.Round(i/v)*v;
        }

        public static void TransformVectorsInList(List<Vector3> vectors, Matrix transform)
        {
            for (int i = 0; i < vectors.Count; i++)
                vectors[i] = Vector3.Transform(vectors[i], transform);
        }

        public static void TranslateVectorsInList(List<Vector3> vectors, Vector3 translation)
        {
            for (int i = 0; i < vectors.Count; i++)
                vectors[i] += translation;
        }

        public static void ScaleVectorsInList(List<Vector3> vectors, float scaleFactor)
        {
            for (int i = 0; i < vectors.Count; i++)
                vectors[i] *= scaleFactor;
        }


        public static bool AlignedOnTwoAxis(Vector3 v, Vector3 currentMousePoint)
        {
            int alignCount = 0;
            if (v.X == currentMousePoint.X)
                alignCount++;

            if (v.Y == currentMousePoint.Y)
                alignCount++;

            if (v.Z == currentMousePoint.Z)
                alignCount++;

            if (alignCount > 1)
                return true;
            return false;
        }

        public static void Intersects(ref Ray ray, out float? distance, Vector3 a, Vector3 b, Vector3 c)
        {
            // Set the Distance to indicate no intersect
            distance = null;
            // Compute vectors along two edges of the triangle.
            Vector3 edge1, edge2;

            Vector3.Subtract(ref c, ref b, out edge1);
            Vector3.Subtract(ref a, ref b, out edge2);

            // Compute the determinant.
            Vector3 directionCrossEdge2;
            Vector3.Cross(ref ray.Direction, ref edge2, out directionCrossEdge2);

            float determinant;
            Vector3.Dot(ref edge1, ref directionCrossEdge2, out determinant);

            // If the ray is parallel to the triangle plane, there is no collision.
            if (determinant > -float.Epsilon && determinant < float.Epsilon)
            {
                return;
            }

            float inverseDeterminant = 1.0f/determinant;

            // Calculate the U parameter of the intersection point.
            Vector3 distanceVector;
            Vector3.Subtract(ref ray.Position, ref b, out distanceVector);

            float triangleU;
            Vector3.Dot(ref distanceVector, ref directionCrossEdge2, out triangleU);
            triangleU *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleU < 0 || triangleU > 1)
            {
                return;
            }

            // Calculate the V parameter of the intersection point.
            Vector3 distanceCrossEdge1;
            Vector3.Cross(ref distanceVector, ref edge1, out distanceCrossEdge1);

            float triangleV;
            Vector3.Dot(ref ray.Direction, ref distanceCrossEdge1, out triangleV);
            triangleV *= inverseDeterminant;

            // Make sure it is inside the triangle.
            if (triangleV < 0 || triangleU + triangleV > 1)
            {
                return;
            }

            // == By here the ray must be inside the triangle

            // Compute the distance along the ray to the triangle.
            float length = 0;
            Vector3.Dot(ref edge2, ref distanceCrossEdge1, out length);
            distance = length*inverseDeterminant;
        }

        public static Matrix GenerateMonoMatrixFromBepu(BEPUutilities.Matrix matrix)
        {
            Matrix world = new Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);

            return world;
        }

        public static BEPUutilities.Matrix GenerateBepuMatrixFromMono(Matrix matrix)
        {
            BEPUutilities.Matrix world = new BEPUutilities.Matrix(
                matrix.M11, matrix.M12, matrix.M13, matrix.M14,
                matrix.M21, matrix.M22, matrix.M23, matrix.M24,
                matrix.M31, matrix.M32, matrix.M33, matrix.M34,
                matrix.M41, matrix.M42, matrix.M43, matrix.M44);

            return world;
        }

        public static BEPUutilities.Vector3 Translate(Vector3 position)
        {
            return new BEPUutilities.Vector3(position.X, position.Y, position.Z);
        }

        public static Vector3 Translate(BEPUutilities.Vector3 position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        internal static List<BEPUutilities.Vector3> ConvertVertsToBepu(List<Vector3> vertices)
        {
            List<BEPUutilities.Vector3> bepuVectors = new List<BEPUutilities.Vector3>(vertices.Count);

            for (int i = 0; i < vertices.Count; i++)
            {
                bepuVectors.Add(new BEPUutilities.Vector3(vertices[i].X, vertices[i].Y, vertices[i].Z));
            }

            return bepuVectors;
        }

        public static int[] ConvertShortToInt(short[] shortIntArray)
        {
            var intarray = new int[shortIntArray.Length];

            for (int i = 0; i < shortIntArray.Length; i++)
            {
                intarray[i] = shortIntArray[i];
            }
            return intarray;
        }
    }
}