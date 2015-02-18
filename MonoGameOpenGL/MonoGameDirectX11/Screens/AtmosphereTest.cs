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



        float m_Kr = 0.0025f;		// Rayleigh scattering constant
        private float m_Kr4PI;
        float m_Km = 0.0010f;	// Mie scattering constant
        private float m_Km4PI;
        float m_ESun = 20.0f;		// Sun brightness constant
        float m_g = -0.990f;		// The Mie phase asymmetry factor
        private float m_fExposure = 2.0f;

        private float m_fInnerRadius = 6000;
        private float m_fOuterRadius = 6300;
        private float m_fScale;

        private Vector3 m_fWavelength;
        private Vector3 m_fWavelength4;


        private float m_fRayleighScaleDepth = 0.25f;
        private float m_fMieScaleDepth = 0.1f;


        private Texture2D atmosphereTexture;
        private EffectRenderComponent atmosphereRenderComponent;
        private GameObject planetObject;
        private GameObject atmosphereObject;

        public AtmosphereTest()
            : base()
        {

            m_Kr4PI = m_Kr * 4.0f * MathHelper.Pi;
            m_Km4PI = m_Km * 4.0f * MathHelper.Pi;
            m_fScale = 1 / (m_fOuterRadius - m_fInnerRadius);

            m_fWavelength.X = 0.650f;		// 650 nm for red
            m_fWavelength.Y = 0.570f;		// 570 nm for green
            m_fWavelength.Z = 0.475f;		// 475 nm for blue
            m_fWavelength4.X = (float)Math.Pow(m_fWavelength.X, 4.0f);
            m_fWavelength4.Y = (float)Math.Pow(m_fWavelength.Y, 4.0f);
            m_fWavelength4.Z = (float)Math.Pow(m_fWavelength.Z, 4.0f);

         
            ProceduralSphere planet = new ProceduralSphere(50, 50);
            ProceduralSphere atmosphere = new ProceduralSphere(100, 100);

            mouseCamera.moveSpeed = 10f;
            atmosphere.InsideOut();

            atmosphereTexture = SystemCore.ContentManager.Load<Texture2D>("Textures/AtmosphereGradient3");

            planet.Scale(m_fInnerRadius);
            atmosphere.Scale(m_fOuterRadius);

            planetObject = GameObjectFactory.CreateRenderableGameObjectFromShape(planet,
                EffectLoader.LoadEffect("flatshaded"));


            atmosphereObject = GameObjectFactory.CreateRenderableGameObjectFromShape(atmosphere,
                EffectLoader.LoadEffect("SkyFromSpace"));


            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphereObject);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(planetObject);

            atmosphereRenderComponent = atmosphereObject.GetComponent<EffectRenderComponent>();


            atmosphereRenderComponent.effect.Parameters["v3InvWavelength"].SetValue(new Vector3(1 / m_fWavelength4.X,
                1 / m_fWavelength4.Y, 1 / m_fWavelength4.Z));

            atmosphereRenderComponent.effect.Parameters["fInnerRadius"].SetValue(m_fInnerRadius);
            atmosphereRenderComponent.effect.Parameters["fInnerRadius2"].SetValue(m_fInnerRadius * m_fInnerRadius);
            atmosphereRenderComponent.effect.Parameters["fOuterRadius"].SetValue(m_fOuterRadius);
            atmosphereRenderComponent.effect.Parameters["fOuterRadius2"].SetValue(m_fOuterRadius * m_fOuterRadius);

            atmosphereRenderComponent.effect.Parameters["fKrESun"].SetValue(m_Kr * m_ESun);
            atmosphereRenderComponent.effect.Parameters["fKmESun"].SetValue(m_Km * m_ESun);
            atmosphereRenderComponent.effect.Parameters["fKr4PI"].SetValue(m_Kr4PI);
            atmosphereRenderComponent.effect.Parameters["fKm4PI"].SetValue(m_Km4PI);

            atmosphereRenderComponent.effect.Parameters["g"].SetValue(m_g);
            atmosphereRenderComponent.effect.Parameters["g2"].SetValue(m_g * m_g);

            atmosphereRenderComponent.effect.Parameters["fScale"].SetValue(1.0f / (m_fOuterRadius - m_fInnerRadius));
            atmosphereRenderComponent.effect.Parameters["fScaleDepth"].SetValue(m_fRayleighScaleDepth);
            atmosphereRenderComponent.effect.Parameters["fScaleOverScaleDepth"].SetValue(1.0f/
                                                                                         (m_fOuterRadius -
                                                                                          m_fInnerRadius)/
                                                                                         m_fRayleighScaleDepth);

           


            bool offCenter = false;

            if (offCenter)
            {
                planetObject.Transform.Translate(new Vector3(10000, 0, 0));
                atmosphereObject.Transform.Translate(new Vector3(10000, 0, 0));
                mouseCamera.World.Translation = new Vector3(25000, 0, 0);
            }

            else
            {
                mouseCamera.World.Translation = new Vector3(0, 0, 0);
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

            float camHeight = (mouseCamera.Position - atmosphereObject.Transform.WorldMatrix.Translation).Length();
            atmosphereRenderComponent.effect.Parameters["fCameraHeight"].SetValue(camHeight);
            atmosphereRenderComponent.effect.Parameters["fCameraHeight2"].SetValue(camHeight*camHeight);
            atmosphereRenderComponent.effect.Parameters["v3LightPos"].SetValue(lightPos);
            atmosphereRenderComponent.effect.Parameters["v3CameraPos"].SetValue(mouseCamera.Position);


            DebugText.Write(SystemCore.ActiveCamera.Position.ToString());

            base.Render(gameTime);
        }
    }
}
