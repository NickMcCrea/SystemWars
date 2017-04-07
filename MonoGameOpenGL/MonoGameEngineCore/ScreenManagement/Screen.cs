using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;

namespace MonoGameEngineCore
{
    public abstract class Screen
    {
        protected InputManager input;
        protected FPSCounter fpsCounter;
        protected Label fpsLabel;

        protected Screen()
        {
            input = SystemCore.GetSubsystem<InputManager>();
            fpsCounter = SystemCore.GetSubsystem<FPSCounter>();

            fpsLabel = new Label(GUIFonts.Fonts["test"], "");
            fpsLabel.SetPosition(new Vector2(10, 10));
            fpsLabel.TextColor = SystemCore.ActiveColorScheme.Color1;
            fpsLabel.Visible = false;
            SystemCore.GUIManager.AddControl((fpsLabel));
            
        }

        

        public virtual void OnRemove()
        {

        }

        public virtual void OnInitialise()
        {

        }

        public virtual void Update(GameTime gameTime)
        {


        }

        public virtual void Render(GameTime gameTime)
        {

            fpsLabel.Text = fpsCounter.FPS.ToString();

        }

        public virtual void RenderSprites(GameTime gameTime)
        {

        }
    }


}