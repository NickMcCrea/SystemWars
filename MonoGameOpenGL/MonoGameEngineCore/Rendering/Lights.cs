using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.Rendering
{
    public class Scene
    {
        public List<SceneLight> LightsInScene { get; set; }
        public AmbientLight AmbientLight { get; set; }
        public float FogC { get; set; }
        public float FogB { get; set; }
        public bool FogEnabled { get; set; }
        public Color FogColor { get; set; }

        public Scene()
        {
            LightsInScene = new List<SceneLight>();

            FogC = 0.5f;
            FogB = 0.015f;
            FogColor = new Color(0.5f, 0.6f, 0.7f);
            FogEnabled = true;

        }



        public void SetUpBasicAmbientAndKey()
        {
            LightsInScene.Clear();

            AmbientLight = new AmbientLight(Color.White, 0.2f);
            Vector3 lightDir = new Vector3(1, 1, 1f);
            lightDir.Normalize();
            LightsInScene.Add(new DiffuseLight(lightDir, Color.White, 0.5f));


        }

        public void SetUpAmbientAndFullLightingRig()
        {
            AmbientLight = new AmbientLight(Color.White, 0.1f);
            AddKeyLight(Vector3.Normalize(new Vector3(1, 1, 1)), Color.White, 0.5f, true);
            AddBackLight(Vector3.One, Color.White, 0.4f);
            AddFillLight(Vector3.Normalize(new Vector3(1, 1, 0)), Color.White, 0.2f);
        }

        public void AddPointLight(Vector3 pos, Color col, float fallOffStart, float fallOffEnd, float intensity, PointLightNumber num)
        {
            PointLight p = new PointLight(pos, col, fallOffStart, fallOffEnd, intensity);
            p.Number = num;
            LightsInScene.Add(p);
        }

        public void AddKeyLight(Vector3 dir, Color color, float intensity, bool shadowCasting)
        {
            DiffuseLight key = new DiffuseLight(dir, color, intensity);
            key.DiffuseType = DiffuseLightType.Key;
            key.IsShadowCasting = shadowCasting;
            LightsInScene.Add(key);
        }

        public void AddFillLight(Vector3 dir, Color color, float intensity)
        {
            DiffuseLight fill = new DiffuseLight(dir, color, intensity);
            fill.DiffuseType = DiffuseLightType.Fill;
            fill.IsShadowCasting = false;
            LightsInScene.Add(fill);
        }

        public void AddBackLight(Vector3 dir, Color color, float intensity)
        {
            DiffuseLight back = new DiffuseLight(dir, color, intensity);
            back.DiffuseType = DiffuseLightType.Back;
            back.IsShadowCasting = false;
            LightsInScene.Add(back);
        }

        public DiffuseLight GetBackLight()
        {
            return LightsInScene.Find(x => x.GetType() == typeof(DiffuseLight)
            && ((DiffuseLight)x).DiffuseType == DiffuseLightType.Back) as DiffuseLight;
        }
        public DiffuseLight GetFillLight()
        {
            return LightsInScene.Find(x => x.GetType() == typeof(DiffuseLight)
            && ((DiffuseLight)x).DiffuseType == DiffuseLightType.Fill) as DiffuseLight;
        }
        public DiffuseLight GetKeyLight()
        {
            return LightsInScene.Find(x => x.GetType() == typeof(DiffuseLight)
            && ((DiffuseLight)x).DiffuseType == DiffuseLightType.Key) as DiffuseLight;
        }

        public void SetDiffuseLightDir(int index, Vector3 lightDir)
        {
            if (LightsInScene[index] is DiffuseLight)
                ((DiffuseLight)LightsInScene[index]).LightDirection = lightDir;

        }

        public void ClearLights()
        {
            LightsInScene.Clear();
        }
    }

    public abstract class SceneLight
    {
        public Color LightColor { get; set; }
        public float LightIntensity { get; set; }

        public SceneLight(Color lightColor, float intensity)
        {
            LightColor = lightColor;
            LightIntensity = intensity;
        }
    }

    public enum DiffuseLightType
    {
        Key,
        Fill,
        Back
    }

    public enum PointLightNumber
    {
        One,
        Two,
        Three,
        Four
    }


    public class PointLight : SceneLight
    {
        public Vector3 Position { get; set; }
        public float FallOffStart { get; set; }
        public float FallOffEnd { get; set; }
        public PointLightNumber Number { get; set; }
        public PointLight(Vector3 position, Color color, float fallOffStart, float fallOffEnd, float intensity) : base(color, intensity)
        {
            Position = position;
            FallOffStart = fallOffStart;
            FallOffEnd = fallOffEnd;
            Number = PointLightNumber.One;

        }
    }

    public class DiffuseLight : SceneLight
    {


        public DiffuseLight(Vector3 lightDirection, Color lightColor, float intensity)
            : base(lightColor, intensity)
        {
            LightDirection = lightDirection;
            IsShadowCasting = true;
            DiffuseType = DiffuseLightType.Key;

        }
        public Vector3 LightDirection { get; set; }
        public bool IsShadowCasting { get; set; }
        public DiffuseLightType DiffuseType { get; set; }


    }

    public class AmbientLight : SceneLight
    {
        public AmbientLight(Color lightColor, float intensity)
            : base(lightColor, intensity)
        {
        }
    }
}
