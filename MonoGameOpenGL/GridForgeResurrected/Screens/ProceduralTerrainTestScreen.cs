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
using Microsoft.Xna.Framework.Input;

namespace GridForgeResurrected.Screens
{
    class ProceduralTerrainTestScreen : Screen
    {
        private GameObject cameraGameObject;
        private Vector3 startPos;
        private float planetSize = 50f;
        private List<MiniPlanet> planets;
        MiniPlanet earth;
        int currentParameterIndex = 0;


        public ProceduralTerrainTestScreen()
        {
            startPos = new Vector3(0, planetSize * 1.25f, 0);

            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();


            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(RandomHelper.RandomColor, RandomHelper.RandomColor, RandomHelper.RandomColor));


            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(startPos);
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, -1, 0), new Vector3(0, 0, 1));
            cameraGameObject.AddComponent(new MouseController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());









            planets = new List<MiniPlanet>();



            GenerateEarth();

            //earth.SetOrbit(Vector3.Zero,Vector3.Up,0.001f);
            earth.AddAtmosphere(0.97f, 1.05f);
            planets.Add(earth);



            MiniPlanet moon = new MiniPlanet(new Vector3(600, 0, 0), 15,
                NoiseGenerator.RidgedMultiFractal(0.02f), 31, 1,
                RandomHelper.RandomColor);

            //moon.SetOrbit(earth, Vector3.Up, 0.01f);


            planets.Add(moon);





        }

        private void GenerateEarth()
        {
            if (earth != null)
                earth.DestroyGeometry();
            earth = new MiniPlanet(new Vector3(0, 0, 0), 50,
                NoiseGenerator.ParameterisedFastPlanet(50, NoiseGenerator.miniPlanetParameters), 101, 1,
                RandomHelper.RandomColor);
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



            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
               

                GenerateEarth();
            }

            int currentIndex = 0;
            PlanetParameters currentParameter = null;
            foreach (KeyValuePair<string, PlanetParameters> pair in NoiseGenerator.miniPlanetParameters)
            {
                if (currentIndex == currentParameterIndex)
                {
                    currentParameter = pair.Value;
                    break;
                }
                currentIndex++;
            }

            DebugText.Write(currentParameter.Name + " : " + currentParameter.Value.ToString());

            if (SystemCore.Input.KeyPress(Keys.OemPlus))
            {
                currentParameterIndex++;
                if (currentParameterIndex >= NoiseGenerator.miniPlanetParameters.Count)
                    currentParameterIndex = 0;
            }
            if (SystemCore.Input.KeyPress(Keys.OemMinus))
            {
                currentParameterIndex--;
                if (currentParameterIndex < 0)
                    currentParameterIndex = NoiseGenerator.miniPlanetParameters.Count - 1;
            }

            //double
            if (SystemCore.Input.KeyPress(Keys.NumPad1))
            {
                currentParameter.Value *= 2;
                GenerateEarth();
            }
            //half
            if (SystemCore.Input.KeyPress(Keys.NumPad2))
            {
                currentParameter.Value /= 2f;
                GenerateEarth();
            }

            //
            if (SystemCore.Input.KeyPress(Keys.NumPad3))
            {
                GenerateEarth();
            }
            if (SystemCore.Input.KeyPress(Keys.NumPad4))
            {
                GenerateEarth();
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
