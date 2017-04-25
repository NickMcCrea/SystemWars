using System;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.GameObject.Components
{
   
    public class OrbiterComponent : IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        public float RotationSpeed { get; set; }
        public Vector3 Axis { get; set; }
        public Vector3 OrbitPoint { get; set; }
        public OrbiterComponent(Vector3 axis, Vector3 orbitPoint)
        {
            Enabled = true;
            this.OrbitPoint = orbitPoint;
            RotationSpeed = 0.001f;
            Axis = axis;
        }

        public OrbiterComponent(Vector3 axis, Vector3 orbitPoint, float rotationSpeed)
            : this(axis,orbitPoint)
        {
            RotationSpeed = rotationSpeed;
        }

        public void Update(GameTime gameTime)
        {
            ParentObject.Transform.RotateAround(Axis, OrbitPoint, RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        public bool Enabled { get; private set; }
        public int UpdateOrder { get; private set; }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;


        public void Initialise()
        {

        }
    }

    public class RotatorComponent : IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        public float RotationSpeed { get; set; }
        public Vector3 Axis { get; set; }

        public RotatorComponent(Vector3 axis)
        {
            Enabled = true;
            RotationSpeed = 0.001f;
            Axis = axis;
        }

        public RotatorComponent(Vector3 axis, float rotationSpeed)
            : this(axis)
        {
            RotationSpeed = rotationSpeed;
        }

        public void Update(GameTime gameTime)
        {
            ParentObject.Transform.Rotate(Axis,RotationSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        public bool Enabled { get; private set; }
        public int UpdateOrder { get; private set; }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;


        public void Initialise()
        {

        }
    }

   
    public class TranslatorComponent : IComponent, IUpdateable
    {
        public GameObject ParentObject { get; set; }
        public float Speed { get; set; }
        public Vector3 Direction { get; set; }

        public TranslatorComponent(Vector3 direction)
        {
            Enabled = true;
            Speed = 0.001f;
            Direction = direction;
            Direction.Normalize();
        }

        public TranslatorComponent(Vector3 direction, float rotationSpeed)
            : this(direction)
        {
            Speed = rotationSpeed;
        }

        public void Update(GameTime gameTime)
        {
            ParentObject.Transform.Translate(Direction * Speed * (float)gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        public bool Enabled { get; private set; }
        public int UpdateOrder { get; private set; }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;


        public void Initialise()
        {

        }
    }


}