using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.ScreenManagement;
using MonoGameEngineCore.Helper;
using System.Collections.Generic;
using MonoGameEngineCore.Rendering.Camera;

namespace MonoGameDirectX11.Screens
{
    
    public class SolarSystemTest : MouseCamScreen
    {
        Effect effect;
        Vector3 lastCameraPosition;
        List<GameObject> planets;
        GameObject sun;
        float bodyScale = 1f;
        float distanceScale = 1f;
        float moveSpeed = 10f;
   
        public SolarSystemTest()
            : base()
        {
            effect = EffectLoader.LoadEffect("FlatShaded");

            planets = new List<GameObject>();

            ProceduralSphere sunSphere = new ProceduralSphere(10, 10);
            sunSphere.SetColor(Color.OrangeRed);


            sun = GameObjectFactory.CreateRenderableGameObjectFromShape(sunSphere, effect);
            sun.Transform.Scale = 100 * bodyScale;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(sun);

            planets.Add(sun);

            //mercury
            AddBody(0.3f, 4160);

            //venus
            AddBody(0.8f, 7700);

            //earth
            AddBody(0.9f, 10700);

            //jupiter
            AddBody(10f, 55000);

            lastCameraPosition = Vector3.Zero;

            mouseCamera = new MouseFreeCamera(new Vector3(10700, 0, 0), 1f, 20000f);
            mouseCamera.moveSpeed *= moveSpeed;
            SystemCore.SetActiveCamera(mouseCamera);
        }

        public void AddBody(float size, float distance)
        {
            var planet = GameObjectFactory.CreateRenderableGameObjectFromShape(new ProceduralSphere(10, 10), EffectLoader.LoadEffect("flatshaded"));
            planet.Transform.Scale = size * bodyScale;
            planet.Transform.WorldMatrix.Translation = new Vector3(distance * distanceScale, 0, 0);
            planets.Add(planet);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(planet);
        }


        public override void Update(GameTime gameTime)
        {



            ShiftOrigin();


            lastCameraPosition = SystemCore.ActiveCamera.Position;

            base.Update(gameTime);
        }

        private void ShiftOrigin()
        {
           
                Vector3 resetVector = -SystemCore.ActiveCamera.Position;

                foreach (GameObject o in planets)
                    o.Transform.WorldMatrix.Translation += resetVector;

                mouseCamera.World.Translation = Vector3.Zero;
            
          
        }

        private void DetermineTerrainPatches()
        {
           


        }

        public override void Render(GameTime gameTime)
        {
            DebugText.WritePositionedText(SystemCore.ActiveCamera.Position.ToString(), new Vector2(10, 20));
            DebugText.WritePositionedText(lastCameraPosition.ToString(), new Vector2(10, 40));
            base.Render(gameTime);
        }
    }
}