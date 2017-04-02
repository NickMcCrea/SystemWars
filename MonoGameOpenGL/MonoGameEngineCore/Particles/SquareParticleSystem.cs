#region File Description
//-----------------------------------------------------------------------------
// FireParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Particle3DSample
{
    /// <summary>
    /// Custom particle system for creating a flame effect.
    /// </summary>
    public class SquareParticleSystem : ParticleSystem
    {
        public SquareParticleSystem()    
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "Textures//blank";

            settings.MaxParticles = 2400;

            settings.Duration = TimeSpan.FromSeconds(10);

            settings.DurationRandomness = 1;

            settings.MinHorizontalVelocity = -1;
            settings.MaxHorizontalVelocity = 1f;

            settings.MinVerticalVelocity = -1;
            settings.MaxVerticalVelocity = 1f;

            // Set gravity upside down, so the flames will 'fall' upward.
            settings.Gravity = new Vector3(0, 0, 0);

            settings.MinColor = Color.Green;
            settings.MaxColor = Color.Green;

            settings.MinStartSize = 0.1f;
            settings.MaxStartSize = 1;

            settings.MinEndSize = 0.5f;
            settings.MaxEndSize = 2f;

            settings.MinRotateSpeed = 0.1f;
            settings.MaxRotateSpeed = 0.5f;

            // Use additive blending.
            settings.BlendState = BlendState.Additive;
        }
    }
}
