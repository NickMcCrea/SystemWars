using System;
using HelmsDeepSimulator.Screens;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.Rendering;

namespace HelmsDeepSimulator
{
#if WINDOWS || LINUX
    /// <summary>
    /// The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            SystemCore.ActiveColorScheme = ColorScheme.ColorSchemes["space"];

            ScreenResolutionName resToUse = ScreenResolutionName.WXGA;

            if (System.Environment.MachineName == "NICKMCCREA-PC")
                resToUse = ScreenResolutionName.WUXGA;

            using (var game = new MonoEngineGame(typeof(MainMenuScreen), resToUse, DepthFormat.Depth24, true, false))
                game.Run();
        }
    }
#endif
}
