using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameDirectX11.Screens
{
    class ProceduralTestScreen : TestScreen
    {
    
        public ProceduralTestScreen()
            : base()
        {
           

        }


        public override void OnInitialise()
        {
            base.OnInitialise();

            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(1, 1, 1));

            mouseCamera.moveSpeed = 0.01f;



            for (int i = 0; i < 50; i++)
                CreateCube();


            var heightMap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.1f), 100, 1, 3, 1, 1, 1);
            GameObject heightMapObject = new GameObject();
            ProceduralShape shape = new ProceduralShape(heightMap.GenerateVertexArray(), heightMap.GenerateIndices());
            shape.SetColor(Color.OrangeRed);
            RenderGeometryComponent renderGeom = new RenderGeometryComponent(shape);
            EffectRenderComponent renderComponent = new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded"));
            heightMapObject.AddComponent(renderGeom);
            heightMapObject.AddComponent(renderComponent);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(heightMapObject);

          
       
        }


        public override void OnRemove()
        {
           
            base.OnRemove();
        }



        private static GameObject CreateCube()
        {
            var cube = GameObjectFactory.CreateRenderableGameObjectFromShape(new ProceduralCube(), EffectLoader.LoadSM5Effect("flatshaded"));
            cube.AddComponent(new ShadowCasterComponent());
            cube.AddComponent(new RotatorComponent(Vector3.Up, 0.001f));
            cube.Transform.SetPosition(new Vector3(RandomHelper.GetRandomFloat(5, 100), RandomHelper.GetRandomFloat(2, 10), RandomHelper.GetRandomFloat(5, 100)));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cube);
            return cube;
        }

        public override void Update(GameTime gameTime)
        {
           
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.DarkGray);



            base.Render(gameTime);
        }
    }
}
