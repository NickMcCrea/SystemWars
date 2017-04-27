using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject.Components
{
    public class ShadowMapRenderer
    {
        public RenderTarget2D ShadowMapTarget { get; set; }
        public Matrix LightViewProj { get; set; }
        public bool ShadowPass { get; private set; }

        public ShadowMapRenderer()
        {

            ShadowMapTarget = new RenderTarget2D(SystemCore.GraphicsDevice, 2048, 2048, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
        }


        public void PreDraw(GameTime gameTime)
        {

            if (SystemCore.ActiveScene.LightsInScene.Count == 0)
                return;
         

            SystemCore.GraphicsDevice.BlendState = BlendState.Opaque;
            SystemCore.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            ShadowPass = true;

            DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;

            if (light == null)
                return;

            Matrix lightRotation = Matrix.CreateLookAt(Vector3.Zero, Vector3.Normalize(-light.LightDirection), Vector3.Up);


            // Get the corners of the frustum
            Vector3[] frustumCorners = new BoundingFrustum(SystemCore.ActiveCamera.View
                * SystemCore.ActiveCamera.Projection).GetCorners();

            // Transform the positions of the corners into the direction of the light
            for (int i = 0; i < frustumCorners.Length; i++)
            {
                frustumCorners[i] = Vector3.Transform(frustumCorners[i], lightRotation);
            }

            // Find the smallest box around the points
            BoundingBox lightBox = BoundingBox.CreateFromPoints(frustumCorners);

            Vector3 boxSize = lightBox.Max - lightBox.Min;
            Vector3 halfBoxSize = boxSize * 0.5f;

            // The position of the light should be in the center of the back
            // pannel of the box. 
            Vector3 lightPosition = lightBox.Min + halfBoxSize;
            lightPosition.Z = lightBox.Min.Z;

            lightPosition = Vector3.Transform(lightPosition,
                                              Matrix.Invert(lightRotation));

            // Create the view matrix for the light
            Matrix lightView = Matrix.CreateLookAt(lightPosition,
                                                   lightPosition - light.LightDirection,
                                                   Vector3.Up);

            // Create the projection matrix for the light
            // The projection is orthographic since we are using a directional light
            Matrix lightProjection = Matrix.CreateOrthographic(boxSize.X, boxSize.Y,
                                                               -boxSize.Z, boxSize.Z);

            LightViewProj = lightView * lightProjection;


            SystemCore.GraphicsDevice.SetRenderTarget(ShadowMapTarget);

            // Clear the render target to white or all 1's
            // We set the clear to white since that represents the 
            // furthest the object could be away
            SystemCore.GraphicsDevice.Clear(Color.White);

        }

        public void PostDraw(GameTime gameTime)
        {
            ShadowPass = false;
            SystemCore.GraphicsDevice.SetRenderTarget(null);

        }

    }


    public class ShadowCasterComponent : IComponent
    {
        public readonly Effect effect;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }

        public ShadowCasterComponent()
        {
            this.effect = EffectLoader.LoadEffect("ShadowMap");

            Visible = true;
        }

        public void PostInitialise()
        {

        }


        public virtual void Draw(GameTime gameTime)
        {
            if (!Visible)
                return;

            Matrix lightViewProj = SystemCore.ShadowMapRenderer.LightViewProj;
            effect.Parameters["LightWorldViewProjection"].SetValue(Matrix.CreateScale(ParentObject.Transform.Scale) * ParentObject.Transform.AbsoluteTransform * lightViewProj);


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