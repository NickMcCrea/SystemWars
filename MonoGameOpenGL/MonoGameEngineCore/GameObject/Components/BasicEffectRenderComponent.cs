using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;

namespace MonoGameEngineCore.GameObject.Components
{
    public class BasicEffectRenderComponent : IComponent, IDrawable
    {
        private readonly BasicEffect effect;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }
     
        public BasicEffectRenderComponent(BasicEffect effect)
        {
            this.effect = effect;
            Visible = true;
        }

        public void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            var transform = ParentObject.GetComponent<TransformComponent>();
            var renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();

            effect.World = Matrix.CreateScale(ParentObject.Transform.Scale)  * transform.WorldMatrix;
            effect.View = SystemCore.ActiveCamera.View;
            effect.Projection = SystemCore.ActiveCamera.Projection;

            SystemCore.GraphicsDevice.SetVertexBuffer(renderGeometry.VertexBuffer);
            SystemCore.GraphicsDevice.Indices = renderGeometry.IndexBuffer;

            GameObjectManager.verts += renderGeometry.VertexBuffer.VertexCount;
            GameObjectManager.primitives += renderGeometry.PrimitiveCount;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                SystemCore.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, renderGeometry.VertexBuffer.VertexCount, 0, renderGeometry.PrimitiveCount);
            }
        }

        public event System.EventHandler<System.EventArgs> DrawOrderChanged;

        public event System.EventHandler<System.EventArgs> VisibleChanged;

        public void Initialise()
        {
            
        }
    }
}