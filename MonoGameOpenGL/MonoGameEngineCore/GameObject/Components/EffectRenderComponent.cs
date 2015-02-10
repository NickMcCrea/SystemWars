using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject.Components
{
    public class EffectRenderComponent : IComponent, IDrawable
    {

        protected readonly Effect effect;
        private ICamera camera;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }
        public float ColorSaturation { get; set; }
        public string Camera { get; set; }

        public EffectRenderComponent(Effect effect)
        {
            this.effect = effect;
            Visible = true;

            //default to main active camera in scene. But overridable via property.
            Camera = "main";
        }

        public void Initialise()
        {
            ColorSaturation = 1;
        }

        public virtual void PreDraw(GameTime gameTime)
        {
            AssignMatrixParameters();
            AssignLightingParameters();
            AssignTextureParameters();
        }

        private void AssignTextureParameters()
        {

        }

        public virtual void PostDraw(GameTime gameTime)
        {

        }

        public virtual void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            PreDraw(gameTime);

            var renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();

            SystemCore.GraphicsDevice.SetVertexBuffer(renderGeometry.VertexBuffer);
            SystemCore.GraphicsDevice.Indices = renderGeometry.IndexBuffer;
        
            GameObjectManager.verts += renderGeometry.VertexBuffer.VertexCount;
            GameObjectManager.primitives += renderGeometry.PrimitiveCount;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                SystemCore.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, renderGeometry.VertexBuffer.VertexCount, 0, renderGeometry.PrimitiveCount);
            }

            PostDraw(gameTime);



        }

        public virtual void AssignMatrixParameters()
        {
            var transform = ParentObject.GetComponent<TransformComponent>();
            Matrix worldViewProj = Matrix.CreateScale(ParentObject.Transform.Scale) * transform.WorldMatrix;
            worldViewProj *= SystemCore.GetCamera(Camera).View;
            worldViewProj *= SystemCore.GetCamera(Camera).Projection;

            if (ParameterExists("World"))
                effect.Parameters["World"].SetValue(Matrix.CreateScale(ParentObject.Transform.Scale) * transform.WorldMatrix);

            if (ParameterExists("View"))
                effect.Parameters["View"].SetValue(SystemCore.GetCamera(Camera).View);

            if (ParameterExists("Projection"))
                effect.Parameters["Projection"].SetValue(SystemCore.GetCamera(Camera).Projection);

            if (ParameterExists("WorldViewProjection"))
                effect.Parameters["WorldViewProjection"].SetValue(worldViewProj);
        }

        public virtual void AssignLightingParameters()
        {
            if (ParameterExists("AmbientLightColor"))
                effect.Parameters["AmbientLightColor"].SetValue(SystemCore.ActiveScene.AmbientLight.LightColor.ToVector4());

            if (ParameterExists("AmbientLightIntensity"))
                effect.Parameters["AmbientLightIntensity"].SetValue(SystemCore.ActiveScene.AmbientLight.LightIntensity);


            if (ParameterExists("ColorSaturation"))
                effect.Parameters["ColorSaturation"].SetValue(ColorSaturation);

            foreach (SceneLight light in SystemCore.ActiveScene.LightsInScene)
            {
                if (light is DiffuseLight)
                    AddDiffuseLight(light as DiffuseLight);
            }

        }

        private void AddDiffuseLight(DiffuseLight diffuseLight)
        {
            if (ParameterExists("DiffuseLightColor"))
                effect.Parameters["DiffuseLightColor"].SetValue(diffuseLight.LightColor.ToVector4());
            if (ParameterExists("DiffuseLightDirection"))
                effect.Parameters["DiffuseLightDirection"].SetValue(diffuseLight.LightDirection);
            if (ParameterExists("DiffuseLightIntensity"))
                effect.Parameters["DiffuseLightIntensity"].SetValue(diffuseLight.LightIntensity);

        }

        protected bool ParameterExists(string parameter)
        {
            foreach (EffectParameter effectParameter in effect.Parameters)
            {
                if (effectParameter.Name == parameter)
                    return true;
            }
            return false;
        }

        public event System.EventHandler<System.EventArgs> VisibleChanged;
        public event System.EventHandler<System.EventArgs> DrawOrderChanged;
    }
}