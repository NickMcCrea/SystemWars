﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.Audio;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.Helper;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using BloomPostprocess;

namespace MonoGameEngineCore
{
    public interface IGameSubSystem
    {
        void Initalise();
        void Update(GameTime gameTime);
        void Render(GameTime gameTime);
    }

    public interface IGameComponent
    {
        
    }


    public class SystemCore
    {
        public static bool EscapeQuitsGame = true;
        public static Viewport Viewport { get; private set; }
        public static InputManager Input { get; private set; }
        public static ScreenManager ScreenManager { get; private set; }
        public static GraphicsDeviceManager GraphicsDeviceManager { get; private set; }
        public static GraphicsDevice GraphicsDevice { get; private set; }
        public static ContentManager ContentManager { get; private set; }
        public static GUIManager GUIManager { get; private set; }
        
        public static Space PhysicsSimulation { get; private set; }
        public static GameObjectManager GameObjectManager { get; private set; }
        public static AudioManager AudioManager { get; private set; }
        public static ColorScheme ActiveColorScheme { get; set; }
        public static Game Game { get; private set; }
        public static bool Wireframe { get; set; }
        public static bool CursorVisible { get; set; }
        public static Scene ActiveScene { get; set; }
        public static bool PhysicsOnBackgroundThread = false;
        private static  Dictionary<string, ICamera> cameras;  
        private static List<IGameSubSystem> gameSubSystems;
        private static List<IGameComponent> gameComponents;
        private static DateTime physicsLastUpdate = DateTime.Now;
        public static bool GameExiting;
        public static bool EnableBloom { get; set; }
        private static BloomComponent bloomComponent;

        public static void Startup(Game game, ContentManager content, ScreenResolutionName screenRes, DepthFormat preferreDepthFormat, bool isFixedTimeStep, bool physicsOnBackgroundThread)
        {
            SystemCore.GraphicsDeviceManager = GraphicsDeviceSetup.SetupDisplay(game, screenRes, false, preferreDepthFormat, isFixedTimeStep); ;     
            SystemCore.ContentManager = content;
            SystemCore.Game = game;
            SystemCore.PhysicsOnBackgroundThread = physicsOnBackgroundThread;
            cameras = new Dictionary<string, ICamera>();

            EnableBloom = false;
        }

      
        public static void InitialiseGameSystems()
        {
            SystemCore.GraphicsDevice = SystemCore.GraphicsDeviceManager.GraphicsDevice;

            DebugShapeRenderer.Initialize(SystemCore.GraphicsDevice);
            SystemCore.GraphicsDeviceManager.ApplyChanges();

            InstantiateSystems();

            SystemCore.Viewport = SystemCore.GraphicsDevice.Viewport;
            SystemCore.ScreenManager = GetSubsystem<ScreenManager>();
            SystemCore.GUIManager = GetSubsystem<GUIManager>();
            SystemCore.GameObjectManager = GetSubsystem<GameObjectManager>();
            SystemCore.AudioManager = GetSubsystem<AudioManager>();
            SystemCore.Input = GetSubsystem<InputManager>();
            SystemCore.Game.Exiting += (x, y) => { GameExiting = true; };

            PhysicsSimulation = new Space();

            if (PhysicsOnBackgroundThread)
            {
                PhysicsSimulation.SpaceObjectBuffer.Enabled = true;
                Thread t = new Thread(PhysicsUpdate);
                t.IsBackground = true;
                t.Start();
            }

            foreach (var gameSubSystem in gameSubSystems)
            {
                gameSubSystem.Initalise();
            }

            SystemCore.SetActiveCamera(new DummyCamera(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.3f,
                1000.0f));

            DebugText.InjectDebugFont(GUIFonts.Fonts["test"]);
            DebugText.InjectGraphicsDevice(SystemCore.GraphicsDevice);

            bloomComponent = new BloomComponent(Game);
            bloomComponent.Enabled = EnableBloom;
            bloomComponent.Visible = EnableBloom;
            bloomComponent.DrawOrder = 1000;
            Game.Components.Add(bloomComponent);
        }

        public static void AddNewUpdateRenderSubsystem(IGameSubSystem newSystem)
        {
            newSystem.Initalise();
            gameSubSystems.Add(newSystem);
        }

        public static void AddNewGameComponent(IGameComponent newComponent)
        {
            gameComponents.Add(newComponent);
        }

