using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.Rendering.Camera
{
    public class MouseFreeCamera : ICamera
    {
        public bool Slow { get; set; }
        public Matrix View { get; set; }
        public Matrix Projection { get; set; }
        public Matrix World;
        public Vector3 Position { get { return World.Translation; } }
        public float FarZ
        {
            get; set;
        }

        public float NearZ
        {
            get; set;
        }
        public Vector3 Right
        {
            get
            {
                return Matrix.Invert(View).Right;

            }
            set
            {

            }
        }

        float leftrightRot = MathHelper.PiOver2;
        float updownRot = MathHelper.PiOver4;
        const float rotationSpeed = 0.0003f;
        public float moveSpeed = 0.1f;
        Vector3 moveVector = new Vector3(0, 0, 0);
    

        public MouseFreeCamera(Vector3 startPosition)
        {
            FarZ = 250f;
            NearZ = 0.25f;
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, NearZ, FarZ);
            World.Translation = startPosition;   
            UpdateViewMatrix();
        }

        public MouseFreeCamera(Vector3 startPosition, float near, float far)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, near, far);
            World.Translation = startPosition;
            FarZ = far;
            NearZ = near;
            UpdateViewMatrix();
        }

        public void Update(GameTime gameTime, float xDifference, float yDifference)
        {
         
            leftrightRot -= rotationSpeed * xDifference * gameTime.ElapsedGameTime.Milliseconds;
            updownRot -= rotationSpeed * yDifference * gameTime.ElapsedGameTime.Milliseconds;

            UpdateViewMatrix();

            if (Slow)
                moveVector *= 0.01f;

            AddToCameraPosition(moveVector * gameTime.ElapsedGameTime.Milliseconds);
           
            //zero out for next frame
            moveVector = new Vector3(0, 0, 0);
        }

        public void MoveForward()
        {
            moveVector += new Vector3(0, 0, -1);
        }

        public void MoveBackward()
        {
            moveVector += new Vector3(0, 0, 1);
        }

        public void MoveLeft()
        {
            moveVector += new Vector3(-1, 0, 0);
        }

        public void MoveRight()
        {
            moveVector += new Vector3(1, 0, 0);
        }

        private void AddToCameraPosition(Vector3 vectorToAdd)
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);
            Vector3 rotatedVector = Vector3.Transform(vectorToAdd, cameraRotation);
            World.Translation += moveSpeed * rotatedVector;
            UpdateViewMatrix();
        }

        private void UpdateViewMatrix()
        {
            Matrix cameraRotation = Matrix.CreateRotationX(updownRot) * Matrix.CreateRotationY(leftrightRot);

            Vector3 cameraOriginalTarget = Vector3.Forward;
            Vector3 cameraOriginalUpVector = Vector3.Up;

            Vector3 cameraRotatedTarget = Vector3.Transform(cameraOriginalTarget, cameraRotation);
            Vector3 cameraFinalTarget = World.Translation + cameraRotatedTarget;

            Vector3 cameraRotatedUpVector = Vector3.Transform(cameraOriginalUpVector, cameraRotation);
            World.Forward = cameraFinalTarget;
            World.Up = cameraRotatedUpVector;
            View = Matrix.CreateLookAt(World.Translation, cameraFinalTarget, cameraRotatedUpVector);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="horizontalAngle">Angle away from Vector3.Forward</param>
        /// <param name="verticalAngle">Angle from Vector3.up</param>
        public void SetPositionAndLook(Vector3 pos, float horizontalAngle, float verticalAngle)
        {
            World.Translation = pos;
            updownRot = verticalAngle;
            leftrightRot = horizontalAngle;
        }

    }
}
