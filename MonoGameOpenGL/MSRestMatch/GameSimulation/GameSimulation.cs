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
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace MSRestMatch.GameServer
{
    public class GameSimRules
    {
        public int FragWinLimit { get; set; }
        public int RespawnTime { get; set; }
        public int GameTimeLimit { get; set; }

        public GameSimRules()
        {
            FragWinLimit = 10;
            RespawnTime = 5;
            GameTimeLimit = 300;
        }
    }

    public class GameTile
    {
        public Vector3 Center { get; set; }
        public float Scale { get; set; }
        public List<GameTile> Neighbours { get; set; }

        public GameTile()
        {
            Neighbours = new List<GameTile>();
        }
    }

    class GameSimulation : IGameSubSystem
    {
        public bool TrainingMode { get; set; }
        public Dictionary<Player, DateTime> deadPlayers;
        private GameSimRules ruleSet;

        public List<GameTile> gameTiles;

        public GameSimulation(GameSimRules ruleset)
        {
            deadPlayers = new Dictionary<Player, DateTime>();
            ruleSet = ruleset;
            gameTiles = new List<GameTile>();
        }

        public void Initalise()
        {
            AddPlayer(Vector3.Zero, "Nick", true);

        }

        public void OnRemove()
        {

        }

        public static List<GameObject> GetLivingPlayers()
        {
            return SystemCore.GameObjectManager.GetAllObjects().FindAll(x => x is Player && !((Player)x).Dead);
        }

        public static List<GameObject> GetDeadPlayers()
        {
            return SystemCore.GameObjectManager.GetAllObjects().FindAll(x => x is Player && ((Player)x).Dead);
        }

        public void Render(GameTime gameTime)
        {
            var playerList = SystemCore.GameObjectManager.GetAllObjects().FindAll(x => x is Player && !((Player)x).Dead);

            foreach (Player p in playerList)
            {
                DebugShapeRenderer.AddUnitSphere(p.Transform.AbsoluteTransform.Translation, p.PlayerColor);

                DebugShapeRenderer.AddLine(p.Transform.AbsoluteTransform.Translation,
                    p.Transform.AbsoluteTransform.Translation + p.Transform.AbsoluteTransform.Forward * 1.5f, p.PlayerColor);

            }

            //foreach (GameTile t in gameTiles)
            //{
            //    foreach (GameTile n in t.Neighbours)
            //    {
            //        DebugShapeRenderer.AddLine(t.Center, n.Center, Color.Blue);
            //    }
            //}
        }

        public void Update(GameTime gameTime)
        {
            //find all the dead players
            var players = SystemCore.GameObjectManager.GetAllObjects().FindAll(x => x is Player);

            foreach (Player p in players)
            {
                if (p.Dead)
                {
                    if (!deadPlayers.ContainsKey(p))
                    {
                        deadPlayers.Add(p, DateTime.Now);
                        p.DisablePlayer();
                    }
                }
            }

            var deadPlayerList = deadPlayers.Keys.ToList();
            foreach (Player p in deadPlayerList)
            {
                DateTime deathTime = deadPlayers[p];

                TimeSpan timeSinceDeath = DateTime.Now - deathTime;
                if (timeSinceDeath.TotalSeconds > ruleSet.RespawnTime)
                {
                    p.Respawn();
                    deadPlayers.Remove(p);
                }


            }
        }

        public void AddPlayer(Vector3 startPos, string playerName, bool manualControl = false)
        {
            Player p = new Player(RandomHelper.RandomColor, manualControl);
            p.Transform.AbsoluteTransform.Translation = startPos;
            p.Name = playerName;
            p.DesiredPosition = startPos;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(p);


        }

        internal void CreateTrainingArena(int x, int y, float scale)
        {
            for (float i = -x / 2; i < x / 2; i++)
            {
                for (float j = -y / 2; j < y / 2; j++)
                {
                    var pos = new Vector3(i * scale + (scale / 2), -2, j * scale + (scale / 2));

                    GameTile t = new GameTile();
                    t.Center = pos;
                    t.Scale = scale;
                    gameTiles.Add(t);
                   
                }
            }

            GenerateFloorObjectFromFloorShapes(gameTiles);
            GenerateConnectivity();
        }

        private void GenerateConnectivity()
        {
            foreach (GameTile t in gameTiles)
            {
                foreach (GameTile o in gameTiles)
                {
                    if (t == o)
                        continue;

                    float distance = (t.Center - o.Center).Length();
                    if (distance == 10)
                    {
                        t.Neighbours.Add(o);
                        o.Neighbours.Add(t);
                    }
                }
            }
        }

        private void GenerateFloorObjectFromFloorShapes(List<GameTile> tiles)
        {


            GameObject finalFloorShape = new GameObject();
            ProceduralShape finalShape = new ProceduralShape();

            ProceduralPlane plane = new ProceduralPlane();
            plane.SetColor(Color.LightGray.ChangeTone(RandomHelper.GetRandomInt(-20, 20)));
            plane.Scale(tiles[0].Scale);
            plane.Translate(tiles[0].Center);


            finalShape = plane;
            AddLineBatch(plane);

         
            for (int i = 1; i < tiles.Count; i++)
            {
                ProceduralPlane newPlane = new ProceduralPlane();
                newPlane.SetColor(Color.LightGray.ChangeTone(RandomHelper.GetRandomInt(-20, 20)));
                newPlane.Scale(tiles[i].Scale);
                newPlane.Translate(tiles[i].Center);
                finalShape = ProceduralShape.Combine(finalShape, newPlane);
                AddLineBatch(newPlane);
               
            }

            finalFloorShape.AddComponent(new RenderGeometryComponent(finalShape));
            finalFloorShape.AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            finalFloorShape.AddComponent(new ShadowCasterComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(finalFloorShape);

        }

        private static void AddLineBatch(ProceduralPlane proceduralPlane)
        {
            LineBatch l = new LineBatch(proceduralPlane.GetLineBatchVerts().ToArray());
            for (int i = 0; i < l.Vertices.Count(); i++)
            {
                l.Vertices[i].Position = l.Vertices[i].Position.ReplaceYComponent(-1.9f);
            }

            GameObject lineObject = SystemCore.GameObjectManager.AddLineBatchToScene(l);

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
            p.AddComponent(new CombatDummyComponent());
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(p);
        }
    }
}
