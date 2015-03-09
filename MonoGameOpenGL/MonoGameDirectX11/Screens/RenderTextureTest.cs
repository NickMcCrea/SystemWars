using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;
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
    public class RenderTextureTest : MouseCamScreen
    {

        RenderTarget2D renderTarget;
        GameObject testObject;
        private DummyCamera renderTextureCamera;
        private GameObject planeToDrawOn;
        SpriteBatch spriteBatch;

        public RenderTextureTest()
            : base()
        {
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            mouseCamera.moveSpeed = 0.01f;

            ProceduralCube cube = new ProceduralCube();
            cube.SetColor(Color.OrangeRed);
            testObject = GameObjectFactory.CreateRenderableGameObjectFromShape(cube, EffectLoader.LoadEffect("flatshaded"));
            testObject.AddComponent(new RotatorComponent(Vector3.Up, 0.0001f));
            GameObject.InitialiseAllComponents(testObject);

            ProceduralPlane plane = new ProceduralPlane();
            plane.Scale(10f);
            plane.SetColor(Color.LightBlue);         
            planeToDrawOn = GameObjectFactory.CreateRenderTextureSurface(plane,
                EffectLoader.LoadEffect("rendertexturesurface"));

            planeToDrawOn.Transform.Rotate(Vector3.Left, MathHelper.ToRadians(90));
            planeToDrawOn.Transform.Rotate(Vector3.Up, MathHelper.ToRadians(-90));
            planeToDrawOn.Transform.SetPosition(new Vector3(-5,0,0));

            GameObject.InitialiseAllComponents(planeToDrawOn);
            
            spriteBatch = new SpriteBatch(SystemCore.GraphicsDevice);
            renderTarget = new RenderTarget2D(SystemCore.GraphicsDevice, 500, 500);

            renderTextureCamera = new DummyCamera();
            renderTextureCamera.SetPositionAndLookDir(new Vector3(-5, 0, 0), Vector3.Zero, Vector3.Up);
            SystemCore.AddCamera("renderTextureCamera", renderTextureCamera);
        }

        public override void Update(GameTime gameTime)
        {

            testObject.GetComponent<RotatorComponent>().Update(gameTime);

            base.Update(gameTime);
        }


        public override void Render(GameTime gameTime)
        {

           
            SystemCore.GraphicsDevice.SetRenderTarget(renderTarget);

            //draw some stuff.
            testObject.GetComponent<EffectRenderComponent>().Camera = "renderTextureCamera";
            testObject.GetComponent<EffectRenderComponent>().Draw(gameTime);


            SystemCore.GraphicsDevice.SetRenderTarget(null);


            planeToDrawOn.GetComponent<RenderTextureComponent>().Texture2D = renderTarget;
            planeToDrawOn.GetComponent<RenderTextureComponent>().BorderColor = Color.OrangeRed;
            planeToDrawOn.GetComponent<RenderTextureComponent>().Draw(gameTime);


            testObject.GetComponent<EffectRenderComponent>().Camera = "main";
            testObject.GetComponent<EffectRenderComponent>().Draw(gameTime);


            spriteBatch.Begin();
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, 100, 100), Color.White);
            spriteBatch.End();



        }
    }
}
