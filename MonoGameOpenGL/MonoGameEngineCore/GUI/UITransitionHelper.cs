using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GUI.Controls;
using MonoGameEngineCore.GUI;

namespace NicksLib.Rendering
{

    /// <summary>
    /// Class that encapsulates a single movement.
    /// </summary>
    public class TransitionMovement
    {
        public TransitionMovement(int start, int duration, Vector2 movement)
        {
            StartTime = start;
            Duration = duration;
            Movement = movement;
        }
        public int StartTime { get; set; }
        public int Duration { get; set; }
        public Vector2 Movement { get; set; }
    }

    /// <summary>
    /// An instance of a spritefont or sprite that transitions in some way.
    /// </summary>
    public class TextureTransitionInstances
    {
        public enum InstanceType
        {
            texture,
            spritefont
        }

        public List<TransitionMovement> Movements { get; set; }

        public TextureTransitionInstances()
        {
            Movements = new List<TransitionMovement>();
        }


        public string StringToDisplay { get; set; }
        public SpriteFont Font { get; set; }
        public InstanceType Type { get; set; }
        public Color TexColor { get; set; }
        public DateTime StartTime { get; set; }
        public Texture2D Texture { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 OriginalPosition { get; set; }
        public int DurationBeforeFade { get; set; }
        public int FadeDuration { get; set; }
        public float GrowShrinkFactor { get; set; }
        public Rectangle Rectangle { get; set; }

    }

    /// <summary>
    /// Class for managing 2D graphics transitions - fading and moving of sprites and spritefonts
    /// </summary>
    public class UITransitionHelper
    {
        public List<TextureTransitionInstances> TexturesToFade { get; set; }
        private List<TextureTransitionInstances> m_textureFadesToRemove;
        private Vector2 m_midPoint;
        private SpriteBatch m_spriteBatch;
        private Viewport m_viewPort;

        public UITransitionHelper(Viewport viewPort, SpriteBatch spriteBatch)
        {
            TexturesToFade = new List<TextureTransitionInstances>();
            m_textureFadesToRemove = new List<TextureTransitionInstances>();
            m_viewPort = viewPort;
            m_spriteBatch = spriteBatch;
            m_midPoint = new Vector2(viewPort.Width / 2, viewPort.Height / 2);
        }

        public void Update(GameTime gameTime)
        {
            m_spriteBatch.Begin();


            foreach (TextureTransitionInstances textureFade in TexturesToFade)
            {
                Color texColor = textureFade.TexColor;
                TimeSpan sinceBeginning = DateTime.Now - textureFade.StartTime;

                //first, manage their positions...apply movements
                //at the correct times etc.
                ManageMovements(textureFade, sinceBeginning);

                //we need to start fading
                if (sinceBeginning.TotalMilliseconds > textureFade.DurationBeforeFade)
                {
                    int totalMillisecondsSoFar = (int)sinceBeginning.TotalMilliseconds;
                    int millisecondsIntoFadingTime = totalMillisecondsSoFar - textureFade.DurationBeforeFade;

                    float percentComplete = (float)millisecondsIntoFadingTime / (float)textureFade.FadeDuration;
                    float alphaValue = texColor.A * percentComplete;
                    texColor.A = (byte)((byte)texColor.A - (byte)alphaValue);

                    //we're done, remove this
                    if (sinceBeginning.TotalMilliseconds > (textureFade.DurationBeforeFade + textureFade.FadeDuration))
                    {
                        m_textureFadesToRemove.Add(textureFade);
                        continue;
                    }

                    if (textureFade.Type == TextureTransitionInstances.InstanceType.texture)
                    {
                        if (textureFade.GrowShrinkFactor != 0)
                        {
                            float targetWidth = textureFade.Texture.Width * textureFade.GrowShrinkFactor;
                            float targetHeight = textureFade.Texture.Height * textureFade.GrowShrinkFactor;

                            float newWidth = targetWidth * percentComplete;
                            float newHeight = targetHeight * percentComplete;

                            int newTexX = (int)(m_midPoint.X - newWidth / 2);
                            int newTexY = (int)(m_midPoint.Y - newHeight / 2);

                            textureFade.Rectangle = new Rectangle(newTexX, newTexY, (int)newWidth, (int)newHeight);
                        }
                        m_spriteBatch.Draw(textureFade.Texture, textureFade.Rectangle, texColor);
                    }
                    else
                    {

                        m_spriteBatch.DrawString(textureFade.Font, textureFade.StringToDisplay, textureFade.Position + new Vector2(1, 1), new Color(0, 0, 0, texColor.A));
                        m_spriteBatch.DrawString(textureFade.Font, textureFade.StringToDisplay, textureFade.Position, texColor);
                    }
                }
                else
                {
                    if (textureFade.Type == TextureTransitionInstances.InstanceType.texture)
                        m_spriteBatch.Draw(textureFade.Texture, textureFade.Rectangle, texColor);
                    else
                    {
                        m_spriteBatch.DrawString(textureFade.Font, textureFade.StringToDisplay, textureFade.Position + new Vector2(1, 1), new Color(0, 0, 0, texColor.A));
                        m_spriteBatch.DrawString(textureFade.Font, textureFade.StringToDisplay, textureFade.Position, texColor);
                    }

                }
            }

            foreach (TextureTransitionInstances tf in m_textureFadesToRemove)
            {
                TexturesToFade.Remove(tf);
            }
            m_textureFadesToRemove.Clear();

            m_spriteBatch.End();
        }

