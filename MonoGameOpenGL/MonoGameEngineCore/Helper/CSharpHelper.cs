using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Reflection;

namespace MonoGameEngineCore.Helper
{
    public static class CSharpHelper
    {
        public static void RemoveIfContains<T>(List<T> list, T item)
        {
            if (list.Contains(item))
                list.Remove(item);

        }

        public static bool AddIfNotAlreadyIn<T>(List<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }
            return false;

        }

        public static bool AddIfNotAlreadyIn<T>(HashSet<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }
            return false;

        }

        public static List<Color> GenerateFullColorList()
        {
            List<Color> predefinedColors = new List<Color>();
            // Get all of the public static properties

            PropertyInfo[] properties = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);

            foreach (PropertyInfo propertyInfo in properties)
            {
                // Check to make sure the property has a get method, and returns type "Color"
                if (propertyInfo.GetGetMethod() != null && propertyInfo.PropertyType == typeof(Color))
                {
                    // Get the color returned by the property by invoking it
                    Color color = (Color)propertyInfo.GetValue(null, null);

                    
                    predefinedColors.Add(color);
                }
            }

            predefinedColors = predefinedColors.OrderBy(x => x.Hue()).ToList();
          
            return predefinedColors;

        }

       
    }
}
        
    

