﻿using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using SystemWar;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.Rendering.Camera;
using Math = System.Math;

namespace MonoGameEngineCore.Editor
{

    //To do
    //color changing
    //non-voxel mode
    //baking + saving

  

    public class SimpleModelEditor : IGameSubSystem
    {
        private List<Vector3> currentVertices; 
        public bool RenderGrid { get; set; }
        public bool RenderActivePlaneOnly { get; set; }
        public Vector3 CurrentSnapPoint { get; set; }
        public EditMode CurrentMode { get; set; }  
        public int CurrentXIndex { get; set; }
        public int CurrentZIndex { get; set; }
        public int CurrentYIndex { get; set; }
        public CurrentActivePlane ActivePlane { get; set; }
        private Color currentColour = Color.Blue;
        public enum EditMode
        {
            Vertex,
            Voxel
        }

        public enum CurrentActivePlane
        {
            XZ,
            YZ,
            XY
        };

        private Dictionary<GameObject.GameObject,ProceduralShape> shapesToBake;
        private ProceduralShapeBuilder shapeBuilder;
        private string shapeFolderPath = "//Editor//Shapes//";
        private float modellingAreaSize;
        private GameObject.GameObject cameraGameObject;
        private Vector3 currentbuildPoint;
       
        public SimpleModelEditor(int modellingAreaSize = 8)
        {
            RenderGrid = true;
            RenderActivePlaneOnly = true;
            this.modellingAreaSize = modellingAreaSize;
        }

        private GameObject.GameObject mouseCursor;

        public void OnRemove()
        {

        }

        public void Initalise()
        {
            currentVertices = new List<Vector3>();

            SystemCore.ActiveScene.SetUpAmbientAndFullLightingRig();

            SystemCore.ActiveScene.AddPointLight(new Vector3(0, 15, 0), Color.White, 20, 20, 1, PointLightNumber.One);
            SystemCore.ActiveScene.AddPointLight(new Vector3(0, -15, 0), Color.White, 20, 20, 1, PointLightNumber.Two);
            SystemCore.ActiveScene.AddPointLight(new Vector3(0, 0, 15), Color.White, 20, 20, 1, PointLightNumber.Three);
            SystemCore.ActiveScene.AddPointLight(new Vector3(0, 0, -15), Color.White, 20, 20, 1, PointLightNumber.Four);

            SystemCore.AddNewUpdateRenderSubsystem(new SkyDome(Color.LightGray, Color.Gray, Color.DarkBlue));

            shapeBuilder = new ProceduralShapeBuilder();
            CurrentMode = EditMode.Voxel;
            shapesToBake = new Dictionary<GameObject.GameObject, ProceduralShape>();


            cameraGameObject = new GameObject.GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(new Vector3(0, 0, 20));
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());


            mouseCursor = GameObjectFactory.CreateRenderableGameObjectFromShape(new ProceduralCube(),
                EffectLoader.LoadSM5Effect("flatshaded"));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(mouseCursor);

     

            AddGUI();
        }

