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
        public ProceduralTerrainTestScreen()
        {


            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(1, 1, 1));
            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(Color.DarkBlue, Color.Black, Color.SkyBlue));


            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(new Vector3(0, 30, 0));
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, -1, 0), new Vector3(0, 0, 1));
            cameraGameObject.AddComponent(new MouseController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());



            Heightmap heightmap = NoiseGenerator.CreateHeightMap(NoiseGenerator.FastPlanet(1000), 100, 1, 40f, 0, 0,
                1);


            GameObject terrain = new GameObject("terrain");
            var verts = BufferBuilder.VertexBufferBuild(heightmap.GenerateVertexArray(-50,0,-50));

         

            var indices = BufferBuilder.IndexBufferBuild(heightmap.GenerateIndices());
            terrain.AddComponent(new RenderGeometryComponent(verts, indices, indices.IndexCount/3));

            Effect effect = EffectLoader.LoadSM5Effect("AtmosphericScatteringGround");
            helper = new GroundScatteringHelper(effect, 100, 1);
            
            terrain.AddComponent(new EffectRenderComponent(effect));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrain);

        }

       

       

        public override void Update(GameTime gameTime)
        {

            DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;

            helper.Update(cameraGameObject.Transform.WorldMatrix.Translation.Y, light.LightDirection, cameraGameObject.Transform.WorldMatrix.Translation);

            base.Update(gameTime);

        }

     
        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Gray);
            DebugShapeRenderer.VisualiseAxes(5f);


            base.Render(gameTime);



        }
    }
}
