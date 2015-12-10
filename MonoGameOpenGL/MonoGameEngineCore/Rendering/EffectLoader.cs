using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.Rendering
{
    public static class EffectLoader
    {
        private static string sm5effectPath = "Effects/SM5.0/";
        private static string effectPath = "Effects/";


        public static Effect LoadSM5Effect(string name)
        {
            return SystemCore.ContentManager.Load<Effect>(EffectLoader.sm5effectPath + name);
        }

        public static Effect LoadEffect(string name)
        {
            return SystemCore.ContentManager.Load<Effect>(EffectLoader.effectPath + name);
        }

    }
}
