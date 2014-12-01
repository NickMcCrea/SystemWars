using Microsoft.Xna.Framework;

namespace MonoGameEngineCore
{
    public class FPSCounter : IGameSubSystem
    {

        int totalFrames = 0;
        float elapsedTime = 0.0f;
        public int FPS { get; private set; }

        public void Initalise()
        {
            
        }

        public void Update(GameTime gameTime)
        {
            // Update
            elapsedTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
 
            // 1 Second has passed
            if (elapsedTime >= 1000.0f)
            {
                FPS = totalFrames;
                totalFrames = 0;
                elapsedTime = 0;
            }
        }

        public void Render(GameTime gameTime)
        {
            totalFrames++;
        }
    }
}