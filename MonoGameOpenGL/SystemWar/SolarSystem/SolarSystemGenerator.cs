using SystemWar.SolarSystem;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore;

namespace SystemWar.Screens
{
    public class SolarSystemGenerator
    {
        internal static void Generate(SolarSystemSettings solarSystemSettings)
        {

            StarHelper.CreateAndInitialiseSystemStar(1000000f, StarHelper.BasicSunColor());

            GameObject earth = new GameObject("earth");
            earth.AddComponent(new HighPrecisionPosition());
            earth.AddComponent(new Planet(NoiseGenerator.FastPlanet(6000),
                EffectLoader.LoadEffect("flatshaded"), 6000, Color.SeaGreen, Color.SaddleBrown, Color.SaddleBrown.ChangeTone(-10)));
            //earth.AddComponent(new TranslatorComponent(Vector3.Up, 0.0001f));
            //earth.AddComponent(new RotatorComponent(Vector3.Up, 0.0001f));

           
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(earth);
            earth.Transform.SetPosition(new Vector3d(150000000, 0, 0));


            GameObject moon = new GameObject("moon");
            moon.AddComponent(new HighPrecisionPosition());
            moon.AddComponent(new Planet(NoiseGenerator.RidgedMultiFractal(0.05f),
                EffectLoader.LoadEffect("flatshaded"), 2500, Color.DarkGray.ChangeTone(-100), Color.DarkGray.ChangeTone(-100), Color.DarkGray.ChangeTone(-100)));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon);
            moon.Transform.SetPosition(new Vector3d(150250000, 0, 0));

            
        }

        public static Color GetRandomSeaColor()
        {
            Color startColor = Color.SeaGreen;
            return startColor.ChangeTone(RandomHelper.GetRandomInt(0, 10));
        }
    }
}