        private void AddGUI()
        {
            var modeButton = AddButton(GUIManager.ScreenRatioX(0.05f), GUIManager.ScreenRatioY(0.05f), 150, 50, CurrentMode.ToString());
            modeButton.OnClick += (sender, args) =>
            {
                if (CurrentMode == EditMode.Vertex)
                {
                    CurrentMode = EditMode.Voxel;
                    mouseCursor.Transform.Scale = 1f;
                }
                else if (CurrentMode == EditMode.Voxel)
                {
                    CurrentMode = EditMode.Vertex;
                    mouseCursor.Transform.Scale = 0.1f;
                }

                modeButton.SetLabelText(CurrentMode.ToString());
            };

            SystemCore.GUIManager.AddControl(modeButton);


            var save = AddButton(GUIManager.ScreenRatioX(0.05f), GUIManager.ScreenRatioY(0.15f), 130, 50, "Save");
            save.OnClick += (sender, args) =>
            {

                TextBox textBox =
                    new TextBox(new Rectangle(GUIManager.ScreenRatioX(0.5f)-100, GUIManager.ScreenRatioY(0.5f)-50, 200, 100),
                        GUITexture.Textures["blank"], GUIFonts.Fonts["neuropolitical"]);

                SystemCore.GUIManager.AddControl(textBox);

                textBox.OnReturnEvent += (o, eventArgs) =>
                {
                    SaveIntermediateShapes(textBox.Text);
                    SystemCore.GUIManager.RemoveControl(textBox);
                };


            };
            SystemCore.GUIManager.AddControl(save);


            var load = AddButton(GUIManager.ScreenRatioX(0.17f), GUIManager.ScreenRatioY(0.15f), 130, 50, "Load");
            load.OnClick += (sender, args) =>
            {

                TextBox textBox =
                    new TextBox(new Rectangle(GUIManager.ScreenRatioX(0.5f) - 100, GUIManager.ScreenRatioY(0.5f) - 50, 200, 100),
                        GUITexture.Textures["blank"], GUIFonts.Fonts["neuropolitical"]);

                SystemCore.GUIManager.AddControl(textBox);

                textBox.OnReturnEvent += (o, eventArgs) =>
                {
                    LoadIntermediateShapes(textBox.Text);
                    SystemCore.GUIManager.RemoveControl(textBox);
                };


            };

            SystemCore.GUIManager.AddControl(load);

            var clear = AddButton(GUIManager.ScreenRatioX(0.05f), GUIManager.ScreenRatioY(0.35f), 130, 50, "Clear");
            clear.OnClick += (sender, args) =>
            {

               ClearEditor();

            };

            SystemCore.GUIManager.AddControl(clear);

            var colourPanel =
                new ButtonGridPanel(
                    new Rectangle(GUIManager.ScreenRatioX(0.05f), GUIManager.ScreenRatioY(0.25f), 300, 300),
                    GUITexture.BlankTexture);
            colourPanel.SelectableItems = true;
            colourPanel.MainColor = Color.Gray;
            colourPanel.HighlightColor = Color.LightGray;
            colourPanel.MainAlpha = 0.1f;
            colourPanel.HighlightAlpha = 0.1f;

            colourPanel.AddColorPanel(ButtonGridPanel.GenerateColorList(), 15, 15, 2, 2, 4, 2);
            colourPanel.AddFadeInTransition(500);

            colourPanel.OnSelectionChanged += (sender, args) =>
            {
                currentColour = colourPanel.SelectedItem.MainColor;
            };


            
            SystemCore.GUIManager.AddControl(colourPanel);

        }

        private void RehydrateShape(ProceduralShape shape)
        {

            //take the translation expressed in the verts, and switch it over to 
            //the world transform so the physics collision works.
            //then translate back for when we bake out again.
            var midPoint = shape.GetMidPoint();
            shape.Translate(-midPoint);

            var gameObj = GameObjectFactory.CreateRenderableGameObjectFromShape(shape,
                EffectLoader.LoadSM5Effect("flatshaded"));

            gameObj.Transform.Translate(midPoint);

            gameObj.AddComponent(new PhysicsComponent(false, false, PhysicsMeshType.box));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(gameObj);

            shapesToBake.Add(gameObj, shape);
            shape.Translate(midPoint);
        }

        private Button AddButton(int xPos, int yPos, int width, int height, string label)
        {
            var modeButton =
                new Button(new Rectangle(xPos,yPos,width,height),
                    GUITexture.Textures["blank"]);

            modeButton.MainColor = Color.Gray;
            modeButton.HighlightColor = Color.LightGray;
            modeButton.MainAlpha = 0.1f;
            modeButton.HighlightAlpha = 0.1f;
            modeButton.AddFadeInTransition(500);
            modeButton.AttachLabel(new Label(GUIFonts.Fonts["neuropolitical"], label));
            return modeButton;
        }
  
        public void Clear()
        {
            shapeBuilder = new ProceduralShapeBuilder();
        }

