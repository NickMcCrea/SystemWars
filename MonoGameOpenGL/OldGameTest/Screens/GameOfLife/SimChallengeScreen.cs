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
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using BEPUphysics;

namespace OldGameTest.Screens
{

    class SimChalleneScreen : Screen
    {
        protected MouseFreeCamera mouseCamera;
        protected bool releaseMouse;
        GradientSkyDome skyDome;
        GameObject[,] gameObjectArray;
        XNATimer simulationTickTimer;
        GameObject cameraGameObject;

        private Vector3 boardMidpoint = new Vector3(15, 0, 15);
        private int boardSize = 30;
        private int tickFrequency = 500;
        public static Color activeCellColour = Color.Red;
        public static Color inactiveCellColour = Color.DarkGray;

        public SimChalleneScreen() : base()
        {



        }

        public override void OnInitialise()
        {

            base.OnInitialise();

            SystemCore.Game.Window.Title = "Conway's Game of Life";

            mouseCamera = new MouseFreeCamera(new Vector3(0, 0, 0));
            SystemCore.SetActiveCamera(mouseCamera);
            mouseCamera.moveSpeed = 0.01f;
            mouseCamera.SetPositionAndLook(new Vector3(50, 30, -20), (float)Math.PI, (float)-Math.PI / 5);


            SystemCore.ActiveScene.SetUpAmbientAndFullLightingRig();
            SystemCore.ActiveScene.AmbientLight.LightIntensity = 0.075f;
            SystemCore.ActiveScene.GetBackLight().LightIntensity = 0f;
            SystemCore.ActiveScene.GetFillLight().LightIntensity = 0.1f;
            SystemCore.ActiveScene.FogEnabled = false;

            SystemCore.CursorVisible = true;
            //fpsLabel.Visible = true;


            skyDome = new GradientSkyDome(Color.LightGray, Color.DarkGray);

            simulationTickTimer = new XNATimer(tickFrequency, x => RunSimulationTick(x));
            simulationTickTimer.Enabled = true;

            AddInputBindings();


            // var heightMapObject = CreateHeightMapGameObject();
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(heightMapObject);


            CreateGameOfLifeBoard();


            cameraGameObject = new GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(new Vector3(50, 15, 50));

            Vector3 lookAt = Vector3.Normalize(boardMidpoint - cameraGameObject.Position);
            cameraGameObject.Transform.SetLookAndUp(lookAt, Vector3.Up);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());

            //var sphere = GameObjectFactory.CreateRenderableGameObjectFromShape(new ProceduralSphere(10, 10), EffectLoader.LoadSM5Effect("flatshaded"));
            //sphere.Transform.SetPosition(boardMidpoint);
            //sphere.Transform.Scale = 5f;
            //SystemCore.GameObjectManager.AddAndInitialiseGameObject(sphere);

            AddGUI();

        }

