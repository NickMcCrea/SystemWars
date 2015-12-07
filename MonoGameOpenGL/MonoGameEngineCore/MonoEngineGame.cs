using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Rendering;
using BloomPostprocess;

namespace MonoGameEngineCore
{
    public class MonoEngineGame : Game
    {
        private Type startScreenType;

        private static BloomComponent bloomComponent;
        private bool enableBloom = false;

        public MonoEngineGame(Type startScreenType, ScreenResolutionName resolution, DepthFormat preferreDepthFormat, bool isFixedTimeStep, bool physicsOnBackgroundThread)
            : base()
        {

            
            this.startScreenType = startScreenType;
            Content.RootDirectory = "Content";
            SystemCore.Startup(this, Content, resolution, preferreDepthFormat,isFixedTimeStep, physicsOnBackgroundThread);
            SystemCore.CursorVisible = true;
          
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
           
            SystemCore.InitialiseGameSystems();

            bloomComponent = new BloomComponent(this);
            bloomComponent.Settings = new BloomSettings(null, 0.25f, 4, 2, 1, 1.5f, 1);
            bloomComponent.DrawOrder = 1000;

            if (enableBloom)
                this.Components.Add(bloomComponent);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Screen menu = Activator.CreateInstance(startScreenType) as Screen;
            SystemCore.ScreenManager.AddAndSetActive(menu);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {

        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
         

            SystemCore.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (enableBloom)
                bloomComponent.BeginDraw();
         
            SystemCore.Render(gameTime);

            base.Draw(gameTime);
           
        }



      
    }
}
