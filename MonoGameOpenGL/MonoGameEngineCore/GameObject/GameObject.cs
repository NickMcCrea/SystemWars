using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject.Components;

namespace MonoGameEngineCore.GameObject
{

    public class GameObject
    {
        public GameObject ParentGameObject;
        public List<GameObject> Children = new List<GameObject>();
        protected static int nextId;
        public Vector3 Position { get { return Transform.AbsoluteTransform.Translation; } set { Transform.SetPosition(value); } }
        public int ID { get; set; }
        protected List<IComponent> components;
        public TransformComponent Transform { get; private set; }
        public string Name { get; set; }
       
        public GameObject()
        {
            ID = GetId();

            components = new List<IComponent>();
            Transform = new TransformComponent();
            Transform.ParentObject = this;
            components.Add(Transform);

        }

        public GameObject(string objectName)
            : this()
        {

            Name = objectName;

        }

        private static int GetId()
        {
            return ++nextId;
        }

        public void AddComponent(IComponent component)
        {
            components.Add(component);
            component.ParentObject = this;
        }

        public void AddAndInitialise(IComponent component)
        {
            AddComponent(component);
            component.Initialise();
            component.PostInitialise();
            SystemCore.GameObjectManager.AddComponent(component);
        }

        public T GetComponent<T>()
        {
            var component = components.Find(x => (x.GetType() == typeof(T)));
            if (component != null)
                return (T)component;
            return default(T);
        }

        public bool ContainsComponent<T>()
        {
            var component = components.Find(x => (x.GetType() == typeof(T)));
            if (component != null)
                return true;
            return false;
        }

        internal List<IComponent> GetAllComponents()
        {
            return components;
        }

        public void AddChild(GameObject child)
        {

            if (!Children.Contains(child))
            {
                Children.Add(child);
                child.ParentGameObject = this;
            }
        }

        public void RemoveChild(GameObject child)
        {

            if (Children.Contains(child))
            {
                Children.Remove(child);
                child.ParentGameObject = null;
            }
        }

        public void RemoveComponent(IComponent component)
        {
            components.Remove(component);
            SystemCore.GameObjectManager.RemoveComponent(component);
        }

        internal void RemoveAllComponents()
        {
            components.Clear();
        }

        public static void InitialiseAllComponents(GameObject obj)
        {
            var componentList = obj.GetAllComponents();

            foreach (IComponent component in componentList)
            {
                component.Initialise();
            }
            foreach (IComponent component in componentList)
            {
                component.PostInitialise();
            }
        }
    }
}
