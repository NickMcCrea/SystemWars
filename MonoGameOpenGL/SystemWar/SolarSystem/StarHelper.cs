using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace SystemWar
{
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
}