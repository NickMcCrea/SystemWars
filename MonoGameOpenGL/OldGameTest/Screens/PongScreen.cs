﻿using MonoGameEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace OldGameTest.Screens
{
    class PongScreen : Screen
    {

        DummyOrthographicCamera camera;
        GameObject ball;


        public PongScreen() : base()
        {

           

        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = false;

            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(0, -1, 0));

            camera = new DummyOrthographicCamera(50, 50, 0.1f, 100f);
            camera.World = Matrix.CreateWorld(new Vector3(0, 20, 0), new Vector3(0,-1,0), new Vector3(0,0,1));
            camera.View = Matrix.Invert(camera.World);
            //   camera.SetPositionAndLookDir(new Vector3(0, 10, 0), Vector3.Zero);


            var cube = new ProceduralCube();
            cube.SetColor(Color.White);
            ball = GameObjectFactory.CreateRenderableGameObjectFromShape(cube, EffectLoader.LoadSM5Effect("flatshaded"));
            ball.Transform.SetPosition(Vector3.Zero);

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(ball);

            base.OnInitialise();
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }


        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
           // SystemCore.GraphicsDevice.Clear(Color.Black);
            base.Render(gameTime);
        }

        public override void RenderSprites(GameTime gameTime)
        {
            base.RenderSprites(gameTime);
        }
    }
}