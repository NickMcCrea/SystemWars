using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGameEngineCore;
using MonoGameEngineCore.Camera;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.GUI;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace Pong
{
    class GameScreen : Screen
    {
        DummyOrthographicCamera camera;
        Label p1Score;
        Label p2Score;
        SpriteBatch spriteBatch;
        Texture2D plainTexture;
        GameObject playerPaddle;
        GameObject aiPaddle;
        GameObject ball;
        private int player1Score;
        private int aiScore;
        private float ballSpeed = 0.09f;
        float paddleSpeed = 1f;


        public GameScreen()
            : base()
        {
            camera = new DummyOrthographicCamera(SystemCore.GraphicsDevice.Viewport.Width / 5, SystemCore.GraphicsDevice.Viewport.Height / 5, 0.1f, 100f);
            camera.SetPositionAndLookDir(new Vector3(0, 0, -10), Vector3.Zero);
            SystemCore.SetActiveCamera(camera);
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            ((DiffuseLight)SystemCore.ActiveScene.LightsInScene[0]).LightDirection = new Vector3(0, 0, -1);

            var effect = EffectLoader.LoadEffect("FlatShaded");
            var font = SystemCore.ContentManager.Load<SpriteFont>("Pong");
            plainTexture = SystemCore.ContentManager.Load<Texture2D>("blank");

            SystemCore.AudioManager.AddSound("blip", "blip");


            var ballShape = new ProceduralCube();
            ball = GameObjectFactory.CreateCollidableObject(ballShape, effect, PhysicsMeshType.box);
            SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(ball);

            ResetBall();

            var topAndBottomShape = new ProceduralCuboid(125, 2, 1);
            var leftAndRightShape = new ProceduralCuboid(1, 2, 71);

            var topBorder = GameObjectFactory.CreateCollidableObject(topAndBottomShape, effect, PhysicsMeshType.box);
            topBorder.Transform.SetPosition(new Vector3(0, 70, 0));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(topBorder);

            var bottomBorder = GameObjectFactory.CreateCollidableObject(topAndBottomShape, effect, PhysicsMeshType.box);
            bottomBorder.Transform.SetPosition(new Vector3(0, -70, 0));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(bottomBorder);

            var leftBorder = GameObjectFactory.CreateCollidableObject(leftAndRightShape, effect, PhysicsMeshType.box);
            leftBorder.Transform.SetPosition(new Vector3(125, 0, 0));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(leftBorder);

            var rightBorder = GameObjectFactory.CreateCollidableObject(leftAndRightShape, effect, PhysicsMeshType.box);
            rightBorder.Transform.SetPosition(new Vector3(-125, 0, 0));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(rightBorder);

            var paddleShape = new ProceduralCuboid(2, 2, 10);

            playerPaddle = GameObjectFactory.CreateCollidableObject(paddleShape, effect, PhysicsMeshType.box);
            playerPaddle.Transform.SetPosition(new Vector3(100, 0, 0));
            SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(playerPaddle);

            aiPaddle = GameObjectFactory.CreateCollidableObject(paddleShape, effect, PhysicsMeshType.box);
            aiPaddle.Transform.SetPosition(new Vector3(-100, 0, 0));
            SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(aiPaddle);

            p1Score = new Label(font, "0");
            p1Score.Alignment = LabelAlignment.middle;
            p1Score.SetPosition(new Vector2(GUIManager.TopMidLeft(), 50));

            p2Score = new Label(font, "0");
            p2Score.Alignment = LabelAlignment.middle;
            p2Score.SetPosition(new Vector2(GUIManager.TopMidRight(), 50));

            SystemCore.GUIManager.AddControl(p1Score);
            SystemCore.GUIManager.AddControl(p2Score);


            spriteBatch = new SpriteBatch(SystemCore.GraphicsDevice);

            input = SystemCore.GetSubsystem<InputManager>();
            input.AddKeyDownBinding("PaddleUp", Keys.Up);
            input.AddKeyDownBinding("PaddleDown", Keys.Down);


            ball.GetComponent<PhysicsComponent>().PhysicsEntity.CollisionInformation.Events.InitialCollisionDetected +=
              (sender, other, pair) =>
              {


                  //undo last frame's movement
                  ball.Transform.WorldMatrix.Translation -= ball.Transform.Velocity;

                  //invert Y dimension of velocity if this is one of the top / bottom walls
                  if ((int)other.Tag == topBorder.ID || (int)other.Tag == bottomBorder.ID)
                  {
                      ball.Transform.Velocity.Y = -ball.Transform.Velocity.Y;
                      SystemCore.AudioManager.PlaySound("blip");
                      return;
                  }


                  //score!
                  if ((int)other.Tag == rightBorder.ID)
                  {
                      player1Score++;
                      ResetBall();
                      return;

                  }

                  //boo!
                  if ((int)other.Tag == leftBorder.ID)
                  {
                      aiScore++;
                      ResetBall();
                      return;
                  }

                  //paddle bounce
                  if ((int)other.Tag == playerPaddle.ID || ((int)other.Tag == aiPaddle.ID))
                  {
                      Vector3 normalOfContactPoint = MonoMathHelper.Translate(pair.Contacts[0].Contact.Normal);

                      if (normalOfContactPoint == Vector3.Right || normalOfContactPoint == Vector3.Left)
                          ball.Transform.Velocity.X = -ball.Transform.Velocity.X;
                      if (normalOfContactPoint == Vector3.Up || normalOfContactPoint == Vector3.Down)
                          ball.Transform.Velocity.Y = -ball.Transform.Velocity.Y;

                      SystemCore.AudioManager.PlaySound("blip");
                  }

              };


        }

        private void ResetBall()
        {
            ball.Transform.SetPosition(Vector3.Zero);

            float xVel = RandomHelper.GetRandomFloat(10, 15) / 10f;
            float yVel = RandomHelper.GetRandomFloat(5, 10) / 10f;

            if (RandomHelper.CoinToss())
                xVel *= -1;
            if (RandomHelper.CoinToss())
                yVel *= -1;

            ball.Transform.Velocity =
                new Vector3(xVel, yVel, 0) *
                ballSpeed;
        }

        public override void Update(GameTime gameTime)
        {

            if (input.IsKeyDown(Keys.Escape))
                SystemCore.Game.Exit();

            if (input.EvaluateInputBinding("PaddleUp"))
                    playerPaddle.Transform.MoveUp(paddleSpeed);


            if (input.EvaluateInputBinding("PaddleDown"))
                    playerPaddle.Transform.MoveDown(paddleSpeed);


            var physicsComponent = playerPaddle.GetComponent<PhysicsComponent>();
            if (physicsComponent.InCollision())
            {
                if (!physicsComponent.CollidedWithEntity(ball.ID))
                {
                    float penetration = physicsComponent.PhysicsEntity.CollisionInformation.Pairs[0].Contacts[0].Contact.PenetrationDepth;

                    if (penetration > 0)
                    {
                        Vector3 normal = MonoMathHelper.Translate(physicsComponent.PhysicsEntity.CollisionInformation.Pairs[0].Contacts[0].Contact.Normal);
                        playerPaddle.Position = playerPaddle.Position + normal * penetration;
                    }
                }

            }




            p1Score.Text = player1Score.ToString();
            p2Score.Text = aiScore.ToString();


            UpdateAI();


            base.Update(gameTime);
        }

        private void UpdateAI()
        {

            Vector3 velocity = Vector3.Up * paddleSpeed;

            if (aiPaddle.Transform.WorldMatrix.Translation.Y > ball.Position.Y)
                velocity = -velocity;

            float movementThreshold = 5f;
            float diff = aiPaddle.Position.Y - ball.Position.Y;
            if (Math.Abs(diff) > movementThreshold)
                aiPaddle.Position = aiPaddle.Position + velocity;

            var physicsComponent = aiPaddle.GetComponent<PhysicsComponent>();
            if (physicsComponent.InCollision())
            {
                if (!physicsComponent.CollidedWithEntity(ball.ID))
                {
                    float penetration = physicsComponent.PhysicsEntity.CollisionInformation.Pairs[0].Contacts[0].Contact.PenetrationDepth;
                    Vector3 normal = MonoMathHelper.Translate(physicsComponent.PhysicsEntity.CollisionInformation.Pairs[0].Contacts[0].Contact.Normal);
                    aiPaddle.Position = aiPaddle.Position + normal;
                }

            }

        }

        public override void Render(GameTime gameTime)
        {
            SystemCore.GraphicsDevice.Clear(Color.Black);




            base.Render(gameTime);
        }

        public override void RenderSprites(GameTime gameTime)
        {
            spriteBatch.Begin();



            int netWidth = 10;
            int netHeight = 20;
            int netGap = 8;

            for (int y = 10; y < SystemCore.Viewport.Height - netHeight; y += netHeight + netGap)
            {
                spriteBatch.Draw(plainTexture, new Rectangle(SystemCore.Viewport.Width / 2 - (netWidth / 2), y, netWidth, netHeight), Color.White);
            }

            spriteBatch.End();
            base.RenderSprites(gameTime);
        }
    }
}