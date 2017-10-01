using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GUI;

namespace NickMathHelper.Rendering
{
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

            if (activeGuiTransitions.Count > 0)
                activeGuiTransitions.RemoveAll(x => x.IsComplete);
        }

        internal void AddTransition(GUITransition fadeOutTransition)
        {
            activeGuiTransitions.Add(fadeOutTransition);
        }
    }
}