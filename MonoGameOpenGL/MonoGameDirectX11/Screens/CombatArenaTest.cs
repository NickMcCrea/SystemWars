using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using ConversionHelper;
using GridForgeResurrected.Game;
using Microsoft.Xna.Framework;
using MonoGameDirectX11;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GameObject.Components.Controllers;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
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
    class CombatArena : Screen
    {
        private GameObject cameraGameObject;
        private GridWarrior player;
        private List<SimpleEnemy> enemies;
        private Label healthLabel;
        private Label killLabel;

        public CombatArena()
        {

          
         

        }

        public override void OnInitialise()
        {
            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
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
                SpawnEnemy();
            }


            healthLabel = new Label(GUIFonts.Fonts["neuropolitical"], "Health:");

            healthLabel.SetPosition(new Vector2(GUIManager.GetFractionOfWidth(0.1f),
                GUIManager.GetFractionOfHeight(0.02f)));

            SystemCore.GUIManager.AddControl(healthLabel);

            killLabel = new Label(GUIFonts.Fonts["neuropolitical"], "Kills:");
            killLabel.SetPosition(new Vector2(GUIManager.GetFractionOfWidth(0.9f),
              GUIManager.GetFractionOfHeight(0.02f)));

            SystemCore.GUIManager.AddControl(killLabel);
            base.OnInitialise();
        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            SystemCore.GameObjectManager.ClearAllObjects();
            input.ClearBindings();
            CollisionRules.DefaultCollisionRule = CollisionRule.Normal;

            base.OnRemove();
        }

        private void SpawnEnemy()
        {
            int spread = 50;
            var enemy =
                new SimpleEnemy(new Vector3(RandomHelper.GetRandomInt(-spread, spread), 5,
                    RandomHelper.GetRandomInt(-spread, spread)));
            enemies.Add(enemy);
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
                EffectLoader.LoadSM5Effect("flatshaded"));


            arenaObject.AddComponent(new StaticMeshColliderComponent(arenaObject, final.GetVertices(),
                final.GetIndicesAsInt().ToArray(), Vector3.Zero));


           // arenaObject.AddComponent(new RotatorComponent(Vector3.Up, 0.0001f));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(arenaObject);


            LineBatch l = new LineBatch(new Vector3(-arenaSize, 0.1f, -arenaSize), new Vector3(-arenaSize, 0.1f, arenaSize), new Vector3(arenaSize, 0.1f, arenaSize), new Vector3(arenaSize, 0.1f, -arenaSize), new Vector3(-arenaSize, 0.1f, -arenaSize));
            GameObject lineObject = SystemCore.GameObjectManager.AddLineBatchToScene(l);
            arenaObject.AddChild(lineObject);



            return arenaObject;
        }

        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            cameraGameObject.Transform.Translate(new Vector3(0, -(float) input.ScrollDelta/10f, 0));

            player.Update(gameTime);

            foreach (SimpleEnemy enemy in enemies)
            {
                enemy.Update(gameTime);
            }

            foreach (CollidablePairHandler handler in SystemCore.PhysicsSimulation.NarrowPhase.Pairs)
            {
                //two things are colliding.
                if (handler.Contacts.Count > 0 && handler is BoxSpherePairHandler)
                {
                    GameObject a = handler.EntityA.Tag as GameObject;
                    GameObject b = handler.EntityB.Tag as GameObject;

                    if ((a is GridWarrior && b is SimpleEnemy) || (b is GridWarrior && a is SimpleEnemy))
                        player.Damage(1);

                    if (b.Name == "simpleenemy")
                        RemoveEnemy(b as SimpleEnemy);
                    if (a.Name == "simpleenemy")
                        RemoveEnemy(a as SimpleEnemy);

                    SpawnEnemy();

                }
            }

            healthLabel.Text = "Health: " + player.Health;
            killLabel.Text = "Kills: " + player.Score;
            base.Update(gameTime);

        }

        private void RemoveEnemy(SimpleEnemy enemy)
        {
            SystemCore.GameObjectManager.RemoveObject(enemy);
            enemies.Remove(enemy);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Gray);
            DebugShapeRenderer.VisualiseAxes(5f);

       
            base.Render(gameTime);

          

        }
    }
}
