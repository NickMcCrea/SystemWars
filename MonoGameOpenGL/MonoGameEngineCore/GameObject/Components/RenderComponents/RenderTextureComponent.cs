using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameEngineCore.GameObject.Components
{
    public class RenderTextureComponent : IComponent, IDrawable
    {
        public readonly Effect effect;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }
        public Texture2D Texture2D { get; set; }
        public Color BorderColor { get; set; }
        public float BorderSize { get; set; }

        public string Camera { get; set; }

        public RenderTextureComponent(Effect effect)
        {
            this.effect = effect;
            Visible = true;
            Camera = "main";
            BorderSize = 0.01f;
            BorderColor = Color.White;
        }

        public virtual void PreDraw(GameTime gameTime)
        {
            AssignMatrixParameters();        
            AssignTextureParameters();   
        }

        private void AssignTextureParameters()
        {
            if (ParameterExists("surfaceTexture"))
            {
                effect.Parameters["surfaceTexture"].SetValue(Texture2D);
            }
            if (ParameterExists("borderColor"))
            {
                effect.Parameters["borderColor"].SetValue(BorderColor.ToVector4());
            }
            if (ParameterExists("borderSize"))
            {
                effect.Parameters["borderSize"].SetValue(BorderSize);
            }
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


            for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
            {
                effect.CurrentTechnique.Passes[i].Apply();
                SystemCore.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, renderGeometry.VertexBuffer.VertexCount, 0, renderGeometry.PrimitiveCount);
            }

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

        public event System.EventHandler<System.EventArgs> VisibleChanged;
        public event System.EventHandler<System.EventArgs> DrawOrderChanged;


        public void Initialise()
        {
            
        }
    }
}