using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.GameObject.Components
{
    public class TransformComponent : IComponent, IUpdateable
    {
        public Matrix WorldMatrix;
        public Vector3 Velocity;
        public float Scale { get; set; }
        public GameObject ParentObject { get; set; }
        private bool highPrecisionmode = false;
        private HighPrecisionPosition highPrecisionPosition;

        public TransformComponent()
        {
            WorldMatrix = Matrix.Identity;
            Scale = 1f;
            Enabled = true;
        }

        public void Initialise()
        {
            //if a high precisionposition component is detected, the transform will assume a coordinate system where the camera 
            //is always centered at zero, and transformations will behave accordingly.
            highPrecisionPosition = ParentObject.GetComponent<HighPrecisionPosition>();
            if (highPrecisionPosition != null)
                highPrecisionmode = true;
        }

        public void Rotate(Vector3 axis, float amount)
        {
            Vector3 pos = WorldMatrix.Translation;
            WorldMatrix.Translation = Vector3.Zero;
            WorldMatrix = WorldMatrix *= Matrix.CreateFromAxisAngle(axis, amount);
            WorldMatrix.Translation = pos;
        }

        public void RotateAround(Vector3 axis, Vector3 OrbitPoint, float amount)
        {

            WorldMatrix.Translation -= OrbitPoint;
            WorldMatrix = WorldMatrix *= Matrix.CreateFromAxisAngle(axis, amount);
            WorldMatrix.Translation += OrbitPoint;
        }

        public void SetPosition(Vector3d position)
        {
            if (highPrecisionPosition == null)
                highPrecisionPosition = ParentObject.GetComponent<HighPrecisionPosition>();

            highPrecisionPosition.Position = position;

         
        }

        public void SetPosition(Vector3 position)
        {
            if (highPrecisionmode)
            {
               highPrecisionPosition.Position = new Vector3d(position);
            }
            else
                WorldMatrix.Translation = position;
        }

        public void Translate(Vector3 translation)
        {
            if (highPrecisionmode)
            {
                highPrecisionPosition.Position += translation;
            }
            else
                WorldMatrix.Translation += translation;

        }

        public bool Enabled { get; set; }

        public event System.EventHandler<System.EventArgs> EnabledChanged;

        public event System.EventHandler<System.EventArgs> UpdateOrderChanged;

        public void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                Vector3 currentPos = WorldMatrix.Translation;
                currentPos += Velocity * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                WorldMatrix.Translation = currentPos;
            }
        }

        public int UpdateOrder { get; set; }

        public void MoveUp(float movementAmount)
        {
            Vector3 pos = WorldMatrix.Translation;
            pos.Y += movementAmount;
            WorldMatrix.Translation = pos;
        }

        public void MoveDown(float movementAmount)
        {
            Vector3 pos = WorldMatrix.Translation;
            pos.Y -= movementAmount;
            WorldMatrix.Translation = pos;
        }


    }
}
