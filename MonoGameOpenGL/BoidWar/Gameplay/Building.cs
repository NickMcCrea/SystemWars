using Microsoft.Xna.Framework;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoidWar.Gameplay
{

    public class MainBase : Building
    {


        public MainBase()
        {
            ProceduralCube shape = new ProceduralCube();
            shape.Scale(5f);
            shape.SetColor(Color.Blue);
            AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape),
                BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
            AddComponent(new EffectRenderComponent(EffectLoader.LoadSM5Effect("flatshaded")));
            AddComponent(new ShadowCasterComponent());
            var physicsComponent = new PhysicsComponent(false, false, PhysicsMeshType.box);
            AddComponent(physicsComponent);


            Health = 100;

        }

        public void Update(GameTime gameTime)
        {

        }

    }


    public class Building : GameObject
    {
        public int Health { get; set; }

    }



}
