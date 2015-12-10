using Microsoft.Xna.Framework;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore
{
    public static class SystemCoreHelper
    {
        /// <summary>
        /// Sets up default ambient and a single diffuse light for the scene, adds a cube at Vector3.Zero. For scene testing at the start of a project.
        /// </summary>
        public static void SetUpTestCubeAndLighting()
        {
            SystemCore.ActiveScene.SetUpDefaultAmbientAndDiffuseLights();
            ProceduralCube cube = new ProceduralCube();
            cube.SetColor(Color.OrangeRed);
            GameObject.GameObject testObject = GameObjectFactory.CreateRenderableGameObjectFromShape(cube, EffectLoader.LoadSM5Effect("flatshaded"));
            testObject.AddComponent(new RotatorComponent(Vector3.Up, 0.0001f));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(testObject);
        }
    }
}
