using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using Vector.Screens.Entities;

namespace Vector.Screens
{
    class GameScreen : Screen
    {
        private DummyCamera dummyCamera;
        private Player player;
        public GameScreen()
            : base()
        {
            
            dummyCamera = new DummyCamera();
            dummyCamera.SetPositionAndLookDir(new Vector3(5, 20, -5), Vector3.Zero, Vector3.UnitZ);
            SystemCore.SetActiveCamera(dummyCamera);
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();


            player = new Player(Vector3.Zero);
           
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(player);


            //things to think about.
            //1. Game entity system. Handles creation and management of in game entities.
            //2. Collisions, both projectile, and entity / environment.
            //3. Particles.
            //4. 
        }


        public override void Update(GameTime gameTime)
        {
            player.Update(PlayerIndex.One);

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Black);
            DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(Vector3.Zero, 1f), Color.Blue);
            base.Render(gameTime);
        }

        public override void RenderSprites(GameTime gameTime)
        {

        }
    }
}
