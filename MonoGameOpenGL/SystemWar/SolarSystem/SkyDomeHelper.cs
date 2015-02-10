using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemWar.SolarSystem
{
    public class SkyDome : IGameSubSystem
    {
        static SkyDomeRenderer skyDomeRenderer;


        public void Initalise()
        {
            var skyDomeGameObject = GameObjectFactory.CreateSkyDomeObject(new Color(10, 10, 50, 255), 100, 10);

            skyDomeRenderer = skyDomeGameObject.GetComponent<SkyDomeRenderer>();
            skyDomeRenderer.AmbientLightColor = Color.Black;
            skyDomeRenderer.AmbientLightIntensity = 0.2f;
            skyDomeRenderer.DiffuseLightColor = Color.LightBlue;
            skyDomeRenderer.DiffuseLightIntensity = 0.3f;
            skyDomeRenderer.DiffuseLightDirection = Vector3.Forward;

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(skyDomeGameObject);
        }

        public static void TransitionToColor(Color newColor, float transitionTime)
        {

        }

        public static void SetSunDir(Vector3d cameraPosition)
        {
            Vector3 toSun = cameraPosition.ToVector3();
            toSun.Normalize();
            skyDomeRenderer.DiffuseLightDirection = toSun;
        }

        public static void SetSunDir(Vector3 cameraPosition)
        {
            Vector3 toSun = cameraPosition - Vector3.Zero;
            toSun.Normalize();
            skyDomeRenderer.DiffuseLightDirection = toSun;
        }

        public void Update(GameTime gameTime)
        {

           
        }

        public void Render(GameTime gameTime)
        {

        }
    }

    public static class StarHelper
    {
        public static Color BasicSunColor()
        {
            return new Color(127, 49, 0, 255);
        }

        public static void CreateAndInitialiseSystemStar(float scale, Color color)
        {
            var sunShape = new ProceduralSphere(20, 20);
            sunShape.SetColor(color);
            var sun = GameObjectFactory.CreateRenderableGameObjectFromShape("sun", sunShape, EffectLoader.LoadEffect("flatshaded"));
            sun.Transform.Scale = scale;
            sun.AddComponent(new SolarSystemPlaneteryBody(0, Vector3d.Zero, 0));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(sun);
        }

        public static void CreateTestPlanet(string name, float scale, float distance, Color color)
        {
            var shape = new ProceduralSphere(20, 20);
            shape.SetColor(color);
            var planet = GameObjectFactory.CreateRenderableGameObjectFromShape(shape, EffectLoader.LoadEffect("flatshaded"));
            planet.Transform.Scale = scale;
            planet.Name = name;
            var solarBody = new SolarSystemPlaneteryBody(0, Vector3d.Zero, 0);
            solarBody.Position = new Vector3d(distance, 0, 0);
            planet.AddComponent(solarBody);

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(planet);
        }
    }

    public static class SolarSystemHelper
    {
        public static void AdjustObjectsForRendering(Vector3d cameraPosition)
        {
            var gameObjects = SystemCore.GameObjectManager.GetAllObjects();


            foreach (GameObject o in gameObjects)
            {

                var largeScalePositionComponent = o.GetComponent<HighPrecisionPosition>();

                if (largeScalePositionComponent == null)
                    largeScalePositionComponent = o.GetComponent<SolarSystemPlaneteryBody>();

                if (largeScalePositionComponent != null)
                {
                    var renderPosition = GetRenderPosition(cameraPosition, largeScalePositionComponent.Position);
                    o.Transform.WorldMatrix.Translation = renderPosition;
                }
            }
        }

        public static Vector3 GetRenderPosition(Vector3d camPosition, Vector3d solarSystemPosition)
        {
            Vector3d distanceFromCamera = solarSystemPosition - camPosition;
            Vector3 renderPosition = distanceFromCamera.ToVector3();
            return renderPosition;
        }

        internal static double CalculateDistanceToPlanet(string planet, Vector3d position)
        {
            Planet p = SystemCore.GameObjectManager.GetObject(planet) as Planet;
            var toPlanet = p.GetComponent<HighPrecisionPosition>().Position - position;
            return toPlanet.Length;
        }
        internal static double CalculateDistanceToPlanet(Planet planet, Vector3d position)
        {      
            var toPlanet = planet.GetComponent<HighPrecisionPosition>().Position - position;
            return toPlanet.Length;
        }

    }


}
