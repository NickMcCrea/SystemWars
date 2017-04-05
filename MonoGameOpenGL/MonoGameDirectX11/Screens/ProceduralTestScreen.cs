using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using System;
using SystemWar;

namespace MonoGameDirectX11.Screens
{
    class ProceduralTestScreen : TestScreen
    {
        GradientSkyDome skyDome;
        public ProceduralTestScreen()
            : base()
        {


        }


        public override void OnInitialise()
        {
            base.OnInitialise();

            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(1,1,1));



            skyDome = new GradientSkyDome(Color.MediumBlue, Color.LightCyan);
          
       

            mouseCamera.moveSpeed = 0.01f;

            mouseCamera.SetPositionAndLook(new Vector3(50, 30, -20), (float)Math.PI, (float)-Math.PI / 5);

            for (int i = 0; i < 50; i++)
                AddPhysicsCube();


            var heightMap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.03f), 100, 1, 50, 1, 1, 1);
            var vertexArray = heightMap.GenerateVertexArray();
            var indexArray = heightMap.GenerateIndices();
            GameObject heightMapObject = new GameObject();
            ProceduralShape shape = new ProceduralShape(vertexArray, indexArray);
            shape.SetColor(Color.OrangeRed);
            RenderGeometryComponent renderGeom = new RenderGeometryComponent(shape);
            EffectRenderComponent renderComponent = new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded"));
            heightMapObject.AddComponent(renderGeom);
            heightMapObject.AddComponent(renderComponent);
            heightMapObject.AddComponent(new StaticMeshColliderComponent(heightMapObject, heightMap.GetVertices(), heightMap.GetIndices().ToArray()));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(heightMapObject);

            SystemCore.PhysicsSimulation.ForceUpdater.Gravity = new BEPUutilities.Vector3(0, -9.81f, 0);



        


        }

        private void AddPhysicsCube()
        {
            ProceduralCube shape = new ProceduralCube();
            var gameObject = new GameObject();
            gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
            gameObject.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            gameObject.AddComponent(new ShadowCasterComponent());
            gameObject.AddComponent(new PhysicsComponent(true, true, PhysicsMeshType.box));
            gameObject.Transform.SetPosition(RandomHelper.GetRandomVector3(10, 100));
            SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(gameObject);
        }

        public override void OnRemove()
        {

            base.OnRemove();
        }

      

        public override void Update(GameTime gameTime)
        {

            RayCastResult result;
            if (input.MouseLeftPress())
            {

               

                if (SystemCore.PhysicsSimulation.RayCast(input.GetBepuProjectedMouseRay(), out result))
                {
                    GameObject parent = result.HitObject.Tag as GameObject;
                }
            }

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            


            base.Render(gameTime);
        }
    }
}
