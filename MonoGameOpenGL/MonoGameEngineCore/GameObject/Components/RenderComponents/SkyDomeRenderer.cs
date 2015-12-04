using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;

namespace MonoGameEngineCore.GameObject.Components
{
    public class SkyDomeRenderer : IComponent, IDrawable
    {

        
        protected readonly Effect effect;
        private ICamera camera;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }


        public Color AmbientLightColor { get; set; }
        public float AmbientLightIntensity { get; set; }
        public Color DiffuseLightColor { get; set; }
        public float DiffuseLightIntensity { get; set; }
        public Vector3 DiffuseLightDirection { get; set; }

        public SkyDomeRenderer(Effect effect)
        {
            this.effect = effect;
            Visible = true;

        }

        public void Initialise()
        {

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

            var tempDepthState = new DepthStencilState();
            tempDepthState.DepthBufferWriteEnable = false;
            SystemCore.GraphicsDevice.DepthStencilState = tempDepthState;
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

            var depthState = new DepthStencilState();
            depthState.DepthBufferEnable = true;
            depthState.DepthBufferWriteEnable = true;
            SystemCore.GraphicsDevice.DepthStencilState = depthState;



        }

        public virtual void AssignMatrixParameters()
        {
            var transform = ParentObject.GetComponent<TransformComponent>();
            Matrix world = Matrix.CreateWorld(SystemCore.ActiveCamera.Position, Vector3.Forward, Vector3.Up);
            Matrix worldViewProj = Matrix.CreateScale(ParentObject.Transform.Scale) * world;
            worldViewProj *= SystemCore.ActiveCamera.View;
            worldViewProj *= SystemCore.ActiveCamera.Projection;

            if (ParameterExists("World"))
                effect.Parameters["World"].SetValue(Matrix.CreateScale(ParentObject.Transform.Scale) * world);

            if (ParameterExists("View"))
                effect.Parameters["View"].SetValue(SystemCore.ActiveCamera.View);

            if (ParameterExists("Projection"))
                effect.Parameters["Projection"].SetValue(SystemCore.ActiveCamera.Projection);

            if (ParameterExists("WorldViewProjection"))
                effect.Parameters["WorldViewProjection"].SetValue(worldViewProj);
        }

        public virtual void AssignLightingParameters()
        {
            if (ParameterExists("AmbientLightColor"))
                effect.Parameters["AmbientLightColor"].SetValue(AmbientLightColor.ToVector4());

            if (ParameterExists("AmbientLightIntensity"))
                effect.Parameters["AmbientLightIntensity"].SetValue(AmbientLightIntensity);


            if (ParameterExists("ColorSaturation"))
                effect.Parameters["ColorSaturation"].SetValue(1);

            if (ParameterExists("DiffuseLightColor"))
                effect.Parameters["DiffuseLightColor"].SetValue(DiffuseLightColor.ToVector4());
            if (ParameterExists("DiffuseLightDirection"))
                effect.Parameters["DiffuseLightDirection"].SetValue(DiffuseLightDirection);
            if (ParameterExists("DiffuseLightIntensity"))
                effect.Parameters["DiffuseLightIntensity"].SetValue(DiffuseLightIntensity);

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