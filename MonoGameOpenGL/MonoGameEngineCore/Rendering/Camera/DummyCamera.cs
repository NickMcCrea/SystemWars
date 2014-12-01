using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.Camera
{



    public class DummyCamera : ICamera
    {
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Vector3 Position { get { return World.Translation; } }
        public Matrix World;


        public DummyCamera()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);
        }

        public DummyCamera(float fov, float aspect, float near, float far)
        {

            Projection = Matrix.CreatePerspectiveFieldOfView(fov, aspect, near, far);

        }

        public void SetPositionAndLookDir(Vector3 pos, Vector3 target)
        {
            World = MonoMathHelper.GenerateWorldMatrixFromPositionAndTarget(pos, target, Vector3.Up);
            View = Matrix.Invert(World);
        }


        public void SetPosition(Vector3 newPosition)
        {
            World.Translation = newPosition;
            View = Matrix.Invert(World);
        }
    }

    public class DummyOrthographicCamera : ICamera
    {
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Matrix World;
        public Vector3 Position{get { return World.Translation; }}

        public DummyOrthographicCamera(float width, float height, float near, float far)
        {

            Projection = Matrix.CreateOrthographic(width, height, near, far);

        }

        public void SetPositionAndLookDir(Vector3 pos, Vector3 target)
        {
            World = MonoMathHelper.GenerateWorldMatrixFromPositionAndTarget(pos, target, Vector3.Up);
            View = Matrix.Invert(World);
        }


        public void SetPosition(Vector3 newPosition)
        {
            World.Translation = newPosition;
            View = Matrix.Invert(World);
        }
    }
}
