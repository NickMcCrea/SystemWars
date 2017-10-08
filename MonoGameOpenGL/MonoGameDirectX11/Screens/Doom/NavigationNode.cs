using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml.Serialization;
using MonoGameDirectX11.Screens;

namespace NickLib.Pathfinding
{
    public class NavigationNode : IPartitionItem
    {
     
        public List<NavigationNode> Neighbours { get; set; }
        public string Type { get { return "NavigationNode"; } }
        public bool done;
        public NavigationNode()
        {
            Neighbours = new List<NavigationNode>();
        }

        public bool Navigable { get; set; }
        public Vector3 WorldPosition { get; set; }
    }
}
