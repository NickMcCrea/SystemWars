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
            startPos = new Vector3(0, planetSize*1.25f, 0);

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


            
           

            planets = new List<MiniPlanet>();


            //int planetCount = 10;

            //for (int i = 0; i < planetCount; i++)
            //{
            //    var noiseGenerator = NoiseGenerator.RidgedMultiFractal(RandomHelper.GetRandomFloat(1000,3000)/100000f);
            //    int startX = RandomHelper.GetRandomInt(-1000, 1000);
            //    int startY = RandomHelper.GetRandomInt(-100, 100);
            //    int startZ = RandomHelper.GetRandomInt(-1000, 1000);

            //    int planetRadius = RandomHelper.GetRandomInt(20, 60);

            //    MiniPlanet p = new MiniPlanet(new Vector3(startX, startY, startZ), planetRadius, planetRadius-2,
            //        planetRadius *1.05f, noiseGenerator, planetRadius*2+1, 1, RandomHelper.RandomColor);
            //    planets.Add(p);
            //}


            MiniPlanet earth = new MiniPlanet(new Vector3(200, 0, 0), 50,
                NoiseGenerator.RidgedMultiFractal(0.03f), 101, 1,
                Color.DarkOrange);

            earth.SetOrbit(Vector3.Zero,Vector3.Up,0.001f);
            earth.AddAtmosphere(0.97f, 1.05f);
            planets.Add(earth);



            MiniPlanet moon = new MiniPlanet(new Vector3(300, 0, 0), 15,
                NoiseGenerator.RidgedMultiFractal(0.02f), 31, 1,
                Color.DarkGray);

            moon.SetOrbit(earth, Vector3.Up, 0.01f);

          
            planets.Add(moon);





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
                 (cameraGameObject.Transform.WorldMatrix.Translation - miniPlanet.CurrentCenterPosition).Length();

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
