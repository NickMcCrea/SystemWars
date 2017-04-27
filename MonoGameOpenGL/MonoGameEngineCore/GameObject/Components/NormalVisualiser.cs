using Microsoft.Xna.Framework;
using MonoGameEngineCore.Helper;
using MonoGameEngineCore.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonoGameEngineCore.GameObject.Components
{
    public class NormalVisualiser : IComponent, IDrawable
    {

        private VertexPositionColorTextureNormal[] renderData;

        public NormalVisualiser()
        {
            Visible = true;

        }

        public GameObject ParentObject { get; set; }

        public void Draw(GameTime gameTime)
        {
            //read render data from buffer.
            if (renderData == null)
            {

                RenderGeometryComponent renderGeometry = ParentObject.GetComponent<RenderGeometryComponent>();
                renderData = new VertexPositionColorTextureNormal[renderGeometry.VertexBuffer.VertexCount];
                renderGeometry.VertexBuffer.GetData<VertexPositionColorTextureNormal>(renderData);
            }

            for (int i = 0; i < renderData.Length; i++)
            {
                Vector3 transformedPosition = Vector3.Transform(renderData[i].Position, Matrix.CreateScale(ParentObject.Transform.Scale) * ParentObject.Transform.AbsoluteTransform);
                Vector3 transformedNormal = Vector3.Transform(renderData[i].Normal, Matrix.CreateScale(ParentObject.Transform.Scale) * ParentObject.Transform.AbsoluteTransform);
                DebugShapeRenderer.AddLine(transformedPosition, transformedPosition + transformedNormal * 0.1f, Color.Red);
            }
        }

        public int DrawOrder
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> DrawOrderChanged;

        public bool Visible
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> VisibleChanged;

        public void PostInitialise()
        {

        }

        public void Initialise()
        {

        }
    }
}
