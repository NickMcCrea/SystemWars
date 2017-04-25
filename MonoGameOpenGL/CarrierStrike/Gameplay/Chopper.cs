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
        public Chopper() : base()
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
          
            //var rotorCuboid = new ProceduralCuboid(0.03f, 0.6f, 0.03f);
            //rotorCuboid.SetColor(chopperColor);
            //GameObject rotor = GameObjectFactory.CreateRenderableGameObjectFromShape(rotorCuboid, EffectLoader.LoadSM5Effect("flatshaded"));
            //rotor.Transform.SetPosition(new Vector3(0, 0.2f, 0));
            ////rotor.AddComponent(new RotatorComponent(Vector3.Up, 0.1f));
            //rotor.AddComponent(new ShadowCasterComponent());
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(rotor);
            //Children.Add(rotor);

        }


        


    }

    class ChopperController : IComponent, IUpdateable
    {
        public bool Enabled
        {
            get;set;
        }

        public GameObject ParentObject
        {
            get;set;
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
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}
