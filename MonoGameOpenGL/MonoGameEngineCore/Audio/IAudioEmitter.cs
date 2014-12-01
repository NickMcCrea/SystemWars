#region File Description
//-----------------------------------------------------------------------------
// IAudioEmitter.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements

using Microsoft.Xna.Framework;

#endregion

namespace MonoGameEngineCore.Audio
{
    /// <summary>
    /// Interface used by the AudioManager to look up the position
    /// and velocity of entities that can emit 3D sounds.
    /// </summary>
    public interface IAudioEmitter
    {
        Vector3 Position { get; set; }
        Vector3 Forward { get; set; }
        Vector3 Up { get; set; }
        Vector3 Velocity { get; set; }
    }
}