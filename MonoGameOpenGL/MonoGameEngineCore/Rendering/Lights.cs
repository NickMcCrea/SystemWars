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

     

        public void SetUpDefaultAmbientAndDiffuseLights()
        {
            AmbientLight = new AmbientLight(Color.White, 0.2f);
            Vector3 lightDir = new Vector3(1,0,0f);
            lightDir.Normalize();
            LightsInScene.Add(new DiffuseLight(lightDir, Color.White, 1f));

       
        }

        public DiffuseLight GetDiffuseLight()
        {
            return LightsInScene[0] as DiffuseLight;
        }

        public void SetDiffuseLightDir(int index, Vector3 lightDir)
        {
            if (LightsInScene[index] is DiffuseLight)
                ((DiffuseLight)LightsInScene[index]).LightDirection = lightDir;

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
    
    public class DiffuseLight : SceneLight
    {
        public DiffuseLight(Vector3 lightDirection, Color lightColor, float intensity)
            : base(lightColor, intensity)
        {
            LightDirection = lightDirection;
            IsShadowCasting = true;
        }
        public Vector3 LightDirection { get; set; }
        public bool IsShadowCasting { get; set; }


    }
    
    public class AmbientLight : SceneLight
    {
        public AmbientLight(Color lightColor, float intensity)
            : base(lightColor, intensity)
        {
        }
    }
}
