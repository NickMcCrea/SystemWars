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
        Vector3 startPos = new Vector3(0, 6500, 0);
      
        public ProceduralTerrainTestScreen()
        {
           
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
            sphere.Scale(5000);
            var obj = GameObjectFactory.CreateRenderableGameObjectFromShape(sphere, effect);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(obj);

            helper = new GroundScatteringHelper(effect, 5000 * 1.05f, 5000);
            atmosphere = new Atmosphere(5000*1.05f, 5000);
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
