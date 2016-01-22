﻿using System.Collections.Generic;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
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
    //undo or delete
    //color changing
    //non-voxel mode
    //baking + saving
    //camera improvements
    //fix 'stutter'
  

    public class SimpleModelEditor : IGameSubSystem
    {
        public bool RenderGrid { get; set; }
        public bool RenderActivePlaneOnly { get; set; }
        public Vector3 CurrentSnapPoint { get; set; }
        public EditMode CurrentMode { get; set; }  
        public int CurrentXIndex { get; set; }
        public int CurrentZIndex { get; set; }
        public int CurrentYIndex { get; set; }
        public CurrentActivePlane ActivePlane { get; set; }
    
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

        private List<ProceduralShape> shapesToBake;
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

        public void Initalise()
        {
            shapeBuilder = new ProceduralShapeBuilder();

            CurrentMode = EditMode.Voxel;
            cameraGameObject = new GameObject.GameObject("camera");
            cameraGameObject.AddComponent(new ComponentCamera());
            cameraGameObject.Transform.SetPosition(new Vector3(0, 0, 20));
            cameraGameObject.Transform.SetLookAndUp(new Vector3(0, 0, -1), new Vector3(0, 1, 0));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraGameObject);
            SystemCore.SetActiveCamera(cameraGameObject.GetComponent<ComponentCamera>());

            mouseCursor = GameObjectFactory.CreateRenderableGameObjectFromShape(new ProceduralCube(),
                EffectLoader.LoadSM5Effect("flatshaded"));

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(mouseCursor);

            shapesToBake = new List<ProceduralShape>();

        }

        public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Color col)
        {
            shapeBuilder.AddTriangleWithColor(col, a, b, c);
        }

        public void AddFace(Color col, params Vector3[] points)
        {
            shapeBuilder.AddFaceWithColor(col, points);
        }

        public void Clear()
        {
            shapeBuilder = new ProceduralShapeBuilder();
        }

        public void SaveCurrentShape(string name)
        {
            ProceduralShape s = shapeBuilder.BakeShape();

            BinaryFormatter bf = new BinaryFormatter();
            using (FileStream fs = new FileStream(name + ".shape", FileMode.Create))
            {
                bf.Serialize(fs, s);
            }

        }

        public ProceduralShape LoadShape(string name)
        {
            BinaryFormatter bf = new BinaryFormatter();
            ProceduralShape s = null;
            using (FileStream fs = new FileStream(name + ".shape", FileMode.Open))
            {
                s = bf.Deserialize(fs) as ProceduralShape;
            }

            return s;
        }

        public void Update(GameTime gameTime)
        {

            if (SystemCore.Input.KeyPress(Keys.G))
            {
                RenderGrid = !RenderGrid;
                mouseCursor.GetComponent<EffectRenderComponent>().Visible = RenderGrid;
            }

            MoveCursor();

            MoveCamera();

            ControlActivePlane();
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
                cameraGameObject.Transform.RotateAround(cameraGameObject.Transform.WorldMatrix.Right, Vector3.Zero,
                    yChange/speedFactor);
            }

            Vector3 position = cameraGameObject.Transform.WorldMatrix.Translation;
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
                collisionPoint.X = (float)Math.Floor(collisionPoint.X);
                collisionPoint.Y = (float)Math.Floor(collisionPoint.Y);
                collisionPoint.Z = (float)Math.Floor(collisionPoint.Z);

                mouseCursor.Transform.SetPosition(collisionPoint);
                currentbuildPoint = collisionPoint;
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

                    if (!RenderActivePlaneOnly)
                    {
                        //DebugShapeRenderer.AddXYGrid(new Vector3(-modellingAreaSize / 2, -modellingAreaSize / 2, i),
                        //   modellingAreaSize,
                        //   modellingAreaSize, 1, (i == CurrentYIndex) ? xyColor : Color.DarkGray);

                        //DebugShapeRenderer.AddXZGrid(new Vector3(-modellingAreaSize / 2, i, -modellingAreaSize / 2),
                        //   modellingAreaSize,
                        //   modellingAreaSize, 1, (i == CurrentYIndex) ? xzColor : Color.DarkGray);

                        //DebugShapeRenderer.AddYZGrid(new Vector3(i, -modellingAreaSize / 2, -modellingAreaSize / 2),
                        //  modellingAreaSize, modellingAreaSize, 1,
                        //  (i == CurrentXIndex) ? yzColor : Color.DarkGray);
                    }



                }
            }
        }

        public void AddVoxel(Color color)
        {
            if (!RenderGrid)
                return;

            ProceduralCube c = new ProceduralCube();
            c.Translate(currentbuildPoint);
            c.SetColor(color);
            shapesToBake.Add(c);

            //this object is for visualisation in the editor only. The procedural shape will be 
            //cached and used to bake the final shape for serialization
            var tempObject = GameObjectFactory.CreateRenderableGameObjectFromShape(c,
                EffectLoader.LoadSM5Effect("flatshaded"));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(tempObject);


        }
    }
}
