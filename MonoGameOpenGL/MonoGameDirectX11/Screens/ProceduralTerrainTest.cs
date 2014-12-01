using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using MonoGameEngineCore.Helper;
using System.Collections.Generic;

namespace MonoGameDirectX11.Screens
{
    public class ProceduralTerrainTest : MouseCamScreen
    {
        Effect effect;
        PlanetQuadTree planetQuadTreeOne;
        PlanetQuadTree planetQuadTreeTwo;
        List<GameObject> activeTerrainTiles = new List<GameObject>();
        int tileSize = 100;
        public ProceduralTerrainTest()
            : base()
        {
            effect = EffectLoader.LoadEffect("FlatShaded");


            AddTile(0, 0);
         

            DetermineTerrainPatches();


           // planetQuadTreeOne = new PlanetQuadTree(NoiseGenerator.RidgedMultiFractal(0.5f), effect, new Vector3(10, 10, 0), 10f);
            //planetQuadTreeTwo = new PlanetQuadTree(NoiseGenerator.RidgedMultiFractal(0.5f), effect, new Vector3(-40, 20, 10), 20f);

            for (int i = 0; i < 10; i++)
                AddAsteroid();

            mouseCamera.World.Translation = new Vector3(100, 20, 100);
        }

        private static void AddAsteroid()
        {
            var asteroid = NoiseGenerator.ProceduralAsteroid();
            var gameObject = GameObjectFactory.CreateRenderableGameObjectFromShape(asteroid, EffectLoader.LoadEffect("FlatShaded"));
            gameObject.Transform.Scale = RandomHelper.GetRandomFloat(1, 5);
            gameObject.Transform.SetPosition(RandomHelper.GetRandomVector3(new Vector3(-100, 0, -100), new Vector3(100, 50, 100)));
            gameObject.AddComponent(new RotatorComponent(RandomHelper.RandomNormalVector3, RandomHelper.GetRandomFloat(0, 1000) / 1000000f));
            gameObject.AddComponent(new RotatorComponent(RandomHelper.RandomNormalVector3, RandomHelper.GetRandomFloat(0, 1000) / 1000000f));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);
        }

        private void AddTile(double x, double y)
        {
            var heightMapTileOne = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.08f), tileSize, 1, 10f, x, y, 1f);
            ProceduralShape tile1 = new ProceduralShape(heightMapTileOne.GenerateVertexArray(), heightMapTileOne.GenerateIndices());
            tile1.Translate(new Vector3((float)x, 0, (float)y));
            tile1.SetColor(SystemCore.ActiveColorScheme.RandomColor());

            var go = GameObjectFactory.CreateRenderableGameObjectFromShape(tile1, effect);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(go);
            activeTerrainTiles.Add(go);

        }

        public override void Update(GameTime gameTime)
        {
          

            //planetQuadTreeOne.Update(SystemCore.ActiveCamera.Position);
            //planetQuadTreeTwo.Update(SystemCore.ActiveCamera.Position);

         

            if (SystemCore.Input.KeyPress(Microsoft.Xna.Framework.Input.Keys.T))
            {
                SystemCore.GameObjectManager.RemoveGameObjects(activeTerrainTiles);
                activeTerrainTiles.Clear();
                DetermineTerrainPatches();
          
            }
            base.Update(gameTime);
        }

        private void DetermineTerrainPatches()
        {
            int camX = (int)SystemCore.ActiveCamera.Position.X;
            int camY = (int)SystemCore.ActiveCamera.Position.Z;

            camX = MonoMathHelper.RoundToNearest(camX, 99);
            camY = MonoMathHelper.RoundToNearest(camY, 99);

            //all tiles surrounding the player's position
            int adjustedTileSize = tileSize - 1;

            AddTile(camX, camY);
            AddTile(camX + adjustedTileSize, camY);
            AddTile(camX - adjustedTileSize, camY);
            AddTile(camX, camY + adjustedTileSize);
            AddTile(camX, camY - adjustedTileSize);
            AddTile(camX + adjustedTileSize, camY + adjustedTileSize);
            AddTile(camX - adjustedTileSize, camY - adjustedTileSize);
            AddTile(camX + adjustedTileSize, camY - adjustedTileSize);
            AddTile(camX - adjustedTileSize, camY + adjustedTileSize);


        }

        public override void Render(GameTime gameTime)
        {

            base.Render(gameTime);
        }
    }
}