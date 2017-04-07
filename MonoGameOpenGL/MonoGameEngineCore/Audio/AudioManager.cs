#region File Description
//-----------------------------------------------------------------------------
// AudioManager.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

#endregion

namespace MonoGameEngineCore.Audio
{
    /// <summary>
    /// Audio manager keeps track of what 3D sounds are playing, updating
    /// their settings as the camera and entities move around the world,
    /// and automatically disposing cue instances after they finish playing.
    /// </summary>
    public class AudioManager : IGameSubSystem
    {
        private Dictionary<string, SoundEffect> sounds; 

        public AudioManager()
        {
            sounds = new Dictionary<string, SoundEffect>();
        }



        /// <summary>
        /// Updates the state of the 3D audio system.
        /// </summary>
        public void Update(GameTime gameTime)
        {
        


        }

        public void AddSound(string cueName, string fileName)
        {
            SoundEffect effect = SystemCore.ContentManager.Load<SoundEffect>(fileName);
            sounds.Add(cueName,effect);
        }

        public void Initalise()
        {
            
        }

        public void OnRemove()
        {

        }

        public void Render(GameTime gameTime)
        {
            
        }

        public void PlaySound(string soundName)
        {
            if (sounds.ContainsKey(soundName))
                sounds[soundName].Play();
        }
    }
}