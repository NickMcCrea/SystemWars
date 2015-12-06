using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using ConversionHelper;
using GridForgeResurrected.Game;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GameObject.Components.Controllers;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using Particle3DSample;
using System.Collections.Generic;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GridForgeResurrected.Screens
{
    /// <summary>
    /// Test for major core systems - collision, combat etc.
    /// </summary>
    class CombatArenaTest : Screen
    {
        private GameObject cameraGameObject;
        private GridWarrior player;
        private List<SimpleEnemy> enemies;
        ParticleSystem fireParticles;

        public CombatArenaTest()
        {
           
            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(1, 1, 1));

            float arenaSize = 40f;
            var arenaObject = CreateTestArena(arenaSize);

            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(new Vector3(0, 200, 0));
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, -1, 0), new Vector3(0, 0, 1));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());



            player = new GridWarrior(new Vector3(0, 5, 0));

            enemies = new List<SimpleEnemy>();
            for (int i = 0; i < 5; i++)
            {
                int spread = 50;
                var enemy = new SimpleEnemy(new Vector3(RandomHelper.GetRandomInt(-spread, spread), 5, RandomHelper.GetRandomInt(-spread, spread)));
                enemies.Add(enemy);
            }

            fireParticles = new FireParticleSystem(SystemCore.Game, SystemCore.ContentManager);
            fireParticles.DrawOrder = 500;
            SystemCore.Game.Components.Add(fireParticles);
        }


        private static GameObject CreateTestArena(float arenaSize)
        {

            ProceduralShapeBuilder floor = new ProceduralShapeBuilder();

            floor.AddSquareFace(new Vector3(arenaSize, 0, arenaSize), new Vector3(-arenaSize, 0, arenaSize),
                new Vector3(arenaSize, 0, -arenaSize), new Vector3(-arenaSize, 0, -arenaSize));

            ProceduralShape arenaFloor = floor.BakeShape();
            arenaFloor.SetColor(Color.DarkOrange);

            ProceduralCuboid a = new ProceduralCuboid(arenaSize, 1, arenaSize / 5);
            a.Translate(new Vector3(0, arenaSize / 5, arenaSize));
            a.SetColor(Color.LightGray);

            ProceduralShape b = a.Clone();
            b.Translate(new Vector3(0, 0, -arenaSize * 2));

            ProceduralShape c = ProceduralShape.Combine(a, b);
            ProceduralShape d = b.Clone();
            d.Transform(MonoMathHelper.RotateNinetyDegreesAroundUp(true));

            ProceduralShape e = ProceduralShape.Combine(arenaFloor, c, d);



            var side2 = e.Clone();
            var side3 = e.Clone();
            var side4 = e.Clone();

            e.Translate(new Vector3(-arenaSize * 2, 0, 0));

            side2.Transform(MonoMathHelper.RotateHundredEightyDegreesAroundUp(true));
            side2.Translate(new Vector3(arenaSize * 2, 0, 0));
            side3.Transform(MonoMathHelper.RotateNinetyDegreesAroundUp(true));
            side3.Translate(new Vector3(0, 0, arenaSize * 2));
            side4.Transform(MonoMathHelper.RotateNinetyDegreesAroundUp(false));
            side4.Translate(new Vector3(0, 0, -arenaSize * 2));



            var final = ProceduralShape.Combine(e, side2, side3, side4, arenaFloor);

            var arenaObject = GameObjectFactory.CreateRenderableGameObjectFromShape(final,
                EffectLoader.LoadEffect("flatshaded"));


            arenaObject.AddComponent(new StaticMeshColliderComponent(arenaObject, final.GetVertices(),
                final.GetIndicesAsInt().ToArray()));


           // arenaObject.AddComponent(new RotatorComponent(Vector3.Up, 0.0001f));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(arenaObject);


            LineBatch l = new LineBatch(new Vector3(-arenaSize, 0.1f, -arenaSize), new Vector3(-arenaSize, 0.1f, arenaSize), new Vector3(arenaSize, 0.1f, arenaSize), new Vector3(arenaSize, 0.1f, -arenaSize), new Vector3(-arenaSize, 0.1f, -arenaSize));
            GameObject lineObject = SystemCore.GameObjectManager.AddLineBatchToScene(l);

            arenaObject.AddChild(lineObject);



            return arenaObject;
        }

        public override void Update(GameTime gameTime)
        {
            const int fireParticlesPerFrame = 20;

            // Create a number of fire particles, randomly positioned around a circle.
            for (int i = 0; i < fireParticlesPerFrame; i++)
            {
                fireParticles.AddParticle(RandomHelper.RandomNormalVector3, Vector3.Zero);
            }


            cameraGameObject.Transform.Translate(new Vector3(0, -(float)input.ScrollDelta/10f, 0));

            player.Update(gameTime);

            foreach (SimpleEnemy enemy in enemies)
            {
                enemy.Update(gameTime);
            }

            base.Update(gameTime);

        }


        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Gray);
            DebugShapeRenderer.VisualiseAxes(5f);

            fireParticles.SetCamera(SystemCore.ActiveCamera.View, SystemCore.ActiveCamera.Projection);


            base.Render(gameTime);
        }
    }
}
