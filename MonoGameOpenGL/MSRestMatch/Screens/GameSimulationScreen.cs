using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using MSRestMatch.GameServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace MSRestMatch.Screens
{
    class TrainingSimScreen : Screen
    {

        GameObject cameraObject;
        WebServiceHost host;

        public TrainingSimScreen()
        {

        }

        public override void OnInitialise()
        {

            var gameSim = new GameSimulation();
            SystemCore.AddNewUpdateRenderSubsystem(gameSim);
            SystemCore.CursorVisible = true;
            SystemCore.ActiveScene.SetUpAmbientAndFullLightingRig();
            SystemCore.ActiveScene.GetKeyLight().LightColor = Color.LightBlue;
            SystemCore.ActiveScene.FogEnabled = false;
            

            cameraObject = new GameObject();
            cameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, 200f, false));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            cameraObject.Transform.AbsoluteTransform = Matrix.CreateWorld(new Vector3(0, 200, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1));


            host = WebHostHelper.CreateWebHost(gameSim);

            gameSim.CreateFloor(4,4,10);

            gameSim.CreateTrainingArena();

            gameSim.TrainingMode = true;
          

            base.OnInitialise();
        }

        private static void CreateTestArena()
        {
            //var groundShape = new ProceduralCuboid(200, 200, 0.5f);
            //groundShape.SetColor(Color.DarkOrange);
            //var gameObjectPlane = new GameObject();
            //gameObjectPlane.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(groundShape), BufferBuilder.IndexBufferBuild(groundShape), groundShape.PrimitiveCount));
            //gameObjectPlane.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            //gameObjectPlane.Transform.Scale = 1f;
            //gameObjectPlane.AddComponent(new ShadowCasterComponent());
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObjectPlane);
            //gameObjectPlane.Transform.SetPosition(new Vector3(0, -4, 0));

            float arenaSize = 40f;


            ProceduralCuboid a = new ProceduralCuboid(arenaSize, 1, arenaSize / 5);
            a.Translate(new Vector3(0, arenaSize / 5, arenaSize));
            a.SetColor(Color.LightGray);

            ProceduralShape b = a.Clone();
            b.Translate(new Vector3(0, 0, -arenaSize * 2));

            ProceduralShape c = ProceduralShape.Combine(a, b);
            ProceduralShape d = b.Clone();
            d.Transform(MonoMathHelper.RotateNinetyDegreesAroundUp(true));

            ProceduralShape e = ProceduralShape.Combine(c, d);



            var side2 = e.Clone();
            var side3 = e.Clone();
            var side4 = e.Clone();

            e.Translate(new Vector3(-arenaSize * 2, 0, 0));

            side2.Transform(MonoMathHelper.RotateHundredEightyDegreesAroundUp(true));
            side2.Translate(new Vector3(arenaSize * 2, 0, 0));
            side3.Transform(MonoMathHelper.RotateNinetyDegreesAroundUp(true));
            side3.Translate(new Vector3(0, 0, arenaSize * 2));
            side4.Transform(MonoMathHelper.RotateNinetyDegreesAroundUp(false));
            side4.Translate(new Vector3(0, 0, -arenaSize * 2));



            var final = ProceduralShape.Combine(e, side2, side3, side4);

            var arenaObject = GameObjectFactory.CreateRenderableGameObjectFromShape(final,
                EffectLoader.LoadSM5Effect("flatshaded"));


            arenaObject.AddComponent(new StaticMeshColliderComponent(arenaObject, final.GetVertices(),
                final.GetIndicesAsInt().ToArray()));

            arenaObject.AddComponent(new ShadowCasterComponent());


            SystemCore.GameObjectManager.AddAndInitialiseGameObject(arenaObject);
        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            host.Close();
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            float currentHeight = cameraObject.Transform.AbsoluteTransform.Translation.Y;
            cameraObject.Transform.Translate(new Vector3(0, -input.ScrollDelta / 10f, 0));

            float cameraSpeed = 1f;

            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                cameraObject.Transform.Translate(new Vector3(0, 0, cameraSpeed));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                cameraObject.Transform.Translate(new Vector3(0, 0, -cameraSpeed));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                cameraObject.Transform.Translate(new Vector3(cameraSpeed, 0, 0));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                cameraObject.Transform.Translate(new Vector3(-cameraSpeed, 0, 0));

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            

            base.Render(gameTime);
        }
    }
}
