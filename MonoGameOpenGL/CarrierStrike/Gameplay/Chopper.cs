using BEPUphysics;
using BEPUphysics.Constraints.SingleEntity;
using BEPUphysics.Entities;
using BEPUphysicsDemos.SampleCode;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarrierStrike.Gameplay
{
    class Chopper : GameObject
    {
        

        public Chopper() : base("chopper")
        {

            Color chopperColor = Color.Red;

            this.AddComponent(new ChopperController());

            ProceduralShape body = new ProceduralCuboid(0.15f, 0.3f, 0.15f);
            body.SetColor(chopperColor);
            RenderGeometryComponent renderGeom = new RenderGeometryComponent(body);
            EffectRenderComponent effectComp = new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded"));
            ShadowCasterComponent shadowComp = new ShadowCasterComponent();
            AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.box));


            AddComponent(renderGeom);
            AddComponent(effectComp);
            AddComponent(shadowComp);

            var rotorCuboid = new ProceduralCuboid(0.03f, 0.6f, 0.03f);
            rotorCuboid.SetColor(chopperColor);
            GameObject rotor = GameObjectFactory.CreateRenderableGameObjectFromShape(rotorCuboid, EffectLoader.LoadSM5Effect("flatshaded"));
            rotor.Transform.RelativeTransform.Translation = new Vector3(0, 0.2f, 0);
            rotor.AddComponent(new RotatorComponent(Vector3.Up, 0.1f));
            rotor.AddComponent(new ShadowCasterComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(rotor);
            AddChild(rotor);

        }





    }

    class ChopperController : IComponent, IUpdateable
    {
        PhysicsComponent physicsEntity;
        InputManager input;
        public bool Enabled
        {
            get; set;
        }
        private ControlScheme currentControlScheme = ControlScheme.B;

        private enum ControlScheme
        {
            A,
            B
        }

        public GameObject ParentObject
        {
            get; set;
        }

        public int UpdateOrder
        {
            get;
        }
        float tiltForce = 0.01f;
        float lateralForce = 0.1f;
        float rotateForce = 0.1f;

        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;

        public void Initialise()
        {
            Enabled = true;
            physicsEntity = ParentObject.GetComponent<PhysicsComponent>();
            input = SystemCore.Input;
            AddInputBindings();

        }

        public void PostInitialise()
        {
            physicsEntity.PhysicsEntity.IsAffectedByGravity = false;
            physicsEntity.PhysicsEntity.LinearDamping = 0.8f;
            physicsEntity.PhysicsEntity.AngularDamping = 0.8f;

            UprightSpring uprightSpringConstraint = new UprightSpring(physicsEntity.PhysicsEntity, BEPUutilities.Vector3.Up, 0.1f, 100, 0.4f);
            SystemCore.PhysicsSimulation.Add(uprightSpringConstraint);

        }

        private void AddInputBindings()
        {
            input.AddKeyDownBinding("Left", Microsoft.Xna.Framework.Input.Keys.A);
            input.AddKeyDownBinding("Right", Microsoft.Xna.Framework.Input.Keys.D);
            input.AddKeyDownBinding("Forward", Microsoft.Xna.Framework.Input.Keys.W);
            input.AddKeyDownBinding("Back", Microsoft.Xna.Framework.Input.Keys.S);

            input.AddKeyDownBinding("LeftRotate", Microsoft.Xna.Framework.Input.Keys.O);
            input.AddKeyDownBinding("RightRotate", Microsoft.Xna.Framework.Input.Keys.P);


            input.AddKeyDownBinding("Descend", Microsoft.Xna.Framework.Input.Keys.L);
            input.AddKeyDownBinding("Ascend", Microsoft.Xna.Framework.Input.Keys.OemComma);

        }

        public void Update(GameTime gameTime)
        {


            if (currentControlScheme == ControlScheme.A)
            {
                ControlSchemeA();
            }
            if (currentControlScheme == ControlScheme.B)
            {
                ControlSchemeB();
            }

            RayCastResult result;
            Vector3 pos = ParentObject.Transform.AbsoluteTransform.Translation;
            pos += new Vector3(0, -2, 0);
            BEPUutilities.Vector3 chopperPos = new BEPUutilities.Vector3(pos.X, pos.Y, pos.Z);
            

            BEPUutilities.Ray r = new BEPUutilities.Ray(chopperPos, BEPUutilities.Vector3.Down);
            if (SystemCore.PhysicsSimulation.RayCast(r, out result))
            {
                if (result.HitObject.Tag is Chopper)
                {
                   
                }
                else
                {
                    float distance = result.HitData.T;
                    if (distance < 5)
                        Descend();
                    else
                        Ascend();
                }
            }

        }


        private void ControlSchemeB()
        {
            var currentLeft = physicsEntity.PhysicsEntity.WorldTransform.Left;
            var currentForward = physicsEntity.PhysicsEntity.WorldTransform.Forward;
            var cameraForward = Matrix.Invert(SystemCore.ActiveCamera.View).Forward;

            cameraForward.Y = 0;

            currentLeft.Y = 0;
            currentForward.Y = 0;

            Vector2 leftStick = input.GetLeftStickState();
            leftStick.X = -leftStick.X;
            Vector2 rightStick = input.GetRightStickState();

            

            Vector2 current2DForwardVec = new Vector2(currentForward.X, currentForward.Z);
            Vector2 current2DLeftVec = new Vector2(currentLeft.X, currentLeft.Z);
            Vector2 current2DCameraVec = new Vector2(cameraForward.X, cameraForward.Z);


            //move forward with a speed that varies in proportion to how "forward" we're pointing the stick
            float angle = (float)Math.Atan2(current2DForwardVec.Y - leftStick.Y, current2DForwardVec.X - leftStick.X);
            physicsEntity.PhysicsEntity.LinearVelocity += currentForward * (leftStick.Length() * (lateralForce * 2));
            var speedFactor = (float)Math.PI * 2 - angle;
            DebugText.Write(speedFactor.ToString());

            float dot = Vector2.Dot(current2DLeftVec, Vector2.Normalize(leftStick));

      
            if (dot > 0)
                LeftRotate();
            if (dot < -0f)
                RightRotate();



        }

        private void ControlSchemeA()
        {
            var currentLeft = physicsEntity.PhysicsEntity.WorldTransform.Left;
            var currentForward = physicsEntity.PhysicsEntity.WorldTransform.Forward;
            currentLeft.Y = 0;
            currentForward.Y = 0;

            if (input.EvaluateInputBinding("Left"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity += currentLeft * lateralForce / 2;
                physicsEntity.PhysicsEntity.AngularVelocity -= currentForward * tiltForce;
            }

            if (input.EvaluateInputBinding("Right"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity -= currentLeft * lateralForce / 2;
                physicsEntity.PhysicsEntity.AngularVelocity += currentForward * tiltForce;
            }
            if (input.EvaluateInputBinding("Forward"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity += currentForward * 0.1f;
                physicsEntity.PhysicsEntity.AngularVelocity += currentLeft * tiltForce;
            }

            if (input.EvaluateInputBinding("Back"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity -= currentForward * lateralForce / 2;
                physicsEntity.PhysicsEntity.AngularVelocity -= currentLeft * tiltForce;
            }

            if (input.EvaluateInputBinding("LeftRotate"))
            {
                LeftRotate();
            }

            if (input.EvaluateInputBinding("RightRotate"))
            {
                RightRotate();
            }

            if (input.EvaluateInputBinding("Descend"))
            {
                Descend();
            }

            if (input.EvaluateInputBinding("Ascend"))
            {
                Ascend();
            }

            Vector2 leftStick = input.GetLeftStickState();
            float forwardBack = leftStick.Y;
            float leftRight = leftStick.X;

            Vector2 rightStick = input.GetRightStickState();


            physicsEntity.PhysicsEntity.LinearVelocity += currentForward * (forwardBack * lateralForce);
            physicsEntity.PhysicsEntity.LinearVelocity -= currentLeft * (leftRight * lateralForce / 2);
            physicsEntity.PhysicsEntity.AngularVelocity += currentForward * (leftRight * tiltForce);
            physicsEntity.PhysicsEntity.AngularVelocity += currentLeft * (forwardBack * tiltForce);

            if (rightStick.X > 0.1)
                RightRotate();
            if (rightStick.X < -0.1)
                LeftRotate();
            if (rightStick.Y > 0.1)
                Descend();
            if (rightStick.Y < -0.1)
                Ascend();
        }

        private void RightRotate()
        {
            physicsEntity.PhysicsEntity.AngularVelocity -= BEPUutilities.Vector3.Up * rotateForce;
        }

        private void LeftRotate()
        {
            physicsEntity.PhysicsEntity.AngularVelocity += BEPUutilities.Vector3.Up * rotateForce;
        }

        private void Ascend()
        {
            physicsEntity.PhysicsEntity.LinearVelocity -= BEPUutilities.Vector3.Up * lateralForce / 2;
        }

        private void Descend()
        {
            physicsEntity.PhysicsEntity.LinearVelocity += BEPUutilities.Vector3.Up * lateralForce / 2;
        }
    }
}
