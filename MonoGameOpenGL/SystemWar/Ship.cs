using System;
using System.Security.Cryptography;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities.DataStructures;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore;
using MonoGameEngineCore.Rendering.Camera;
using System.Diagnostics;

namespace SystemWar
{
    public class Ship : GameObject, IUpdateable
    {
        public HighPrecisionPosition HighPrecisionPositionComponent { get; private set; }
        public SolarSystem SolarSystem { get; set; }
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
        private Vector3 movementAppliedLastFrame;
        private float mainThrustBleed = 0.98f;
        private float orientationThrustBleed = 0.8f;
        private float lateralThrustBleed = 0.9f;
        private Vector3 lateralThrust;
        float maxVelocityAtmoshpere = 50f;
        float maxVelocitySpace = 1000f;
        float maxVelocityOrbit = 500f;
        float superThrustVelocity = 5000f;
        public bool Landed { get; private set; }

        public Ship(string name)
            : base(name)
        {
            Enabled = true;
            shipCameraObject = new GameObject("shipCam");
            shipCameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.1f, ScaleHelper.Billions(3), true));
            AddComponent(new HighPrecisionPosition());
            AddComponent(new ShipController());

            if (System.Environment.MachineName != "NICKMCCREA-PC")
                AddComponent(new MouseKeyboardShipController());

