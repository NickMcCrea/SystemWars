using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameEngineCore.GUI
{
    public static class GUITexture
    {
       
        public static Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        public static Texture2D BlankTexture
        {
            get { return Textures["blank"]; }
        }

   
    }
}
