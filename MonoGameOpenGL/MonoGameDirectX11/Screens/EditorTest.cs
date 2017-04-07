using Microsoft.Xna.Framework;
using MonoGameDirectX11;
using MonoGameEngineCore;
using MonoGameEngineCore.Editor;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SystemWar;

namespace GridForgeResurrected.Screens
{
    public class Editor : Screen
    {
        private SimpleModelEditor modelEditor;
     

        public Editor()
        {
           
            

        }

        public override void OnInitialise()
        {
            modelEditor = new SimpleModelEditor(10);

            SystemCore.AddNewUpdateRenderSubsystem(modelEditor);
            base.OnInitialise();
        }

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            SystemCore.GameObjectManager.ClearAllObjects();
            input.ClearBindings();
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {

            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            base.Update(gameTime);
        }


        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Gray);

       
            DebugShapeRenderer.VisualiseAxes(5f);
      

            base.Render(gameTime);
        }
    }
}
