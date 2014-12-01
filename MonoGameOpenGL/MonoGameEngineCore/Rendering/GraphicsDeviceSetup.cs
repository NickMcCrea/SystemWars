using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameEngineCore.Rendering
{
    public enum ScreenResolutionName
    {
        SVGA,
        WXGA,
        WUXGA,
    }

    public struct ScreenResolution
    {
        public ScreenResolutionName Name { get; set; }
        public int width;
        public int height;
    }

    public class GraphicsDeviceSetup
    {

        public static GraphicsDeviceManager SetupDisplay(Game game, ScreenResolutionName resolutionName, bool fullScreen, DepthFormat preferreDepthFormat)
        {
            GraphicsDeviceManager gd = new GraphicsDeviceManager(game);
            //gd.PreferredDepthStencilFormat = preferreDepthFormat;
            var resolution = CreateResolution(resolutionName);
            gd.PreferredBackBufferHeight = resolution.height;
            gd.PreferredBackBufferWidth = resolution.width;
            gd.PreferMultiSampling = true;
            gd.IsFullScreen = fullScreen;
            gd.ToggleFullScreen();
            gd.ApplyChanges();
            return gd;
        }


        private static ScreenResolution CreateResolution(ScreenResolutionName resolutionName)
        {
            switch (resolutionName)
            {
                case( ScreenResolutionName.SVGA):
                    return new ScreenResolution() { Name = ScreenResolutionName.SVGA, height = 600, width = 800 };
               
                case(ScreenResolutionName.WUXGA):
                    return new ScreenResolution() { Name = ScreenResolutionName.WUXGA, height = 1080, width = 1920 };
                 
                case(ScreenResolutionName.WXGA):
                    return new ScreenResolution() { Name = ScreenResolutionName.WXGA, height =720 , width = 1280 };
                  

                default:
                    return new ScreenResolution() { Name = ScreenResolutionName.SVGA, height = 600, width = 800 };
                   
            }
        }



    }

   
}
