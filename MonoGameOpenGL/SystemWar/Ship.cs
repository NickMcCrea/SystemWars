﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore;
using MonoGameEngineCore.Rendering.Camera;

namespace SystemWar
{
    public class Ship : GameObject, IUpdateable
    {
        public HighPrecisionPosition HighPrecisionPositionComponent { get; private set; }
        public GameObject shipCameraObject;
        private float desiredMainThrust;
        private float desiredSuperThrust;
        private float currentSuperThrust;
        private float currentMainThrust;
        public bool InOrbit { get; private set; }
        public bool InAtmosphere { get; private set; }
        public Planet CurrentPlanet { get; private set; }
        private float yawThrust, desiredYawThrust;
        private float pitchThrust, desiredPitchThrust;
        private float rollThrust, desiredRollThrust;
        public bool LookMode { get; private set; }
        private float mainThrustUpSpeed = 0.01f;
        private float mainThrustDownSpeed = 0.1f;
        private float otherThrustAlterationSpeed = 0.04f;
        private float lateralThrustAlterationSpeed = 0.005f;
        private float maxRoll = 0.05f;
        private float maxPitch = 10;
        private float maxYaw = 20;
        private float lateralFactor = 0.02f;
        private float mass = 100;
        private Vector3 velocity;
        private float mainThrustBleed = 0.98f;
        private float orientationThrustBleed = 0.8f;
        private float lateralThrustBleed = 0.9f;
        private Vector3 lateralThrust;
        float maxVelocityAtmoshpere = 40f;
        float maxVelocitySpace = 1000f;
        float maxVelocityOrbit = 500f;
        public Ship(string name)
            : base(name)
        {
            Enabled = true;
            shipCameraObject = new GameObject("shipCam");
            shipCameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.1f, ScaleHelper.Billions(3), true));
            AddComponent(new HighPrecisionPosition());
            AddComponent(new ShipController());
            AddComponent(new MouseKeyboardShipController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(shipCameraObject);
            HighPrecisionPositionComponent = GetComponent<HighPrecisionPosition>();
        }

        public void AlterThrust(float amount)
        {

            desiredMainThrust += amount;
          
        }

        public void SetThrust(float amount)
        {
            desiredMainThrust = amount;
            DebugText.Write("desiredThrust: " + desiredMainThrust);
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
            Roll(-amount);
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

            Roll(-leftRight/1000f);

        }

        public void Update(GameTime gameTime)
        {
            //determine max vel according to environment.
            float maxVelToUse = maxVelocitySpace;
            if (InOrbit)
                maxVelToUse = maxVelocityOrbit;
            if (InAtmosphere)
                maxVelToUse = maxVelocityAtmoshpere;
          

            
            if (desiredMainThrust > currentMainThrust)
                currentMainThrust = MathHelper.Lerp(currentMainThrust, desiredMainThrust, mainThrustUpSpeed);
            else
                currentMainThrust = MathHelper.Lerp(currentMainThrust, desiredMainThrust, mainThrustDownSpeed);

            DebugText.Write("currentMainThrust: " + currentMainThrust.ToString());

            if (desiredSuperThrust > currentSuperThrust)
                currentSuperThrust = MathHelper.Lerp(currentSuperThrust, desiredSuperThrust, mainThrustUpSpeed);
            else
                currentSuperThrust = MathHelper.Lerp(currentSuperThrust, desiredSuperThrust, mainThrustDownSpeed);


            rollThrust = MathHelper.Lerp(rollThrust, desiredRollThrust, otherThrustAlterationSpeed * 2);
            pitchThrust = MathHelper.Lerp(pitchThrust, desiredPitchThrust, otherThrustAlterationSpeed);
            yawThrust = MathHelper.Lerp(yawThrust, desiredYawThrust, otherThrustAlterationSpeed);


            if (currentMainThrust > 0)
            {
                Vector3 velChange = (maxVelToUse * currentMainThrust * Transform.WorldMatrix.Forward) / mass;
                velChange *= (100 - (mainThrustBleed * 100));
                velocity += velChange;
            }
            velocity += lateralThrust;

            if (velocity.Length() > maxVelToUse)
            {
                float overShootVel = velocity.Length() - maxVelToUse;
                Vector3 adjustment = -Vector3.Normalize(velocity) * (overShootVel / 20f);
                velocity += adjustment;
            }

            if (currentSuperThrust > 0)
                velocity += (currentSuperThrust) * Transform.WorldMatrix.Forward;


            DebugText.Write("velocity length: " + velocity.Length().ToString()); 
            Transform.Translate(velocity * (float)gameTime.ElapsedGameTime.TotalSeconds);


            if (rollThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Forward, rollThrust);
            if (pitchThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Left, pitchThrust);
            if (yawThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Up, yawThrust);


            velocity *= mainThrustBleed;
            DebugText.Write("velocity after bleed: " + velocity.Length().ToString()); 
        

            lateralThrust *= lateralThrustBleed;
            desiredRollThrust *= orientationThrustBleed;
            desiredPitchThrust *= orientationThrustBleed;
            desiredYawThrust *= orientationThrustBleed;
            pitchThrust *= orientationThrustBleed;
            yawThrust *= orientationThrustBleed;
            rollThrust *= orientationThrustBleed;

            if (!LookMode)
                shipCameraObject.Transform.WorldMatrix = Transform.WorldMatrix;


        }

        public void RealignShip()
        {
            //we want to un-roll the ship to up.
            Vector3 shipUp = Transform.WorldMatrix.Left;
            Vector3 planetUp = Vector3.Normalize((CurrentPlanet.Transform.WorldMatrix.Translation - Transform.WorldMatrix.Translation));

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

        internal void SetInOrbit(Planet planet)
        {
            CurrentPlanet = planet;
            InOrbit = true;
        }

        internal void ExitedOrbit()
        {
            InOrbit = false;
            CurrentPlanet = null;
        }

        internal void SetInAtmosphere()
        {
          
            InAtmosphere = true;
        }

        internal void ExitedAtmosphere()
        {
          
            InAtmosphere = false;
        }

        internal void ToggleCameraCoupling()
        {
            LookMode = !LookMode;
        }
    }
}