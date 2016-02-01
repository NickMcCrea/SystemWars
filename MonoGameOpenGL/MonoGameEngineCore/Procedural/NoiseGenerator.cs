using LibNoise;
using LibNoise.Modifiers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.Procedural
{

    public class PlanetParameters
    {
        public PlanetParameters(string name, float defaultValue)
        {
            Name = name;
            Value = defaultValue;
            DefaultValue = defaultValue;
        }

        public string Name { get; set; }
        public float Value { get; set; }
        public float DefaultValue { get; set; }

    }

    

    public class NoiseGenerator
    {
        public static IModule Perlin()
        {
            var module = new Perlin();
            module.Frequency = 0.08f;
            module.Lacunarity = 2;
            module.OctaveCount = 6;
            module.NoiseQuality = NoiseQuality.High;
            module.Seed = 0;
            return module;
        }

        public static IModule Billow()
        {
            var module = new Billow();
            module.Frequency = 0.08f;
            module.Lacunarity = 2;
            module.OctaveCount = 6;
            module.NoiseQuality = NoiseQuality.High;
            module.Seed = 0;
            return module;
        }

        public static IModule RidgedMultiFractal(float frequency)
        {
            var module = new FastRidgedMultifractal();
            ((FastRidgedMultifractal)module).Frequency = frequency;
            ((FastRidgedMultifractal)module).NoiseQuality = NoiseQuality.High;
            ((FastRidgedMultifractal)module).Seed = 0;
            ((FastRidgedMultifractal)module).OctaveCount = 6;
            ((FastRidgedMultifractal)module).Lacunarity = 2;
            return module;
        }

        public static Dictionary<string, PlanetParameters> miniPlanetParameters;

        static NoiseGenerator()
        {

            miniPlanetParameters = new Dictionary<string, PlanetParameters>();
            AddParameterToDictionary("continentFrequency", 0.0001f);
            AddParameterToDictionary("lowlandFrequency", 0.01f);
            AddParameterToDictionary("lowlandScale", 1);
            AddParameterToDictionary("lowlandBias", 2);
            AddParameterToDictionary("mountainBaseFrequency", 0.008f);
            AddParameterToDictionary("mountainScale", 1);
            AddParameterToDictionary("mountainBias", 1);
            AddParameterToDictionary("mountainTurbulencePower", 1);
            AddParameterToDictionary("mountainTurbulenceFrequency", 0.05f);
            AddParameterToDictionary("planetlandFilterFrequency", 0.0005f);
            AddParameterToDictionary("landEdgeFallOff", 0.1f);
            AddParameterToDictionary("oceanFrequency", 0.01f);
            AddParameterToDictionary("oceanBias", -2f);
            AddParameterToDictionary("oceanScale", 1);
            AddParameterToDictionary("finalEdgeFallOff", 0.01f);

        }

        private static void AddParameterToDictionary(string name, float defaultValue)
        {
            miniPlanetParameters.Add(name, new PlanetParameters(name, defaultValue));

        }
      
        public static IModule ParameterisedFastPlanet(float planetRadius, Dictionary<string,PlanetParameters> parameters)
        {
            int seed = RandomHelper.GetRandomInt(1000);
            FastNoise fastPlanetContinents = new FastNoise(seed);
            fastPlanetContinents.Frequency = parameters["continentFrequency"].Value;

            FastBillow fastPlanetLowlands = new FastBillow();
            fastPlanetLowlands.Frequency = parameters["lowlandFrequency"].Value;
            LibNoise.Modifiers.ScaleBiasOutput fastPlanetLowlandsScaled = new ScaleBiasOutput(fastPlanetLowlands);
            fastPlanetLowlandsScaled.Scale = parameters["lowlandScale"].Value;
            fastPlanetLowlandsScaled.Bias = parameters["lowlandBias"].Value;

            FastRidgedMultifractal fastPlanetMountainsBase = new FastRidgedMultifractal(seed);
            fastPlanetMountainsBase.Frequency = parameters["mountainBaseFrequency"].Value;

            //mountains scaled to 1000th the radius, and biased that amount upwards.
            ScaleBiasOutput fastPlanetMountainsScaled = new ScaleBiasOutput(fastPlanetMountainsBase);
            fastPlanetMountainsScaled.Scale = parameters["mountainScale"].Value;
            fastPlanetMountainsScaled.Bias = parameters["mountainBias"].Value;

            FastTurbulence fastPlanetMountains = new FastTurbulence(fastPlanetMountainsScaled);
            fastPlanetMountains.Power = parameters["mountainTurbulencePower"].Value;
            fastPlanetMountains.Frequency = parameters["mountainTurbulenceFrequency"].Value;

            FastNoise fastPlanetLandFilter = new FastNoise(seed + 1);
            fastPlanetLandFilter.Frequency = parameters["planetlandFilterFrequency"].Value;


            Select fastPlanetLand = new Select(fastPlanetLandFilter, fastPlanetLowlandsScaled, fastPlanetMountains);
            fastPlanetLand.SetBounds(0, planetRadius);
            fastPlanetLand.EdgeFalloff = parameters["landEdgeFallOff"].Value;

            FastBillow fastPlanetOceanBase = new FastBillow(seed);
            fastPlanetOceanBase.Frequency = parameters["oceanFrequency"].Value;
            ScaleBiasOutput biasOceanOutput = new ScaleBiasOutput(fastPlanetOceanBase);
            biasOceanOutput.Bias = parameters["oceanBias"].Value;
            biasOceanOutput.Scale = parameters["oceanScale"].Value;



            Select fastPlanetFinal = new Select(fastPlanetContinents, biasOceanOutput, fastPlanetLand);
            fastPlanetFinal.SetBounds(0, planetRadius);
            fastPlanetFinal.EdgeFalloff = parameters["finalEdgeFallOff"].Value;

            return fastPlanetFinal;
        }

        //biased for realistic scale planets of thousands of units radius
        public static IModule FastPlanet(float planetRadius)
        {
            int seed = RandomHelper.GetRandomInt(1000);
            FastNoise fastPlanetContinents = new FastNoise(seed);
            fastPlanetContinents.Frequency = 0.0001f;

            FastBillow fastPlanetLowlands = new FastBillow();
            fastPlanetLowlands.Frequency = 0.01;
            LibNoise.Modifiers.ScaleBiasOutput fastPlanetLowlandsScaled = new ScaleBiasOutput(fastPlanetLowlands);
            fastPlanetLowlandsScaled.Scale = 1;
            fastPlanetLowlandsScaled.Bias = 2;

            FastRidgedMultifractal fastPlanetMountainsBase = new FastRidgedMultifractal(seed);
            fastPlanetMountainsBase.Frequency = 0.008;

            //mountains scaled to 1000th the radius, and biased that amount upwards.
            ScaleBiasOutput fastPlanetMountainsScaled = new ScaleBiasOutput(fastPlanetMountainsBase);
            fastPlanetMountainsScaled.Scale = planetRadius / 400;
            fastPlanetMountainsScaled.Bias = planetRadius / 200;

            FastTurbulence fastPlanetMountains = new FastTurbulence(fastPlanetMountainsScaled);
            fastPlanetMountains.Power = 1;
            fastPlanetMountains.Frequency = 0.05;

            FastNoise fastPlanetLandFilter = new FastNoise(seed + 1);
            fastPlanetLandFilter.Frequency = 0.0005;


            Select fastPlanetLand = new Select(fastPlanetLandFilter, fastPlanetLowlandsScaled, fastPlanetMountains);
            fastPlanetLand.SetBounds(0, planetRadius);
            fastPlanetLand.EdgeFalloff = 0.1f;

            FastBillow fastPlanetOceanBase = new FastBillow(seed);
            fastPlanetOceanBase.Frequency = 0.01f;
            ScaleBiasOutput biasOceanOutput = new ScaleBiasOutput(fastPlanetOceanBase);
            biasOceanOutput.Bias = -2f;
            biasOceanOutput.Scale = 1f;



            Select fastPlanetFinal = new Select(fastPlanetContinents, biasOceanOutput, fastPlanetLand);
            fastPlanetFinal.SetBounds(0, planetRadius);
            fastPlanetFinal.EdgeFalloff = 0.01;

            return fastPlanetFinal;
        }


        public static IModule Voronoi(float frequency)
        {
            var module = new Voronoi();
            ((Voronoi)module).Frequency = frequency;
            return module;
        }



        public static ProceduralShape ProceduralAsteroid()
        {
            var module = new FastRidgedMultifractal();
            ((FastRidgedMultifractal)module).Frequency = 0.2f;
            ((FastRidgedMultifractal)module).NoiseQuality = NoiseQuality.Standard;
            ((FastRidgedMultifractal)module).Seed = RandomHelper.GetRandomInt(0, 5000);
            ((FastRidgedMultifractal)module).OctaveCount = 6;
            ((FastRidgedMultifractal)module).Lacunarity = 2;

            Sphere sphere = new Sphere(32, 32);

            VertexPositionColorTextureNormal[] verts = sphere.GenerateVertexArray();
            short[] ind = sphere.GenerateIndices();
            var p = verts.Select(x => x.Position).ToList();
            var s = BoundingSphere.CreateFromPoints(p);


            ProceduralShape asteroid = new ProceduralShape(verts, ind);
            return asteroid;

        }



        public static Heightmap CreateHeightMap(IModule noiseModule, int vertsPerSide, float horizontalScale, float verticalScale, double xSampleOffset, double ySampleOffset, double sampleStep)
        {
            var heightMap = new Heightmap(vertsPerSide, horizontalScale);
            heightMap.SetData(noiseModule, sampleStep, verticalScale, xSampleOffset, ySampleOffset);
            return heightMap;
        }
    }
}
