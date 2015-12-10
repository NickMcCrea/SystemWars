using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.GUI
{
    public class Palette
    {
        public Color MainColor { get; set; }
        public Color HighlightColor { get; set; }
        public Color SecondaryColor{get;set;}
        public Color BorderColor { get; set; }
        public Color TextColor { get; set; }
        public Color TextOutlineColor { get; set; }
        public bool TextOutline { get; set; }
        public bool Border { get; set; }
        public int BorderThickness { get; set; }
       
    }


    public class ColorScheme
    {

        public static Dictionary<string, ColorScheme> ColorSchemes;

        static ColorScheme()
        {
            ColorSchemes = new Dictionary<string, ColorScheme>();
            ColorScheme neutralBlue = new ColorScheme("neutralblue",
                new Color(252, 255, 245),
                new Color(209, 219, 189),
                new Color(145, 170, 157),
                new Color(62, 96, 111),
                 new Color(25, 52, 65));

            ColorSchemes.Add(neutralBlue.name, neutralBlue);

            AddColorScheme("firenze", 70, 137, 102, 255, 240, 165, 255, 176, 59, 182, 73, 38, 142, 40, 0);
            AddColorScheme("flatui", 44, 62, 80, 231, 76, 60, 236, 240, 241, 52, 152, 219, 41, 128, 185);
            AddColorScheme("space", 43, 68, 140, 42, 85, 140, 90, 184, 243, 160, 227, 242, 13, 13, 13);
            AddColorScheme("elgray", 30,30,31,66,65,67,103,102,106,128,127,131,203,201,207);
            AddColorScheme("grayyellow", 37, 38, 35, 99,97,89, 222,204,0,191,182,168,237,237,230);


        }

        static void AddColorScheme(string name, params int[] colorValues)
        {
          
                Color a = new Color(colorValues[0], colorValues[1], colorValues[2]);
                Color b = new Color(colorValues[3], colorValues[4], colorValues[5]);
                Color c = new Color(colorValues[6], colorValues[7], colorValues[8]);
                Color d = new Color(colorValues[9], colorValues[10], colorValues[11]);
                Color e = new Color(colorValues[12], colorValues[13], colorValues[14]);

                ColorScheme s = new ColorScheme(name, a, b, c, d, e);
                ColorSchemes.Add(name, s);
            
        }

        public static ColorScheme Random()
        {
            return new ColorScheme("random", RandomHelper.GetRandomColor(), RandomHelper.GetRandomColor(), RandomHelper.GetRandomColor(), RandomHelper.GetRandomColor(), RandomHelper.GetRandomColor());
            
        }

        public ColorScheme(string name, Color one, Color two, Color three, Color four, Color five)
        {
            this.name = name;
            Color1 = one;
            Color2 = two;
            Color3 = three;
            Color4 = four;
            Color5 = five;
        
        }

        public Color RandomColor()
        {
            int random = RandomHelper.GetRandomInt(5);

            if (random == 1)
                return Color1;
            if (random == 2)
                return Color2;
            if (random == 3)
                return Color3;
            if (random == 4)
                return Color4;
            if (random == 6)
                return Color5;

            return Color1;
        }

        public string name;
        public Color Color1;
        public Color Color2;
        public Color Color3;
        public Color Color4;
        public Color Color5;
    }

    
}
