using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.ScreenManagement;
using Particle3DSample;
using MonoGameEngineCore;

namespace MonoGameDirectX11.Screens
{
    public class ParticleTestScreen : MouseCamScreen
    {

        ParticleSystem testParticleSystem;

        public ParticleTestScreen()
            : base()
        {
            var testSettings = new ParticleSettings();
            testSettings.TextureName = "smoke";
            testParticleSystem = new ParticleSystem(testSettings);
            testParticleSystem.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            testParticleSystem.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
            testParticleSystem.SetCamera(SystemCore.ActiveCamera.View, SystemCore.ActiveCamera.Projection);
            testParticleSystem.Draw(gameTime);
            base.Render(gameTime);
        }
    }
}
