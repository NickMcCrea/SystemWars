using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using System;
using SystemWar.Shapes;
using Microsoft.Xna.Framework.Graphics;

namespace SystemWar.Screens
{
    public class MainGameScreen : Screen
    {
        Ship ship;
        Vector3d oldPos;
        private SolarSystem solarSystem;
        bool firstTimePlacement = false;
        private Vector3 hitPos = Vector3.Zero;
        private PlanetNode hitNode = null;
        private float shipDistanceOnFirstPlacement = 50000;

        private PlanetSurfacePosition testPlanetSurfacePosition;

        public MainGameScreen()
            : base()
        {

            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome());


            ship = new Ship("ship");
            SystemCore.SetActiveCamera(ship.shipCameraObject.GetComponent<ComponentCamera>());



            Model geoDesicModel = SystemCore.ContentManager.Load<Model>("Models/geodesic2");
            ProceduralShape geodesicShape = ModelMeshParser.GetShapeFromModelWithUVs(geoDesicModel);
            geodesicShape.Scale(1f);
            geodesicShape.InsideOut();

            ship.AddComponent(new RenderGeometryComponent(geodesicShape));
            //var cockpitEffectComponent = new EffectRenderComponent(EffectLoader.LoadEffect("cockpitscreen"));
            //cockpitEffectComponent.DrawOrder = 100;
            //ship.AddComponent(cockpitEffectComponent);

            oldPos = ship.GetComponent<HighPrecisionPosition>().Position;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(ship);

        
            testPlanetSurfacePosition = new PlanetSurfacePosition();
            testPlanetSurfacePosition.Latitude = 40;
            testPlanetSurfacePosition.Longitude = -90;

            solarSystem = new SolarSystem();
            solarSystem.PlayerShip = ship;
            ship.SolarSystem = solarSystem;
            SystemCore.AddNewGameComponent(solarSystem);
            ship.Transform.Rotate(Vector3.Up, MathHelper.PiOver2);


        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();

            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {


            if (input.KeyPress(Keys.Space))
                SystemCore.Wireframe = !SystemCore.Wireframe;

            solarSystem.Update(gameTime);

            //RaycastTest();

            var earth = SystemCore.GameObjectManager.GetObject("earth");

            //DebugShapeRenderer.AddBoundingSphere(
            //    new BoundingSphere(
            //        testPlanetSurfacePosition.GetPosition(earth as Planet, ship.HighPrecisionPositionComponent), 1000f),
            //    Color.Red);

            //DebugShapeRenderer.AddLine(earth.Transform.WorldMatrix.Translation,
            //    earth.Transform.WorldMatrix.Translation + earth.Transform.WorldMatrix.Forward*7000, Color.Blue);
            //DebugShapeRenderer.AddLine(earth.Transform.WorldMatrix.Translation,
            //earth.Transform.WorldMatrix.Translation + earth.Transform.WorldMatrix.Right * 7000, Color.Red);
            //DebugShapeRenderer.AddLine(earth.Transform.WorldMatrix.Translation,
            //earth.Transform.WorldMatrix.Translation + earth.Transform.WorldMatrix.Up * 7000, Color.Green);



            if (!firstTimePlacement)
            {
                ship.GetComponent<HighPrecisionPosition>().Position =
                    SystemCore.GameObjectManager.GetObject("earth").GetComponent<HighPrecisionPosition>().Position +
                    new Vector3d(shipDistanceOnFirstPlacement, 0, 0);
                firstTimePlacement = true;
            }

            if(!SystemWarGlobalSettings.BuildPatchesOnBackgroundThread)
                PlanetBuilder.Update();

            base.Update(gameTime);



        }

        private void RaycastTest()
        {
            RayCastResult result;
            Matrix camWorld = Matrix.Invert(SystemCore.ActiveCamera.View);
            BEPUutilities.Ray ray =
                new BEPUutilities.Ray(camWorld.Translation.ToBepuVector() + camWorld.Forward.ToBepuVector()*3f,
                    camWorld.Forward.ToBepuVector());

            if (SystemCore.PhysicsSimulation.RayCast(ray, out result))
            {
                hitPos = result.HitData.Location.ToXNAVector();
                DebugShapeRenderer.AddLine(hitPos,
                    hitPos + Vector3.Normalize(result.HitData.Normal.ToXNAVector()*5f), Color.Blue);
            }
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.CornflowerBlue);

            DebugText.Write(SystemCore.GetSubsystem<FPSCounter>().FPS.ToString());
            base.Render(gameTime);
        }

       
       
    }
}
