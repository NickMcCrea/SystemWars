using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject.Components
{
    public class ShadowMapRenderTarget
    {
          public RenderTarget2D ShadowMapTarget { get; set; }
        public Matrix LightViewProj { get; set; }
        public bool ShadowPass { get; private set; }

        public ShadowMapRenderTarget()
        {
            
            ShadowMapTarget = new RenderTarget2D(SystemCore.GraphicsDevice, 512, 512, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
        }


        public void PreDraw(GameTime gameTime)
        {
        
            if (SystemCore.ActiveScene.LightsInScene.Count == 0)
                return;

            ShadowPass = true;

            DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;

            Vector3 lightPos = new Vector3(100, 0, 0);

            Matrix lightView = Matrix.CreateLookAt(lightPos, lightPos - light.LightDirection, Vector3.Up);

            Matrix lightProjection = Matrix.CreateOrthographic(SystemCore.GraphicsDevice.Viewport.Width,
                SystemCore.GraphicsDevice.Viewport.Height, SystemCore.ActiveCamera.NearZ, SystemCore.ActiveCamera.FarZ);

            LightViewProj = lightView * lightProjection;


            SystemCore.GraphicsDevice.SetRenderTarget(ShadowMapTarget);

        }

        public void PostDraw(GameTime gameTime)
        {
            ShadowPass = false;
            SystemCore.GraphicsDevice.SetRenderTarget(null);
        }

    }


    public class ShadowRenderComponent : IComponent
    {
        public readonly Effect effect;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }

        public ShadowRenderComponent()
        {
            this.effect = EffectLoader.LoadEffect("ShadowMap");

            Visible = true;
        }

       
        public virtual void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            Matrix lightViewProj = SystemCore.shadowMapComponent.LightViewProj;
            effect.Parameters["LightWorldViewProjection"].SetValue(Matrix.CreateScale(ParentObject.Transform.Scale) * ParentObject.Transform.WorldMatrix * lightViewProj);


            var renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();

            if (renderGeometry != null)
            {

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

            var modelComponent = ParentObject.GetComponent<ModelComponent>();
            if (modelComponent != null)
            {
                Model m = modelComponent.Model;
                foreach (var mesh in m.Meshes)
                {
                    foreach (ModelMeshPart p in mesh.MeshParts)
                    {
                        SystemCore.GraphicsDevice.SetVertexBuffer(p.VertexBuffer);
                        SystemCore.GraphicsDevice.Indices = p.IndexBuffer;

                        GameObjectManager.verts += p.VertexBuffer.VertexCount;
                        GameObjectManager.primitives += p.IndexBuffer.IndexCount;

                        for (int i = 0; i < effect.CurrentTechnique.Passes.Count; i++)
                        {
                            effect.CurrentTechnique.Passes[i].Apply();
                            SystemCore.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, p.PrimitiveCount);
                        }
                    }
                }
            }


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