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
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(1, 1, 1));
            SystemCore.ActiveScene.FogEnabled = true;

            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);
            mouseCamera.moveSpeed = 0.1f;
            mouseCamera.SetPositionAndLook(new Vector3(50, 30, -20), (float)Math.PI, (float)-Math.PI / 5);

            cameraObject = new GameObject();
            cameraObject.AddComponent(new ComponentCamera());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            cameraOffset = new Vector3(10, 10, 10);
            

            AddInputBindings();

            SetUpSkyDome();

            SetUpGameWorld(100, 2, 2);

            chopper = new Chopper();
            chopper.Transform.SetPosition(new Vector3(50, 5, 50));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(chopper);

            carrier = new Carrier();
            carrier.Transform.SetPosition(new Vector3(50, 0, 50));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(carrier);


            carrier.GetComponent<PhysicsComponent>().PhysicsEntity.IsAffectedByGravity = false;
            carrier.GetComponent<PhysicsComponent>().PhysicsEntity.Mass = 100;

            SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -1f, 0);

            //Take the cross product of your forward vector with your global(0, 1, 0) up vector. 
            //This gives you a right vector. Cross the right vector with the forward vector to get a up vector that "points up" but is 
            //perpendicular to the forward and right vectors. Normalize all the vectors. You'll get problems if your forward vector is (0,1,0), 
            //but that shouldn't happen with a chase cam I guess.
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

        private void SetUpGameWorld(int patchSize, int widthInTerrainPatches, int heightInTerrainPatches)
        {
            var noiseModule = NoiseGenerator.Island((patchSize * widthInTerrainPatches) / 2, (patchSize * widthInTerrainPatches) / 2, 25, 0.08f, RandomHelper.GetRandomInt(1000));


            for (int i = 0; i < widthInTerrainPatches; i++)
                for (int j = 0; j < heightInTerrainPatches; j++)
                {
                    int xsampleOffset = i * (patchSize - 1);
                    int zsampleOffset = j * (patchSize - 1);

                    var hm = NoiseGenerator.CreateHeightMap(noiseModule, patchSize, 1, 40, xsampleOffset, zsampleOffset, 1);
                    var hmObj = hm.CreateTranslatedRenderableHeightMap(Color.MonoGameOrange, EffectLoader.LoadSM5Effect("flatshaded"), new Vector3(xsampleOffset - 1, 0, zsampleOffset - 1));
                    SystemCore.GameObjectManager.AddAndInitialiseGameObject(hmObj);
                }




            Heightmap seaHeightMap = new Heightmap(patchSize / 4 * widthInTerrainPatches, 1);
            var seaObject = seaHeightMap.CreateRenderableHeightMap(Color.Blue, EffectLoader.LoadSM5Effect("flatshaded"));
            seaObject.Transform.SetPosition(new Vector3(-50, 0, -50));
            seaObject.Transform.Scale = 10;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(seaObject);
        }

        private void SetUpSkyDome()
        {
            var skyDome = new GradientSkyDome(Color.MediumBlue, Color.LightCyan);

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

            if (input.EvaluateInputBinding("SwitchCamera"))
            {
                if (SystemCore.ActiveCamera is ComponentCamera)
                    SystemCore.SetActiveCamera(mouseCamera);
                else
                    SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            }

            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            if (input.EvaluateInputBinding("CameraRotateLeft"))
            {
                //take the camera offset and rotate it 90 degrees
                cameraOffset = Vector3.Transform(cameraOffset, Matrix.CreateRotationY(MathHelper.PiOver2));
            }
            if (input.EvaluateInputBinding("CameraRotateRight"))
            {
                cameraOffset = Vector3.Transform(cameraOffset, Matrix.CreateRotationY(-MathHelper.PiOver2));
            }


            var chopper = SystemCore.GameObjectManager.GetObject("chopper");
            if (chopper != null)
            {
                OrientCamera(chopper as Chopper);
            }

          

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            DebugText.Write(SystemCore.ActiveCamera.Position.ToString());

            base.Render(gameTime);
        }


    }
}
