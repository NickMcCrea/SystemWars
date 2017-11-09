using MonoGameEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GameObject.Components.RenderComponents;
using Microsoft.Xna.Framework.Graphics;

namespace MSRestMatch.GameServer
{
    class GameSimulation : IGameSubSystem
    {
        public bool TrainingMode { get; set; }

        public GameSimulation()
        {
          
        }

        public void Initalise()
        {
            //AddPlayer(new Vector3(-100,0,0), "Nick", Color.Red);
            // AddPlayer(new Vector3(0, 0, 0), "Jim", Color.Blue);
            //AddPlayer(new Vector3(0,0,10), "Neil", Color.Orange);
            //AddPlayer(new Vector3(10,0,10), "Arran", Color.Blue);
           
        }

        public void OnRemove()
        {

        }

        public void Render(GameTime gameTime)
        {
            var playerList = SystemCore.GameObjectManager.GetAllObjects().FindAll(x => x is Player);

            foreach (Player p in playerList)
            {
                DebugShapeRenderer.AddUnitSphere(p.Transform.AbsoluteTransform.Translation, p.PlayerColor);

                DebugShapeRenderer.AddLine(p.Transform.AbsoluteTransform.Translation,
                    p.Transform.AbsoluteTransform.Translation + p.Transform.AbsoluteTransform.Forward * 1.5f, p.PlayerColor);

            }
        }

        public void Update(GameTime gameTime)
        {

        }

        public void AddPlayer(Vector3 startPos, string playerName)
        {
            Player p = new Player(RandomHelper.RandomColor);
            p.Transform.AbsoluteTransform.Translation = startPos;
            p.Name = playerName;
            p.DesiredPosition = startPos;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(p);

        
        }

        internal void CreateFloor(int x, int y, float scale)
        {

            for (int i = -x / 2; i < x / 2; i++)
            {
                for (int j = -y / 2; j < y / 2; j++)
                {
                    GameObject o = new GameObject();
                    o.AddComponent(new ModelComponent(SystemCore.ContentManager.Load<Model>("Models/Sci-Fi-Floor-1-OBJ")));
                    MaterialFactory.ApplyMaterialComponent(o, "Grid");
                    o.AddComponent(new ShadowCasterComponent());
                    o.Transform.Scale = scale;
                    o.Transform.Translate(new Vector3(i * scale*2, -5, j * scale*2));
                    SystemCore.GameObjectManager.AddAndInitialiseGameObject(o);
                }
            }


            //GameObject gameObject = new GameObject();
            //gameObject.AddComponent(new ModelComponent(SystemCore.ContentManager.Load<Model>("Models/Sci-Fi-Floor-1-OBJ")));
            //MaterialFactory.ApplyMaterialComponent(gameObject, "SciFiFloor");
            //gameObject.AddComponent(new ShadowCasterComponent());
            //gameObject.Transform.Scale = 10f;
            //gameObject.Transform.Translate(new Vector3(0, -5, 0));
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);

            //GameObject gameObject2 = new GameObject();
            //gameObject2.AddComponent(new ModelComponent(SystemCore.ContentManager.Load<Model>("Models/Sci-Fi-Floor-1-OBJ")));
            //MaterialFactory.ApplyMaterialComponent(gameObject2, "SciFiFloor");
            //gameObject2.AddComponent(new ShadowCasterComponent());
            //gameObject2.Transform.Scale = 10f;
            //gameObject2.Transform.Translate(new Vector3(20, -5, 0));
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject2);
        }

        internal void CreateTrainingArena()
        {
           
        }



        internal void AddTrainingDummy()
        {
            Player p = new Player(RandomHelper.RandomColor);
            p.Name = "TrainingDummy";
            p.AddComponent(new TrainingDummyComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(p);
        }

        public void RemovePlayer(string name)
        {
            GameObject o = SystemCore.GameObjectManager.GetObject(name);
            SystemCore.GameObjectManager.RemoveObject(o);
        }

        public void RemovePlayer(int id)
        {
            GameObject o = SystemCore.GameObjectManager.GetObject(id);
            if (o != null)
                SystemCore.GameObjectManager.RemoveObject(o);
        }

        internal void AddCombatDummy()
        {
            Player p = new Player(RandomHelper.RandomColor);
            p.Name = "CombatDummy";
            p.AddComponent(new TrainingDummyComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(p);
        }
    }
}
