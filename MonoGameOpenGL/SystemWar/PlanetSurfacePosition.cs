using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;

namespace SystemWar
{
    public class PlanetSurfacePosition
    {
        public PlanetSurfacePosition()
        {
            Latitude = 0;
            Longitude = 0;
            Angle = 0;
            Altitude = 0;
        }

        public float Latitude;
        public float Longitude;
        public float Angle;
        public float Altitude;

        public Vector3 GetPosition(Planet planet, HighPrecisionPosition cameraPosition)
        {
         
            //let's make Vector3.Forward our Greenwich at 0,0, with 360 degree on longitude, and 180 on latitude.


            Vector3 forwardPointOnSurface = planet.Transform.WorldMatrix.Forward;


            Vector3 rotateLatitude = Vector3.Transform(forwardPointOnSurface,
                Matrix.CreateFromAxisAngle(planet.Transform.WorldMatrix.Right, MathHelper.ToRadians(Latitude)));

            Vector3 rotateLongAndLat = Vector3.Transform(rotateLatitude,
                Matrix.CreateFromAxisAngle(planet.Transform.WorldMatrix.Up, MathHelper.ToRadians(Longitude)));




            return planet.Transform.WorldMatrix.Translation +  rotateLongAndLat*(planet.radius + Altitude);


        }
    }

    

    public class PlanetStructure : GameObject, IUpdateable
    {
        private PlanetSurfacePosition planetSurfacePosition;
        private Planet planet;

        public PlanetStructure(PlanetSurfacePosition position, Planet planet)
        {
            this.planetSurfacePosition = position;
            this.planet = planet;
            Enabled = true;
        }

        public bool Enabled { get; set; }
        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
          
            //calculate the position of the building given its lat / long.

        }

        public int UpdateOrder { get; set; }
        public event EventHandler<EventArgs> UpdateOrderChanged;

        
    }
}