        private static void InstantiateSystems()
        {
            gameComponents = new List<IGameComponent>();
            gameSubSystems = new List<IGameSubSystem>();
            gameSubSystems.Add(new InputManager());
            gameSubSystems.Add(new ScreenManager());
            gameSubSystems.Add(new GameObjectManager());
            gameSubSystems.Add(new FPSCounter());
            gameSubSystems.Add(new GUIManager());
            gameSubSystems.Add(new AudioManager());
            ActiveScene = new Scene();


        }

        public static void Update(GameTime gameTime)
        {
            for (int i = 0; i < gameSubSystems.Count; i++)
            {
                gameSubSystems[i].Update(gameTime);
            }

            if (CursorVisible)
            {
                if (!SystemCore.Game.IsMouseVisible)
                    SystemCore.Game.IsMouseVisible = true;
            }
            else
            {
                if (SystemCore.Game.IsMouseVisible)
                    SystemCore.Game.IsMouseVisible = false;
            }


            if(SystemCore.EscapeQuitsGame)
                if(SystemCore.Input.KeyPress(Keys.Escape))
                    SystemCore.Game.Exit();

            if (!PhysicsOnBackgroundThread)
                PhysicsSimulation.Update((float) gameTime.ElapsedGameTime.TotalSeconds);

            DebugText.Update(gameTime);

            bloomComponent.Enabled = EnableBloom;
            bloomComponent.Visible = EnableBloom;

        }

        public static void PhysicsUpdate()
        {
            while (!GameExiting && PhysicsOnBackgroundThread)
            {
                DateTime now = DateTime.Now;
                double elapsed = (now - physicsLastUpdate).TotalMilliseconds;
                PhysicsSimulation.Update((float) elapsed/1000);
                physicsLastUpdate = now;
                Thread.Sleep(5);
            }
        }

        public static void Render(GameTime gameTime)
        {
            if (Wireframe)
            {
                if (GraphicsDevice.RasterizerState.FillMode != FillMode.WireFrame)
                {
                    RasterizerState rasterizerState = new RasterizerState();
                    rasterizerState.FillMode = FillMode.WireFrame;
                    GraphicsDevice.RasterizerState = rasterizerState;
                }
            }
            else
            {
                if (GraphicsDevice.RasterizerState.FillMode == FillMode.WireFrame)
                {
                    RasterizerState rasterizerState = new RasterizerState();
                    rasterizerState.FillMode = FillMode.Solid;
                    GraphicsDevice.RasterizerState = rasterizerState;
                }
            }

            if(EnableBloom)
            {
                bloomComponent.BeginDraw();
            }

            for (int i = 0; i < gameSubSystems.Count; i++)
            {
                gameSubSystems[i].Render(gameTime);
            }


            DebugShapeRenderer.Draw(gameTime, ActiveCamera.View, ActiveCamera.Projection);
            DebugText.Draw(gameTime);
            ScreenManager.RenderSprites(gameTime);
        }

        public static void SetActiveCamera(ICamera camera)
        {
            if (cameras.ContainsKey("main"))
                cameras["main"] = camera;
            else
            {
                cameras.Add("main", camera);
            }
        }

        public static ICamera ActiveCamera
        {
            get { return cameras["main"]; }
        }

        public static void AddCamera(string cameraName, ICamera camera)
        {
            cameras.Add(cameraName, camera);
        }

        public static ICamera GetCamera(string cameraName)
        {
            return cameras[cameraName];
        } 

        public static T GetSubsystem<T>()
        {
            var gameSubSystem = gameSubSystems.Find(x => (x.GetType() == typeof(T)));
            if (gameSubSystem != null)
                return (T)gameSubSystem;
            return default(T);
        }

        public static T GetGameComponent<T>()
        {
            var gameSubSystem = gameComponents.Find(x => (x.GetType() == typeof(T)));
            if (gameSubSystem != null)
                return (T)gameSubSystem;
            return default(T);
            
        }

        public static Vector3 Unproject(Vector3 inWorldPoint)
        {
            return MonoMathHelper.ScreenProject(inWorldPoint, SystemCore.Viewport, SystemCore.ActiveCamera.View, SystemCore.ActiveCamera.Projection, Matrix.Identity);
        }

        public static Vector3 Project(Vector2 screenPoint)
        {
            return Viewport.Project(new Vector3(screenPoint.X, screenPoint.Y, 0), ActiveCamera.Projection, ActiveCamera.View, Matrix.Identity);
        }

    }




}


