using MonoGameEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;

namespace MSRestMatch.GameServer
{
    class GameSimulation : IGameSubSystem
    {


        public GameSimulation()
        {

        }

        public void Initalise()
        {
           // AddPlayer(new Vector3(-100,0,0), "Nick", Color.Red);
           // AddPlayer(new Vector3(0, 0, 0), "Jim", Color.Blue);
            //AddPlayer(new Vector3(0,0,10), "Neil", Color.Orange);

            //AddPlayer(new Vector3(10,0,10), "Arran", Color.Blue);

        }

        public void OnRemove()
        {

        }

        public void Render(GameTime gameTime)
        {
           
        }

        public void Update(GameTime gameTime)
        {

        }

        public void AddPlayer(Vector3 startPos, string playerName, Color? color = null)
        {
            Player p = new Player();
            p.Transform.AbsoluteTransform.Translation = startPos;
            p.Name = playerName;
            p.DesiredPosition = startPos;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(p);

            if (color.HasValue)
                p.PlayerColor = color.Value;
        }

        public void RemovePlayer(string name)
        {
            GameObject o = SystemCore.GameObjectManager.GetObject(name);
            SystemCore.GameObjectManager.RemoveObject(o);
        }
    }
}
