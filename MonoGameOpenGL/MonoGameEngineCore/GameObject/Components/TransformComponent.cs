using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.GameObject.Components
{
    public class TransformComponent : IComponent, IUpdateable
    {
        public Matrix AbsoluteTransform;
        public Matrix RelativeTransform;
        public Vector3 Velocity;
        public float Scale { get; set; }
        public GameObject ParentObject { get; set; }

        private HighPrecisionPosition highPrecisionPosition;

        public TransformComponent()
        {
            AbsoluteTransform = Matrix.Identity;
            RelativeTransform = Matrix.Identity;
            Scale = 1f;
            Enabled = true;
        }

        public void Initialise()
        {
            //if a high precisionposition component is detected, the transform will assume a coordinate system where the camera 
            //is always centered at zero, and transformations will behave accordingly.
            highPrecisionPosition = ParentObject.GetComponent<HighPrecisionPosition>();
           
        }

        public void Rotate(Vector3 axis, float amount)
        {
            if (ParentObject.ParentGameObject == null)
            {
                Vector3 pos = AbsoluteTransform.Translation;
                AbsoluteTransform.Translation = Vector3.Zero;
                AbsoluteTransform = AbsoluteTransform *= Matrix.CreateFromAxisAngle(axis, amount);
                AbsoluteTransform.Translation = pos;
            }
            else
            {
                Vector3 pos = RelativeTransform.Translation;
                RelativeTransform.Translation = Vector3.Zero;
                RelativeTransform = RelativeTransform *= Matrix.CreateFromAxisAngle(axis, amount);
                RelativeTransform.Translation = pos;
            }


        }

        public void RotateAround(Vector3 axis, Vector3 OrbitPoint, float amount)
        {

            AbsoluteTransform.Translation -= OrbitPoint;
            AbsoluteTransform = AbsoluteTransform *= Matrix.CreateFromAxisAngle(axis, amount);
            AbsoluteTransform.Translation += OrbitPoint;


        }

        public void SetPosition(Vector3 position)
        {

            Vector3 oldPos = AbsoluteTransform.Translation;
            AbsoluteTransform.Translation = position;

            if (ParentObject.ContainsComponent<PhysicsComponent>())
                ParentObject.GetComponent<PhysicsComponent>().SetPosition(position);

        }

        public void Translate(Vector3 translation)
        {

            AbsoluteTransform.Translation += translation;

            if (ParentObject.ContainsComponent<PhysicsComponent>())
                ParentObject.GetComponent<PhysicsComponent>().SetPosition(AbsoluteTransform.Translation);


        }

        public bool Enabled { get; set; }

        public event System.EventHandler<System.EventArgs> EnabledChanged;

        public event System.EventHandler<System.EventArgs> UpdateOrderChanged;

        public void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                Vector3 currentPos = AbsoluteTransform.Translation;
                currentPos += Velocity * (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                AbsoluteTransform.Translation = currentPos;


                if(ParentObject.ParentGameObject != null)
                {
                    AbsoluteTransform = RelativeTransform * ParentObject.ParentGameObject.Transform.AbsoluteTransform;
                }
            }
        }

        public int UpdateOrder { get; set; }

        public void MoveUp(float movementAmount)
        {
            Vector3 pos = AbsoluteTransform.Translation;
            pos.Y += movementAmount;
            AbsoluteTransform.Translation = pos;
        }

        public void MoveDown(float movementAmount)
        {
            Vector3 pos = AbsoluteTransform.Translation;
            pos.Y -= movementAmount;
            AbsoluteTransform.Translation = pos;
        }

        public void SetLookAndUp(Vector3 lookAt, Vector3 up)
        {
            Vector3 pos = AbsoluteTransform.Translation;
            AbsoluteTransform = Matrix.CreateWorld(pos, lookAt, up);

        }

        public void SetHighPrecisionPosition(Vector3d position)
        {
            if (highPrecisionPosition == null)
                highPrecisionPosition = ParentObject.GetComponent<HighPrecisionPosition>();

            highPrecisionPosition.Position = position;


        }

        public void SetHighPrecisionPosition(Vector3 position)
        {
            if (highPrecisionPosition == null)
                highPrecisionPosition = ParentObject.GetComponent<HighPrecisionPosition>();

            highPrecisionPosition.Position = new Vector3d(position);


        }

        public void HighPrecisionTranslate(Vector3 translation)
        {

            highPrecisionPosition.Position += translation;

        }
    }
}
