using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameDirectX11.Screens
{
    class ProceduralTestScreen : MouseCamScreen
    {


        public ProceduralTestScreen()
            : base()
        {
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            //AddLine(new Vector3(5, 5, 0), new Vector3(-5, -5, 0), 0.1f);

            //AddLine(Vector3.Zero, Vector3.Right * 5f, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Up * 5, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Left * 5f, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Down * 5, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Forward * 5f, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Backward * 5, 0.1f);

            //SystemCore.GameObjectManager.AddTestSphere(Vector3.Up, 0.5f);
            // SystemCore.GameObjectManager.AddTestUnitCube(new Vector3(10,0,0));


            //var capsuleCube = CompoundShapeBuilder.CapsuleCube();
            //SystemCore.GameObjectManager.AddShapeToScene(capsuleCube);

            var lines = new LineBatch(Vector3.Zero, Vector3.Left, Vector3.Up, Vector3.Right, Vector3.Down);
            SystemCore.GameObjectManager.AddLineBatchToScene(lines);

            mouseCamera.moveSpeed = 0.001f;

        }

        private static void AddLine(Vector3 start, Vector3 end, float thickness)
        {
            ProceduralShape line = CompoundShapeBuilder.Capsule(start, end, thickness);
            SystemCore.GameObjectManager.AddShapeToScene(line);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.DarkGray);

            DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(Vector3.Zero, 1f), Color.Blue);

            base.Render(gameTime);
        }
    }
}
