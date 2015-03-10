using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject.Components
{
    public enum EffectParamType
    {
        floatParam,
        boolParam,
        matrixParam,
        vector3Param,
        vector4Param,
    }

    public struct EffectParameterHelper
    {
        public EffectParamType TypeOfParam { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class EffectRenderComponent : IComponent, IDrawable
    {

        public readonly Effect effect;
        private ICamera camera;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }
        public float ColorSaturation { get; set; }
        public string Camera { get; set; }
        private List<EffectParameterHelper> paramsToSetBeforeNextRender;

        public EffectRenderComponent(Effect effect)
        {
            this.effect = effect;
            Visible = true;
            paramsToSetBeforeNextRender = new List<EffectParameterHelper>();
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
            SetVariableParams();
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

            if (ParameterExists("time"))
            {
                effect.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalMilliseconds);
            }

            var renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();

            SystemCore.GraphicsDevice.SetVertexBuffer(renderGeometry.VertexBuffer);
            SystemCore.GraphicsDevice.Indices = renderGeometry.IndexBuffer;

            GameObjectManager.verts += renderGeometry.VertexBuffer.VertexCount;
            GameObjectManager.primitives += renderGeometry.PrimitiveCount;


            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                effect.CurrentTechnique.Passes[i].Apply();
                SystemCore.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, renderGeometry.VertexBuffer.VertexCount, 0, renderGeometry.PrimitiveCount);
            }

            PostDraw(gameTime);



        }

        private void SetVariableParams()
        {
            for (int i = 0; i < paramsToSetBeforeNextRender.Count; i++)
            {
                var effectParameter = paramsToSetBeforeNextRender[i];
                if (effectParameter.TypeOfParam == EffectParamType.vector3Param)
                {
                    Vector3 value = (Vector3)effectParameter.Value;
                    effect.Parameters[effectParameter.Name].SetValue(value);
                }
            }
            paramsToSetBeforeNextRender.Clear();
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

            if (ParameterExists("CameraPosition"))
                effect.Parameters["CameraPosition"].SetValue(SystemCore.GetCamera(Camera).Position);

            if (ParameterExists("ViewInvert"))
                effect.Parameters["ViewInvert"].SetValue(Matrix.Invert(SystemCore.GetCamera(Camera).View));

           
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
            for (int i = 0; i < effect.Parameters.Count; i++)
            {
                if (effect.Parameters[i].Name == parameter)
                    return true;
            }
            return false;
        }

        public void SetParameterForNextFrame(string name, Vector3 vec)
        {
            EffectParameterHelper helper = new EffectParameterHelper();
            helper.Name = name;
            helper.TypeOfParam = EffectParamType.vector3Param;
            helper.Value = vec;
            paramsToSetBeforeNextRender.Add(helper);
        }
        public void SetParameterForNextFrame(string name, float floatParam)
        {
            EffectParameterHelper helper = new EffectParameterHelper();
            helper.Name = name;
            helper.TypeOfParam = EffectParamType.floatParam;
            helper.Value = floatParam;
            paramsToSetBeforeNextRender.Add(helper);
        }

        public event System.EventHandler<System.EventArgs> VisibleChanged;
        public event System.EventHandler<System.EventArgs> DrawOrderChanged;
    }
}