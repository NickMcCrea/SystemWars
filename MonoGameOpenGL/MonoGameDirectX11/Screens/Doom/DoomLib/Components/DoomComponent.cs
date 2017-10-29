using Microsoft.Xna.Framework;
using MonoGameEngineCore.DoomLib;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using NickLib.Pathfinding;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoGameDirectX11.Screens.Doom.DoomLib
{
    public class DoomComponent : IComponent
    {

        public string DoomType { get; set; }
        public double Health { get; set; }
        public double Angle { get; set; }
        public double Distance { get; set; }
        public bool ForwardHitVector { get; set; }
        public bool LeftHitVector { get; set; }
        public bool RightHightVector { get; set; }
        public float HitVectorSize { get; set; }
        public GameObject ParentObject
        {
            get; set;
        }

        public void Initialise()
        {

        }

        public void PostInitialise()
        {

        }
    }

  
   
}