        private void AddGUI()
        {
            var clear = AddButton(GUIManager.ScreenRatioX(0.01f), GUIManager.ScreenRatioY(0.25f), 50, 45, "Clear");
            clear.OnClick += (sender, args) =>
            {
                Clear();
            };
            SystemCore.GUIManager.AddControl(clear);


            var faster = AddButton(GUIManager.ScreenRatioX(0.06f), GUIManager.ScreenRatioY(0.20f), 50, 45, ">>>");
            faster.OnClick += (sender, args) =>
            {
                if (simulationTickTimer.Interval > 100)
                    simulationTickTimer.Interval -= 100;
            };
            var slower = AddButton(GUIManager.ScreenRatioX(0.01f), GUIManager.ScreenRatioY(0.20f), 50, 45, "<<<");
            slower.OnClick += (sender, args) =>
            {
                if (simulationTickTimer.Interval < 5000)
                    simulationTickTimer.Interval += 100;
            };
            SystemCore.GUIManager.AddControl(faster);
            SystemCore.GUIManager.AddControl(slower);

            var stopStartButton = AddButton(GUIManager.ScreenRatioX(0.01f), GUIManager.ScreenRatioY(0.02f), 150, 45, "Stop / Start");
            stopStartButton.OnClick += (sender, args) =>
            {
                simulationTickTimer.Enabled = !simulationTickTimer.Enabled;
            };

            SystemCore.GUIManager.AddControl(stopStartButton);

            var randomise = AddButton(GUIManager.ScreenRatioX(0.01f), GUIManager.ScreenRatioY(0.07f), 150, 45, "Randomise");
            randomise.OnClick += (sender, args) =>
            {
                Randomise();
            };

            SystemCore.GUIManager.AddControl(randomise);

            var activeCellColourPanel =
               new ButtonGridPanel(
                   new Rectangle(GUIManager.ScreenRatioX(0.01f), GUIManager.ScreenRatioY(0.13f), 300, 300),
                   GUITexture.BlankTexture);
            activeCellColourPanel.SelectableItems = true;
            activeCellColourPanel.MainColor = Color.Gray;
            activeCellColourPanel.HighlightColor = Color.LightGray;
            activeCellColourPanel.MainAlpha = 0.5f;
            activeCellColourPanel.HighlightAlpha = 0.1f;

            activeCellColourPanel.AddColorPanel(ButtonGridPanel.GenerateColorList(), 15, 15, 2, 2, 4, 2);
            activeCellColourPanel.AddFadeInTransition(500);

            activeCellColourPanel.OnSelectionChanged += (sender, args) =>
            {
                activeCellColour = activeCellColourPanel.SelectedItem.MainColor;
                RefreshColours();
            };

            SystemCore.GUIManager.AddControl(activeCellColourPanel);

            var inactiveColourPanel =
            new ButtonGridPanel(
                new Rectangle(GUIManager.ScreenRatioX(0.05f), GUIManager.ScreenRatioY(0.13f), 300, 300),
                GUITexture.BlankTexture);
            inactiveColourPanel.SelectableItems = true;
            inactiveColourPanel.MainColor = Color.Gray;
            inactiveColourPanel.HighlightColor = Color.LightGray;
            inactiveColourPanel.MainAlpha = 0.5f;
            inactiveColourPanel.HighlightAlpha = 0.1f;

            inactiveColourPanel.AddColorPanel(ButtonGridPanel.GenerateColorList(), 15, 15, 2, 2, 4, 2);
            inactiveColourPanel.AddFadeInTransition(500);

            inactiveColourPanel.OnSelectionChanged += (sender, args) =>
            {
                inactiveCellColour = inactiveColourPanel.SelectedItem.MainColor;
                RefreshColours();
            };

            SystemCore.GUIManager.AddControl(inactiveColourPanel);

        }

