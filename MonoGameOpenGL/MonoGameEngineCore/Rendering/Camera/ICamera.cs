using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.Camera
{
    public interface ICamera
    {
        Matrix View { get; set; }
        Matrix Projection { get; set; }
        Vector3 Position { get; }
        
    }
}
