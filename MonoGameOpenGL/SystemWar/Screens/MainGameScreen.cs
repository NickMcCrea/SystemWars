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
using System.Linq;
using System.Text;
using SystemWar.SolarSystem;

namespace SystemWar.Screens
{
    public class MainGameScreen : Screen
    {
        GameObject ship;
        Vector3d oldPos;
        DiffuseLight sunLight;
        GameObject sun;
        private DummyCamera testCamera;
        Planet earthPlanet;

        public MainGameScreen()
            : base()
        {

            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            SystemCore.AddNewSubSystem(new SkyDome());

            

            var solarSystemSettings = new SolarSystemSettings();
            SolarSystemGenerator.Generate(solarSystemSettings);


            ship = new GameObject("ship");
            ship.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.1f, ScaleHelper.Billions(3), true));
            SystemCore.SetActiveCamera(ship.GetComponent<ComponentCamera>());
            ship.AddComponent(new HighPrecisionPosition());
           
            ship.AddComponent(new ShipController());

            if (Environment.MachineName == "NICK-PC")
                ship.AddComponent(new MouseObjectController());


            oldPos = ship.GetComponent<HighPrecisionPosition>().Position;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(ship);
            ship.Transform.SetPosition(new Vector3d(150020000, 0, 0));


            sunLight = SystemCore.ActiveScene.LightsInScene.First() as DiffuseLight;
            sun = SystemCore.GameObjectManager.GetObject("sun");

            var earth = SystemCore.GameObjectManager.GetObject("earth");

            earthPlanet = earth.GetComponent<Planet>();
        
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
            {
                Vector3 location;
                if (earthPlanet.RayCast(ship.Position, ship.Transform.WorldMatrix.Forward, 10000f, out location))
                {
                    
                }
            }


            var earth = SystemCore.GameObjectManager.GetObject("earth");

            var distance = earth.GetComponent<HighPrecisionPosition>().Position - ship.GetComponent<HighPrecisionPosition>().Position;
            if (distance.Length < 20000)
            {
                earth.AddChild(ship);
            }
            else
                earth.Children.Remove(ship);

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.CornflowerBlue);

            PrintDebugInfo(gameTime);

            base.Render(gameTime);
        }

        private void PrintDebugInfo(GameTime gameTime)
        {
            Vector3d currentPos = ship.GetComponent<HighPrecisionPosition>().Position;

            double magnitude = currentPos.Length;
            string formattedDistance = String.Format("{0:#,###,###.##}", magnitude);

            Vector3d distanceTravelled = currentPos - oldPos;
            double speedPerFrame = distanceTravelled.Length;
            double speedPerSecond = speedPerFrame*(1000/gameTime.ElapsedGameTime.Milliseconds);
            string formattedSpeed = String.Format("{0:#,###,###.##}", speedPerSecond);

            Vector3d distanceFromEarthCore = currentPos - new Vector3d(150000000, 0, 0);
            float distanceFromSurface = (float) distanceFromEarthCore.Length - 6000;
            DebugText.WritePositionedText("Distance From Star in km: " + formattedDistance, new Vector2(500, 20));
            DebugText.WritePositionedText("Speed km/s: " + formattedSpeed, new Vector2(500, 40));
            DebugText.WritePositionedText("Distance From Surface: " + distanceFromSurface, new Vector2(500, 60));


            DebugText.Write("FPS " + SystemCore.GetSubsystem<FPSCounter>().FPS.ToString());
            DebugText.Write("Draw calls " + GameObjectManager.drawCalls.ToString());
            DebugText.Write("Primitives " + GameObjectManager.primitives.ToString());
            DebugText.Write("Verts " + GameObjectManager.verts.ToString());

            DebugText.Write("Patch Builds Per Second: " + earthPlanet.BuildCountPerSecond);

            oldPos = currentPos;
        }
    }
}
