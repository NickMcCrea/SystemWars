using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.ScreenManagement;

namespace MonoGameDirectX11.Screens
{
    public class ShadowTestScreen : MouseCamScreen
    {
        private Model _model;
        private Model model2;
        private Matrix[] _modelTransforms;
        

        public ShadowTestScreen()
            : base()
        {

           


        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {

           

            SystemCore.GraphicsDevice.Clear(SystemCore.ActiveColorScheme.Color2);

     


            base.Render(gameTime);
        }
    }
}

