using Microsoft.Xna.Framework;
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
    public class EditorTest : Screen
    {
        private SimpleModelEditor modelEditor;
     

        public EditorTest()
        {
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();

            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(Color.LightBlue, Color.OrangeRed, Color.DarkBlue));


 
            modelEditor = new SimpleModelEditor(10);

            SystemCore.AddNewUpdateRenderSubsystem(modelEditor);

        }

        public override void Update(GameTime gameTime)
        {
            if (SystemCore.Input.MouseLeftPress())
            {
                modelEditor.AddTriangle(new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), Color.White);
                modelEditor.SaveCurrentShape("testShape");

                var shape = modelEditor.LoadShape("testShape");
                SystemCore.GameObjectManager.AddAndInitialiseGameObject(GameObjectFactory.CreateRenderableGameObjectFromShape(shape, EffectLoader.LoadSM5Effect("flatshaded")));
            }


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
