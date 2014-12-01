using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.GameObject
{
    public class GameObjectManager : IGameSubSystem
    {

        private Dictionary<int, GameObject> gameObjects;
        private List<IUpdateable> updateableGameOjectComponents;
        private List<IDrawable> drawableGameObjectComponents;
        private Dictionary<int, GameObject> objectsToRemoveNextFrame;
        private Dictionary<int, GameObject> objectsToAddNextFrame;
        public static int drawCalls;
        public static int verts;
        public static int primitives;

        public void Initalise()
        {
            gameObjects = new Dictionary<int, GameObject>();
            updateableGameOjectComponents = new List<IUpdateable>();
            drawableGameObjectComponents = new List<IDrawable>();
            objectsToRemoveNextFrame = new Dictionary<int, GameObject>();
            objectsToAddNextFrame = new Dictionary<int, GameObject>();

        }

        public void AddAndInitialiseObjectOnNextFrame(GameObject obj)
        {
            if (objectsToAddNextFrame.ContainsKey(obj.ID))
                throw new Exception("FAIL");

            objectsToAddNextFrame.Add(obj.ID, obj);

        }

        public void AddAndInitialiseGameObject(GameObject obj)
        {

            var componentList = obj.GetAllComponents();

            foreach (IComponent component in componentList)
            {
                component.Initialise();
            }

            FindUpdatableComponents(componentList);
            FindRenderableComponents(componentList);

            if (gameObjects.ContainsKey(obj.ID))
                throw new Exception("FAIL");


            gameObjects.Add(obj.ID, obj);
        }

        public void RemoveGameObjectOnNextFrame(GameObject obj)
        {

            objectsToRemoveNextFrame.Add(obj.ID, obj);
        }

        public bool RemoveObjectImmediately(GameObject obj)
        {

            var componentList = obj.GetAllComponents();

            foreach (IComponent comp in componentList)
            {
                if (comp is IDrawable)
                    drawableGameObjectComponents.Remove(comp as IDrawable);
                if (comp is IUpdateable)
                    updateableGameOjectComponents.Remove(comp as IUpdateable);
            }

            gameObjects.Remove(obj.ID);


            return true;
        }

        public void RemoveGameObjects(List<GameObject> objects)
        {
            foreach (GameObject o in objects)
                RemoveGameObjectOnNextFrame(o);
        }

        public List<GameObject> GetCollisions(GameObject objectUnderTest)
        {
            return null;
        }

        private void FindRenderableComponents(List<IComponent> componentList)
        {
            var renderableComponents = componentList.FindAll(x => x is IDrawable);
            foreach (IComponent drawableComponent in renderableComponents)
            {
                drawableGameObjectComponents.Add(drawableComponent as IDrawable);
            }
            drawableGameObjectComponents = drawableGameObjectComponents.OrderBy(x => x.DrawOrder).ToList();
        }

        private void FindUpdatableComponents(List<IComponent> componentList)
        {
            var updateAbleComponents = componentList.FindAll(x => x is IUpdateable);
            foreach (IComponent updateAbleComponent in updateAbleComponents)
            {
                updateableGameOjectComponents.Add(updateAbleComponent as IUpdateable);
            }
            updateableGameOjectComponents = updateableGameOjectComponents.OrderBy(x => x.UpdateOrder).ToList();
        }

        public void Update(GameTime gameTime)
        {

            foreach (IUpdateable updateable in updateableGameOjectComponents)
            {
                if (updateable.Enabled)
                    updateable.Update(gameTime);
            }

            foreach (GameObject o in objectsToAddNextFrame.Values)
            {
                AddAndInitialiseGameObject(o);
            }
            objectsToAddNextFrame.Clear();

            foreach (GameObject o in objectsToRemoveNextFrame.Values)
            {
                RemoveObjectImmediately(o);
            }
            objectsToRemoveNextFrame.Clear();
        }

        private static void UpdateComponents(GameTime gameTime, GameObject o)
        {
            var components = o.GetAllComponents();
            foreach (IComponent c in components)
                if (c is IUpdateable)
                    ((IUpdateable)c).Update(gameTime);
        }

        public void Render(GameTime gameTime)
        {
            drawCalls = 0;
            primitives = 0;
            verts = 0;
            SystemCore.GraphicsDevice.BlendState = BlendState.Opaque;
            SystemCore.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            SystemCore.GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            foreach (IDrawable drawable in drawableGameObjectComponents)
            {
                if (drawable.Visible)
                {
                    drawable.Draw(gameTime);
                    drawCalls++;
                }
            }

        }

        public List<GameObject> GetAllObjects()
        {
            return gameObjects.Values.ToList();
        }

        public GameObject GetObject(string name)
        {
            foreach (GameObject o in gameObjects.Values)
                if (o.Name == name)
                    return o;

            return null;
        }

        public GameObject GetObject(int id)
        {
            if (gameObjects.ContainsKey(id))
                return gameObjects[id];

            return null;
        }

        public void ReAssignObject(GameObject obj)
        {
            gameObjects[obj.ID] = obj;
        }

        public void ReAssignPendingObject(GameObject obj)
        {
            objectsToAddNextFrame[obj.ID] = obj;
        }

        public bool ObjectAboutToBeAdded(int id)
        {
            return objectsToAddNextFrame.ContainsKey(id);
        }

        public bool ObjectsAboutToBeRemove(int id)
        {
            return objectsToRemoveNextFrame.ContainsKey(id);
        }

        public bool ObjectInManager(int id)
        {
            return gameObjects.ContainsKey(id);
        }
    }
}