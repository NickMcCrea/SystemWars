using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;

namespace SystemWar
{
    public class SkyDome : IGameSubSystem
    {
        static SkyDomeRenderer skyDomeRenderer;


        public SkyDome(Color vertexColour, Color ambientLightColor, Color diffuseLightColor)
        {
             var skyDomeGameObject = GameObjectFactory.CreateSkyDomeObject(vertexColour, 100, 10);

            skyDomeRenderer = skyDomeGameObject.GetComponent<SkyDomeRenderer>();
            skyDomeRenderer.AmbientLightColor = ambientLightColor;
            skyDomeRenderer.AmbientLightIntensity = 0.2f;
            skyDomeRenderer.DiffuseLightColor = diffuseLightColor;
            skyDomeRenderer.DiffuseLightIntensity = 0.3f;
            skyDomeRenderer.DiffuseLightDirection = Vector3.Forward;

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(skyDomeGameObject);
        }

        public void OnRemove()
        {

        }

        public static void TransitionToColor(Color newColor, float transitionTime)
        {

        }

        public static void SetSunDir(Vector3d cameraPosition)
        {
            Vector3 toSun = cameraPosition.ToVector3();
            toSun.Normalize();
            skyDomeRenderer.DiffuseLightDirection = toSun;
        }

        public static void SetSunDir(Vector3 cameraPosition)
        {
            Vector3 toSun = cameraPosition - Vector3.Zero;
            toSun.Normalize();
            skyDomeRenderer.DiffuseLightDirection = toSun;
        }

        public void Update(GameTime gameTime)
        {


        }

        public void Render(GameTime gameTime)
        {

        }

        public void Initalise()
        {
            
        }
    }

    public class GradientSkyDome : IGameSubSystem
    {
        static GradientSkyDomeRenderer skyDomeRenderer;


        public GradientSkyDome(Color apexColor, Color centerColor)
        {
            var skyDomeGameObject = GameObjectFactory.CreateGradientSkyDomeObject(100);
            skyDomeRenderer = skyDomeGameObject.GetComponent<GradientSkyDomeRenderer>();
            skyDomeRenderer.ApexColor = apexColor;
            skyDomeRenderer.CenterColor = centerColor;
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(skyDomeGameObject);
        }

        public void OnRemove()
        {

        }

        public static void TransitionToColor(Color newColor, float transitionTime)
        {

        }

        public static void SetSunDir(Vector3d cameraPosition)
        {
            Vector3 toSun = cameraPosition.ToVector3();
            toSun.Normalize();
       
        }

        public static void SetSunDir(Vector3 cameraPosition)
        {
            Vector3 toSun = cameraPosition - Vector3.Zero;
            toSun.Normalize();
           
        }

        public void Update(GameTime gameTime)
        {


        }

        public void Render(GameTime gameTime)
        {

        }

        public void Initalise()
        {

        }
    }
}