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
            SystemCore.ActiveScene.SetDiffuseLightDir(1,new Vector3(1, 1, 1));

            //AddLine(new Vector3(5, 5, 0), new Vector3(-5, -5, 0), 0.1f);

            //AddLine(Vector3.Zero, Vector3.Right * 5f, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Up * 5, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Left * 5f, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Down * 5, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Forward * 5f, 0.1f);
            //AddLine(Vector3.Zero, Vector3.Backward * 5, 0.1f);

            //SystemCore.GameObjectManager.AddTestSphere(Vector3.Up, 0.5f);
            //SystemCore.GameObjectManager.AddTestUnitCube(new Vector3(0,0,0));


            ProceduralPlane p = new ProceduralPlane();
            GameObject plane = GameObjectFactory.CreateRenderableGameObjectFromShape(p, EffectLoader.LoadEffect("flatshaded"));
            

            LineBatch l = new LineBatch(new Vector3(0.5f, 0, 0.5f), new Vector3(0.5f, 0, -0.5f), new Vector3(-0.5f, 0, -0.5f), new Vector3(-0.5f, 0, 0.5f), new Vector3(0.5f, 0, 0.5f));
            GameObject lineObject = SystemCore.GameObjectManager.AddLineBatchToScene(l);

            plane.AddChild(lineObject);
            plane.AddComponent(new RotatorComponent(Vector3.Up, 0.001f));


            SystemCore.GameObjectManager.AddAndInitialiseGameObject(plane);




            //var capsuleCube = CompoundShapeBuilder.CapsuleCube();
            //SystemCore.GameObjectManager.AddShapeToScene(capsuleCube);

            

            mouseCamera.moveSpeed = 0.01f;

        }

        private static void AddLine(Vector3 start, Vector3 end, float thickness)
        {
            ProceduralShape line = CompoundShapeHelper.Capsule(start, end, thickness);
            SystemCore.GameObjectManager.AddShapeToScene(line);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.DarkGray);

            //DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(Vector3.Zero, 1f), Color.Blue);

            base.Render(gameTime);
        }
    }
}
