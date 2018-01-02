using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidWar.Gameplay
{
    class EnemyManager
    {

        XNATimer xnaTimer;
        private GameSimulation simulation;

        public EnemyManager(GameSimulation simulation)
        {
            this.simulation = simulation;
            xnaTimer = new XNATimer(1000, SpawnSimpleEnemy);
        }

        private void SpawnSimpleEnemy(GameTime gameTime)
        {
            SimpleEnemy e = new SimpleEnemy();
            e.Transform.AbsoluteTransform.Translation = new Vector3(RandomHelper.GetRandomFloat(-200, 200), 2, RandomHelper.GetRandomFloat(-200, 200));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(e);
            e.DesiredPosiiton = simulation.MainBase.Transform.AbsoluteTransform.Translation + new Vector3(0, 2, 0);
        }

        public void Update(GameTime gameTime)
        {
            xnaTimer.Update(gameTime);   
        }

        internal void RemoveAll()
        {
            
        }
    }
}
