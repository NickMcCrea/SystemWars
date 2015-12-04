using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject.Components
{
    public class LineRenderComponent : IComponent, IDrawable
    {
        private readonly BasicEffect effect;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }

        public LineRenderComponent(BasicEffect effect)
        {
            this.effect = effect;
            effect.EnableDefaultLighting();
            effect.VertexColorEnabled = true;
            effect.PreferPerPixelLighting = true;
            effect.DirectionalLight1.Enabled = false;
            effect.DirectionalLight2.Enabled = false;
            effect.AmbientLightColor = SystemCore.ActiveScene.AmbientLight.LightColor.ToVector3();
            effect.AmbientLightColor *= 0.1f;
            effect.FogEnabled = true;
            effect.FogColor = Color.CornflowerBlue.ToVector3();
            effect.FogStart = 50;
            effect.FogEnd = 1000;
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


            DiffuseLight sun = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;
            effect.DirectionalLight0.Direction = sun.LightDirection;
            effect.DirectionalLight0.DiffuseColor = sun.LightColor.ToVector3();
      
        
            SystemCore.GraphicsDevice.SetVertexBuffer(renderGeometry.VertexBuffer);
            SystemCore.GraphicsDevice.Indices = renderGeometry.IndexBuffer;

            GameObjectManager.verts += renderGeometry.VertexBuffer.VertexCount;
            GameObjectManager.primitives += renderGeometry.PrimitiveCount;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                SystemCore.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0, renderGeometry.VertexBuffer.VertexCount, 0, renderGeometry.PrimitiveCount);
            }
        }

        public event System.EventHandler<System.EventArgs> DrawOrderChanged;

        public event System.EventHandler<System.EventArgs> VisibleChanged;

        public void Initialise()
        {
            
        }
    }
}