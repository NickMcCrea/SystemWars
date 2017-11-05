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
            Vector3 currentForward = ParentObject.Transform.AbsoluteTransform.Forward;

            float heading = MathHelper.ToDegrees(MonoMathHelper.GetHeadingFromVector(currentForward.ToVector2XZ()));
            heading = (heading + 360) % 360;


            if (heading != player.DesiredHeading)
            {
                heading = MathHelper.Lerp(heading, player.DesiredHeading, 0.1f);
                Vector2 forward = MonoMathHelper.GetVectorFromHeading(MathHelper.ToRadians(heading - 360));
                player.Transform.SetLookAndUp(new Vector3(forward.X, 0, forward.Y), Vector3.Up);
            }



        }
    }
}
