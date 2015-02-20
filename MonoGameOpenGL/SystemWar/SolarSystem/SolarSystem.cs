using System.Collections.Generic;
using System.Linq;
using SystemWar.Screens;
using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;


namespace SystemWar
{
    public class SolarSystem : MonoGameEngineCore.IGameComponent
    {

      
        public Ship PlayerShip { get; set; }
        public DiffuseLight SunLight { get; set; }
        public GameObject Sun { get; set; }

        public SolarSystem()
        {
            SolarSystemGenerator.Generate(new SolarSystemSettings());
            Sun = SystemCore.GameObjectManager.GetObject("sun");
            PlayerShip = SystemCore.GameObjectManager.GetObject("ship") as Ship;
            SunLight = SystemCore.ActiveScene.LightsInScene.First() as DiffuseLight;
        }

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

        public static DiffuseLight GetSun()
        {
            return SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;
        }

        internal void Update(GameTime gameTime)
        {
            AdjustObjectsForRendering(PlayerShip.GetComponent<HighPrecisionPosition>().Position);

            ((DiffuseLight)SystemCore.ActiveScene.LightsInScene[0]).LightDirection = Vector3.Normalize(Sun.Transform.WorldMatrix.Translation);

            List<GameObject> planets = SystemCore.GameObjectManager.GetAllObjects().FindAll(x => x is Planet);

            SortForRendering(planets);

            bool inAtmostphere = false;
            foreach (Planet p in planets)
            {

                double distanceToPlanet = SolarSystem.CalculateDistanceToPlanet(p,
                PlayerShip.GetComponent<HighPrecisionPosition>().Position);
                if (distanceToPlanet < p.radius * 2)
                {
                    p.AddToInfluence(PlayerShip);

                    if (p.HasAtmosphere)
                        if (WithinAtmosphere(p, PlayerShip))
                        {
                            PlayerShip.SetInAtmosphere();
                            inAtmostphere = true;
                        }

                }
                else
                {
                    p.RemoveFromInfluence(PlayerShip);
                }
            }
            if (!inAtmostphere)
                PlayerShip.ExitedAtmosphere();
        }

        private bool WithinAtmosphere(Planet p, Ship PlayerShip)
        {
            float atmosphereRadius = p.atmosphere.m_fOuterRadius;

            Vector3d distanceFromPlanetCenter = p.GetComponent<HighPrecisionPosition>().Position -
                                                PlayerShip.HighPrecisionPositionComponent.Position;

            if ((float) distanceFromPlanetCenter.Length < atmosphereRadius)
                return true;
            return false;
        }

        private void SortForRendering(List<GameObject> planets)
        {
            if (PlayerShip.InAtmosphere)
            {

                PlayerShip.CurrentPlanet.atmosphere.GetComponent<EffectRenderComponent>().DrawOrder = 2;
                PlayerShip.CurrentPlanet.DrawOrder = 3;

                foreach (Planet planet in planets)
                {
                    if (planet == PlayerShip.CurrentPlanet)
                        continue;

                    planet.DrawOrder = 1;
                }
            }
            else
            {
                foreach (Planet planet in planets)
                {
                    if (planet.HasAtmosphere)
                    {
                        planet.atmosphere.GetComponent<EffectRenderComponent>().DrawOrder = 0;
                        planet.DrawOrder = 1;
                    }
                    else
                    {
                        planet.DrawOrder = 2;
                    }
                }
            }

        }
    }


}
