#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.Rendering;
using Vector.Screens;

#endregion

namespace Vector
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
            ScreenResolutionName resToUse = ScreenResolutionName.WXGA;
            SystemCore.ActiveColorScheme = ColorScheme.ColorSchemes["space"];

            if (System.Environment.MachineName == "NICKMCCREA-PC")
                resToUse = ScreenResolutionName.WUXGA;

            using (var game = new MonoEngineGame(typeof(MainMenuScreen), resToUse, DepthFormat.Depth24))
                game.Run();
        }
    }
#endif
}
