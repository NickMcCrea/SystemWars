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

namespace Vector.Screens
{
    class GameScreen : Screen
    {
        private DummyCamera dummyCamera;

        public GameScreen()
            : base()
        {
            dummyCamera = new DummyCamera();
            dummyCamera.SetPositionAndLookDir(new Vector3(0, 10, 0), Vector3.Zero, Vector3.UnitZ);
            SystemCore.SetActiveCamera(dummyCamera);
        }


        public override void Update(GameTime gameTime)
        {



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
