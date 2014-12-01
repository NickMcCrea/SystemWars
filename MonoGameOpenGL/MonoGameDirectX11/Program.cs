#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.Rendering;

#endregion

namespace MonoGameDirectX11
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

            using (var game = new MonoEngineGame(typeof(MainMenuScreen), ScreenResolutionName.WUXGA, DepthFormat.Depth24))
                game.Run();
        }
    }
#endif
}