        private void RefreshColours()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    var comp = gameObjectArray[i, j].GetComponent<GameOfLifeComponent>();
                    if (comp.active)
                    {
                        gameObjectArray[i, j].GetComponent<RenderGeometryComponent>().SetColour(activeCellColour);
                    }
                    else
                    {
                        gameObjectArray[i, j].GetComponent<RenderGeometryComponent>().SetColour(inactiveCellColour);
                    }
                }

            }
        }

        private Button AddButton(int xPos, int yPos, int width, int height, string label)
        {
            var modeButton =
                new Button(new Rectangle(xPos, yPos, width, height),
                    GUITexture.Textures["blank"]);

            modeButton.MainColor = Color.Gray;
            modeButton.HighlightColor = Color.LightGray;
            modeButton.MainAlpha = 0.1f;
            modeButton.HighlightAlpha = 0.1f;
            modeButton.AddFadeInTransition(500);
            modeButton.AttachLabel(new Label(GUIFonts.Fonts["neurosmall"], label));
            return modeButton;
        }

        private void MoveCamera()
        {

            float speedFactor = 100;
            if (SystemCore.Input.MouseRightDown())
            {


                float xChange = SystemCore.Input.MouseDelta.X;
                cameraGameObject.Transform.RotateAround(Vector3.Up, boardMidpoint, xChange / speedFactor);

                float yChange = SystemCore.Input.MouseDelta.Y;
                cameraGameObject.Transform.RotateAround(cameraGameObject.Transform.AbsoluteTransform.Right, boardMidpoint,
                    yChange / speedFactor);
            }

            Vector3 forward = cameraGameObject.Transform.AbsoluteTransform.Forward;
            cameraGameObject.Transform.SetPosition(cameraGameObject.Transform.AbsoluteTransform.Translation + forward * SystemCore.Input.ScrollDelta / speedFactor);
        }

        private void CreateGameOfLifeBoard()
        {
            BasicEffect effect = new BasicEffect(SystemCore.GraphicsDevice);
            effect.LightingEnabled = false;
            effect.SpecularPower = 0;
            ProceduralCuboid cubeoid = new ProceduralCuboid(0.5f, 0.5f, 2);
            cubeoid.SetColor(Color.Gray);
            gameObjectArray = new GameObject[boardSize, boardSize];
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    var gameObject = GameObjectFactory.CreateRenderableGameObjectFromShape(cubeoid, EffectLoader.LoadSM5Effect("flatshaded"));
                    gameObject.Transform.SetPosition(new Vector3(i, 0, j));
                    //gameObject.AddComponent(new ShadowCasterComponent());
                    gameObject.AddComponent(new LineRenderComponent(effect));
                    gameObject.AddComponent(new GameOfLifeComponent());
                    gameObject.AddComponent(new PhysicsComponent(true, false, PhysicsMeshType.box));
                    SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObject);
                    gameObjectArray[i, j] = gameObject;

                    if (RandomHelper.CoinToss())
                        gameObject.GetComponent<GameOfLifeComponent>().ActivateCell();

                }
            }

            //gameObjectArray[5, 5].GetComponent<GameOfLifeComponent>().ActivateCell();
            // gameObjectArray[5, 6].GetComponent<GameOfLifeComponent>().ActivateCell();
            //gameObjectArray[5, 7].GetComponent<GameOfLifeComponent>().ActivateCell();

        }

        private void Randomise()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {

                    if (RandomHelper.CoinToss())
                        gameObjectArray[i, j].GetComponent<GameOfLifeComponent>().ActivateCell();
                    else
                        gameObjectArray[i, j].GetComponent<GameOfLifeComponent>().DeactivateCell();
                }

            }
        }

        private void Clear()
        {
            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    gameObjectArray[i, j].GetComponent<GameOfLifeComponent>().DeactivateCell();
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
            List<GameObject> neighbours = new List<GameObject>();
            for (int xOffset = -1; xOffset < 2; xOffset++)
            {
                for (int yOffset = -1; yOffset < 2; yOffset++)
                {
                    if (xOffset != 0 || yOffset != 0)
                    {

                        GameObject neighbour = GetCell(row + yOffset, col + xOffset);
                        if (neighbour != null)
                        {
                            neighbours.Add(neighbour);
                        }
                    }
                }
            }


            return neighbours;



        }

        private GameObject GetCell(int row, int column)
        {
            if (row >= 0 && row < gameObjectArray.GetLength(0))
            {
                if (column >= 0 && column < gameObjectArray.GetLength(1))
                {
                    return gameObjectArray[row, column];
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

            simulationTickTimer.Update(gameTime);

            MoveCamera();

            if (input.MouseLeftPress() && !SystemCore.GUIManager.MouseOverGUIElement)
            {
                RayCastResult result;
                if (SystemCore.PhysicsSimulation.RayCast(input.GetBepuProjectedMouseRay(), out result))
                {
                    GameObject parent = result.HitObject.Tag as GameObject;

                    parent.GetComponent<GameOfLifeComponent>().FlipState();
                }

            }

            SystemCore.ActiveScene.UpdateBackandFillLights(cameraGameObject.Transform.AbsoluteTransform.Forward, cameraGameObject.Transform.AbsoluteTransform.Right);

            base.Update(gameTime);
        }

        private void RunSimulationTick(GameTime time)
        {

            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    GameObject cell = gameObjectArray[i, j];
                    GameOfLifeComponent cellComponent = cell.GetComponent<GameOfLifeComponent>();

                    var neighbours = CalculateNeighbours(i, j);
                    int aliveNeighbours = neighbours.Count(x => x.GetComponent<GameOfLifeComponent>().active == true);

                    if (cellComponent.active)
                    {
                        if (aliveNeighbours < 2)
                        {
                            cellComponent.ScheduleFlip();
                            continue;
                        }
                        if (aliveNeighbours == 2 || aliveNeighbours == 3)
                        {
                            //do nothing
                        }
                        if (aliveNeighbours > 3)
                        {
                            cellComponent.ScheduleFlip();
                            continue;
                        }
                    }
                    else
                    {
                        if (aliveNeighbours == 3)
                        {
                            cellComponent.ScheduleFlip();
                            continue;
                        }
                    }

                }
            }


            for (int i = 0; i < boardSize; i++)
            {
                for (int j = 0; j < boardSize; j++)
                {
                    GameObject cell = gameObjectArray[i, j];
                    GameOfLifeComponent cellComponent = cell.GetComponent<GameOfLifeComponent>();
                    cellComponent.FlipIfScheduled();
                }
            }


        }

        public override void Render(GameTime gameTime)
        {
            //for (int i = 0; i < 20; i++)
            //{
            //    for (int j = 0; j < 20; j++)
            //    {
            //        if (RandomHelper.GetRandomInt(0, 100000) > 99990)
            //        {
            //            gameObjectArray[i, j].GetComponent<GameOfLifeComponent>().FlipState();
            //        }
            //    }
            //}


            base.Render(gameTime);
        }

        public override void RenderSprites(GameTime gameTime)
        {
            base.RenderSprites(gameTime);
        }
    }
}
