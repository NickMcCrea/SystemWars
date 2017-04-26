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
    class Carrier : GameObject
    {
        public Carrier() : base()
        {

            Color carrierColor = Color.Red;

            this.AddComponent(new CarrierController());

            ProceduralShape body = new ProceduralCuboid(1f, 2f, 0.5f);
            body.SetColor(carrierColor);
            RenderGeometryComponent renderGeom = new RenderGeometryComponent(body);
            EffectRenderComponent effectComp = new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded"));
            ShadowCasterComponent shadowComp = new ShadowCasterComponent();
            AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.box));
            AddComponent(renderGeom);
            AddComponent(effectComp);
            AddComponent(shadowComp);

       

            var conTower = new ProceduralCuboid(0.25f, 0.25f, 0.35f);
            conTower.SetColor(carrierColor);
            GameObject conT = GameObjectFactory.CreateRenderableGameObjectFromShape(conTower, EffectLoader.LoadSM5Effect("flatshaded"));
            conT.Transform.RelativeTransform.Translation = new Vector3(0.8f, 0.8f, 0);
            conT.AddComponent(new ShadowCasterComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(conT);
            AddChild(conT);

        }





    }

    class CarrierController : IComponent, IUpdateable
    {
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
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}
