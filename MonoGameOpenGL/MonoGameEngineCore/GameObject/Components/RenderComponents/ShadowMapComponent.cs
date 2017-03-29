using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject.Components
{
    public class ShadowMapComponent 
    {
        public readonly Effect effect;
        public GameObject ParentObject { get; set; }
        public int DrawOrder { get; set; }
        public bool Visible { get; set; }
        public RenderTarget2D ShadowMapTarget { get; set; }
     
       
        public ShadowMapComponent()
        {
            this.effect = EffectLoader.LoadEffect("ShadowMap");
            Visible = true;

            ShadowMapTarget = new RenderTarget2D(SystemCore.GraphicsDevice, 2048, 2048, false, 
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);
        }

        


        public void PreDraw(GameTime gameTime)
        {
            if (!Visible)
                return;

            if (SystemCore.ActiveScene.LightsInScene.Count == 0)
                return;

            DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;
            Vector3 dirLight = light.LightDirection;
            Vector3 lightpos = -dirLight * 100f;

            Matrix lightView = Matrix.CreateLookAt(lightpos,
                        lightpos + dirLight,
                        Vector3.Up);

            Matrix lightProjection = Matrix.CreateOrthographic(SystemCore.GraphicsDevice.Viewport.Width, 
                SystemCore.GraphicsDevice.Viewport.Height, SystemCore.ActiveCamera.NearZ, SystemCore.ActiveCamera.FarZ);

            Matrix lightViewProjection = lightView * lightProjection;


            SystemCore.GraphicsDevice.SetRenderTarget(ShadowMapTarget);

        }

      

       
        

    }
}