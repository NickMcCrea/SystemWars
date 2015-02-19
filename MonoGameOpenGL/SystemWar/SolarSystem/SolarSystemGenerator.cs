using SystemWar.SolarSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private static float sunOrbitFactor = 1;

        internal static void Generate(SolarSystemSettings solarSystemSettings)
        {
          

            StarHelper.CreateAndInitialiseSystemStar(ScaleHelper.Millions(1), StarHelper.BasicSunColor());

            string planetShader = "flatshadedwithalpha";

            Planet earth = new Planet("earth", new Vector3d(ScaleHelper.Millions(20), 0, 0),
                NoiseGenerator.FastPlanet(6000),
               EffectLoader.LoadEffect("AtmosphericScatteringGround").Clone(),
                6000, Color.DarkSeaGreen.ChangeTone(-100), Color.SaddleBrown, Color.SaddleBrown.ChangeTone(-10),0.00001f);
            earth.Orbit(Vector3d.Zero, ScaleHelper.Millions(20), ScaleHelper.Millionths(0.1f));
            earth.AddAtmosphere(Color.SaddleBrown.ChangeTone(50));

            Planet moon = new Planet("moon", new Vector3d(ScaleHelper.Millions(20) + 20000, 0, 0),
             NoiseGenerator.RidgedMultiFractal(0.02f),
             EffectLoader.LoadEffect(planetShader).Clone(),
             2000, Color.DarkGray.ChangeTone(10), Color.DarkGray, Color.DarkGray.ChangeTone(-10));
            moon.Orbit(earth, 20000, ScaleHelper.Millionths(1000f));
            moon.orbitAngle = RandomHelper.GetRandomeAngle();

            Planet moon2 = new Planet("moon2", new Vector3d(ScaleHelper.Millions(20) + 40000, 0, 0),
            NoiseGenerator.RidgedMultiFractal(0.02f),
            EffectLoader.LoadEffect(planetShader).Clone(),
            1000, Color.SandyBrown.ChangeTone(-80), Color.SandyBrown.ChangeTone(-90), Color.SandyBrown.ChangeTone(-100));
            moon2.Orbit(earth, 40000, ScaleHelper.Millionths(500f));
            moon2.orbitAngle = RandomHelper.GetRandomeAngle();

            Planet moon3 = new Planet("moon3", new Vector3d(ScaleHelper.Millions(20) + 45000, 0, 0),
           NoiseGenerator.RidgedMultiFractal(0.02f),
           EffectLoader.LoadEffect(planetShader).Clone(),
           500, Color.DarkSlateGray.ChangeTone(10), Color.DarkSlateGray, Color.DarkSlateGray.ChangeTone(-10));
            moon3.Orbit(moon2, 5000, ScaleHelper.Millionths(5000f));
            moon3.orbitAngle = RandomHelper.GetRandomeAngle();

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(earth);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon2);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(moon3);



        }

        public static Color GetRandomSeaColor()
        {
            Color startColor = Color.SeaGreen;
            return startColor.ChangeTone(RandomHelper.GetRandomInt(0, 10));
        }
    }
}