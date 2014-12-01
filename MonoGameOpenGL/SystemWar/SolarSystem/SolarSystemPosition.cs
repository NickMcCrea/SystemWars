using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SystemWar.SolarSystem
{


 

    public class SolarSystemPlaneteryBody : HighPrecisionPosition, IComponent, IUpdateable
    {
      
        public SolarSystemPlaneteryBody ParentBody { get; set; }
        public List<SolarSystemPlaneteryBody> Children;
        private Vector3d orbitDirection;
        private float rotationSpeed;
        private float orbitSpeed;
     
        public SolarSystemPlaneteryBody(float rotationSpeed, Vector3d orbitDirection, float orbitSpeed)
        {
            this.rotationSpeed = rotationSpeed;
            this.orbitDirection = orbitDirection;
            this.orbitSpeed = orbitSpeed;
            Children = new List<SolarSystemPlaneteryBody>();
            Enabled = true;
        }

        public void Update(GameTime gameTime)
        {

            base.Update(gameTime);
        }

    

    }
}
