using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidWar.Gameplay
{
    class GameSimulation
    {
        public EnemyManager EnemyManager { get; private set; }
        public MainBase MainBase { get; private set; }

        public GameSimulation()
        {
           
        }

        public void InitaliseSimulation()
        {
            EnemyManager = new EnemyManager(this);
            CreateTerrain();
            CreateMainBase();
        }

        public void RemoveSimulation()
        {
            SystemCore.GameObjectManager.RemoveObject(MainBase);
            EnemyManager.RemoveAll();
        }

        private void CreateTerrain()
        {
            //world setup
            Heightmap heightMap = new Heightmap(100, 5);
            var terrainObject = heightMap.CreateTranslatedRenderableHeightMap(Color.OrangeRed, EffectLoader.LoadSM5Effect("flatshaded"), new Vector3(-250, 0, -250));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrainObject);
        }

        private void CreateMainBase()
        {

            MainBase = new MainBase();
            MainBase.Position = Vector3.Zero;
            
        }

        public void Update(GameTime gameTime)
        {
            EnemyManager.Update(gameTime);
        }
    }
}
