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



       

        private GameObject planetObject;
        private Atmosphere atmosphereObject;

       public AtmosphereTest()
            : base()
        {

         
         
            ProceduralSphere planet = new ProceduralSphere(50, 50);
            ProceduralSphere atmosphere = new ProceduralSphere(100, 100);

            mouseCamera.moveSpeed = 2f;
            atmosphere.InsideOut();

       
            planet.Scale(6000);
         
            planetObject = GameObjectFactory.CreateRenderableGameObjectFromShape(planet,
                EffectLoader.LoadEffect("flatshaded"));


            atmosphereObject = new Atmosphere(6300,6000);

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphereObject);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(planetObject);

            

           
           


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

     
        public override void Render(GameTime gameTime)
        {

            Vector3 lightPos = Vector3.Zero;
            foreach (SceneLight light in SystemCore.ActiveScene.LightsInScene)
            {
                if (light is DiffuseLight)
                    lightPos = ((DiffuseLight) light).LightDirection;

            }

          
            atmosphereObject.Update(lightPos,mouseCamera.Position);

            base.Render(gameTime);
        }
    }
}
