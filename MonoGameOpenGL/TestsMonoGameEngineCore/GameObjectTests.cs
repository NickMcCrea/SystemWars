using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using Moq;
using MonoGameEngineCore;

namespace TestsMonoGameEngineCore
{
    [TestClass]
    public class GameObjectTests
    {

        [ClassInitialize]
        public static void Initialise(TestContext context)
        {
            SystemCore.GameObjectManager = new GameObjectManager();
        }


        [TestMethod]
        public void ConstructedGameObjectHasTransform()
        {
            GameObject o = new GameObject();
            Assert.IsNotNull(o.Transform);
            Assert.IsNotNull(o.GetComponent<TransformComponent>());
        }

        [TestMethod]
        public void TransformInComponentsList()
        {
            GameObject o = new GameObject();
            Assert.IsNotNull(o.GetComponent<TransformComponent>());

         
        }

        [TestMethod]
        public void ComponentAddedToObjectSuccessfully()
        {
            GameObject o = new GameObject();

            var mockComponent = Mock.Of<IComponent>();
            o.AddAndInitialise(mockComponent);

           
        }


        [TestMethod]
        public void GameObjectHasId()
        {
            GameObject o = new GameObject();
            Assert.IsFalse(o.ID == 0);
        }

        [TestMethod]
        public void GameObjectsHaveSequentialIds()
        {
            GameObject o = new GameObject();
            int id = o.ID;

            GameObject p = new GameObject();
            Assert.IsTrue(p.ID == id + 1);
        }


    }
}
