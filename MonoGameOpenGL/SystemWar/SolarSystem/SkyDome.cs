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


        public void Initalise()
        {
            var skyDomeGameObject = GameObjectFactory.CreateSkyDomeObject(Color.Black, 100, 10);

            skyDomeRenderer = skyDomeGameObject.GetComponent<SkyDomeRenderer>();
            skyDomeRenderer.AmbientLightColor = Color.Black;
            skyDomeRenderer.AmbientLightIntensity = 0.2f;
            skyDomeRenderer.DiffuseLightColor = Color.Black;
            skyDomeRenderer.DiffuseLightIntensity = 0.3f;
            skyDomeRenderer.DiffuseLightDirection = Vector3.Forward;

            SystemCore.GameObjectManager.AddAndInitialiseGameObject(skyDomeGameObject);
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
    }
}