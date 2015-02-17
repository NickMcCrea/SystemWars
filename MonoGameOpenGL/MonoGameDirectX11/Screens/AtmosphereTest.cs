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
        private GameObject planetObject;
        private GameObject atmosphereObject;

        public AtmosphereTest()
            : base()
        {

            ProceduralSphere planet = new ProceduralSphere(50, 50);
            ProceduralSphere atmosphere = new ProceduralSphere(100, 100);

            mouseCamera.moveSpeed = 10f;
            atmosphere.InsideOut();

            atmosphereTexture = SystemCore.ContentManager.Load<Texture2D>("Textures/AtmosphereGradient3");

            planet.Scale(6000);
            atmosphere.Scale(6300);

            planetObject = GameObjectFactory.CreateRenderableGameObjectFromShape(planet,
                EffectLoader.LoadEffect("flatshaded"));


            atmosphereObject = GameObjectFactory.CreateRenderableGameObjectFromShape(atmosphere,
                EffectLoader.LoadEffect("atmosphere"));


            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphereObject);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(planetObject);

            atmosphereRenderComponent = atmosphereObject.GetComponent<EffectRenderComponent>();


            atmosphereRenderComponent.effect.Parameters["gTex"].SetValue(atmosphereTexture);

            bool offCenter = true;

            if (offCenter)

            {
                planetObject.Transform.Translate(new Vector3(10000, 0, 0));
                atmosphereObject.Transform.Translate(new Vector3(10000, 0, 0));
                mouseCamera.World.Translation = new Vector3(25000, 0, 0);
            }

            else
            {
                mouseCamera.World.Translation = new Vector3(15000, 0, 0);
            }

        }

        public override void Update(GameTime gameTime)
        {
         
            //planetObject.Transform.Translate(new Vector3(1,0,0));
            //atmosphereObject.Transform.Translate(new Vector3(1, 0, 0));
            base.Update(gameTime);
        }

        private float angle;
        Vector3 lightPos = new Vector3(1,0,0);
        public override void Render(GameTime gameTime)
        {
            //gLamp0DirPos
            Vector3 lightAngle = Vector3.Transform(lightPos, Matrix.CreateRotationY(angle));
            atmosphereRenderComponent.effect.Parameters["gLamp0DirPos"].SetValue(lightAngle);
            angle += 0.01f;
            atmosphereRenderComponent.effect.Parameters["CameraPositionInObjectSpace"].SetValue(mouseCamera.Position - atmosphereObject.Transform.WorldMatrix.Translation);
            atmosphereRenderComponent.effect.Parameters["AtmosphereRadius"].SetValue(6300f);
            atmosphereRenderComponent.effect.Parameters["SurfaceRadius"].SetValue(6000f);

            DebugText.Write(SystemCore.ActiveCamera.Position.ToString());

            base.Render(gameTime);
        }
    }
}
