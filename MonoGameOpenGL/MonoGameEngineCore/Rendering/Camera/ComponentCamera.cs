using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.Rendering.Camera
{
    public class ComponentCamera : ICamera, IComponent, IUpdateable
    {
        public bool HighPrecisionMode { get; set; }
        public Matrix World;
        public Matrix View
        {
            get;
            set;
        }
        public Matrix Projection
        {
            get;
            set;
        }
        public Vector3 Position
        {
            get { return World.Translation; }
        }
        public GameObject.GameObject ParentObject
        {
            get;
            set;
        }

        public ComponentCamera(float fov, float aspect, float near, float far, bool highPrecision)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(fov, aspect, near, far);
            HighPrecisionMode = highPrecision;
        }

        public ComponentCamera()
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.3f, 1000.0f);
            HighPrecisionMode = false;
        }

        public void Initialise()
        {
            Enabled = true;
        }

        public void PostInitialise()
        {

        }

        public void SetPositionAndLookDir(Vector3 pos, Vector3 target)
        {

            World = MonoMathHelper.GenerateWorldMatrixFromPositionAndTarget(pos, target, Vector3.Up);
            if (HighPrecisionMode)
            {
                World.Translation = Vector3.Zero;

            }
            View = Matrix.Invert(World);
        }

        public void SetPosition(Vector3 newPosition)
        {
            if (!HighPrecisionMode)
                World.Translation = newPosition;
            View = Matrix.Invert(World);
        }

        public bool Enabled
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
            if (HighPrecisionMode)
                World = Matrix.CreateWorld(Vector3.Zero, ParentObject.Transform.AbsoluteTransform.Forward,
                    ParentObject.Transform.AbsoluteTransform.Up);
            else
            {
                World = Matrix.CreateWorld(ParentObject.Transform.AbsoluteTransform.Translation, ParentObject.Transform.AbsoluteTransform.Forward,
                   ParentObject.Transform.AbsoluteTransform.Up);
            }
            View = Matrix.Invert(World);
        }

        public int UpdateOrder
        {
            get;
            set;
        }

        public float FarZ
        {
            get; set;
        }

        public float NearZ
        {
            get; set;
        }
        public Vector3 Right { get; set; }

        public event EventHandler<EventArgs> UpdateOrderChanged;
    }


}
