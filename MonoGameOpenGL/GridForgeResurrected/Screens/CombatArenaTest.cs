using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using BEPUutilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GameObject.Components.Controllers;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using MonoGameEngineCore.ScreenManagement;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Vector3 = Microsoft.Xna.Framework.Vector3;

namespace GridForgeResurrected.Screens
{
    /// <summary>
    /// Test for major core systems - collision, combat etc.
    /// </summary>
    class CombatArenaTest : Screen
    {
        private GameObject cameraGameObject;
        private GameObject testPlayer;

        public CombatArenaTest()
        {

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

            ProceduralCube playerShape = new ProceduralCube();
            playerShape.Scale(5f);
            testPlayer = GameObjectFactory.CreateCollidableObject(playerShape,
                EffectLoader.LoadEffect("flatshaded"), PhysicsMeshType.box);

            testPlayer.AddComponent(new TopDownMouseAndKeyboardController());

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(testPlayer);

            testPlayer.Transform.SetPosition(new Vector3(0, 0, 0));

         
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


            //arenaObject.AddComponent(new RotatorComponent(Vector3.Up, 0.0001f));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(arenaObject);


            LineBatch l = new LineBatch(new Vector3(-arenaSize, 0.1f, -arenaSize), new Vector3(-arenaSize, 0.1f, arenaSize), new Vector3(arenaSize, 0.1f, arenaSize), new Vector3(arenaSize, 0.1f, -arenaSize), new Vector3(-arenaSize, 0.1f, -arenaSize));
            GameObject lineObject = SystemCore.GameObjectManager.AddLineBatchToScene(l);

            arenaObject.AddChild(lineObject);



            return arenaObject;
        }

        public override void Update(GameTime gameTime)
        {


            PhysicsComponent playerPhysics = testPlayer.GetComponent<PhysicsComponent>();
            if (playerPhysics.InCollision())
            {
                testPlayer.Transform.Velocity = Vector3.Zero;
                var pairs = playerPhysics.PhysicsEntity.CollisionInformation.Pairs;

                foreach (CollidablePairHandler pair in pairs)
                {
                    if (pair.EntityA != playerPhysics.PhysicsEntity)
                        continue;

                    var contacts = pair.Contacts;
                    foreach (ContactInformation contact in contacts)
                    {
                        var remove = (-pair.Contacts[0].Contact.Normal * pair.Contacts[0].Contact.PenetrationDepth).ToXNAVector();
                        testPlayer.Transform.Translate(remove);             
                        break;
                    }
                   
                }



            }

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
