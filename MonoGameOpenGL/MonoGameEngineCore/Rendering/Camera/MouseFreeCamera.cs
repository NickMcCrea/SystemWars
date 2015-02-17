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

        float leftrightRot = MathHelper.PiOver2;
        float updownRot = MathHelper.PiOver4;
        const float rotationSpeed = 0.0003f;
        public float moveSpeed = 0.1f;
        Vector3 moveVector = new Vector3(0, 0, 0);
    

        public MouseFreeCamera(Vector3 startPosition)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.01f, 50000f);
            World.Translation = startPosition;

            UpdateViewMatrix();
        }

        public MouseFreeCamera(Vector3 startPosition, float near, float far)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, near, far);
            World.Translation = startPosition;

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

        public void Left()
        {
            moveVector += new Vector3(-1, 0, 0);
        }

        public void Right()
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

            View = Matrix.CreateLookAt(World.Translation, cameraFinalTarget, cameraRotatedUpVector);
        }

    }
}
