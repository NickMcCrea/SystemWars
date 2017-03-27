using BEPUphysics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.EntityStateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using System.Collections.Generic;
using System.Diagnostics;

namespace MonoGameDirectX11
{
    public class PhysicsTestScreen : Screen
    {
        readonly MouseFreeCamera mouseCamera;
        ColorScheme colorScheme;
        public PhysicsTestScreen()
            : base()
        {

            SystemCore.CursorVisible = false;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            colorScheme = SystemCore.ActiveColorScheme;

            SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);


            mouseCamera = new MouseFreeCamera(new Vector3(10, 10, 10));
            SystemCore.SetActiveCamera(mouseCamera);


            var effect = EffectLoader.LoadSM5Effect("FlatShaded");

            //ground plane
            var groundShape = new ProceduralCuboid(10,10,0.5f);
            groundShape.SetColor(colorScheme.Color5);
            var gameObjectPlane = new GameObject();
            gameObjectPlane.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(groundShape), BufferBuilder.IndexBufferBuild(groundShape), groundShape.PrimitiveCount));
            gameObjectPlane.AddComponent(new EffectRenderComponent(effect));
            var groundPhysicsComponent = new PhysicsComponent(false, true, PhysicsMeshType.box);
            gameObjectPlane.AddComponent(groundPhysicsComponent);
           
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObjectPlane);
            groundPhysicsComponent.PhysicsEntity.IsAffectedByGravity = false;
        

            var shape = new ProceduralCube();
            shape.SetColor(colorScheme.Color4);

            for (int i = 0; i < 100; i++)
                AddCube(shape, effect, RandomHelper.GetRandomVector3(new Vector3(-10, 2, -10), new Vector3(10, 20, 10)));
         
            AddInputBindings();
        }

        private static void AddCube(ProceduralCube shape, Effect effect, Vector3 position)
        {
            var gameObject = new GameObject();
            gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
            gameObject.AddComponent(new EffectRenderComponent(effect));
            gameObject.AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.box));
            gameObject.Transform.SetPosition(position);
            gameObject.GetComponent<PhysicsComponent>().Simulated = RandomHelper.CoinToss();
            SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(gameObject);
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
            if (input.EvaluateInputBinding("CameraForward"))
                mouseCamera.MoveForward();
            if (input.EvaluateInputBinding("CameraBackward"))
                mouseCamera.MoveBackward();
            if (input.EvaluateInputBinding("CameraLeft"))
                mouseCamera.MoveLeft();
            if (input.EvaluateInputBinding("CameraRight"))
                mouseCamera.MoveRight();

            RayCastResult result;
            if (input.MouseLeftPress())
            {
                Matrix camWorld = Matrix.Invert(SystemCore.ActiveCamera.View);
                BEPUutilities.Ray ray = new BEPUutilities.Ray(camWorld.Translation.ToBepuVector(), camWorld.Forward.ToBepuVector());
                
                if (SystemCore.PhysicsSimulation.RayCast(ray, out result))
                {
                    Debugger.Break();
                }
            }

            mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);
            input.CenterMouse();

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
            SystemCore.GraphicsDevice.Clear(colorScheme.Color2);
            DebugShapeRenderer.VisualiseAxes(5f);
            base.Render(gameTime);


        }

    }
}
