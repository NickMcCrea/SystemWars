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

        public Scene()
        {
            LightsInScene = new List<SceneLight>();
        }

        public void AddAmbientLight(AmbientLight ambientLight)
        {
            AmbientLight = ambientLight;
        }

        public void SetUpDefaultAmbientAndDiffuseLights()
        {
            AddAmbientLight(new AmbientLight(Color.White, 0.1f));
            Vector3 lightDir = new Vector3(0.5f, -1, 0.7f);
            lightDir.Normalize();
            LightsInScene.Add(new DiffuseLight(lightDir, Color.LightYellow, 0.2f));
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
    public abstract class PositionalLight : SceneLight
    {
        protected PositionalLight(Vector3 lightPosition, Color lightColor, float intensity)
            : base(lightColor, intensity)
        {
            LightPosition = lightPosition;
        }

        public Vector3 LightPosition { get; set; }
    }
    public class PointLight : PositionalLight
    {
        public PointLight(Vector3 lightPosition, Color lightColor, float intensity, float range)
            : base(lightPosition, lightColor, intensity)
        {
            Range = range;
        }

        public float Range { get; set; }
    }
    public class DiffuseLight : SceneLight
    {
        public DiffuseLight(Vector3 lightDirection, Color lightColor, float intensity)
            : base(lightColor, intensity)
        {
            LightDirection = lightDirection;
        }

        public Vector3 LightDirection { get; set; }
    }
    public class DirectionalLight : PositionalLight
    {
        public DirectionalLight(Vector3 lightposition, Vector3 lightDir, Color lightColor, float intensity)
            : base(lightposition, lightColor, intensity)
        {
            LightDirection = lightDir;
        }

        public Vector3 LightDirection { get; set; }
    }
    public class AmbientLight : SceneLight
    {
        public AmbientLight(Color lightColor, float intensity)
            : base(lightColor, intensity)
        {
        }
    }
}
