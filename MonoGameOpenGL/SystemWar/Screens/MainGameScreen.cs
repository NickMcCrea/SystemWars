using BEPUphysics;
using ConversionHelper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using MonoGameEngineCore.ScreenManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using SystemWar.Shapes;
using SystemWar.SolarSystem;

namespace SystemWar.Screens
{
    public class MainGameScreen : Screen
    {
        Ship ship;
        Vector3d oldPos;
        DiffuseLight sunLight;
        GameObject sun;
        Planet earthPlanet;
        Planet moon;
        Vector3 collisionPoint;
        RayCastResult result;
        bool firstTimePlacement = false;

        public MainGameScreen()
            : base()
        {

            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            SystemCore.AddNewSubSystem(new SkyDome());

            collisionPoint = new Vector3(1000, 1000, 1000);

            var solarSystemSettings = new SolarSystemSettings();
            SolarSystemGenerator.Generate(solarSystemSettings);


            ship = new Ship("ship");
            
            SystemCore.SetActiveCamera(ship.shipCameraObject.GetComponent<ComponentCamera>());
            ship.AddComponent(new HighPrecisionPosition());
            ship.AddComponent(new ShipController());
            ship.AddComponent(new MouseKeyboardShipController());


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
            ship.AddComponent(new EffectRenderComponent(EffectLoader.LoadEffect("flatshaded")));

            oldPos = ship.GetComponent<HighPrecisionPosition>().Position;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(ship);


            sunLight = SystemCore.ActiveScene.LightsInScene.First() as DiffuseLight;
            sun = SystemCore.GameObjectManager.GetObject("sun");

         
            earthPlanet = SystemCore.GameObjectManager.GetObject("earth") as Planet;
            moon = SystemCore.GameObjectManager.GetObject("moon") as Planet;
          
            


        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();

            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            SolarSystemHelper.AdjustObjectsForRendering(ship.GetComponent<HighPrecisionPosition>().Position);
            SkyDome.SetSunDir(ship.GetComponent<HighPrecisionPosition>().Position);

            ((DiffuseLight)sunLight).LightDirection = Vector3.Normalize(sun.Transform.WorldMatrix.Translation);

            if (input.KeyPress(Keys.Space))
                SystemCore.Wireframe = !SystemCore.Wireframe;

            if (input.KeyPress(Keys.Enter))
                ship.Transform.SetPosition(earthPlanet.GetComponent<HighPrecisionPosition>().Position + new Vector3d(6500, 0, 0));

            if (input.KeyPress(Keys.M))
                ship.Transform.SetPosition(moon.GetComponent<HighPrecisionPosition>().Position);



            List<GameObject> planets = SystemCore.GameObjectManager.GetAllObjects().FindAll(x => x is Planet);
            foreach (Planet p in planets)
            {
                double distanceToPlanet = SolarSystemHelper.CalculateDistanceToPlanet(p,
                ship.GetComponent<HighPrecisionPosition>().Position);
                if (distanceToPlanet < p.radius * 2)
                {
                    p.AddToInfluence(ship);

                }
                else
                {
                    p.RemoveFromInfluence(ship);
                }
            }

            if (!firstTimePlacement)
            {
                ship.GetComponent<HighPrecisionPosition>().Position = earthPlanet.GetComponent<HighPrecisionPosition>().Position + new Vector3d(0, 0, 6050);
                firstTimePlacement = true;
            }

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.CornflowerBlue);

            //PrintDebugInfo(gameTime);
           
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




            DebugShapeRenderer.AddBoundingSphere(new BoundingSphere(collisionPoint, 5f), Color.Red);
        }
    }
}