        private void ManageMovements(TextureTransitionInstances textureFade, TimeSpan sinceBeginning)
        {
            List<TransitionMovement> movementsToClear = new List<TransitionMovement>();
            foreach (TransitionMovement movement in textureFade.Movements)
            {
                if (movement.StartTime == 0)
                {
                    //do this immediately.
                    movement.StartTime = (int)sinceBeginning.TotalMilliseconds - 1;
                }

                if (sinceBeginning.TotalMilliseconds > movement.StartTime)
                {
                    if (sinceBeginning.TotalMilliseconds < (movement.StartTime + movement.Duration))
                    {
                        //we should apply this movement.
                        int totalMillisecondsSoFar = (int)sinceBeginning.TotalMilliseconds;
                        int millisecondsIntoMovement = totalMillisecondsSoFar - movement.StartTime;
                        float percentComplete = (float)millisecondsIntoMovement / (float)movement.Duration;

                        if (textureFade.Type == TextureTransitionInstances.InstanceType.spritefont)
                        {
                            textureFade.Position = textureFade.OriginalPosition + (percentComplete * movement.Movement);
                        }
                        else
                        {
                            Vector2 newRecPos = textureFade.OriginalPosition + (percentComplete * movement.Movement);
                            textureFade.Rectangle = new Rectangle((int)newRecPos.X, (int)newRecPos.Y, textureFade.Rectangle.Width, textureFade.Rectangle.Height);
                        }
                    }
                    else
                    {
                        //we've finished the movement - make sure we're in the desired spot and remove the
                        //movement transition from the list of things to apply.
                        movementsToClear.Add(movement);
                        textureFade.Position = textureFade.OriginalPosition + movement.Movement;
                        textureFade.OriginalPosition = textureFade.Position;
                    }
                }
            }
            foreach (TransitionMovement movement in movementsToClear)
            {
                textureFade.Movements.Remove(movement);
            }
            movementsToClear.Clear();

        }

        public TextureTransitionInstances CreateAndAddTexture(Texture2D tex, Vector2 origin, int timeBeforeFade, int fadeDuration, float growShrinkFactor, Color color)
        {
            TextureTransitionInstances fader = new TextureTransitionInstances();
            fader.Type = TextureTransitionInstances.InstanceType.texture;
            fader.Rectangle = new Rectangle((int)origin.X, (int)origin.Y, tex.Width, tex.Height);
            fader.StartTime = DateTime.Now;
            fader.TexColor = color;
            fader.Texture = tex;
            fader.Position = origin;
            fader.OriginalPosition = origin;
            fader.FadeDuration = fadeDuration;
            fader.DurationBeforeFade = timeBeforeFade;
            fader.GrowShrinkFactor = growShrinkFactor;
            TexturesToFade.Insert(0, fader); //always insert textures at the back of the list, so strings get drawn later
            return fader;
        }

        public TextureTransitionInstances CreateAndAddString(SpriteFont font, string text, Vector2 origin, int timeBeforeFade, int fadeDuration, float growShrinkFactor, Color color)
        {
            TextureTransitionInstances fader = new TextureTransitionInstances();
            fader.Type = TextureTransitionInstances.InstanceType.spritefont;
            fader.StringToDisplay = text;
            fader.StartTime = DateTime.Now;
            fader.TexColor = color;
            fader.Font = font;
            fader.Position = origin;
            fader.OriginalPosition = origin;
            fader.FadeDuration = fadeDuration;
            fader.DurationBeforeFade = timeBeforeFade;
            fader.GrowShrinkFactor = growShrinkFactor;
            TexturesToFade.Add(fader);
            return fader;
        }

        public TextureTransitionInstances CreateString(SpriteFont font, string text, Vector2 origin, int timeBeforeFade, int fadeDuration, float growShrinkFactor, Color color)
        {
            TextureTransitionInstances fader = new TextureTransitionInstances();
            fader.Type = TextureTransitionInstances.InstanceType.spritefont;
            fader.StringToDisplay = text;
            fader.StartTime = DateTime.Now;
            fader.TexColor = color;
            fader.Font = font;
            fader.Position = origin;
            fader.OriginalPosition = origin;
            fader.FadeDuration = fadeDuration;
            fader.DurationBeforeFade = timeBeforeFade;
            fader.GrowShrinkFactor = growShrinkFactor;

            return fader;
        }

        public TextureTransitionInstances CreateTexture(Texture2D tex, Vector2 origin, int timeBeforeFade, int fadeDuration, float growShrinkFactor, Color color)
        {
            TextureTransitionInstances fader = new TextureTransitionInstances();
            fader.Type = TextureTransitionInstances.InstanceType.texture;
            fader.Rectangle = new Rectangle((int)origin.X, (int)origin.Y, tex.Width, tex.Height);
            fader.StartTime = DateTime.Now;
            fader.TexColor = color;
            fader.Texture = tex;
            fader.Position = origin;
            fader.OriginalPosition = origin;
            fader.FadeDuration = fadeDuration;
            fader.DurationBeforeFade = timeBeforeFade;
            fader.GrowShrinkFactor = growShrinkFactor;
            return fader;
        }

    }

    public class GUITransitionManager
    {

        List<GUITransition> activeGuiTransitions;

        public GUITransitionManager()
        {
            activeGuiTransitions = new List<GUITransition>();
        }

        public void Update(GameTime gameTime)
        {
            foreach (GUITransition transition in activeGuiTransitions)
            {
                transition.Update(gameTime);
            }
        }
    }
}
