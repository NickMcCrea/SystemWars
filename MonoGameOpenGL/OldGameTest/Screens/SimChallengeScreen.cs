using MonoGameEngineCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using Microsoft.Xna.Framework.Input;
using SystemWar;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.GameObject.Components;

namespace OldGameTest.Screens
{
    class GameOfLifeComponent : IComponent, IDrawable, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        private bool active = false;
        public int DrawOrder { get; set; }

        public bool Visible { get; set; }

        public bool Enabled { get; set; }

        public int UpdateOrder { get; set; }

        public event EventHandler<EventArgs> DrawOrderChanged;
        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;



        public void Draw(GameTime gameTime)
        {

        }

        public void ActivateCell()
        {
            ParentObject.GetComponent<RenderGeometryComponent>().SetColour(Color.Red);
            active = true;
        }
        public void DeactivateCell()
        {

            ParentObject.GetComponent<RenderGeometryComponent>().SetColour(Color.Gray);
            active = false;
        }
        public void Initialise()
        {
            Enabled = true;
        }

        public void PostInitialise()
        {

        }

        internal void FlipState()
        {
            if (active)
                DeactivateCell();
            else
                ActivateCell();
        }



        public void Update(GameTime gameTime)
        {
            if (active)
            {
                float newY = MathHelper.Lerp(ParentObject.Position.Y, 2, 0.9f);
                ParentObject.Position = ParentObject.Position.ReplaceYComponent(newY);
            }
            else
            {
                ParentObject.Position = ParentObject.Position.ReplaceYComponent(MathHelper.Lerp(ParentObject.Position.Y, 0, 0.9f));
            }
        }
    }

    class SimChalleneScreen : Screen
    {
        protected MouseFreeCamera mouseCamera;
        protected bool releaseMouse;
        GradientSkyDome skyDome;
        GameObject[,] gameObjectArray;

        public SimChalleneScreen() : base()
        {



        }

        public override void OnInitialise()
        {

            base.OnInitialise();

            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);
            mouseCamera.moveSpeed = 0.01f;
            mouseCamera.SetPositionAndLook(new Vector3(50, 30, -20), (float)Math.PI, (float)-Math.PI / 5);


            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();
            SystemCore.ActiveScene.SetDiffuseLightDir(0, new Vector3(1, 1, 1));
            SystemCore.ActiveScene.FogEnabled = true;

            SystemCore.CursorVisible = false;
            fpsLabel.Visible = true;


            skyDome = new GradientSkyDome(Color.MediumBlue, Color.LightCyan);



            AddInputBindings();


            // var heightMapObject = CreateHeightMapGameObject();
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(heightMapObject);


            CreateGameOfLifeBoard();


        }

        private void CreateGameOfLifeBoard()
        {
            ProceduralCuboid cubeoid = new ProceduralCuboid(0.5f, 0.5f, 3);
            cubeoid.SetColor(Color.Gray);
            gameObjectArray = new GameObject[20, 20];
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    var gameObject = GameObjectFactory.CreateRenderableGameObjectFromShape(cubeoid, EffectLoader.LoadSM5Effect("flatshaded"));
                    gameObject.Transform.SetPosition(new Vector3(i, 0, j));
                    gameObject.AddComponent(new ShadowCasterComponent());
                    gameObject.AddComponent(new GameOfLifeComponent());
                    SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);
                    gameObjectArray[i, j] = gameObject;

                }
            }

        }

        private void AddInputBindings()
        {
            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("CameraForward", Keys.Up);
            input.AddKeyDownBinding("CameraBackward", Keys.Down);
            input.AddKeyDownBinding("CameraLeft", Keys.Left);
            input.AddKeyDownBinding("CameraRight", Keys.Right);
            input.AddKeyDownBinding("SlowCamera", Keys.RightShift);
            var binding = input.AddKeyPressBinding("WireframeToggle", Keys.Space);

            input.AddKeyPressBinding("MainMenu", Keys.Escape);

            var releaseMouseBinding = input.AddKeyPressBinding("MouseRelease", Keys.M);
            releaseMouseBinding.InputEventActivated += (x, y) =>
            {
                releaseMouse = !releaseMouse;
                SystemCore.CursorVisible = releaseMouse;
            };
            binding.InputEventActivated += (x, y) => { SystemCore.Wireframe = !SystemCore.Wireframe; };

        }


        List<GameObject> CalculateNeighbours(int row, int col)
        {
            
            for (int xOffset = -1; xOffset < 2; xOffset++)
            {
                for (int yOffset = -1; yOffset < 2; yOffset++)
                {
                    if ((xOffset != 0 || yOffset != 0)
                          && board.get(row + yOffset, col + xOffset))
                    {
                        neighbours++;
                    }
                }
            }


            return null;



        }


        private GameObject CreateHeightMapGameObject()
        {
            var heightMap = NoiseGenerator.CreateHeightMap(NoiseGenerator.RidgedMultiFractal(0.003f), 100, 1, 100, 1, 1, 1);
            return heightMap.CreateRenderableHeightMap(Color.MonoGameOrange, EffectLoader.LoadSM5Effect("flatshaded"));
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (!SystemCore.CursorVisible)
            {

                mouseCamera.Slow = input.EvaluateInputBinding("SlowCamera");

                if (input.EvaluateInputBinding("CameraForward"))
                    mouseCamera.MoveForward();
                if (input.EvaluateInputBinding("CameraBackward"))
                    mouseCamera.MoveBackward();
                if (input.EvaluateInputBinding("CameraLeft"))
                    mouseCamera.MoveLeft();
                if (input.EvaluateInputBinding("CameraRight"))
                    mouseCamera.MoveRight();

                if (!releaseMouse)
                {
                    mouseCamera.Update(gameTime, input.MouseDelta.X, input.MouseDelta.Y);
                    input.CenterMouse();
                }
            }

            if (input.EvaluateInputBinding("MainMenu"))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 20; j++)
                {
                    if (RandomHelper.GetRandomInt(0, 100000) > 99990)
                    {
                        gameObjectArray[i, j].GetComponent<GameOfLifeComponent>().FlipState();
                    }
                }
            }


            base.Render(gameTime);
        }

        public override void RenderSprites(GameTime gameTime)
        {
            base.RenderSprites(gameTime);
        }
    }
}
