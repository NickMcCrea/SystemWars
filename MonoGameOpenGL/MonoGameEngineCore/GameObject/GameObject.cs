using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject.Components;

namespace MonoGameEngineCore.GameObject
{

    public class GameObject
    {
        public List<GameObject> Children = new List<GameObject>();
        private static int nextId;
        public Vector3 Position { get { return Transform.WorldMatrix.Translation; } set { Transform.SetPosition(value); } }
        public int ID { get; set; }
        private List<IComponent> components;
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
                Children.Add(child);
        }

        public void RemoveChild(GameObject child)
        {

            if (Children.Contains(child))
                Children.Remove(child);
        }
    }
}
