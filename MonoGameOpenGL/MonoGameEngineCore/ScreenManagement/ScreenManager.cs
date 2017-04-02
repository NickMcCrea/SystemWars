using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MonoGameEngineCore.ScreenManagement
{
    public class ScreenManager : IGameSubSystem
    {
        private List<Screen> gameScreens;
        private Screen activeScreen;

        public void Initalise()
        {
            gameScreens = new List<Screen>();
        }

        private void AddScreen(Screen newScreen)
        {
            newScreen.OnInitialise();
            gameScreens.Add(newScreen);
        }

        public void AddAndSetActive(Screen newScreen)
        {
            if (activeScreen != null)
                RemoveScreen(activeScreen);


            AddScreen(newScreen);
            SetActive(newScreen);
        }

        private void RemoveScreen(Screen screenToRemove)
        {
            screenToRemove.OnRemove();
            gameScreens.Remove(screenToRemove);
        }


        public void Update(GameTime gameTime)
        {
            if (activeScreen != null)
                activeScreen.Update(gameTime);
        }

        public void Render(GameTime gameTime)
        {
            if(activeScreen != null)
            activeScreen.Render(gameTime);
        }

        public void RenderSprites(GameTime gameTime)
        {
            if (activeScreen != null)
                activeScreen.RenderSprites(gameTime);
        }

        private void SetActive(Screen newActiveScreen)
        {
            activeScreen = newActiveScreen;
        }

        private void SetActive(Type screen)
        {
            SetActive(gameScreens.Find(x => x.GetType() == screen));
        }
    }
}