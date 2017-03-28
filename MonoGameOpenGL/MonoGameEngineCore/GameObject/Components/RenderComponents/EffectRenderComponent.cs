using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.GameObject.Components.RenderComponents;

namespace MonoGameEngineCore.GameObject.Components
{
    

    public class EffectRenderComponent : IComponent, IDrawable
    {

        public readonly Effect effect;
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
            AssignTextureParameters();
        }

        public virtual void PreDraw(GameTime gameTime)
        {
            AssignMatrixParameters();
            AssignLightingParameters();
            AssignMaterialParameters();
        }

        private void AssignTextureParameters()
        {
            //check it has a texture

            var material = ParentObject.GetComponent<MaterialComponent>();
            if(material != null)
            {
                if (!string.IsNullOrEmpty(material.TextureName))
                {
                    Texture2D t = SystemCore.ContentManager.Load<Texture2D>(material.TextureName);

                    if (ParameterExists("ModelTexture"))
                        effect.Parameters["ModelTexture"].SetValue(t);

                    if (ParameterExists("TextureIntensity"))
                        effect.Parameters["TextureIntensity"].SetValue(material.TextureIntensity);
                }
            }

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

            if (ParameterExists("CameraPosition"))
                effect.Parameters["CameraPosition"].SetValue(SystemCore.GetCamera(Camera).Position);

            if (ParameterExists("ViewInvert"))
                effect.Parameters["ViewInvert"].SetValue(Matrix.Invert(SystemCore.GetCamera(Camera).View));

            if(ParameterExists("WorldInverseTranspose"))
                effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(transform.WorldMatrix)));

            if(ParameterExists("ViewVector"))
                effect.Parameters["ViewVector"].SetValue((SystemCore.GetCamera(Camera).View).Forward);



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

        public virtual void AssignMaterialParameters()
        {
            MaterialComponent mat = ParentObject.GetComponent<MaterialComponent>();

            if(ParameterExists("Shininess"))
                effect.Parameters["Shininess"].SetValue(mat.Shininess);

            if(ParameterExists("SpecularLightColor"))
                effect.Parameters["SpecularLightColor"].SetValue(mat.SpecularColor.ToVector4());

            if (ParameterExists("SpecularLightIntensity"))
                effect.Parameters["SpecularLightIntensity"].SetValue(mat.SpecularIntensity);

            if (ParameterExists("DiffuseColor"))
                effect.Parameters["DiffuseColor"].SetValue(mat.MatColor.ToVector4());

            if (ParameterExists("DiffuseColorIntensity"))
                effect.Parameters["DiffuseColorIntensity"].SetValue(mat.MatColorIntensity);

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

       

        public event System.EventHandler<System.EventArgs> VisibleChanged;
        public event System.EventHandler<System.EventArgs> DrawOrderChanged;
    }

    public class Material
    {
        public Vector3 Ka { get; set; }
        public Vector3 Kd { get; set; }
        public Vector3 Ks { get; set; }
        public Texture2D DiffuseTexture { get; set; }
    }
}