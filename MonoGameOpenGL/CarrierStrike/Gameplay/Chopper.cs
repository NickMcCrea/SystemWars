using BEPUphysics.Constraints.SingleEntity;
using BEPUphysics.Entities;
using BEPUphysicsDemos.SampleCode;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
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

        public GameObject ParentObject
        {
            get; set;
        }

        public int UpdateOrder
        {
            get;
        }

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
        }

        public void Update(GameTime gameTime)
        {
            var currentLeft = physicsEntity.PhysicsEntity.WorldTransform.Left;
            var currentForward = physicsEntity.PhysicsEntity.WorldTransform.Forward;
            currentLeft.Y = 0;
            currentForward.Y = 0;
            float tiltForce = 0.01f;
            float lateralForce = 0.1f;
            float rotateForce = 0.1f;

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
                physicsEntity.PhysicsEntity.AngularVelocity += BEPUutilities.Vector3.Up * rotateForce;
            }

            if (input.EvaluateInputBinding("RightRotate"))
            {

                physicsEntity.PhysicsEntity.AngularVelocity -= BEPUutilities.Vector3.Up * rotateForce;
            }
        }
    }
}
