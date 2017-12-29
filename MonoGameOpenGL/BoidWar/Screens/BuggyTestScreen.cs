using MonoGameEngineCore;
using System;
using Microsoft.Xna.Framework;
using SystemWar;
using MonoGameEngineCore.Rendering.Camera;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GameObject.Components;
using BoidWar.Gameplay;
using MonoGameEngineCore.Camera;
using GridForgeResurrected.Screens;
using BEPUphysics.UpdateableSystems.ForceFields;
using System.Collections.Generic;

namespace BoidWar.Screens
{


    class BuggyTestScreen : Screen
    {
        DuneBuggy duneBuggyOne;
        MouseFreeCamera mouseCamera;
       
        private string currentVehicle = "spaceship";


        public BuggyTestScreen() : base()
        {

        }

        public override void OnInitialise()
        {
            fpsLabel.Visible = true;
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(0.01f, 1, 0.01f));
            SystemCore.ActiveScene.FogEnabled = false;

            //mouse camera
            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0), 1f, 500f);
            mouseCamera.moveSpeed = 1f;
            mouseCamera.SetPositionAndLook(new Vector3(0, 200, -200), (float)Math.PI, (float)-Math.PI / 5);
            SystemCore.SetActiveCamera(mouseCamera);





            AddInputBindings();



            SetUpGameWorld();



            base.OnInitialise();
        }

        private void AddInputBindings()
        {

            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("CameraForward", Keys.Up);
            input.AddKeyDownBinding("CameraBackward", Keys.Down);
            input.AddKeyDownBinding("CameraLeft", Keys.Left);
            input.AddKeyDownBinding("CameraRight", Keys.Right);


            input.AddKeyPressBinding("MainMenu", Keys.Escape);


            var binding = input.AddKeyPressBinding("WireframeToggle", Keys.Space);
            binding.InputEventActivated += (x, y) => { SystemCore.Wireframe = !SystemCore.Wireframe; };




        }

        private void SetUpGameWorld()
        {
            


            SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);
            Heightmap heightMap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.03f), 100, 5, 20, 1, 1, 1);
            var terrainObject = heightMap.CreateTranslatedRenderableHeightMap(Color.OrangeRed, EffectLoader.LoadSM5Effect("flatshaded"), new Vector3(-250, 0, -250));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrainObject);

            mouseCamera.SetPositionAndLook(new Vector3(0,50,0), (float)Math.PI, (float)-Math.PI / 5);


            duneBuggyOne = new DuneBuggy(PlayerIndex.One, Color.Red, new Vector3(0, 20, 0));

            duneBuggyOne.Activate();
           

        }

     
        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            SystemCore.GameObjectManager.ClearAllObjects();
            SystemCore.ActiveScene.ClearLights();
            input.ClearBindings();
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {

            EvaluateMouseCamControls(gameTime);

            
            duneBuggyOne.Update(gameTime);


            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());




            base.Update(gameTime);
        }

        private void EvaluateMouseCamControls(GameTime gameTime)
        {

            if (SystemCore.ActiveCamera is MouseFreeCamera)
            {
                mouseCamera.Slow = input.EvaluateInputBinding("SlowCamera");
                if (input.EvaluateInputBinding("CameraForward"))
                    mouseCamera.MoveForward();
                if (input.EvaluateInputBinding("CameraBackward"))
                    mouseCamera.MoveBackward();
                if (input.EvaluateInputBinding("CameraLeft"))
                    mouseCamera.MoveLeft();
                if (input.EvaluateInputBinding("CameraRight"))
                    mouseCamera.MoveRight();





                if (input.IsKeyDown(Keys.RightShift))
                    mouseCamera.moveSpeed = 10f;
                else if (input.IsKeyDown(Keys.RightControl))
                    mouseCamera.moveSpeed = 0.1f;
                else
                    mouseCamera.moveSpeed = 1f;

                mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);

            }


            if (input.KeyPress(Keys.F))
            {
                duneBuggyOne.Flip();
            }


        }

        public override void Render(GameTime gameTime)
        {


            base.Render(gameTime);

        }


    }
}