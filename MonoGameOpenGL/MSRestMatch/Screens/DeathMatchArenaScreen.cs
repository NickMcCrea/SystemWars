using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Rendering.Camera;
using MSRestMatch.GameServer;
using System;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace MSRestMatch
{
    internal class DeathMatchArenaScreen : Screen
    {
        GameObject cameraObject;
        WebServiceHost host;

        public DeathMatchArenaScreen()
        {

        }

        public override void OnInitialise()
        {
            GameSimRules rules = new GameSimRules();
            rules.FragWinLimit = 20;
            rules.RespawnTime = 5;
            rules.GameTimeLimit = 300;

            var gameSim = new GameSimulation(rules);
            SystemCore.AddNewUpdateRenderSubsystem(gameSim);
            SystemCore.CursorVisible = true;
            SystemCore.ActiveScene.SetUpAmbientAndFullLightingRig();

            cameraObject = new GameObject();
            cameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, 200f, false));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            cameraObject.Transform.AbsoluteTransform = Matrix.CreateWorld(new Vector3(0, 200, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1));

          

            host = WebHostHelper.CreateWebHost(gameSim);


            base.OnInitialise();
        }

   
       

        public override void OnRemove()
        {
            SystemCore.GUIManager.ClearAllControls();
            host.Close();
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            float currentHeight = cameraObject.Transform.AbsoluteTransform.Translation.Y;
            cameraObject.Transform.Translate(new Vector3(0, -input.ScrollDelta / 10f, 0));

            float cameraSpeed = 1f;

            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                cameraObject.Transform.Translate(new Vector3(0, 0, cameraSpeed));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                cameraObject.Transform.Translate(new Vector3(0, 0, -cameraSpeed));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                cameraObject.Transform.Translate(new Vector3(cameraSpeed, 0, 0));
            if (input.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                cameraObject.Transform.Translate(new Vector3(-cameraSpeed, 0, 0));

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {


            base.Render(gameTime);
        }
    }
}