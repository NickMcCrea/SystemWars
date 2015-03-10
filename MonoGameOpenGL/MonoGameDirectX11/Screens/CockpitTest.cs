using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using MonoGameEngineCore.ScreenManagement;

namespace MonoGameDirectX11.Screens
{
    class CockpitTest : Screen
    {
      
      
        private SpriteFont font;
        private GameObject cameraObject;

        public CockpitTest()
            : base()
        {
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
        
          
            Model geoDesicModel = SystemCore.ContentManager.Load<Model>("Models/geodesic2");
            ProceduralShape geodesicShape = ModelMeshParser.GetShapeFromModelWithUVs(geoDesicModel);
            geodesicShape.InsideOut();

            cameraObject = new GameObject();
            cameraObject.AddComponent(new RenderGeometryComponent(geodesicShape));
            cameraObject.AddComponent(new EffectRenderComponent(EffectLoader.LoadEffect("cockpitscreen")));
            cameraObject.GetComponent<EffectRenderComponent>().DrawOrder = 100;
            cameraObject.AddComponent(new ComponentCamera());
            cameraObject.AddComponent(new MouseController());

            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);

            font = SystemCore.ContentManager.Load<SpriteFont>("Fonts/neuropolitical");

            var testCube = GameObjectFactory.CreateRenderableGameObjectFromShape(new ProceduralCube(),
                EffectLoader.LoadEffect("flatshaded"));
            
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(testCube);
        }

        public override void Update(GameTime gameTime)
        {

          
            base.Update(gameTime);
        }


        public override void Render(GameTime gameTime)
        {

            SystemCore.GraphicsDevice.Clear(Color.Black);

            base.Render(gameTime);
        }

        public override void RenderSprites(GameTime gameTime)
        {
           
        }
    }
}
