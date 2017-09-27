using MonoGameEngineCore;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using MarcelJoachimKloubert.DWAD;
using System.IO;
using MarcelJoachimKloubert.DWAD.WADs.Lumps.Linedefs;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Rendering.Camera;
using System;

namespace MonoGameDirectX11.Screens
{
    class DoomWadViewer : Screen
    {
        GameObject cameraObject;
        private IWADFile currentFile;
        string filePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads\\Doom1.WAD";
        float scale = 50f;
        float offsetX = 0;
        float offsetY = 0;

        public DoomWadViewer()
        {

        }


        public override void OnInitialise()
        {
            SystemCore.CursorVisible = true;
            
            SystemCore.ActiveScene.SetUpBasicAmbientAndKey();

            cameraObject = new GameObject();
            cameraObject.AddComponent(new ComponentCamera(MathHelper.PiOver4, SystemCore.GraphicsDevice.Viewport.AspectRatio, 0.25f, 1000.0f, false));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(cameraObject);
            SystemCore.SetActiveCamera(cameraObject.GetComponent<ComponentCamera>());
            cameraObject.Transform.AbsoluteTransform = Matrix.CreateWorld(new Vector3(0, 500, 0), new Vector3(0, -1, 0), new Vector3(0, 0, 1));


            var file = new FileInfo(filePath);
            if (!file.Exists)
            {
                return;
            }

            using (var fs = file.OpenRead())
            {
                currentFile = WADFileFactory.FromStream(fs).FirstOrDefault();
            }

            //List<Vector3> vecList = new List<Vector3>();
            //foreach (var lump in currentFile.EnumerateLumps().OfType<ILinedefsLump>())
            //{
            //    foreach (var linedef in lump.EnumerateLinedefs())
            //    {
            //        var p1 = new Vector3((linedef.Start.X) / scale + offsetX,0,
            //                           (linedef.Start.Y) / scale + offsetY);

            //        var p2 = new Vector3((linedef.End.X) / scale + offsetX,0,
            //                           (linedef.End.Y) / scale + offsetY);


            //        vecList.Add(p1);
            //        vecList.Add(p2);
                  
            //    }
            //}
            
         
            //vecList.Add(vecList[0]);
            //LineBatch l = new LineBatch(vecList.ToArray());
            //l.SetColor(Color.Orange);
            //GameObject lineObject = SystemCore.GameObjectManager.AddLineBatchToScene(l);
            base.OnInitialise();
        }

        public override void OnRemove()
        {
            base.OnRemove();
        }

        public override void Update(GameTime gameTime)
        {
            if (input.KeyPress(Microsoft.Xna.Framework.Input.Keys.Escape))
                SystemCore.ScreenManager.AddAndSetActive(new MainMenuScreen());

            float currentHeight = cameraObject.Transform.AbsoluteTransform.Translation.Y;
            cameraObject.Transform.Translate(new Vector3(0, -input.ScrollDelta/10f, 0));

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
            SystemCore.GraphicsDevice.Clear(Color.Black);
            DebugShapeRenderer.VisualiseAxes(5f);

            foreach (var lump in currentFile.EnumerateLumps().OfType<ILinedefsLump>())
            {
                foreach (var linedef in lump.EnumerateLinedefs())
                {
                    var p1 = new Vector3((linedef.Start.X) / scale + offsetX, 0,
                                       (linedef.Start.Y) / scale + offsetY);

                    var p2 = new Vector3((linedef.End.X) / scale + offsetX, 0,
                                       (linedef.End.Y) / scale + offsetY);

                    // g.DrawLine(whitePen,
                    //            p1, p2);
                    DebugShapeRenderer.AddLine(p1, p2, Color.Orange);
                }
            }

            base.Render(gameTime);
        }


    }
}
