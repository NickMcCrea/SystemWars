﻿using SystemWar.SolarSystem;
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
            //BasicEffect effect = new BasicEffect(SystemCore.GraphicsDevice);
            Effect effect = EffectLoader.LoadEffect("flatshaded");

            StarHelper.CreateAndInitialiseSystemStar(ScaleHelper.Millions(1), StarHelper.BasicSunColor());


            Planet earth = new Planet("earth", new Vector3d(ScaleHelper.Millions(20), 0, 0),
                NoiseGenerator.FastPlanet(6000),
               effect,
                6000, Color.CornflowerBlue, Color.SaddleBrown, Color.SaddleBrown.ChangeTone(-10),0.0001f);
            earth.Orbit(Vector3d.Zero,ScaleHelper.Millions(20), ScaleHelper.Millionths(0.1f));

            Planet moon = new Planet("moon", new Vector3d(ScaleHelper.Millions(20) + 20000, 0, 0),
             NoiseGenerator.FastPlanet(1000),
             effect,
             1000, Color.DarkRed.ChangeTone(10), Color.DarkRed, Color.DarkRed.ChangeTone(-10));
            moon.Orbit(earth, 20000, ScaleHelper.Millionths(100f));

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