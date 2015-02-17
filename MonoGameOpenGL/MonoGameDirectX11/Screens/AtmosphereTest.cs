using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using Particle3DSample;
using MonoGameEngineCore;
using MonoGameEngineCore.Helper;

namespace MonoGameDirectX11.Screens
{
    public class AtmosphereTest : MouseCamScreen
    {


        private Texture2D atmosphereTexture;
        private EffectRenderComponent atmosphereRenderComponent;

        public AtmosphereTest()
            : base()
        {
        
            ProceduralSphere planet = new ProceduralSphere(50,50);
            ProceduralSphere atmosphere = new ProceduralSphere(100,100);
            atmosphere.Indices = atmosphere.Indices.Reverse().ToArray();
            atmosphereTexture = SystemCore.ContentManager.Load<Texture2D>("Textures/AtmosphereGradient3");

            planet.Scale(100f);
            atmosphere.Scale(120f);
         
            var planetObject = GameObjectFactory.CreateRenderableGameObjectFromShape(planet,
                EffectLoader.LoadEffect("flatshaded"));


            var atmosphereObject = GameObjectFactory.CreateRenderableGameObjectFromShape(atmosphere,
                EffectLoader.LoadEffect("atmosphere"));

            
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphereObject);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(planetObject);

            atmosphereRenderComponent = atmosphereObject.GetComponent<EffectRenderComponent>();


            atmosphereRenderComponent.effect.Parameters["gTex"].SetValue(atmosphereTexture);

          

        }

        public override void Update(GameTime gameTime)
        {
         
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            //gLamp0DirPos
            atmosphereRenderComponent.effect.Parameters["gLamp0DirPos"].SetValue(new Vector3(1,0,0));
            atmosphereRenderComponent.effect.Parameters["CameraPositionInObjectSpace"].SetValue(SystemCore.ActiveCamera.Position);


            DebugText.Write(SystemCore.ActiveCamera.Position.ToString());

            base.Render(gameTime);
        }
    }
}
