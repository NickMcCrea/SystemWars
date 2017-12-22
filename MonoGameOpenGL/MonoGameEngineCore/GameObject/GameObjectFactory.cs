using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using Microsoft.Xna.Framework;
using MonoGameEngineCore.Rendering;

namespace MonoGameEngineCore.GameObject
{
    public static class GameObjectFactory
    {
        /// <summary>
        /// No physics component. 
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        public static GameObject CreateRenderableGameObjectFromShape(ProceduralShape shape, Effect effect)
        {
            var ob = new GameObject();
            ob.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape), BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));


            if (shape is LineBatch)
                ob.AddComponent(new LineRenderComponent(effect as BasicEffect));

            ob.AddComponent(new EffectRenderComponent(effect));


            return ob;
        }

        public static GameObject CreateRenderTextureSurface(ProceduralPlane plane, Effect renderTextureEffect)
        {
            var ob = new GameObject();
            ob.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(plane), BufferBuilder.IndexBufferBuild(plane), plane.PrimitiveCount));

            ob.AddComponent(new RenderTextureComponent(renderTextureEffect));
            return ob;
        }

        public static GameObject CreateRenderableGameObjectFromShape(int id, ProceduralShape shape, Effect effect)
        {
            var ob = CreateRenderableGameObjectFromShape(shape, effect);
            ob.ID = id;
            return ob;
        }

        public static GameObject CreateRenderableGameObjectFromShape(string name, ProceduralShape shape, Effect effect)
        {
            var obj = CreateRenderableGameObjectFromShape(shape, effect);
            obj.Name = name;
            return obj;
        }


        public static GameObject CreateSkyDomeObject(Color color, int complexity, float scale)
        {
            ProceduralSphere sphere = new ProceduralSphere(complexity, complexity);
            sphere.Indices = sphere.Indices.Reverse().ToArray();

            var ob = new GameObject();
            ob.Name = "skydome";
            ob.Transform.Scale = scale;
            sphere.SetColor(color);
            ob.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(sphere), BufferBuilder.IndexBufferBuild(sphere), sphere.PrimitiveCount));
            ob.AddComponent(new SkyDomeRenderer(EffectLoader.LoadSM5Effect("skydome")));

            return ob;
        }

        public static GameObject CreateGradientSkyDomeObject(int complexity)
        {
            ProceduralSphere sphere = new ProceduralSphere(complexity, complexity);
            sphere.Indices = sphere.Indices.Reverse().ToArray();

            var ob = new GameObject();
            ob.Name = "skydome";

            ob.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(sphere), BufferBuilder.IndexBufferBuild(sphere), sphere.PrimitiveCount));
            ob.AddComponent(new GradientSkyDomeRenderer(EffectLoader.LoadSM5Effect("skydome2")));

            return ob;
        }
        /// <summary>
        /// Has a physics component for collision, but will not be simulated.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="effect"></param>
        /// <param name="meshType"></param>
        /// <returns></returns>
        public static GameObject CreateCollidableObject(ProceduralShape shape, Effect effect, PhysicsMeshType meshType)
        {
            var ob = CreateRenderableGameObjectFromShape(shape, effect);
            ob.AddComponent(new PhysicsComponent(true, false, meshType));
            return ob;
        }

        /// <summary>
        /// Fully simulated physics object.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="effect"></param>
        /// <param name="meshType"></param>
        /// <returns></returns>
        public static GameObject CreateSimulatedObject(ProceduralShape shape, Effect effect, PhysicsMeshType meshType)
        {
            var ob = CreateRenderableGameObjectFromShape(shape, effect);
            ob.AddComponent(new PhysicsComponent(true, true, meshType));
            return ob;
        }


    }
}
