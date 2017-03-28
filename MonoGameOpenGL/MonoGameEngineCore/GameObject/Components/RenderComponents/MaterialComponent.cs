using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameEngineCore.GameObject.Components.RenderComponents
{

    public class MaterialComponent : IComponent
    {
        public string MaterialName { get; set; }
        public Color MatColor { get; set; }
        public float MatColorIntensity { get; set; }
        public string TextureName { get; set; }
        public float TextureIntensity { get; set; }
        public Color SpecularColor { get; set; }
        public float SpecularIntensity { get; set; }
        public float Shininess { get; set; }
        public string ShaderName { get; set; }

        //Material defines texture, colour and specularity

        public GameObject ParentObject
        {
            get;set;
        }

        public MaterialComponent()
        {

        }

        public void Initialise()
        {
           
        }


        
        
    }

    public static class MaterialFactory
    {
        private static Dictionary<string, MaterialComponent> matDictionary;

        static MaterialFactory()
        {
            matDictionary = new Dictionary<string, MaterialComponent>();



            MaterialComponent orangeGloss = new MaterialComponent();
            orangeGloss.MaterialName = "OrangeGloss";
            orangeGloss.MatColor = Color.Orange;
            orangeGloss.MatColorIntensity = 0.4f;
            orangeGloss.SpecularColor = Color.White;
            orangeGloss.Shininess = 200;
            orangeGloss.SpecularIntensity = 1;
            orangeGloss.TextureName = "";
            orangeGloss.ShaderName = "DiffuseSpecularTextured";
            matDictionary.Add(orangeGloss.MaterialName, orangeGloss);

            MaterialComponent redGloss = new MaterialComponent();
            redGloss.MaterialName = "RedGloss";
            redGloss.MatColor = Color.Red;
            redGloss.MatColorIntensity = 0.4f;
            redGloss.SpecularColor = Color.White;
            redGloss.Shininess = 200;
            redGloss.SpecularIntensity = 1;
            redGloss.TextureName = "";
            redGloss.ShaderName = "DiffuseSpecularTextured";
            matDictionary.Add(redGloss.MaterialName, redGloss);

            MaterialComponent crateMat = new MaterialComponent();
            crateMat.MaterialName = "WoodenCrate";
            crateMat.MatColor = Color.White;
            crateMat.SpecularColor = Color.White;
            crateMat.Shininess = 0;
            crateMat.SpecularIntensity = 0;
            crateMat.TextureName = "";
            crateMat.ShaderName = "DiffuseSpecularTextured";
            matDictionary.Add(crateMat.MaterialName, crateMat);

            MaterialComponent marioMat = new MaterialComponent();
            marioMat.MaterialName = "Mario";
            marioMat.MatColor = Color.White;
            marioMat.MatColorIntensity = 0f;
            marioMat.SpecularColor = Color.White;
            marioMat.Shininess = 200;
            marioMat.SpecularIntensity = 1;
            marioMat.TextureName = "Textures/marioD";
            marioMat.TextureIntensity = 10f;
            marioMat.ShaderName = "DiffuseSpecularTextured";
            matDictionary.Add(marioMat.MaterialName, marioMat);


        }

        public static void ApplyMaterialComponent(GameObject go, string materialName)
        {
            var mat = matDictionary[materialName];
            Effect effect = EffectLoader.LoadSM5Effect(mat.ShaderName).Clone();
            EffectRenderComponent effectComponent = new EffectRenderComponent(effect);
            go.AddComponent(mat);
            go.AddComponent(effectComponent);
        }
    }

}
