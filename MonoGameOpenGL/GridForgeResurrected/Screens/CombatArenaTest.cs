using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;

namespace GridForgeResurrected.Screens
{
    /// <summary>
    /// Test for major core systems - collision, combat etc.
    /// </summary>
    class CombatArenaTest : MouseCamScreen
    {
        public CombatArenaTest()
        {
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            SystemCore.ActiveScene.SetDiffuseLightDir(1, new Vector3(1, 1, 1));

            var arenaObject = CreateTestArena();

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(arenaObject);






        }

        private static GameObject CreateTestArena()
        {
            float arenaSize = 40f;
            ProceduralShapeBuilder floor = new ProceduralShapeBuilder();

            floor.AddSquareFace(new Vector3(arenaSize, 0, arenaSize), new Vector3(-arenaSize, 0, arenaSize),
                new Vector3(arenaSize, 0, -arenaSize), new Vector3(-arenaSize, 0, -arenaSize));

            ProceduralShape arenaFloor = floor.BakeShape();
            arenaFloor.SetColor(Color.DarkOrange);

            ProceduralCuboid a = new ProceduralCuboid(arenaSize, 1, arenaSize/5);
            a.Translate(new Vector3(0, arenaSize/5, arenaSize));
            a.SetColor(Color.LightGray);

            ProceduralShape b = a.Clone();
            b.Translate(new Vector3(0, 0, -arenaSize*2));

            ProceduralShape c = ProceduralShape.Combine(a, b);
            ProceduralShape d = c.Clone();
            d.Transform(Matrix.CreateRotationY(MathHelper.Pi/2));

            ProceduralShape e = ProceduralShape.Combine(arenaFloor, c, d);

            var arenaObject = GameObjectFactory.CreateRenderableGameObjectFromShape(e,
                EffectLoader.LoadEffect("flatshaded"));

            arenaObject.AddComponent(new RotatorComponent(Vector3.Up, 0.0001f));
            return arenaObject;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Gray);
            DebugShapeRenderer.VisualiseAxes(5f);
            base.Render(gameTime);
        }
    }
}
