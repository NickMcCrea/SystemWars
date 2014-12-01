using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;


namespace MonoGameEngineCore.Helper
{
    /// <summary>
    /// Takes text and writes it in the corner
    /// Toggle with +
    /// </summary>
    public class DebugText
    {
        public static SpriteFont debugFont;
        static string debugString;
        public static bool showDebugString = true;
        static GraphicsDevice graphicsDevice;
        static SpriteBatch spriteBatch;
        static KeyboardState curKeys;
        static KeyboardState oldKeys;
        private static MessageText messageText;

        private struct PositionedText
        {
            public string text;
            public Vector2 position;
            public Color col;
        }

        private struct MessageText
        {
            public string text;
            public Vector2 position;
            public Color col;
            public DateTime firstShown;
            public TimeSpan intendedDuration;
        }

        static List<PositionedText> positionedStringList = new List<PositionedText>();
        static Dictionary<PositionedText, XNATimer> messageTimerMap = new Dictionary<PositionedText, XNATimer>();
        static List<PositionedText> removeList = new List<PositionedText>();
        /// <summary>
        /// For drawing
        /// </summary>        
        public static void InjectGraphicsDevice(GraphicsDevice device)
        {
            graphicsDevice = device;
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        /// <summary>
        /// Font used for writing debug text, load elsewhere
        /// </summary>        
        public static void InjectDebugFont(SpriteFont font)
        {
            debugFont = font;
        }

        static DebugText()
        {
            debugString = "";
        }

        public static void WritePositionedText(string text, Vector2 position)
        {
            PositionedText posText = new PositionedText();
            position.X = (int)position.X;
            position.Y = (int)position.Y;
            posText.position = position;
            posText.text = text;
            posText.col = Color.White;
            positionedStringList.Add(posText);
        }

        public static void WritePositionedText(string text, Vector2 position, Color col)
        {
            PositionedText posText = new PositionedText();
            position.X = (int)position.X;
            position.Y = (int)position.Y;
            posText.position = position;
            posText.text = text;
            posText.col = col;
            positionedStringList.Add(posText);
        }

        public static void WriteNotificationMessage(string text)
        {
            messageText = new MessageText();
            messageText.position = DetermineMessagePosition(text);
            messageText.text = text;
            messageText.col = Color.White;
            messageText.firstShown = DateTime.Now;
            messageText.intendedDuration = new TimeSpan(0, 0, 2);
        }

        public static void WriteNotificationMessage(string text, int messageTime)
        {
            messageText = new MessageText();
            messageText.position = DetermineMessagePosition(text);
            messageText.text = text;
            messageText.col = Color.White;
            messageText.intendedDuration = new TimeSpan(0, 0, messageTime);
        }

        public static void Write(string text)
        {
            debugString += text;
            debugString += "\n";

        }

        public static void Update(GameTime gameTime)
        {
            foreach (PositionedText t in messageTimerMap.Keys)
            {
                XNATimer timer = messageTimerMap[t];
                timer.Update(gameTime);
            }

            foreach (PositionedText t in removeList)
            {
                messageTimerMap.Remove(t);
            }
            removeList.Clear();
        }

        public static void Draw(GameTime gameTime)
        {
            oldKeys = curKeys;
            curKeys = Keyboard.GetState();
            if (curKeys.IsKeyDown(Keys.Add) && oldKeys.IsKeyUp(Keys.Add))
                showDebugString = !showDebugString;

            //Vector2 startCorner = new Vector2(graphicsDevice.Viewport.Width - 128, 10);
            Vector2 startCorner = new Vector2(10, 10);



            graphicsDevice.BlendState = BlendState.AlphaBlend;
            graphicsDevice.DepthStencilState = DepthStencilState.None;
            graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;


            spriteBatch.Begin();
            if (showDebugString)
            {
                spriteBatch.DrawString(debugFont, debugString, startCorner, Color.White);
            }
            else
            {
                spriteBatch.DrawString(debugFont, "[+]", startCorner, Color.White);
            }

            foreach (PositionedText posText in positionedStringList)
            {
                spriteBatch.DrawString(debugFont, posText.text, posText.position, posText.col);
            }

            TimeSpan timeSinceFirstShown = DateTime.Now - messageText.firstShown;
            if (timeSinceFirstShown < messageText.intendedDuration)
                spriteBatch.DrawString(debugFont, messageText.text, messageText.position, messageText.col);

            spriteBatch.End();

            debugString = "";

            positionedStringList.Clear();
        }

        public static Vector2 DetermineMessagePosition(string s)
        {
            Vector2 measureString = debugFont.MeasureString(s);
            Vector2 midPoint = new Vector2(graphicsDevice.Viewport.Width / 2, 20);
            return midPoint - measureString / 2;
        }
    }
}
