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
using CarrierStrike.Gameplay;
using MonoGameEngineCore.GameObject.Components;
using BEPUphysics.UpdateableSystems;
using System.Collections.Generic;

namespace CarrierStrike.Screens
{
    class TestIslandScreen : Screen
    {

        MouseFreeCamera mouseCamera;
        GameObject cameraObject;
        Chopper chopper;
        Carrier carrier;
        Vector3 cameraOffset;
        bool releaseMouse;

        public TestIslandScreen() : base()
        {

        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = false;

            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(0.01f, 1, 0.01f));
            SystemCore.ActiveScene.FogEnabled = true;

            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);
            mouseCamera.moveSpeed = 0.1f;
            mouseCamera.SetPositionAndLook(new Vector3(50, 30, -20), (float)Math.PI, (float)-Math.PI / 5);

            cameraObject = new GameObject();
            cameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.25f, 100.0f, false));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            cameraOffset = new Vector3(-10, 10, -10);


            AddInputBindings();



            SetUpGameWorld();

            chopper = new Chopper();
            chopper.Transform.SetPosition(new Vector3(10, 0.5f, 10));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(chopper);
            chopper.Transform.Rotate(Vector3.Up, MathHelper.Pi);

            carrier = new Carrier();
            carrier.Transform.SetPosition(new Vector3(10, 0, 10));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(carrier);


            carrier.GetComponent<PhysicsComponent>().PhysicsEntity.IsAffectedByGravity = false;
            carrier.GetComponent<PhysicsComponent>().PhysicsEntity.Mass = 100;

            SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -1f, 0);


            OrientCamera(chopper);

            base.OnInitialise();
        }

        private void OrientCamera(Chopper chopper)
        {
            cameraObject.Transform.SetPosition(chopper.Transform.AbsoluteTransform.Translation + cameraOffset);
            Vector3 forward = chopper.Transform.AbsoluteTransform.Translation - cameraObject.Transform.AbsoluteTransform.Translation;
            forward.Normalize();
            Vector3 right = Vector3.Cross(forward, Vector3.Up);
            Vector3 newUp = Vector3.Cross(right, forward);
            cameraObject.Transform.SetLookAndUp(forward, newUp);
        }

        private void AddInputBindings()
        {
            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("CameraForward", Keys.Up);
            input.AddKeyDownBinding("CameraBackward", Keys.Down);
            input.AddKeyDownBinding("CameraLeft", Keys.Left);
            input.AddKeyDownBinding("CameraRight", Keys.Right);
            input.AddKeyPressBinding("CameraRotateLeft", Keys.N);
            input.AddKeyPressBinding("CameraRotateRight", Keys.M);

            input.AddKeyPressBinding("MainMenu", Keys.Escape);

            input.AddKeyPressBinding("SwitchCamera", Keys.Enter);

            var releaseMouseBinding = input.AddKeyPressBinding("MouseRelease", Keys.M);
            releaseMouseBinding.InputEventActivated += (x, y) =>
            {
                releaseMouse = !releaseMouse;
                SystemCore.CursorVisible = releaseMouse;
            };

            var binding = input.AddKeyPressBinding("WireframeToggle", Keys.Space);
            binding.InputEventActivated += (x, y) => { SystemCore.Wireframe = !SystemCore.Wireframe; };



        }

        private void SetUpGameWorld()
        {
            //Sky dome first (depth buffer will be disabled on draw for this)
            var skyDome = new GradientSkyDome(Color.MediumBlue, Color.LightCyan);

            int patchSize = 100;


            int widthInTerrainPatches = 1;
            int heightInTerrainPatches = 1;
        
            CreateIsland(patchSize, widthInTerrainPatches, heightInTerrainPatches, Vector3.Zero);
            CreateIsland(patchSize, widthInTerrainPatches, heightInTerrainPatches, new Vector3(100, 0, 100));
            CreateIsland(patchSize, widthInTerrainPatches, heightInTerrainPatches, new Vector3(130, 0, 0));
            CreateIsland(patchSize, widthInTerrainPatches, heightInTerrainPatches, new Vector3(0, 0, 170));


            //temporary water
            Heightmap seaHeightMap = new Heightmap(patchSize / 4 * widthInTerrainPatches, 1);
            var seaObject = seaHeightMap.CreateRenderableHeightMap(Color.Blue, EffectLoader.LoadSM5Effect("flatshaded"));
            seaObject.Transform.SetPosition(new Vector3(-50, 0, -50));
            seaObject.Transform.Scale = 20;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(seaObject);



        }

        private static void CreateIsland(int patchSize, int widthInTerrainPatches, int heightInTerrainPatches, Vector3 islandPatchOrigin)
        {
            var noiseModule = NoiseGenerator.Island((patchSize * widthInTerrainPatches) / 2 + islandPatchOrigin.X, (patchSize * widthInTerrainPatches) / 2 + islandPatchOrigin.Z, 25, 0.08f, RandomHelper.GetRandomInt(1000));


            for (int i = 0; i < widthInTerrainPatches; i++)
                for (int j = 0; j < heightInTerrainPatches; j++)
                {
                    int xsampleOffset = i * (patchSize - 1);
                    int zsampleOffset = j * (patchSize - 1);

                    var hm = NoiseGenerator.CreateHeightMap(noiseModule, patchSize, 1, 40, xsampleOffset + islandPatchOrigin.X, zsampleOffset + islandPatchOrigin.Z, 1);
                    var hmObj = hm.CreateTranslatedRenderableHeightMap(Color.MonoGameOrange, EffectLoader.LoadSM5Effect("flatshaded"), new Vector3(xsampleOffset + islandPatchOrigin.X - 1, 0, zsampleOffset + islandPatchOrigin.Z - 1));
                    SystemCore.GameObjectManager.AddAndInitialiseGameObject(hmObj);
                }
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
            if (!SystemCore.CursorVisible)
            {
                EvaluateMouseCamControls(gameTime);
            }

            if (input.EvaluateInputBinding("SwitchCamera"))
            {
                if (SystemCore.ActiveCamera is ComponentCamera)
                    SystemCore.SetActiveCamera(mouseCamera);
                else
                    SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            }

            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            EvaluateCamera();




            base.Update(gameTime);
        }

        private void EvaluateCamera()
        {
            if (input.EvaluateInputBinding("CameraRotateLeft") || input.GamePadButtonPress(Buttons.LeftShoulder))
            {
                //take the camera offset and rotate it 90 degrees
                cameraOffset = Vector3.Transform(cameraOffset, Matrix.CreateRotationY(MathHelper.PiOver2));
            }
            if (input.EvaluateInputBinding("CameraRotateRight") || input.GamePadButtonPress(Buttons.RightShoulder))
            {
                cameraOffset = Vector3.Transform(cameraOffset, Matrix.CreateRotationY(-MathHelper.PiOver2));
            }

            var chopper = SystemCore.GameObjectManager.GetObject("chopper");
            if (chopper != null)
            {
                OrientCamera(chopper as Chopper);
            }
        }

        private void EvaluateMouseCamControls(GameTime gameTime)
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


            if (!releaseMouse)
            {
                mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);
                input.CenterMouse();
            }
        }

        public override void Render(GameTime gameTime)
        {
            DebugText.Write(SystemCore.ActiveCamera.Position.ToString());

            base.Render(gameTime);
        }


    }
}
