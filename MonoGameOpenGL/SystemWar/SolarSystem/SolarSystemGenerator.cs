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


            Planet earth = new Planet("earth", new Vector3d(150000000, 0, 0),
                NoiseGenerator.FastPlanet(6000),
                EffectLoader.LoadEffect("flatshaded"),
                6000, Color.CornflowerBlue, Color.SaddleBrown, Color.SaddleBrown.ChangeTone(-10));

            Planet moon = new Planet("moon", new Vector3d(150050000, 0, 0),
             NoiseGenerator.FastPlanet(2000),
             EffectLoader.LoadEffect("flatshaded"),
             6000, Color.Black, Color.Black, Color.Black.ChangeTone(-10));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(earth);

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon);
            
        }

        public static Color GetRandomSeaColor()
        {
            Color startColor = Color.SeaGreen;
            return startColor.ChangeTone(RandomHelper.GetRandomInt(0, 10));
        }
    }
}