using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BloomPostprocess;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameEngineCore.Rendering.BloomPostProcess
{
    public class PostProcessComponent : DrawableGameComponent
    {
        SpriteBatch spriteBatch;
        RenderTarget2D sceneRenderTarget;
        public Effect Effect { get; private set; }

        public PostProcessComponent(Game game, Effect postProcessEffect) : base(game)
        {
            Effect = postProcessEffect;
        }

        protected override void LoadContent()
        {     // Look up the resolution and format of our main backbuffer.
            PresentationParameters pp = GraphicsDevice.PresentationParameters;
            SurfaceFormat format = pp.BackBufferFormat;

            // Create a texture for rendering the main scene, prior to applying bloom.
            sceneRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false,
                format, pp.DepthStencilFormat, pp.MultiSampleCount,
                RenderTargetUsage.DiscardContents);

            spriteBatch = new SpriteBatch(GraphicsDevice);

         
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            sceneRenderTarget.Dispose();
            base.UnloadContent();
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void BeginDraw()
        {
            if (Visible)
            {
                GraphicsDevice.SetRenderTarget(sceneRenderTarget);
            }
        }

        public override void Draw(GameTime gameTime)
        {


            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

          
            GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(0, BlendState.Opaque, null, null, null, Effect);
            spriteBatch.Draw(sceneRenderTarget, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();


            base.Draw(gameTime);
        }
    }
}
