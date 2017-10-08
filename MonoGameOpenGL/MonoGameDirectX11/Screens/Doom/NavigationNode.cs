using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;

namespace NickLib.Pathfinding
{
    public class NavigationNode
    {
     
        public List<NavigationNode> Neighbours { get; set; }
        public bool done;
        public NavigationNode()
        {
            Neighbours = new List<NavigationNode>();
        }

        public bool Navigable { get; set; }
        public Vector3 WorldPosition { get; set; }
    }
}