            AddComponent(new PhysicsComponent(true, false, PhysicsMeshType.sphere));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(shipCameraObject);
            HighPrecisionPositionComponent = GetComponent<HighPrecisionPosition>();


        }

        public void AlterThrust(float amount)
        {
            if (Landed)
                return;

            desiredMainThrust += amount;

        }

        public void SetThrust(float amount)
        {
            if (Landed)
                return;
            if (amount > 1)
                amount = 1;
            desiredMainThrust = amount;
        }

        public void SetSuperThrust(float amount)
        {
            if (Landed)
                return;
            desiredSuperThrust = amount;
        }

        public void Roll(float amount)
        {
            if (Landed)
                return;
            desiredRollThrust += amount;
            if (desiredRollThrust > maxRoll)
                desiredRollThrust = maxRoll;
            if (desiredRollThrust < -maxRoll)
                desiredRollThrust = -maxRoll;
        }

        public void Pitch(float amount)
        {
            if (Landed)
                return;
            desiredPitchThrust += amount;
            if (desiredPitchThrust > maxPitch)
                desiredPitchThrust = maxPitch;
            if (desiredPitchThrust < -maxPitch)
                desiredPitchThrust = -maxPitch;
        }

        public void Yaw(float amount)
        {
            if (Landed)
                return;
            if (InAtmosphere)
                Roll(-amount);

            desiredYawThrust += amount;
            if (desiredYawThrust > maxYaw)
                desiredYawThrust = maxYaw;
            if (desiredYawThrust < -maxYaw)
                desiredYawThrust = -maxYaw;
        }

        public void LateralThrust(float leftRight, float upDown)
        {
            if (Landed && upDown > 0)
                Landed = false;


            if (Landed)
                return;

            Vector3 vec = Transform.WorldMatrix.Left * leftRight;
            Vector3 vec3 = Transform.WorldMatrix.Up * upDown;
            vec += vec3;
            lateralThrust += vec * lateralFactor;

            Roll(-leftRight / 1000f);



        }

        public void Update(GameTime gameTime)
        {




            //determine max vel according to environment.
            float maxVelToUse = maxVelocitySpace;
            if (InOrbit)
                maxVelToUse = maxVelocityOrbit;

            if (InAtmosphere)
            {
                maxVelToUse = maxVelocityAtmoshpere;

                Vector3 realWorldPos = SolarSystem.GetRenderPosition(HighPrecisionPositionComponent.Position, CurrentPlanet.Position.Position);
                realWorldPos.Normalize();

                float downAngle = Vector3.Dot(Transform.WorldMatrix.Forward, realWorldPos);

                //increase max velocity as the nose points down, and vice versa.
                float velAdjustForGravity = MonoMathHelper.MapFloatRange(0, 2, 0.5f, 2f, downAngle + 1);
                maxVelToUse *= velAdjustForGravity;
            }




            if (desiredMainThrust > currentMainThrust)
                currentMainThrust = MathHelper.Lerp(currentMainThrust, desiredMainThrust, mainThrustUpSpeed);
            else
                currentMainThrust = MathHelper.Lerp(currentMainThrust, desiredMainThrust, mainThrustDownSpeed);


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
                velocity += (currentSuperThrust) * superThrustVelocity * Transform.WorldMatrix.Forward;



            movementAppliedLastFrame = velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            Transform.Translate(movementAppliedLastFrame);


            if (rollThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Forward,
                    rollThrust * (float)gameTime.ElapsedGameTime.TotalSeconds * 100);
            if (pitchThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Left,
                    pitchThrust * (float)gameTime.ElapsedGameTime.TotalSeconds * 100);
            if (yawThrust != 0)
                Transform.Rotate(Transform.WorldMatrix.Up,
                    yawThrust * (float)gameTime.ElapsedGameTime.TotalSeconds * 100);


            HandleCollision();

            SolarSystem.AdjustObjectsForRendering(HighPrecisionPositionComponent.Position);

            velocity *= mainThrustBleed;
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

        private void HandleCollision()
        {

            PhysicsComponent comp = GetComponent<PhysicsComponent>();
            if(comp == null)
                return;
            
            ReadOnlyList<CollidablePairHandler> pairs;

            if (SystemCore.PhysicsOnBackgroundThread)
            {
                lock (SystemCore.PhysicsSimulation.BufferedStates.InterpolatedStates.FlipLocker)
                {
                    pairs = comp.PhysicsEntity.CollisionInformation.Pairs;
                }
            }
            else
            {
                pairs = comp.PhysicsEntity.CollisionInformation.Pairs;
            }

            foreach (CollidablePairHandler collidablePairHandler in pairs)
            {

                if (collidablePairHandler.Colliding)
                {

                    foreach (ContactInformation contactInformation in collidablePairHandler.Contacts)
                    {
                        BEPUutilities.Vector3 removeVector = -contactInformation.Contact.Normal *
                                                              contactInformation.Contact.PenetrationDepth;
                        Vector3 slopeNormal = contactInformation.Contact.Normal.ToXNAVector();

                        if (Landed)
                        {
                            AlignToAbitraryAxis(contactInformation.Contact.Normal.ToXNAVector());
                        }
                        else
                        {
                            Vector3 velNormal = Vector3.Normalize(velocity);
                            float angle = MonoMathHelper.GetAngleBetweenVectors(velNormal,
                                Transform.WorldMatrix.Down);
                            float speed = velocity.Length();


                            //we're going slow
                            if (speed < maxVelocityAtmoshpere)
                            {
                                //we're heading down
                                if (angle < MathHelper.ToRadians(20))
                                {
                                    //the angle of the slope we've hit is sufficiently shallow
                                    Vector3 planetUp = Vector3.Normalize((CurrentPlanet.Transform.WorldMatrix.Translation - Transform.WorldMatrix.Translation));

                                    if (MonoMathHelper.GetAngleBetweenVectors(slopeNormal, planetUp) <
                                        MathHelper.ToDegrees(20))
                                        LandShip();
                                    else
                                    {
                                        //too steep to land on
                                        Transform.Translate(removeVector.ToXNAVector());
                                        //Stop();
                                    }

                                }
                                else //not moving down
                                {
                                    Transform.Translate(removeVector.ToXNAVector());
                                    //Stop();
                                }

                            }
                            else //too fast
                            {
                                //explode!
                                Transform.Translate(removeVector.ToXNAVector());
                                //Stop();
                            }
                        }


                    }

                }
            }


        }

        private void Stop()
        {
            velocity = Vector3.Zero;
            desiredMainThrust = 0;
            currentMainThrust = 0;
            lateralThrust = Vector3.Zero;
        }

        private void LandShip()
        {
            Landed = true;
            velocity = Vector3.Zero;
            velocity *= 0;
            lateralThrust *= 0;
            desiredRollThrust *= 0;
            desiredPitchThrust *= 0;
            desiredYawThrust *= 0;
            pitchThrust *= 0;
            yawThrust *= 0;
            rollThrust *= 0;
        }

        public void AlignToAbitraryAxis(Vector3 up)
        {
            //pitch and roll until the ship up vector matches the input vector.
            if (!float.IsNaN(up.X))
            {
                float angle = Vector3.Dot(Transform.WorldMatrix.Left, up);
                desiredRollThrust += angle / 50f;
                angle = Vector3.Dot(Transform.WorldMatrix.Forward, up);
                desiredPitchThrust -= angle / 50;

                if (desiredPitchThrust > maxPitch)
                    desiredPitchThrust = maxPitch;
                if (desiredPitchThrust < -maxPitch)
                    desiredPitchThrust = -maxPitch;

                if (desiredRollThrust > maxRoll)
                    desiredRollThrust = maxRoll;
                if (desiredRollThrust < -maxRoll)
                    desiredRollThrust = -maxRoll;

            }
        }

        public void RealignShipToPlanetUp()
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
