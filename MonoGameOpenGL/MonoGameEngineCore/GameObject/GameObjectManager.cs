using System;
using System.Collections.Generic;
using System.Linq;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using MonoGameEngineCore.GameObject.Components;

namespace MonoGameEngineCore.GameObject
{
    public class GameObjectManager : IGameSubSystem
    {

        private Dictionary<int, GameObject> gameObjects;
        private List<IUpdateable> updateableGameOjectComponents;
        private List<IDrawable> drawableGameObjectComponents;
        private List<IUpdateable> updateableObjects;
        private BasicEffect lineEffect;
        public static int drawCalls;
        public static int verts;
        public static int primitives;
        public ShadowMapComponent shadowMapComponent;

        public void Initalise()
        {
            gameObjects = new Dictionary<int, GameObject>();
            updateableGameOjectComponents = new List<IUpdateable>();
            drawableGameObjectComponents = new List<IDrawable>();
            updateableObjects = new List<IUpdateable>();
            lineEffect = new BasicEffect(SystemCore.GraphicsDevice);
            shadowMapComponent = new ShadowMapComponent();
        }

        public GameObject AddShapeToScene(ProceduralShape shape)
        {
            GameObject o = GameObjectFactory.CreateRenderableGameObjectFromShape(shape, EffectLoader.LoadSM5Effect("flatshaded"));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(o);
            return o;
        }

        public GameObject AddLineBatchToScene(LineBatch lines)
        {
            GameObject o = GameObjectFactory.CreateRenderableGameObjectFromShape(lines, lineEffect);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(o);
            return o;
        }

        public void AddAndInitialiseGameObject(GameObject obj)
        {
            if (obj is IUpdateable)
                updateableObjects.Add(obj as IUpdateable);

            var componentList = obj.GetAllComponents();

            foreach (IComponent component in componentList)
            {
                component.Initialise();
            }

            FindUpdatableComponents(componentList);
            FindRenderableComponents(componentList);

            gameObjects.Add(obj.ID, obj);
        }

        public bool RemoveObject(GameObject obj)
        {

            var componentList = obj.GetAllComponents();

            for (int i = 0; i < componentList.Count; i++)
            {
                IComponent comp = componentList[i];
                if (comp is IDrawable)
                    drawableGameObjectComponents.Remove(comp as IDrawable);
                if (comp is IUpdateable)
                    updateableGameOjectComponents.Remove(comp as IUpdateable);
                if (comp is IDisposable)
                    ((IDisposable)comp).Dispose();
            }

            obj.RemoveAllComponents();

            gameObjects.Remove(obj.ID);

            if (updateableObjects.Contains(obj as IUpdateable))
                updateableObjects.Remove(obj as IUpdateable);

            return true;
        }

        public void RemoveGameObjects(List<GameObject> objects)
        {
            for (int i = 0; i < gameObjects.Count;i++ )
                RemoveObject(gameObjects[i]);
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

        public void AddComponent(IComponent component)
        {
            if (component is IUpdateable)
                updateableGameOjectComponents.Add(component as IUpdateable);

            if (component is IDrawable)
                drawableGameObjectComponents.Add(component as IDrawable);

        }

        public void Update(GameTime gameTime)
        {
            for (int i = 0; i < updateableObjects.Count; i++)
            {
                updateableObjects[i].Update(gameTime);
            }

            for (int i = 0; i < updateableGameOjectComponents.Count; i++)
            {
                if (updateableGameOjectComponents[i].Enabled)
                    updateableGameOjectComponents[i].Update(gameTime);
            }


        }

        public void RemoveComponent(IComponent component)
        {
            if (component is IDisposable)
                ((IDisposable)component).Dispose();

            if (component is IUpdateable)
                updateableGameOjectComponents.Remove(component as IUpdateable);

            if (component is IDrawable)
                drawableGameObjectComponents.Remove(component as IDrawable);

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


            drawableGameObjectComponents = drawableGameObjectComponents.OrderBy(x => x.DrawOrder).ToList();

        
            for (int i = 0; i < drawableGameObjectComponents.Count; i++)
            {
                var drawable = drawableGameObjectComponents[i];
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

        public GameObject GetRayCastObject()
        {
            RayCastResult result;
            if (SystemCore.PhysicsSimulation.RayCast(SystemCore.Input.GetBepuProjectedMouseRay(), out result))
            {
                return result.HitObject.Tag as GameObject;
            }
            return null;
        }

        public bool ObjectInManager(int id)
        {
            return gameObjects.ContainsKey(id);
        }

        public void AddTestSphere(Vector3 vector3, float p)
        {
            ProceduralSphereTwo s = new ProceduralSphereTwo(10);
            s.Scale(p);
            s.Translate(vector3);
            AddAndInitialiseGameObject(GameObjectFactory.CreateRenderableGameObjectFromShape(s, EffectLoader.LoadSM5Effect("flatshaded")));
        }

        public void AddTestUnitCube(Vector3 vector3)
        {
            ProceduralCube s = new ProceduralCube();
            s.SetColor(Color.White);
            s.Translate(vector3);
            AddAndInitialiseGameObject(GameObjectFactory.CreateRenderableGameObjectFromShape(s, EffectLoader.LoadSM5Effect("flatshaded")));
        }

       
    }
}