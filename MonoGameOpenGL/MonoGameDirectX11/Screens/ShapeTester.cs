using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.ScreenManagement;


namespace MonoGameDirectX11.Screens
{
    public class ShapeTesterScreen : MouseCamScreen
    {


        public ShapeTesterScreen()
            : base()
        {

            //ProceduralShapeBuilder shapeBuilder = new ProceduralShapeBuilder();
            ////shapeBuilder.AddFace(new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0));
            ////shapeBuilder.AddFace(new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1.5f, 0.5f, 0), new Vector3(1, 0, 0), new Vector3(0, 0, 0));
            ////shapeBuilder.AddBevelledSquareFace(new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(0, 0, 0), new Vector3(1, 0, 0),-0.1f, 0.5f);
            //shapeBuilder.AddBevelledFace(0.1f,-0.02f, new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(1.5f,0.5f,0),  new Vector3(1, 0, 0),new Vector3(0, 0, 0));
            //ProceduralShape shape = shapeBuilder.BakeShape();
            //shape.SetColor(Color.Blue);

            ProceduralShape shape = new ProceduralSphere(10,10);
            shape.Translate(new Vector3(-10, 0, 0));


            var shape2 = ProceduralShape.Mirror(shape, new Plane(Vector3.Left,0));
            shape = ProceduralShape.Combine(shape, shape2);

            ProceduralCylinder cylinder = new ProceduralCylinder(1, 1, 5, 10, 10);
            cylinder.SetColor(SystemCore.ActiveColorScheme.Color1);
            shape = ProceduralShape.Combine(cylinder, shape);

         
            var effect = SystemCore.ContentManager.Load<Effect>("Effects/SM5.0/FlatShaded");

           

            var gameObject = new GameObject();
            gameObject.AddComponent(new RenderGeometryComponent(BufferBuilder.VertexBufferBuild(shape), BufferBuilder.IndexBufferBuild(shape), shape.PrimitiveCount));
            gameObject.AddComponent(new EffectRenderComponent(effect));
            //gameObject.AddComponent(new NormalVisualiser());
            //gameObject.Transform.SetPosition(RandomHelper.GetRandomVector3(Vector3.One * -10, Vector3.One * 10));
            gameObject.AddComponent(new RotatorComponent(Vector3.Up));
            gameObject.AddComponent(new RotatorComponent(Vector3.Left));

            SystemCore.GetSubsystem<GameObjectManager>().AddAndInitialiseGameObject(gameObject);

         

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Render(GameTime gameTime)
        {
           
            base.Render(gameTime);
        }
    }
}
