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
        private float shipDistanceOnFirstPlacement = 40000;

        public MainGameScreen()
            : base()
        {

            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome());


            ship = new Ship("ship");
            SystemCore.SetActiveCamera(ship.shipCameraObject.GetComponent<ComponentCamera>());

            var cockpit = SystemWarShapes.CockpitBar();
            cockpit.Translate(Vector3.Forward * 2);
            cockpit.Translate(Vector3.Down * 0.6f);
            cockpit.SetColor(Color.DarkGray);
            cockpit.Scale(0.7f);

            //var panel = SystemWarShapes.CockpitPanel();
            //panel.Transform(Matrix.CreateRotationY(MathHelper.ToRadians(45)));
            //panel.Translate(Vector3.Forward*2 + Vector3.Left * 1.1f + Vector3.Up * 0.5f);
            //panel.Scale(0.2f);
            //panel.SetColor(Color.DarkGray);


            //var panel2 = SystemWarShapes.CockpitPanel();
            //panel2.Transform(Matrix.CreateRotationY(MathHelper.ToRadians(-45)));
            //panel2.Translate(Vector3.Forward * 2 + Vector3.Right * 1.1f + Vector3.Up * 0.5f);
            //panel2.Scale(0.2f);
            //panel2.SetColor(Color.DarkGray);

            //var finalShape = ProceduralShape.Combine(cockpit, panel, panel2);

            ship.AddComponent(new RenderGeometryComponent(cockpit));
            var cockpitEffectComponent = new EffectRenderComponent(EffectLoader.LoadEffect("flatshaded"));
            cockpitEffectComponent.DrawOrder = 100;
            ship.AddComponent(cockpitEffectComponent);

            oldPos = ship.GetComponent<HighPrecisionPosition>().Position;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(ship);


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


            if (!firstTimePlacement)
            {
                ship.GetComponent<HighPrecisionPosition>().Position =
                    SystemCore.GameObjectManager.GetObject("earth").GetComponent<HighPrecisionPosition>().Position +
                    new Vector3d(shipDistanceOnFirstPlacement, 0, 0);
                firstTimePlacement = true;
            }


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

            //PrintDebugInfo(gameTime);
            DebugText.Write(SystemCore.GetSubsystem<FPSCounter>().FPS.ToString());
            base.Render(gameTime);
        }

        private void PrintDebugInfo(GameTime gameTime)
        {
            Vector3d currentPos = ship.GetComponent<HighPrecisionPosition>().Position;

            double magnitude = currentPos.Length;
            string formattedDistance = String.Format("{0:#,###,###.##}", magnitude);

            Vector3d distanceTravelled = currentPos - oldPos;
            double speedPerFrame = distanceTravelled.Length;
            double speedPerSecond = speedPerFrame * (1000 / gameTime.ElapsedGameTime.Milliseconds);
            string formattedSpeed = String.Format("{0:#,###,###.##}", speedPerSecond);

            Vector3d distanceFromEarthCore = currentPos - new Vector3d(150000000, 0, 0);
            float distanceFromSurface = (float)distanceFromEarthCore.Length - 6000;
            DebugText.WritePositionedText("Distance From Star in km: " + formattedDistance, new Vector2(500, 20));
            DebugText.WritePositionedText("Speed km/s: " + formattedSpeed, new Vector2(500, 40));
            DebugText.WritePositionedText("Distance From Surface: " + distanceFromSurface, new Vector2(500, 60));


            DebugText.Write("FPS " + SystemCore.GetSubsystem<FPSCounter>().FPS.ToString());
            DebugText.Write("Draw calls " + GameObjectManager.drawCalls.ToString());
            DebugText.Write("Primitives " + GameObjectManager.primitives.ToString());
            DebugText.Write("Verts " + GameObjectManager.verts.ToString());

            oldPos = currentPos;

        }
    }
}
