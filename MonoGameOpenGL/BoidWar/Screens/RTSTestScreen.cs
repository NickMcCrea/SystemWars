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


    class RTSTestScreen : Screen
    {

        GameObject cameraObject;
        ComponentCamera camComponent;
        GameSimulation simulation;

        public RTSTestScreen() : base()
        {

        }

        public override void OnInitialise()
        {
            //lighting
            fpsLabel.Visible = true;
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(0.01f, 1, 0.01f));
            SystemCore.ActiveScene.FogEnabled = false;



            //input
            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("CameraForward", Keys.Up);
            input.AddKeyDownBinding("CameraBackward", Keys.Down);
            input.AddKeyDownBinding("CameraLeft", Keys.Left);
            input.AddKeyDownBinding("CameraRight", Keys.Right);
            input.AddKeyPressBinding("MainMenu", Keys.Escape);
            var binding = input.AddKeyPressBinding("WireframeToggle", Keys.Space);
            binding.InputEventActivated += (x, y) => { SystemCore.Wireframe = !SystemCore.Wireframe; };


            //camera
            cameraObject = new GameObject();
            camComponent = new ComponentCamera();
            cameraObject.AddComponent(camComponent);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            cameraObject.Transform.SetPosition(new Vector3(0, 100, 0));
            cameraObject.Transform.Rotate(Vector3.Right, MathHelper.ToRadians(-45));
            cameraObject.Transform.Rotate(Vector3.Up, -MathHelper.PiOver4);
            SystemCore.SetActiveCamera(camComponent);


            simulation = new GameSimulation();
            simulation.InitaliseSimulation();

            base.OnInitialise();
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
            CameraInput();

            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());


            simulation.Update(gameTime);

            base.Update(gameTime);
        }

        private void CameraInput()
        {
            if (input.EvaluateInputBinding("CameraForward"))
            {
                cameraObject.Transform.Translate(new Vector3(1, 0, -1));
            }
            if (input.EvaluateInputBinding("CameraBackward"))
            {
                cameraObject.Transform.Translate(new Vector3(-1, 0, 1));
            }
            if (input.EvaluateInputBinding("CameraLeft"))
            {
                cameraObject.Transform.Translate(new Vector3(-1, 0, -1));
            }
            if (input.EvaluateInputBinding("CameraRight"))
            {
                cameraObject.Transform.Translate(new Vector3(1, 0, 1));
            }
            cameraObject.Transform.Translate(new Vector3(0, -input.ScrollDelta / 30f, 0));
        }

        public override void Render(GameTime gameTime)
        {


            base.Render(gameTime);

        }


    }
}