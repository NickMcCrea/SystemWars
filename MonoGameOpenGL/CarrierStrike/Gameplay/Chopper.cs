using BEPUphysics.Constraints.SingleEntity;
using BEPUphysics.Entities;
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

            //SingleEntityAngularMotor mot = new SingleEntityAngularMotor(physicsEntity.PhysicsEntity);

        }



        private void AddInputBindings()
        {
            input.AddKeyDownBinding("Left", Microsoft.Xna.Framework.Input.Keys.J);
            input.AddKeyDownBinding("Right", Microsoft.Xna.Framework.Input.Keys.L);
            input.AddKeyDownBinding("Forward", Microsoft.Xna.Framework.Input.Keys.I);
            input.AddKeyDownBinding("Back", Microsoft.Xna.Framework.Input.Keys.K);


        }

        public void Update(GameTime gameTime)
        {

            if (input.EvaluateInputBinding("Left"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity += BEPUutilities.Vector3.Left * 0.1f;
                // physicsEntity.PhysicsEntity.AngularVelocity -= BEPUutilities.Vector3.Forward * 0.1f;
            }

            if (input.EvaluateInputBinding("Right"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity += BEPUutilities.Vector3.Right * 0.1f;
                // physicsEntity.PhysicsEntity.AngularVelocity += BEPUutilities.Vector3.Forward * 0.1f;
            }
            if (input.EvaluateInputBinding("Forward"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity += BEPUutilities.Vector3.Forward * 0.1f;
            }

            if (input.EvaluateInputBinding("Back"))
            {
                physicsEntity.PhysicsEntity.LinearVelocity += BEPUutilities.Vector3.Backward * 0.1f;
            }
        }
    }
}
