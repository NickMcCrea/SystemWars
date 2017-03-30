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
using System.Collections.Generic;

namespace MonoGameDirectX11
{
    public class RenderTestScreen : Screen
    {
        protected MouseFreeCamera mouseCamera;

        public RenderTestScreen()
            : base()
        {
            SystemCore.CursorVisible = false;
            fpsLabel.Visible = true;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            Vector3 lightDir = new Vector3(-0.5f, 0.5f, -0.5f);
            lightDir.Normalize();
            SystemCore.ActiveScene.GetDiffuseLight().LightDirection = lightDir;
            var effect = EffectLoader.LoadSM5Effect("FlatShaded");


            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);

            var shape = new ProceduralSphere(20, 20);
            shape.SetColor(SystemCore.ActiveColorScheme.Color5);




            for (int i = 0; i < 100; i++)
            {
                var gameObject = new GameObject();
                gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape), BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
                gameObject.AddComponent(new EffectRenderComponent(effect));
                gameObject.Transform.SetPosition(RandomHelper.GetRandomVector3(Vector3.One * -100, Vector3.One * 100));
                gameObject.AddComponent(new RotatorComponent(Vector3.Up));
                SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(gameObject);
                gameObject.Transform.Scale = 5f;
                gameObject.Transform.Velocity = RandomHelper.GetRandomVector3(-Vector3.One, Vector3.One) * 0.01f;
            }


            AddInputBindings();

            AddTestMario("Mario", Vector3.Zero);
            AddTestMario("Mario", new Vector3(10, 0, 0));
            AddTestMario("RedGloss", new Vector3(20, 0, 0));
            AddTestMario("RedMatt", new Vector3(-20, 0, 0));
            AddTestMario("OrangeGloss", new Vector3(-10, 0, 0));
            AddTestMario("WoodenCrate", new Vector3(0, 0, 10));
            AddTestMario("Mario", new Vector3(0, 0, -10));

            var crateObject4 = AddTestModel("Models/Crate", "WoodenCrate");
            crateObject4.Transform.SetPosition(new Vector3(30, 0, 0));
            crateObject4.Transform.Scale = 0.1f;


            var groundShape = new ProceduralCuboid(10, 10, 0.5f);
            groundShape.SetColor(Color.DarkOrange);
            var gameObjectPlane = new GameObject();
            gameObjectPlane.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(groundShape), BufferBuilder.IndexBufferBuild(groundShape), groundShape.PrimitiveCount));
            gameObjectPlane.AddComponent(new EffectRenderComponent(effect));
            gameObjectPlane.Transform.SetPosition(new Vector3(0, -20, 0));
            gameObjectPlane.Transform.Scale = 10f;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObjectPlane);

        }

        private void AddTestMario(string material, Vector3 pos)
        {
            var crateObject3 = AddTestModel("Models/mario-sculpture", material);
            crateObject3.Transform.SetPosition(pos);
            crateObject3.Transform.Scale = 0.1f;
        }

        private GameObject AddTestModel(string model, string materialName)
        {
            GameObject gameObject = new GameObject();
            gameObject.AddComponent(new ModelComponent(SystemCore.ContentManager.Load<Model>(model)));
            MaterialFactory.ApplyMaterialComponent(gameObject, materialName);
            gameObject.AddComponent(new RotatorComponent(Vector3.Up, 0.001f));
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


                mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);
                input.CenterMouse();
            }

            List<GameObject> activeGameObjects = SystemCore.GetSubsystem<GameObjectManager>().GetAllObjects();
            BoundingBox testVolume = new BoundingBox(new Vector3(-100, -100, -100), new Vector3(100, 100, 100));

            foreach (GameObject activeGameObject in activeGameObjects)
            {

                if (testVolume.Contains(activeGameObject.Transform.WorldMatrix.Translation) == ContainmentType.Disjoint)
                {
                    activeGameObject.Transform.SetPosition(-activeGameObject.Transform.WorldMatrix.Translation);
                }

            }

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(SystemCore.ActiveColorScheme.Color2);

            DebugShapeRenderer.VisualiseAxes(5f);

            base.Render(gameTime);


        }

        protected void SetCameraMovement(bool active)
        {
            SystemCore.CursorVisible = !active;
        }
    }
}