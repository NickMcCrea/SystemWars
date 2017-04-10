using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GameObject.Components.RenderComponents;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using Particle3DSample;
using System;
using System.Collections.Generic;

namespace MonoGameDirectX11
{
    public class RenderTest : Screen
    {
        protected MouseFreeCamera mouseCamera;
        bool releaseMouse = false;
        GameObject crate;

        public RenderTest()
            : base()
        {


        }

        public override void OnInitialise()
        {
            SystemCore.CursorVisible = false;
            fpsLabel.Visible = true;

            SystemCore.ActiveScene.AmbientLight = new AmbientLight(Color.White, 0.1f);
            SystemCore.ActiveScene.AddKeyLight(Vector3.Normalize(new Vector3(1, 1, 1)), Color.White, 0.5f, true);
            SystemCore.ActiveScene.AddBackLight(Vector3.One, Color.White, 0.4f);
            SystemCore.ActiveScene.AddFillLight(Vector3.Normalize(new Vector3(0, 1, 1)), Color.White, 0.2f);

            float lightDistance = 80f;
            float fadeStart = 50;
            float fadeEnd = 100;
            SystemCore.ActiveScene.AddPointLight(new Vector3(lightDistance, 0, 0), new Color(0.1f, 0.5f, 0.1f, 1), fadeStart, fadeEnd, 1f, PointLightNumber.One);
            SystemCore.ActiveScene.AddPointLight(new Vector3(-lightDistance, 0, 0), Color.Blue, fadeStart, fadeEnd, 1f, PointLightNumber.Two);
            SystemCore.ActiveScene.AddPointLight(new Vector3(0, 0, -lightDistance), Color.White, fadeStart, fadeEnd, 1f, PointLightNumber.Three);
            SystemCore.ActiveScene.AddPointLight(new Vector3(0, 0, lightDistance), Color.Red, fadeStart, fadeEnd, 1f, PointLightNumber.Four);

            var effect = EffectLoader.LoadSM5Effect("FlatShaded");
            SystemCore.ActiveScene.FogEnabled = false;


            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);

            var shape = new ProceduralSphere(20, 20);
            shape.SetColor(Color.LightGray);


            for (int i = 0; i < 10; i++)
            {
                var gameObject = new GameObject();
                gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape), BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
                gameObject.AddComponent(new EffectRenderComponent(effect));
                gameObject.Transform.SetPosition(RandomHelper.GetRandomVector3(Vector3.One * -100, Vector3.One * 100));
                gameObject.AddComponent(new RotatorComponent(Vector3.Up));
                gameObject.AddComponent(new ShadowCasterComponent());



                SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(gameObject);
                gameObject.Transform.Scale = 5f;
                gameObject.Transform.Velocity = RandomHelper.GetRandomVector3(-Vector3.One, Vector3.One) * 0.01f;


            }


            AddInputBindings();

            AddTestMario("Mario", Vector3.Zero);
            AddTestMario("RedGloss", new Vector3(20, 0, 0));
            AddTestMario("RedMatt", new Vector3(-20, 0, 0));
            AddTestMario("OrangeGloss", new Vector3(0, 0, -20));
            AddTestMario("WoodenCrate", new Vector3(0, 0, 20));

            crate = AddTestModel("Models/Crate", "WoodenCrate");
            crate.Transform.SetPosition(new Vector3(100, 0, 50));
            crate.Transform.Scale = 0.01f;


            var groundShape = new ProceduralCuboid(10, 10, 0.5f);
            groundShape.SetColor(Color.LightGray);
            var gameObjectPlane = new GameObject();
            gameObjectPlane.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(groundShape), BufferBuilder.IndexBufferBuild(groundShape), groundShape.PrimitiveCount));
            gameObjectPlane.AddComponent(new EffectRenderComponent(effect));
            gameObjectPlane.Transform.SetPosition(new Vector3(0, -20, 0));
            gameObjectPlane.Transform.Scale = 10f;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObjectPlane);

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

        private void AddTestMario(string material, Vector3 pos)
        {
            var gameObject = AddTestModel("Models/mario-sculpture", material);
            gameObject.Transform.SetPosition(pos);
            gameObject.Transform.Scale = 0.1f;
        }

        private GameObject AddTestModel(string model, string materialName)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent(new ModelComponent(SystemCore.ContentManager.Load<Model>(model)));
            MaterialFactory.ApplyMaterialComponent(gameObject, materialName);
            gameObject.AddComponent(new ShadowCasterComponent());
            //gameObject.AddComponent(new RotatorComponent(Vector3.Up, 0.001f));
            gameObject.AddComponent(new SquareParticleSystem());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);
            return gameObject;
        }

        private void AddInputBindings()
        {
            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("CameraForward", Keys.Up);
            input.AddKeyDownBinding("CameraBackward", Keys.Down);
            input.AddKeyDownBinding("CameraLeft", Keys.Left);
            input.AddKeyDownBinding("CameraRight", Keys.Right);

            input.AddKeyPressBinding("MainMenu", Keys.Escape);

            var releaseMouseBinding = input.AddKeyPressBinding("MouseRelease", Keys.M);
            releaseMouseBinding.InputEventActivated += (x, y) =>
            {
                releaseMouse = !releaseMouse;
                SystemCore.CursorVisible = releaseMouse;
            };

            var binding = input.AddKeyPressBinding("WireframeToggle", Keys.Space);
            binding.InputEventActivated += (x, y) => { SystemCore.Wireframe = !SystemCore.Wireframe; };
        }

        public override void Update(GameTime gameTime)
        {
            if (!SystemCore.CursorVisible)
            {
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

            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());


            //DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;
            //light.LightDirection = Vector3.Transform(light.LightDirection, Matrix.CreateRotationY(0.001f));

            List<GameObject> activeGameObjects = SystemCore.GetSubsystem<GameObjectManager>().GetAllObjects();
            BoundingBox testVolume = new BoundingBox(new Vector3(-100, -100, -100), new Vector3(100, 100, 100));

            foreach (GameObject activeGameObject in activeGameObjects)
            {

                if (testVolume.Contains(activeGameObject.Transform.WorldMatrix.Translation) == ContainmentType.Disjoint)
                {
                    activeGameObject.Transform.SetPosition(-activeGameObject.Transform.WorldMatrix.Translation);
                }

            }

            var particleSystem = crate.GetComponent<SquareParticleSystem>();
            if (particleSystem != null)
                particleSystem.AddParticle(crate.Transform.WorldMatrix.Translation, Vector3.Up);

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            if (!SystemCore.ShadowMapRenderer.ShadowPass)
                SystemCore.GraphicsDevice.Clear(Color.Black);

            DebugShapeRenderer.VisualiseAxes(5f);

            base.Render(gameTime);


        }

        protected void SetCameraMovement(bool active)
        {
            SystemCore.CursorVisible = !active;
        }
    }
}