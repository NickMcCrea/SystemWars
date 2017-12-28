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


    class PlanetTest : Screen
    {
        DuneBuggy duneBuggyOne;
        MouseFreeCamera mouseCamera;
        Planet earth;
        GravitationalField field;

        private string currentVehicle = "buggy";

        public PlanetTest() : base()
        {
            
        }

        public override void OnInitialise()
        {
            fpsLabel.Visible = true;
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(0.01f, 1, 0.01f));
            SystemCore.ActiveScene.FogEnabled = false;

            //mouse camera
            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0), 1f, 50000f);
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

            float radius = 3000;
            earth = new Planet("earth", new Vector3(0, 0, 0),
                NoiseGenerator.FastPlanet(radius),
               EffectLoader.LoadSM5Effect("AtmosphericScatteringGround").Clone(),
                radius, Color.DarkSeaGreen.ChangeTone(-100), Color.SaddleBrown, Color.SaddleBrown.ChangeTone(-10), 0);
            earth.AddAtmosphere();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(earth);


            mouseCamera.SetPositionAndLook(new Vector3(0, radius + 200, 0), (float)Math.PI, (float)-Math.PI / 5);


            duneBuggyOne = new DuneBuggy(PlayerIndex.One, Color.Red, new Vector3(0, radius + 100, 0));

            field = new GravitationalField(new InfiniteForceFieldShape(), Vector3.Zero.ToBepuVector(), 40000 * radius, 100);
            SystemCore.PhysicsSimulation.Add(field);

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

            PlanetBuilder.Update();
            earth.Update(gameTime);

            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());




            base.Update(gameTime);
        }

        private void SwitchVehicle()
        {
            
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





        }

        public override void Render(GameTime gameTime)
        {
           

            base.Render(gameTime);

        }


    }
}
