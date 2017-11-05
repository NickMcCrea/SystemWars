using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSRestMatch.GameServer
{
    class Player : GameObject
    {
        public Color PlayerColor { get; set; }
        public float DesiredHeading { get; set; }
        public float TurnSpeed { get; set; }
        public Vector3 DesiredPosition { get; set; }
        public float MoveSpeed { get; set; }
        public Player()
        {
            this.AddComponent(new PlayerControlComponent());
        }
    }


    class PlayerControlComponent : IComponent, IUpdateable
    {


        public PlayerControlComponent()
        {
            Enabled = true;
        }
        public bool Enabled
        {
            get; set;
        }
        public GameObject ParentObject
        {
            get; set;
        }
        public int UpdateOrder
        {
            get; set;
        }
        public event EventHandler<EventArgs> EnabledChanged;
        public event EventHandler<EventArgs> UpdateOrderChanged;
        Player player;
        Vector2 currentForward;
        public void Initialise()
        {

        }

        public void PostInitialise()
        {
            player = ParentObject as Player;
        }

        public void Update(GameTime gameTime)
        {
            //get current heading
            currentForward = ParentObject.Transform.AbsoluteTransform.Forward.ToVector2XZ();

            float heading = MathHelper.ToDegrees(MonoMathHelper.GetHeadingFromVector(currentForward));
            heading = (heading + 360) % 360;

            Vector2 desiredForward = MonoMathHelper.GetVectorFromHeading(MathHelper.ToRadians(player.DesiredHeading - 360));

            if (heading != player.DesiredHeading)
            {

                currentForward = Vector2.Lerp(currentForward, desiredForward, 0.1f);
                player.Transform.SetLookAndUp(new Vector3(currentForward.X, 0, currentForward.Y), Vector3.Up);
            }



        }

    
    }
}
