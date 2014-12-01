using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.GameObject
{
    public interface IComponent
    {
        GameObject ParentObject { get; set; }
        void Initialise();
    }
}
