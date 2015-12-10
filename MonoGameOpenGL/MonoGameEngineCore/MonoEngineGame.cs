using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;
using BloomPostprocess;
using MonoGameEngineCore.Rendering.BloomPostProcess;

namespace MonoGameEngineCore
{
    public class MonoEngineGame : Game
    {
        private Type startScreenType;

        private static BloomComponent postProcessComponent;
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

            postProcessComponent = new BloomComponent(this);
            postProcessComponent.Settings = new BloomSettings(null, 0.8f, 0.2f, 2, 1, 1.5f, 1);
            postProcessComponent.DrawOrder = 1000;

            if (enableBloom)
                this.Components.Add(postProcessComponent);

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
                postProcessComponent.BeginDraw();
         
            SystemCore.Render(gameTime);

            base.Draw(gameTime);
           
        }



      
    }
}
