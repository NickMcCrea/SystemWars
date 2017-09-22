using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.GameObject.Components.RenderComponents;
using System;

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

        public void PostInitialise()
        {

        }

        public virtual void PreDraw(GameTime gameTime)
        {
            AssignMatrixParameters();
            AssignLightingParameters();
            AssignMaterialParameters();

            if (ParameterExists("time"))
                effect.Parameters["time"].SetValue((float)gameTime.ElapsedGameTime.TotalSeconds);
        }

        private void AssignTextureParameters()
        {
            //check it has a texture

            var material = ParentObject.GetComponent<MaterialComponent>();
            if (material != null)
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
            Matrix worldViewProj = Matrix.CreateScale(ParentObject.Transform.Scale) * transform.AbsoluteTransform;
            worldViewProj *= SystemCore.GetCamera(Camera).View;
            worldViewProj *= SystemCore.GetCamera(Camera).Projection;

            if (ParameterExists("World"))
                effect.Parameters["World"].SetValue(Matrix.CreateScale(ParentObject.Transform.Scale) * transform.AbsoluteTransform);

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

            if (ParameterExists("WorldInverseTranspose"))
                effect.Parameters["WorldInverseTranspose"].SetValue(Matrix.Transpose(Matrix.Invert(transform.AbsoluteTransform)));

            if (ParameterExists("ViewVector"))
                effect.Parameters["ViewVector"].SetValue((SystemCore.GetCamera(Camera).View.Forward));


            if (ParameterExists("LightViewProj"))
                effect.Parameters["LightViewProj"].SetValue(SystemCore.ShadowMapRenderer.LightViewProj);

            if (ParameterExists("CameraDirection"))
                effect.Parameters["CameraDirection"].SetValue(SystemCore.ActiveCamera.View.Forward);

            if (ParameterExists("CameraPosition"))
                effect.Parameters["CameraPosition"].SetValue(SystemCore.ActiveCamera.Position);


            if (ParameterExists("FogEnabled"))
                effect.Parameters["FogEnabled"].SetValue(SystemCore.ActiveScene.FogEnabled);

            if (ParameterExists("c"))
                effect.Parameters["c"].SetValue(SystemCore.ActiveScene.FogC);

            if (ParameterExists("b"))
                effect.Parameters["b"].SetValue(SystemCore.ActiveScene.FogB);

            if (ParameterExists("fogColor"))
                effect.Parameters["fogColor"].SetValue(SystemCore.ActiveScene.FogColor.ToVector3());




        }

        public virtual void AssignLightingParameters()
        {
            if (SystemCore.ActiveScene.AmbientLight != null)
            {
                if (ParameterExists("AmbientLightColor"))
                    effect.Parameters["AmbientLightColor"].SetValue(SystemCore.ActiveScene.AmbientLight.LightColor.ToVector4());

                if (ParameterExists("AmbientLightIntensity"))
                    effect.Parameters["AmbientLightIntensity"].SetValue(SystemCore.ActiveScene.AmbientLight.LightIntensity);

            }

            if (ParameterExists("ColorSaturation"))
                effect.Parameters["ColorSaturation"].SetValue(ColorSaturation);

            foreach (SceneLight light in SystemCore.ActiveScene.LightsInScene)
            {
                if (light is DiffuseLight)
                {
                    var diffLight = light as DiffuseLight;
                    AddDiffuseLight(diffLight);

                    if (diffLight.DiffuseType == DiffuseLightType.Key)
                    {
                        if (diffLight.IsShadowCasting)
                        {
                            if (ParameterExists("ShadowMap"))
                                effect.Parameters["ShadowMap"].SetValue(SystemCore.ShadowMapRenderer.ShadowMapTarget);
                        }

                        if (ParameterExists("ShadowsEnabled"))
                            effect.Parameters["ShadowsEnabled"].SetValue(diffLight.IsShadowCasting);

                    }
                }

                if (light is PointLight)
                {
                    AddPointLight(light as PointLight);
                }
            }

        }

        private void AddPointLight(PointLight light)
        {
            if (light.Number == PointLightNumber.One)
            {
                if (ParameterExists("Point1Position"))
                    effect.Parameters["Point1Position"].SetValue(light.Position);
                if (ParameterExists("Point1Color"))
                    effect.Parameters["Point1Color"].SetValue(light.LightColor.ToVector4());
                if (ParameterExists("Point1FallOffDistance"))
                    effect.Parameters["Point1FallOffDistance"].SetValue(light.FallOffEnd);
                if (ParameterExists("Point1FullPowerDistance"))
                    effect.Parameters["Point1FullPowerDistance"].SetValue(light.FallOffStart);
                if (ParameterExists("Point1Intensity"))
                    effect.Parameters["Point1Intensity"].SetValue(light.LightIntensity);
            }
            if (light.Number == PointLightNumber.Two)
            {
                if (ParameterExists("Point2Position"))
                    effect.Parameters["Point2Position"].SetValue(light.Position);
                if (ParameterExists("Point2Color"))
                    effect.Parameters["Point2Color"].SetValue(light.LightColor.ToVector4());
                if (ParameterExists("Point2FallOffDistance"))
                    effect.Parameters["Point2FallOffDistance"].SetValue(light.FallOffEnd);
                if (ParameterExists("Point2FullPowerDistance"))
                    effect.Parameters["Point2FullPowerDistance"].SetValue(light.FallOffStart);
                if (ParameterExists("Point2Intensity"))
                    effect.Parameters["Point2Intensity"].SetValue(light.LightIntensity);
            }
            if (light.Number == PointLightNumber.Three)
            {
                if (ParameterExists("Point3Position"))
                    effect.Parameters["Point3Position"].SetValue(light.Position);
                if (ParameterExists("Point3Color"))
                    effect.Parameters["Point3Color"].SetValue(light.LightColor.ToVector4());
                if (ParameterExists("Point3FallOffDistance"))
                    effect.Parameters["Point3FallOffDistance"].SetValue(light.FallOffEnd);
                if (ParameterExists("Point3FullPowerDistance"))
                    effect.Parameters["Point3FullPowerDistance"].SetValue(light.FallOffStart);
                if (ParameterExists("Point3Intensity"))
                    effect.Parameters["Point3Intensity"].SetValue(light.LightIntensity);
            }
            if (light.Number == PointLightNumber.Four)
            {
                if (ParameterExists("Point4Position"))
                    effect.Parameters["Point4Position"].SetValue(light.Position);
                if (ParameterExists("Point4Color"))
                    effect.Parameters["Point4Color"].SetValue(light.LightColor.ToVector4());
                if (ParameterExists("Point4FallOffDistance"))
                    effect.Parameters["Point4FallOffDistance"].SetValue(light.FallOffEnd);
                if (ParameterExists("Point4FullPowerDistance"))
                    effect.Parameters["Point4FullPowerDistance"].SetValue(light.FallOffStart);
                if (ParameterExists("Point4Intensity"))
                    effect.Parameters["Point4Intensity"].SetValue(light.LightIntensity);
            }

        }

        public virtual void AssignMaterialParameters()
        {
            MaterialComponent mat = ParentObject.GetComponent<MaterialComponent>();

            if (mat == null)
                return;

            if (ParameterExists("Shininess"))
                effect.Parameters["Shininess"].SetValue(mat.Shininess);

            if (ParameterExists("SpecularLightColor"))
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
            if (diffuseLight.DiffuseType == DiffuseLightType.Key)
            {
                if (ParameterExists("DiffuseLightColor"))
                    effect.Parameters["DiffuseLightColor"].SetValue(diffuseLight.LightColor.ToVector4());
                if (ParameterExists("DiffuseLightDirection"))
                    effect.Parameters["DiffuseLightDirection"].SetValue(Vector3.Normalize(diffuseLight.LightDirection));
                if (ParameterExists("DiffuseLightIntensity"))
                    effect.Parameters["DiffuseLightIntensity"].SetValue(diffuseLight.LightIntensity);
            }
            if (diffuseLight.DiffuseType == DiffuseLightType.Fill)
            {
                if (ParameterExists("Diffuse2LightColor"))
                    effect.Parameters["Diffuse2LightColor"].SetValue(diffuseLight.LightColor.ToVector4());
                if (ParameterExists("Diffuse2LightDirection"))
                    effect.Parameters["Diffuse2LightDirection"].SetValue(Vector3.Normalize(diffuseLight.LightDirection));
                if (ParameterExists("Diffuse2LightIntensity"))
                    effect.Parameters["Diffuse2LightIntensity"].SetValue(diffuseLight.LightIntensity);
            }
            if (diffuseLight.DiffuseType == DiffuseLightType.Back)
            {
                if (ParameterExists("Diffuse3LightColor"))
                    effect.Parameters["Diffuse3LightColor"].SetValue(diffuseLight.LightColor.ToVector4());
                if (ParameterExists("Diffuse3LightDirection"))
                    effect.Parameters["Diffuse3LightDirection"].SetValue(Matrix.Invert(SystemCore.ActiveCamera.View).Forward);
                if (ParameterExists("Diffuse3LightIntensity"))
                    effect.Parameters["Diffuse3LightIntensity"].SetValue(diffuseLight.LightIntensity);
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
    }

    public class Material
    {
        public Vector3 Ka { get; set; }
        public Vector3 Kd { get; set; }
        public Vector3 Ks { get; set; }
        public Texture2D DiffuseTexture { get; set; }
    }
}