        public void SaveIntermediateShapes(string name)
        {
            int counter = 1;

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), name + "*.shape");

            //delete existing shapes that match this pattern.
            foreach (string file in files)
            {
                File.Delete(file);
            }
            
            foreach (KeyValuePair<GameObject.GameObject, ProceduralShape> kvp in shapesToBake)
            {
                SaveShape(name + counter.ToString(), kvp.Value);
                counter++;

            }
        }

        public void LoadIntermediateShapes(string name)
        {
            int counter = 1;

            var files = Directory.GetFiles(Directory.GetCurrentDirectory(), name + "*.shape");

            foreach (string file in files)
            {
                RehydrateShape(LoadShape(Path.GetFileNameWithoutExtension(file)));
            }


        }

        public void BakeAndSaveCurrentShape(string name)
        {
            List<ProceduralShape> shapes = shapesToBake.Values.ToList();

            ProceduralShape combined = shapes[0];
            for (int i = 1; i < shapes.Count; i++)
            {
                combined = ProceduralShape.Combine(combined, shapes[i]);
            }

            SaveShape(name, combined);
        }

        private static void SaveShape(string name, ProceduralShape combined)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = new FileStream(name + ".shape", FileMode.Create))
            {
                bf.Serialize(fs, combined);
            }
        }

        public static ProceduralShape LoadShape(string name)
        {
            BinaryFormatter bf = new BinaryFormatter();
           
            ProceduralShape s = null;
            if (File.Exists(name + ".shape"))
            {
                using (FileStream fs = new FileStream(name + ".shape", FileMode.Open))
                {
                    s = bf.Deserialize(fs) as ProceduralShape;
                }
            }

            return s;
        }

        public void Update(GameTime gameTime)
        {
            if (SystemCore.Input.MouseLeftPress() && !SystemCore.GUIManager.MouseOverGUIElement)
            {
                if (CurrentMode == EditMode.Voxel)
                    AddVoxel(currentColour);
                if (CurrentMode == EditMode.Vertex)
                    AddVertex();
            }

            if (SystemCore.Input.KeyPress(Keys.G))
            {
                RenderGrid = !RenderGrid;
                mouseCursor.GetComponent<EffectRenderComponent>().Visible = RenderGrid;
            }

            if (SystemCore.Input.KeyPress(Keys.Delete))
            {

                GameObject.GameObject obj =
                    SystemCore.GameObjectManager.GetRayCastObject();

                if (obj != null)
                {
                    shapesToBake.Remove(obj);
                    SystemCore.GameObjectManager.RemoveObject(obj);
                }

            }

            if (SystemCore.Input.KeyPress(Keys.NumPad5))
            {
                cameraGameObject.Transform.SetPosition(Vector3.Backward * 20f);
                cameraGameObject.Transform.SetLookAndUp(Vector3.Forward,Vector3.Up);
            }

            if (SystemCore.Input.KeyPress(Keys.NumPad0))
            {
                cameraGameObject.Transform.SetPosition(Vector3.Forward * 20f);
                cameraGameObject.Transform.SetLookAndUp(Vector3.Backward, Vector3.Up);
            }

            if (SystemCore.Input.KeyPress(Keys.NumPad4))
            {
                cameraGameObject.Transform.SetPosition(Vector3.Left * 20f);
                cameraGameObject.Transform.SetLookAndUp(Vector3.Right, Vector3.Up);
            }

            if (SystemCore.Input.KeyPress(Keys.NumPad6))
            {
                cameraGameObject.Transform.SetPosition(Vector3.Right * 20f);
                cameraGameObject.Transform.SetLookAndUp(Vector3.Left, Vector3.Up);
            }

            if (SystemCore.Input.KeyPress(Keys.NumPad8))
            {
                cameraGameObject.Transform.SetPosition(Vector3.Up * 20f);
                cameraGameObject.Transform.SetLookAndUp(Vector3.Down, Vector3.Backward);
            }

            if (SystemCore.Input.KeyPress(Keys.NumPad2))
            {
                cameraGameObject.Transform.SetPosition(Vector3.Down * 20f);
                cameraGameObject.Transform.SetLookAndUp(Vector3.Up, Vector3.Forward);
            }

            MoveCursor();

            MoveCamera();

            ControlActivePlane();
        }

        private void AddVertex()
        {
            if (currentVertices.Contains(currentbuildPoint))
            {
                currentVertices.Add(currentbuildPoint);
                AddPolygon();
                return;
            }
            currentVertices.Add(currentbuildPoint);
        }

        private void AddPolygon()
        {
            shapeBuilder.AddFaceWithColor(currentColour, currentVertices.ToArray());
            currentVertices.Clear();

            ProceduralShape s = shapeBuilder.BakeShape();

            RehydrateShape(s);

            shapeBuilder.Clear();
        }

        private void ControlActivePlane()
        {
            if (SystemCore.Input.KeyPress(Keys.Y))
            {
                if (ActivePlane != CurrentActivePlane.XZ)
                {
                    ActivePlane = CurrentActivePlane.XZ;
                    return;
                }

                if (SystemCore.Input.IsKeyDown(Keys.LeftShift))
                    CurrentYIndex--;
                else
                    CurrentYIndex++;

                if (CurrentYIndex > modellingAreaSize / 2)
                    CurrentYIndex = -(int)modellingAreaSize / 2;
            }

            if (SystemCore.Input.KeyPress(Keys.Z))
            {
                if (ActivePlane != CurrentActivePlane.XY)
                {
                    ActivePlane = CurrentActivePlane.XY;
                    return;
                }
                if (SystemCore.Input.IsKeyDown(Keys.LeftShift))
                    CurrentZIndex--;
                else
                    CurrentZIndex++;

                if (CurrentZIndex > modellingAreaSize / 2)
                    CurrentZIndex = -(int)modellingAreaSize / 2;
            }

            if (SystemCore.Input.KeyPress(Keys.X))
            {
                if (ActivePlane != CurrentActivePlane.YZ)
                {
                    ActivePlane = CurrentActivePlane.YZ;
                    return;
                }

                if (SystemCore.Input.IsKeyDown(Keys.LeftShift))
                    CurrentXIndex--;
                else
                    CurrentXIndex++;

                if (CurrentXIndex > modellingAreaSize / 2)
                    CurrentXIndex = -(int)modellingAreaSize / 2;
            }
        }

        private void MoveCamera()
        {
            float speedFactor = 40;
            if (SystemCore.Input.MouseRightDown())
            {
              

                float xChange = SystemCore.Input.MouseDelta.X;
                cameraGameObject.Transform.RotateAround(Vector3.Up, Vector3.Zero, xChange/speedFactor);

                float yChange = SystemCore.Input.MouseDelta.Y;
                cameraGameObject.Transform.RotateAround(cameraGameObject.Transform.AbsoluteTransform.Right, Vector3.Zero,
                    yChange/speedFactor);
            }

            Vector3 position = cameraGameObject.Transform.AbsoluteTransform.Translation;
            float distanceFromOrigin = position.Length();
            float newDistance = distanceFromOrigin - SystemCore.Input.ScrollDelta/speedFactor;
            Vector3 newPosition = Vector3.Normalize(position)*newDistance;
            cameraGameObject.Transform.SetPosition(newPosition);
        }

        private void MoveCursor()
        {
            Plane activePlane = new Plane(Vector3.Up, 0);

            //first find the active voxel, then 
            if (ActivePlane == CurrentActivePlane.XZ)
            {
                activePlane = new Plane(Vector3.Up, -CurrentYIndex);
            }

            if (ActivePlane == CurrentActivePlane.YZ)
            {
                activePlane = new Plane(Vector3.Right, -CurrentXIndex);
            }

            if (ActivePlane == CurrentActivePlane.XY)
            {
                activePlane = new Plane(Vector3.Forward, CurrentZIndex);
            }

            Vector3 collisionPoint = Vector3.Zero;
            if (SystemCore.Input.GetPlaneMouseRayCollision(activePlane, out collisionPoint))
            {
                collisionPoint.X = (float)Math.Round(collisionPoint.X);
                collisionPoint.Y = (float)Math.Round(collisionPoint.Y);
                collisionPoint.Z = (float)Math.Round(collisionPoint.Z);

              
                currentbuildPoint = collisionPoint;

                if (currentbuildPoint.X < -modellingAreaSize / 2 || currentbuildPoint.X > modellingAreaSize / 2)
                    return;
                if (currentbuildPoint.Y < -modellingAreaSize / 2 || currentbuildPoint.Y > modellingAreaSize / 2)
                    return;
                if (currentbuildPoint.Z < -modellingAreaSize / 2 || currentbuildPoint.Z > modellingAreaSize / 2)
                    return;

                mouseCursor.Transform.SetPosition(collisionPoint);
              
            }
        }

        public void Render(GameTime gameTime)
        {

            Color xzColor = (ActivePlane == CurrentActivePlane.XZ) ? Color.OrangeRed : Color.DarkGray;
            Color yzColor = (ActivePlane == CurrentActivePlane.YZ) ? Color.CadetBlue : Color.DarkGray;
            Color xyColor = (ActivePlane == CurrentActivePlane.XY) ? Color.DarkGreen : Color.DarkGray;


            if (RenderGrid)
            {
                for (float i = -modellingAreaSize/2; i <= modellingAreaSize/2; i++)
                {
                    if (RenderActivePlaneOnly && i == CurrentZIndex)
                    {
                        DebugShapeRenderer.AddXYGrid(new Vector3(-modellingAreaSize/2, -modellingAreaSize/2, i),
                            modellingAreaSize,
                            modellingAreaSize, 1, (i == CurrentZIndex) ? xyColor : Color.DarkGray);
                    }
                  

                    if (RenderActivePlaneOnly && i == CurrentYIndex)
                    {
                        DebugShapeRenderer.AddXZGrid(new Vector3(-modellingAreaSize/2, i, -modellingAreaSize/2),
                            modellingAreaSize,
                            modellingAreaSize, 1, (i == CurrentYIndex) ? xzColor : Color.DarkGray);
                    }

                    if (RenderActivePlaneOnly && i == CurrentXIndex)
                    {
                        DebugShapeRenderer.AddYZGrid(new Vector3(i, -modellingAreaSize/2, -modellingAreaSize/2),
                            modellingAreaSize, modellingAreaSize, 1,
                            (i == CurrentXIndex) ? yzColor : Color.DarkGray);
                    }


                }
            }


            if (CurrentMode == EditMode.Vertex)
            {
                if (currentVertices.Count > 0)
                {
                    for (int i = 0; i < currentVertices.Count-1; i++)
                    {
                        DebugShapeRenderer.AddLine(currentVertices[i], currentVertices[i + 1], currentColour);
                    }
                    DebugShapeRenderer.AddLine(currentVertices[currentVertices.Count-1], currentbuildPoint, currentColour);
                }
            }
        }

        public void AddVoxel(Color color)
        {
            if (!RenderGrid)
                return;

            if(currentbuildPoint.X < -modellingAreaSize/2 || currentbuildPoint.X > modellingAreaSize/2)
                return;
            if (currentbuildPoint.Y < -modellingAreaSize / 2 || currentbuildPoint.Y > modellingAreaSize / 2)
                return;
            if (currentbuildPoint.Z < -modellingAreaSize / 2 || currentbuildPoint.Z > modellingAreaSize / 2)
                return;

            ProceduralCube c = new ProceduralCube();     
            c.SetColor(color);
         
            //this object is for visualisation in the editor only. The procedural shape will be 
            //cached and used to bake the final shape for serialization
            var tempObject = GameObjectFactory.CreateRenderableGameObjectFromShape(c,
                EffectLoader.LoadSM5Effect("flatshaded"));

            tempObject.AddComponent(new PhysicsComponent(false,false, PhysicsMeshType.box));
            
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(tempObject);
            tempObject.Transform.SetPosition(currentbuildPoint);

            //shape is translated 
            c.Translate(currentbuildPoint);
            shapesToBake.Add(tempObject, c);



        }

        public void ClearEditor()
        {
            foreach (KeyValuePair<GameObject.GameObject, ProceduralShape> keyValuePair in shapesToBake)
            {
                SystemCore.GameObjectManager.RemoveObject(keyValuePair.Key);
            }
            shapesToBake.Clear();
        }
    }
}
