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
        Vector3 startPos = new Vector3(0, 10, 0);
        int terrainSize = 100;

        public ProceduralTerrainTestScreen()
        {
           
            CollisionRules.DefaultCollisionRule = CollisionRule.NoSolver;
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

          
            //SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(Color.DarkBlue, Color.Black, Color.SkyBlue));


            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(startPos);
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, -1, 0), new Vector3(0, 0, 1));
            cameraGameObject.AddComponent(new MouseController());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());



            Heightmap heightmap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.1f), terrainSize, 1, 10f, 0, 0,
                1);


            GameObject terrain = new GameObject("terrain");
            var verts = BufferBuilder.VertexBufferBuild(heightmap.GenerateVertexArray(-terrainSize/2,0,-terrainSize/2));

         

            var indices = BufferBuilder.IndexBufferBuild(heightmap.GenerateIndices());
            terrain.AddComponent(new RenderGeometryComponent(verts, indices, indices.IndexCount/3));

            Effect effect = EffectLoader.LoadSM5Effect("AtmosphericScatteringGround");

            helper = new GroundScatteringHelper(effect, terrainSize * 2, 0);
            
            terrain.AddComponent(new EffectRenderComponent(effect));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrain);

        }

       

       

        public override void Update(GameTime gameTime)
        {

            DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;

            if (helper != null)
            {
                helper.Update(cameraGameObject.Transform.WorldMatrix.Translation.Y, Vector3.Normalize(light.LightDirection),
                    cameraGameObject.Transform.WorldMatrix.Translation);
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
