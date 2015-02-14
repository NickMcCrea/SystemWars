using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;

namespace SystemWar
{
    public class Ship : GameObject, IUpdateable
    {

        private float desiredMainThrust;
        private float desiredSuperThrust;
        private float currentSuperThrust;
        private float currentMainThrust;
        public bool InAtmosphere { get; private set; }
        private Planet currentPlanet;
        private float yawThrust, desiredYawThrust;
        private float pitchThrust, desiredPitchThrust;
        private float rollThrust, desiredRollThrust;
        

        private float mainThrustUpSpeed = 0.01f;
        private float mainThrustDownSpeed = 0.1f;
        private float otherThrustAlterationSpeed = 0.04f;
        private float lateralThrustAlterationSpeed = 0.005f;

        
        private float maxThrust = 1;
        private float minThrust = -0.1f;
        private float maxRoll = 0.02f;
        private float maxPitch = 10;
        private float maxYaw = 20;
        private float lateralFactor = 0.00005f;
        private float mass = 100;
        private Vector3 velocity;
        private float mainThrustBleed = 0.95f;
        private float orientationThrustBleed = 0.8f;
        private float lateralThrustBleed = 0.9f;
        private Vector3 lateralThrust;
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

        public void SetThrust(float amount)
        {
            desiredMainThrust = amount * 0.8f;
        }

        public void SetSuperThrust(float amount)
        {
            desiredSuperThrust = amount;
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
            Roll(-amount / 2);
            desiredYawThrust += amount;
            if (desiredYawThrust > maxYaw)
                desiredYawThrust = maxYaw;
            if (desiredYawThrust < -maxYaw)
                desiredYawThrust = -maxYaw;
        }

        public void LateralThrust(float leftRight, float upDown)
        {
            Vector3 vec = Transform.WorldMatrix.Left * leftRight;
            Vector3 vec3 = Transform.WorldMatrix.Up * upDown;
            vec += vec3;
            lateralThrust += vec * lateralFactor;

        }

        public void Update(GameTime gameTime)
        {
            float threshold = 0.001f;
            if (!MonoMathHelper.AlmostEquals(desiredMainThrust, currentMainThrust, threshold))
            {
                if (desiredMainThrust > currentMainThrust)
                    currentMainThrust = MathHelper.Lerp(currentMainThrust, desiredMainThrust, mainThrustUpSpeed);
                else
                    currentMainThrust = MathHelper.Lerp(currentMainThrust, desiredMainThrust, mainThrustDownSpeed);
            }
            else
            {
                currentMainThrust = 0;
                desiredMainThrust = 0;
            }

            if (!MonoMathHelper.AlmostEquals(desiredSuperThrust, currentSuperThrust, threshold))
            {
                if (desiredSuperThrust > currentSuperThrust)
                    currentSuperThrust = MathHelper.Lerp(currentSuperThrust, desiredSuperThrust, mainThrustUpSpeed);
                else
                    currentSuperThrust = MathHelper.Lerp(currentSuperThrust, desiredSuperThrust, mainThrustDownSpeed);
            }
            else
            {
                currentSuperThrust = 0;
                desiredSuperThrust = 0;
            }



            if (!MonoMathHelper.AlmostEquals(desiredRollThrust, rollThrust, threshold))
            {
                rollThrust = MathHelper.Lerp(rollThrust, desiredRollThrust, otherThrustAlterationSpeed * 2);

            }


            if (!MonoMathHelper.AlmostEquals(desiredPitchThrust, pitchThrust, threshold))
            {
                pitchThrust = MathHelper.Lerp(pitchThrust, desiredPitchThrust, otherThrustAlterationSpeed);
            }


            if (!MonoMathHelper.AlmostEquals(desiredYawThrust, yawThrust, threshold))
            {
                yawThrust = MathHelper.Lerp(yawThrust, desiredYawThrust, otherThrustAlterationSpeed);
            }



            


            if (currentMainThrust > maxThrust)
                currentMainThrust = maxThrust;
            if (currentMainThrust < minThrust)
                currentMainThrust = minThrust;

            if (currentMainThrust > 0)
                velocity += (currentMainThrust / mass) * Transform.WorldMatrix.Forward;

            if (currentSuperThrust > 0)
                velocity += (currentSuperThrust) * Transform.WorldMatrix.Forward;


            velocity += lateralThrust;
            Transform.Translate(velocity * (float)gameTime.ElapsedGameTime.TotalMilliseconds);


            if (rollThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Forward, rollThrust);
            if (pitchThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Left, pitchThrust);
            if (yawThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Up, yawThrust);


            velocity *= mainThrustBleed;
            lateralThrust *= lateralThrustBleed;

            desiredRollThrust *= orientationThrustBleed;
            desiredPitchThrust *= orientationThrustBleed;
            desiredYawThrust *= orientationThrustBleed;
            pitchThrust *= orientationThrustBleed;
            yawThrust *= orientationThrustBleed;
            rollThrust *= orientationThrustBleed;

        }

        public void RealignShip()
        {
            //we want to un-roll the ship to up.
            Vector3 shipUp = Transform.WorldMatrix.Left;
            Vector3 planetUp = Vector3.Normalize((currentPlanet.Transform.WorldMatrix.Translation - Transform.WorldMatrix.Translation));

            if (!float.IsNaN(planetUp.X))
            {
                float angle = Vector3.Dot(shipUp, planetUp);
                Roll(angle / 50);
            }
        }

        public int UpdateOrder { get; set; }
        public bool Enabled { get; set; }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        internal void SetInAtmosphere(Planet planet)
        {
            currentPlanet = planet;
            InAtmosphere = true;
        }

        internal void ExitedAtmosphere()
        {
            InAtmosphere = false;
            currentPlanet = null;
        }
    }
}
