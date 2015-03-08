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
    public class RenderTextureTest : MouseCamScreen
    {

        RenderTarget2D renderTarget;
        GameObject testObject;
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

            spriteBatch = new SpriteBatch(SystemCore.GraphicsDevice);
            renderTarget = new RenderTarget2D(SystemCore.GraphicsDevice, 100, 100);
        }

        public override void Update(GameTime gameTime)
        {

            testObject.GetComponent<RotatorComponent>().Update(gameTime);

            base.Update(gameTime);
        }

     
        public override void Render(GameTime gameTime)
        {

            SystemCore.GraphicsDevice.Clear(Color.LightBlue);

            SystemCore.GraphicsDevice.SetRenderTarget(renderTarget);



            //draw some stuff.
            testObject.GetComponent<EffectRenderComponent>().Draw(gameTime);



            SystemCore.GraphicsDevice.SetRenderTarget(null);


            testObject.GetComponent<EffectRenderComponent>().Draw(gameTime);


            spriteBatch.Begin();
            spriteBatch.Draw(renderTarget, new Rectangle(0, 0, 100, 100), Color.White);
            spriteBatch.End();

          

        }
    }
}
