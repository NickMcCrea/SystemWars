using BEPUphysics.CollisionRuleManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using SystemWar;

namespace GridForgeResurrected.Screens
{
   

    class ProceduralTerrainTestScreen : Screen
    {
        private GameObject cameraGameObject;
        GroundScatteringHelper helper;
        Atmosphere atmosphere;
        private Vector3 startPos;
        private float planetSize = 500f;
      
        public ProceduralTerrainTestScreen()
        {
            startPos = new Vector3(0, planetSize * 1.25f, 0);

            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

          
           SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(Color.DarkBlue, Color.Black, Color.DarkBlue));


            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(startPos);
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, -1, 0), new Vector3(0, 0, 1));
            cameraGameObject.AddComponent(new MouseController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());


            Effect effect = EffectLoader.LoadSM5Effect("AtmosphericScatteringGround");


            //Heightmap heightmap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.1f), 100, 50, 10f, 0, 0,
            //    1);

            //GameObject terrain = new GameObject("terrain");
            //var verts = BufferBuilder.VertexBufferBuild(heightmap.GenerateVertexArray(-5000 / 2, 5000, -5000 / 2));
            //var indices = BufferBuilder.IndexBufferBuild(heightmap.GenerateIndices());
            //terrain.AddComponent(new RenderGeometryComponent(verts, indices, indices.IndexCount / 3));
            //terrain.AddComponent(new EffectRenderComponent(effect));
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrain);

       
            ProceduralSphereTwo sphere = new ProceduralSphereTwo(100);
            sphere.SetColor(Color.DarkOrange);
            sphere.Scale(planetSize);
            var obj = GameObjectFactory.CreateRenderableGameObjectFromShape(sphere, effect);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(obj);

            helper = new GroundScatteringHelper(effect, planetSize * 1.05f, planetSize);
            atmosphere = new Atmosphere(planetSize*1.05f, planetSize);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphere);
        }

       

       

        public override void Update(GameTime gameTime)
        {

            DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;

            if (helper != null)
            {
                helper.Update(cameraGameObject.Transform.WorldMatrix.Translation.Length(), light.LightDirection,
                    cameraGameObject.Transform.WorldMatrix.Translation);

                atmosphere.Update(light.LightDirection, cameraGameObject.Transform.WorldMatrix.Translation);
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
