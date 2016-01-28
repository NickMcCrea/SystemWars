using System.Collections.Generic;
using System.Linq;
using BEPUphysics.CollisionRuleManagement;
using LibNoise.Modifiers;
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
        private float planetSize = 50f;


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


            //var one = AddTerrainSegment(100, 0, 0);
            //var two = AddTerrainSegment(100, 99, 0);
            //two.Transform.Translate(new Vector3(99, 0, 0));

            //var three = AddTerrainSegment(100, -99, 0);
            //three.Transform.Translate(new Vector3(-99, 0, 0));



            //ProceduralSphereTwo sphere = new ProceduralSphereTwo(100);
            //sphere.SetColor(Color.DarkOrange);
            //sphere.Scale(planetSize);
            //var obj = GameObjectFactory.CreateRenderableGameObjectFromShape(sphere, effect);
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(obj);


        

            var noiseGenerator = NoiseGenerator.RidgedMultiFractal(0.05f);

            int radius =49;
            int vertCount = 100;
            float scale = 1;
            //top
            Heightmap top = new Heightmap(vertCount,scale);
            GameObject terrainTopObject = new GameObject();

            VertexPositionColorTextureNormal[] v = top.GenerateVertexArray();

          
            helper = new GroundScatteringHelper(effect, radius * 1.2f, radius * 0.9f);
            atmosphere = new Atmosphere(radius * 1.2f, radius *  0.9f);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphere);

           
            for (int i = 0; i < v.Length; i++)
            {
                v[i].Position = v[i].Position - new Vector3(radius, 0, radius);
                v[i].Position = v[i].Position + Vector3.Up * radius;
                v[i].Position = Vector3.Normalize(v[i].Position) * radius;

                double heightValue = noiseGenerator.GetValue(v[i].Position.X, v[i].Position.Y, v[i].Position.Z);
                v[i].Position = (Vector3.Normalize(v[i].Position)*(radius + (float) heightValue));
            }

            var verts =
               BufferBuilder.VertexBufferBuild(v);
         
            var indices = BufferBuilder.IndexBufferBuild(top.GenerateIndices());
            terrainTopObject.AddComponent(new RenderGeometryComponent(verts, indices, indices.IndexCount / 3));
            terrainTopObject.AddComponent(new EffectRenderComponent(effect));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrainTopObject);



            //front
            Heightmap front = new Heightmap(vertCount, scale);
            GameObject terrainfrontGameObjectObject = new GameObject();

            VertexPositionColorTextureNormal[] vF = front.GenerateVertexArray();


            for (int i = 0; i < vF.Length; i++)
            {
                vF[i].Position = vF[i].Position - new Vector3(radius, 0, radius);
                vF[i].Position = Vector3.Transform(vF[i].Position, Matrix.CreateRotationX(MathHelper.PiOver2));
                vF[i].Position = vF[i].Position - Vector3.Forward * radius;
                vF[i].Position = Vector3.Normalize(vF[i].Position) * radius;

                double heightValue = noiseGenerator.GetValue(vF[i].Position.X, vF[i].Position.Y, vF[i].Position.Z);
                vF[i].Position = (Vector3.Normalize(vF[i].Position) * (radius + (float)heightValue));
            }

            var vertsFront =
               BufferBuilder.VertexBufferBuild(vF);

            var indicesFront = BufferBuilder.IndexBufferBuild(front.GenerateIndices());
            terrainfrontGameObjectObject.AddComponent(new RenderGeometryComponent(vertsFront, indicesFront, indicesFront.IndexCount / 3));
            terrainfrontGameObjectObject.AddComponent(new EffectRenderComponent(effect));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrainfrontGameObjectObject);


        }

        private GameObject AddTerrainSegment(float size, float sampleX, float sampleY)
        {



            int vertCount = 100;
            Heightmap heightmap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.1f), vertCount,
                size / vertCount, 10f, sampleX, sampleY,
                1);

            GameObject terrain = new GameObject("terrain");




            var verts =
                BufferBuilder.VertexBufferBuild(heightmap.GenerateVertexArray());


            var indices = BufferBuilder.IndexBufferBuild(heightmap.GenerateIndices());
            terrain.AddComponent(new RenderGeometryComponent(verts, indices, indices.IndexCount / 3));
            terrain.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));



            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrain);

            return terrain;
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
