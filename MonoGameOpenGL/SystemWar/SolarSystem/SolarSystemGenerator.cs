using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore;

namespace SystemWar.Screens
{
    public class SolarSystemGenerator
    {

        internal static void GenerateRandomSystem(SolarSystemSettings settings)
        {
            int planetsMin = 5;
            int planetsMax = 10;



        }

        internal static void GenerateTestSystem(SolarSystemSettings solarSystemSettings)
        {


            StarHelper.CreateAndInitialiseSystemStar(ScaleHelper.Millions(1), StarHelper.BasicSunColor());

            string planetShader = "flatshaded";

            Planet earth = new Planet("earth", new Vector3d(ScaleHelper.Millions(20), 0, 0),
                NoiseGenerator.FastPlanet(6000),
               EffectLoader.LoadEffect("AtmosphericScatteringGround").Clone(),
                6000, Color.DarkSeaGreen.ChangeTone(-100), Color.SaddleBrown, Color.SaddleBrown.ChangeTone(-10), 0.000001f);
            earth.Orbit(Vector3d.Zero, ScaleHelper.Millions(20), ScaleHelper.Millionths(0.1f));
            earth.AddAtmosphere();
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(earth);


           // Planet moon = new Planet("moon", new Vector3d(ScaleHelper.Millions(20) + 20000, 0, 0),
           //  NoiseGenerator.Voronoi(0.02f),
           //  EffectLoader.LoadEffect(planetShader).Clone(),
           //  2000, Color.DarkGray.ChangeTone(10), Color.DarkGray, Color.DarkGray.ChangeTone(-10));
           // moon.Orbit(earth, 20000, ScaleHelper.Millionths(50f));
           // moon.orbitAngle = RandomHelper.GetRandomeAngle();
           // SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon);


           // Planet moon2 = new Planet("moon2", new Vector3d(ScaleHelper.Millions(20) + 40000, 0, 0),
           // NoiseGenerator.RidgedMultiFractal(0.02f),
           // EffectLoader.LoadEffect(planetShader).Clone(),
           // 1000, Color.SandyBrown.ChangeTone(-80), Color.SandyBrown.ChangeTone(-90), Color.SandyBrown.ChangeTone(-100));
           // moon2.Orbit(earth, 40000, ScaleHelper.Millionths(50f));
           // moon2.orbitAngle = RandomHelper.GetRandomeAngle();
           // SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon2);

           // Planet moon3 = new Planet("moon3", new Vector3d(ScaleHelper.Millions(20) + 45000, 0, 0),
           //NoiseGenerator.RidgedMultiFractal(0.02f),
           //EffectLoader.LoadEffect(planetShader).Clone(),
           //500, Color.DarkSlateGray.ChangeTone(10), Color.DarkSlateGray, Color.DarkSlateGray.ChangeTone(-10));
           // moon3.Orbit(moon2, 5000, ScaleHelper.Millionths(50f));
           // moon3.orbitAngle = RandomHelper.GetRandomeAngle();
           // SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon3);








        }
    }
}