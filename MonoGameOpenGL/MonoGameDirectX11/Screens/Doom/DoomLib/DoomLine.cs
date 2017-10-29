using Microsoft.Xna.Framework;
using MonoGameDirectX11.Screens.Doom.DoomLib;

namespace MonoGameDirectX11.Screens.Doom
{


    public struct DoomLine : IPartitionItem
    {
        public Vector3 start;
        public Vector3 end;
        public Color color;
        public bool BlocksLineOfSight;
        public string Type { get { return "DoomLine"; } }
        public Vector3 MidPoint()
        {
            return (start + end) / 2;
        }



    }


}
