using System.Collections.Generic;
using BEPUphysics.CollisionRuleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using SystemWar;

namespace GridForgeResurrected.Screens
{
    class ProceduralTerrainTestScreen : Screen
    {
        private GameObject cameraGameObject;
        private Vector3 startPos;
        private float planetSize = 50f;
        private List<MiniPlanet> planets;
        private MiniPlanet planet1;
        private MiniPlanet planet2;


        public ProceduralTerrainTestScreen()
        {
            startPos = new Vector3(0, planetSize * 1.25f, 0);

            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();


            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(Color.Black, Color.Black, Color.DarkGray));


            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(startPos);
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, -1, 0), new Vector3(0, 0, 1));
            cameraGameObject.AddComponent(new MouseController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());



            var noiseGenerator = NoiseGenerator.RidgedMultiFractal(0.05f);

            planets = new List<MiniPlanet>();

            planet1 = new MiniPlanet(new Vector3(100,0,0), 49, 48, 49 * 1.05f, noiseGenerator, 100, 1, Color.OrangeRed);
            planets.Add(planet1);

            planet2 = new MiniPlanet(new Vector3(1000, 0, 0), 49, 48, 49 * 1.05f, noiseGenerator, 100, 1, Color.PaleGreen);
            planets.Add(planet2);


        }

        private GameObject AddTerrainSegment(float size, float sampleX, float sampleY)
        {



            int vertCount = 100;
            Heightmap heightmap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.1f), vertCount,
                size / vertCount, 10f, sampleX, sampleY,
                1);

            GameObject terrain = new GameObject("terrain");




            var verts =
                BufferBuilder.VertexBufferBuild(heightmap.GenerateVertexArray());


            var indices = BufferBuilder.IndexBufferBuild(heightmap.GenerateIndices());
            terrain.AddComponent(new RenderGeometryComponent(verts, indices, indices.IndexCount / 3));
            terrain.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));



            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrain);

            return terrain;
        }


        public override void Update(GameTime gameTime)
        {

            foreach (MiniPlanet miniPlanet in planets)
            {
                float distanceFromSurface =
                 (cameraGameObject.Transform.WorldMatrix.Translation - miniPlanet.CurrentPosition).Length();

                miniPlanet.Update(gameTime, distanceFromSurface, cameraGameObject.Transform.WorldMatrix.Translation);
            }





            base.Update(gameTime);

        }


        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Gray);

            DebugText.Write(cameraGameObject.Transform.WorldMatrix.Translation.ToString());

            DebugShapeRenderer.VisualiseAxes(5f);


            base.Render(gameTime);



        }
    }
}
