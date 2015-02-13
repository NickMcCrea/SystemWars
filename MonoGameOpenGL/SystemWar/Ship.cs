using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;

namespace SystemWar
{
    public class Ship : GameObject, IUpdateable
    {

        private float desiredMainThrust;
        private float currentMainThrust;
        private float yawThrust, desiredYawThrust;
        private float pitchThrust, desiredPitchThrust;
        private float rollThrust, desiredRollThrust;

        private float mainThrustAlterationSpeed = 0.05f;
        private float otherThrustAlterationSpeed = 0.04f;
        private float maxThrust = 1;
        private float minThrust = -1;
        private float maxRoll = 0.02f;
        private float maxPitch = 10;
        private float maxYaw = 10;
        private float mass = 100;
        private Vector3 velocity;
        private float mainThrustBleed = 0.95f;
        private float otherThrustBleed = 0.6f;
        public Ship(string name)
            : base(name)
        {
            Enabled = true;
        }

        public void AlterThrust(float amount)
        {

            desiredMainThrust += amount;
            if (desiredMainThrust > maxThrust)
                desiredMainThrust = maxThrust;
            if (desiredMainThrust < minThrust)
                desiredMainThrust = minThrust;
        }


        public void Roll(float amount)
        {
            desiredRollThrust += amount;
            if (desiredRollThrust > maxRoll)
                desiredRollThrust = maxRoll;
            if (desiredRollThrust < -maxRoll)
                desiredRollThrust = -maxRoll;
        }

        public void Pitch(float amount)
        {
            desiredPitchThrust += amount;
            if (desiredPitchThrust > maxPitch)
                desiredPitchThrust = maxPitch;
            if (desiredPitchThrust < -maxPitch)
                desiredPitchThrust = -maxPitch;
        }

        public void Yaw(float amount)
        {
            desiredYawThrust += amount;
            if (desiredYawThrust > maxYaw)
                desiredYawThrust = maxYaw;
            if (desiredYawThrust < -maxYaw)
                desiredYawThrust = -maxYaw;
        }



        public void Update(GameTime gameTime)
        {
            float threshold = 0.001f;
            if (!MonoMathHelper.AlmostEquals(desiredMainThrust, currentMainThrust, threshold))
            {
                currentMainThrust = MathHelper.Lerp(currentMainThrust, desiredMainThrust, mainThrustAlterationSpeed);
            }

            if (!MonoMathHelper.AlmostEquals(desiredRollThrust, rollThrust, threshold))
            {
                rollThrust = MathHelper.Lerp(rollThrust, desiredRollThrust, otherThrustAlterationSpeed);
            }

            if (!MonoMathHelper.AlmostEquals(desiredPitchThrust, pitchThrust, threshold))
            {
                pitchThrust = MathHelper.Lerp(pitchThrust, desiredPitchThrust, otherThrustAlterationSpeed);
            }

            if (!MonoMathHelper.AlmostEquals(desiredYawThrust, yawThrust, threshold))
            {
                yawThrust = MathHelper.Lerp(yawThrust, desiredYawThrust, otherThrustAlterationSpeed);
            }

            if (currentMainThrust != 0)
            {

                if (currentMainThrust > maxThrust)
                    currentMainThrust = maxThrust;
                if (currentMainThrust < minThrust)
                    currentMainThrust = minThrust;

                velocity += (currentMainThrust / mass) * Transform.WorldMatrix.Forward;
                Transform.Translate(velocity*(float) gameTime.ElapsedGameTime.TotalMilliseconds);
            }

            if (rollThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Forward, rollThrust);
            if (pitchThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Left, pitchThrust);
            if (yawThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Up, yawThrust);

            DebugText.Write(currentMainThrust.ToString());
            DebugText.Write(rollThrust.ToString());
            velocity *= mainThrustBleed;
            desiredRollThrust *= otherThrustBleed;
            desiredPitchThrust *= otherThrustBleed;
            desiredYawThrust *= otherThrustBleed;
            //ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Left, pitch);
            // ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Forward, roll);
            // ParentObject.Transform.Rotate(ParentObject.Transform.WorldMatrix.Up, yaw);
        }

        public int UpdateOrder { get; set; }
        public bool Enabled { get; set; }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
    }
}
