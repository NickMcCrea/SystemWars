using System.Collections.Generic;
using BEPUphysics.CollisionRuleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.Editor;
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
        private GameObject testShip;
        int lod = 1;


        public ProceduralTerrainTestScreen()
        {
            startPos = new Vector3(0, planetSize * 1.25f, 0);

            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();


            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(Color.Black, Color.Black, Color.Black));


            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(startPos);
            cameraGameObject.Transform.SetLookAndUp(new Vector3(1, 0, 0), new Vector3(0, 1, 0));
            cameraGameObject.AddComponent(new MouseController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());

            planets = new List<MiniPlanet>();

            var shape = SimpleModelEditor.LoadShape("voxrocket");

            if (shape != null)
            {
                testShip = GameObjectFactory.CreateRenderableGameObjectFromShape(shape,
                    EffectLoader.LoadSM5Effect("flatshaded"));

                testShip.Transform.Rotate(Vector3.Up, -MathHelper.PiOver2);
                testShip.Transform.WorldMatrix.Translation = new Vector3(100, 0, 0);
                //testShip.AddComponent(new RotatorComponent(Vector3.Right, 0.001f));

                SystemCore.GameObjectManager.AddAndInitialiseGameObject(testShip);
            }

            GenerateSystem();


        }

        private void GenerateSystem()
        {
            foreach (MiniPlanet miniPlanet in planets)
            {
                if(miniPlanet != null)
                    miniPlanet.DestroyGeometry();
            }

            planets.Clear();



            earth = new MiniPlanet(new Vector3(200, 0, 0), 50,
                NoiseGenerator.ParameterisedFastPlanet(50, NoiseGenerator.miniPlanetParameters, RandomHelper.GetRandomInt(1000)), 101, 1,
                Color.DarkOrange, Color.PaleGreen, true, 0.97f, 1.05f, 10, 4);

            earth.SetOrbit(Vector3.Zero, Vector3.Up, 0.0001f);         
            earth.SetRotation(Vector3.Up, 0.001f);
            planets.Add(earth);

            MiniPlanet moon = new MiniPlanet(new Vector3(600, 0, 0), 20,
                NoiseGenerator.RidgedMultiFractal(0.01f), 41, 1,
                Color.DarkGray, Color.DarkGray);
            moon.SetOrbit(earth, Vector3.Up, 0.001f);
            planets.Add(moon);
       
        }


        public override void Update(GameTime gameTime)
        {

            SystemCore.ActiveScene.SetDiffuseLightDir(0,Vector3.Normalize(Vector3.Zero - earth.CurrentCenterPosition));

            foreach (MiniPlanet miniPlanet in planets)
            {
                float distanceFromSurface =
                 (cameraGameObject.Transform.WorldMatrix.Translation - miniPlanet.CurrentCenterPosition).Length();

                miniPlanet.Update(gameTime, distanceFromSurface, cameraGameObject.Transform.WorldMatrix.Translation);
            }



            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Enter))
            {
               
                GenerateSystem();
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
                GenerateSystem();
            }
            //half
            if (SystemCore.Input.KeyPress(Keys.NumPad2))
            {
                currentParameter.Value /= 2f;
                GenerateSystem();
            }

            //
            if (SystemCore.Input.KeyPress(Keys.NumPad3))
            {
                if (lod > 1)
                    lod--;
                planets[0].SetLOD(lod);
            }
            if (SystemCore.Input.KeyPress(Keys.NumPad4))
            {
                lod++;
                planets[0].SetLOD(lod);
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
