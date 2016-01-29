using System.Collections.Generic;
using System.Linq;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Procedural;
using MonoGameEngineCore.Rendering;

namespace GridForgeResurrected.Screens
{
    public class MiniPlanet
    {
        private readonly Vector3 startPosition;
        private readonly float planetRadius;
        private readonly float innerAtmosphereRadius;
        private readonly float outerAtmosphereRadius;
        private readonly IModule noiseModule;
        private readonly int heightMapVertCount;
        private readonly float heightMapScale;
        private readonly Color terrainColor;
        private GroundScatteringHelper groundScatteringHelper;
        private Atmosphere atmosphere;
        private List<GameObject> planetPieces;
        private Effect planetEffect;
        public Vector3 CurrentPosition { get; private set; }

        public MiniPlanet(Vector3 startPosition, float planetRadius, float innerAtmosphereRadius, float outerAtmosphereRadius, IModule noiseModule, int heightMapVertCount, float heightMapScale, Color terrainColor)
        {
            this.startPosition = startPosition;
            this.planetRadius = planetRadius;
            this.innerAtmosphereRadius = innerAtmosphereRadius;
            this.outerAtmosphereRadius = outerAtmosphereRadius;
            this.noiseModule = noiseModule;
            this.heightMapVertCount = heightMapVertCount;
            this.heightMapScale = heightMapScale;
            this.terrainColor = terrainColor;
            planetPieces = new List<GameObject>();
            planetEffect = EffectLoader.LoadSM5Effect("AtmosphericScatteringGround").Clone();
            GenerateGeometry();
        }

        public void GenerateGeometry()
        {


            groundScatteringHelper = new GroundScatteringHelper(planetEffect, outerAtmosphereRadius, innerAtmosphereRadius);
            atmosphere = new Atmosphere(outerAtmosphereRadius, innerAtmosphereRadius);
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(atmosphere);


            //top
            GameObject oTop;
            VertexPositionColorTextureNormal[] vTop;
            var hTop = CreatePlanetGameObjects(out oTop, out vTop);

            for (int i = 0; i < vTop.Length; i++)
            {
                vTop[i].Position = vTop[i].Position - new Vector3(planetRadius, 0, planetRadius);
                vTop[i].Position = vTop[i].Position + Vector3.Up * planetRadius;
                vTop[i].Position = Vector3.Normalize(vTop[i].Position) * planetRadius;

                ModifyHeight(vTop, i);
            }

            InitialiseAndAddPlanetPiece(vTop, hTop, oTop);


            //front
            GameObject oFront;
            VertexPositionColorTextureNormal[] vFront;
            var hFront = CreatePlanetGameObjects(out oFront, out vFront);


            for (int i = 0; i < vFront.Length; i++)
            {
                vFront[i].Position = vFront[i].Position - new Vector3(planetRadius, 0, planetRadius);
                vFront[i].Position = Vector3.Transform(vFront[i].Position, Matrix.CreateRotationX(MathHelper.PiOver2));
                vFront[i].Position = vFront[i].Position - Vector3.Forward * planetRadius;
                vFront[i].Position = Vector3.Normalize(vFront[i].Position) * planetRadius;

                ModifyHeight(vFront, i);
            }

            InitialiseAndAddPlanetPiece(vFront, hFront, oFront);


            //back
            GameObject oBack;
            VertexPositionColorTextureNormal[] vBack;
            var hBack = CreatePlanetGameObjects(out oBack, out vBack);


            for (int i = 0; i < vBack.Length; i++)
            {
                vBack[i].Position = vBack[i].Position - new Vector3(planetRadius, 0, planetRadius);
                vBack[i].Position = Vector3.Transform(vBack[i].Position, Matrix.CreateRotationX(MathHelper.PiOver2));
                vBack[i].Position = vBack[i].Position - Vector3.Backward * planetRadius;
                vBack[i].Position = Vector3.Normalize(vBack[i].Position) * planetRadius;

                ModifyHeight(vBack, i);
            }

            InitialiseAndAddPlanetPiece(vBack, hBack, oBack, true);


            //left
            GameObject oLeft;
            VertexPositionColorTextureNormal[] vLeft;
            var hLeft = CreatePlanetGameObjects(out oLeft, out vLeft);


            for (int i = 0; i < vLeft.Length; i++)
            {
                vLeft[i].Position = vLeft[i].Position - new Vector3(planetRadius, 0, planetRadius);
                vLeft[i].Position = Vector3.Transform(vLeft[i].Position, Matrix.CreateRotationZ(MathHelper.PiOver2));
                vLeft[i].Position = vLeft[i].Position - Vector3.Left * planetRadius;
                vLeft[i].Position = Vector3.Normalize(vLeft[i].Position) * planetRadius;

                ModifyHeight(vLeft, i);
            }

            InitialiseAndAddPlanetPiece(vLeft, hLeft, oLeft, true);

            //right 
            GameObject oRight;
            VertexPositionColorTextureNormal[] vRight;
            var hRight = CreatePlanetGameObjects(out oRight, out vRight);


            for (int i = 0; i < vRight.Length; i++)
            {
                vRight[i].Position = vRight[i].Position - new Vector3(planetRadius, 0, planetRadius);
                vRight[i].Position = Vector3.Transform(vRight[i].Position, Matrix.CreateRotationZ(MathHelper.PiOver2));
                vRight[i].Position = vRight[i].Position - Vector3.Right * planetRadius;
                vRight[i].Position = Vector3.Normalize(vRight[i].Position) * planetRadius;

                ModifyHeight(vRight, i);
            }

            InitialiseAndAddPlanetPiece(vRight, hRight, oRight);


            //bottom
            GameObject oBottom;
            VertexPositionColorTextureNormal[] vBottom;
            var hBottom = CreatePlanetGameObjects(out oBottom, out vBottom);

            for (int i = 0; i < vBottom.Length; i++)
            {
                vBottom[i].Position = vBottom[i].Position - new Vector3(planetRadius, 0, planetRadius);
                vBottom[i].Position = vBottom[i].Position + Vector3.Down * planetRadius;
                vBottom[i].Position = Vector3.Normalize(vBottom[i].Position) * planetRadius;

                ModifyHeight(vBottom, i);
            }

            InitialiseAndAddPlanetPiece(vBottom, hBottom, oBottom, true);


            SetPosition(startPosition);

        }

        private void ModifyHeight(VertexPositionColorTextureNormal[] v, int i)
        {
            double heightValue = noiseModule.GetValue(v[i].Position.X, v[i].Position.Y, v[i].Position.Z);
            v[i].Position = (Vector3.Normalize(v[i].Position) * (planetRadius + (float)heightValue));
        }

        private void InitialiseAndAddPlanetPiece(VertexPositionColorTextureNormal[] v, Heightmap hMap,
            GameObject terrainObject, bool reverseIndices = false)
        {
            var verts =
                BufferBuilder.VertexBufferBuild(v);

            var inds = hMap.GenerateIndices();

            if (reverseIndices)
                inds = inds.Reverse().ToArray();

            var indices = BufferBuilder.IndexBufferBuild(inds);



            terrainObject.AddComponent(new RenderGeometryComponent(verts, indices, indices.IndexCount / 3));
            terrainObject.AddComponent(new EffectRenderComponent(planetEffect));
            SystemCore.GameObjectManager.AddAndInitialiseGameObject(terrainObject);
            planetPieces.Add(terrainObject);
        }

        private Heightmap CreatePlanetGameObjects(out GameObject terrainTopObject, out VertexPositionColorTextureNormal[] v)
        {
            Heightmap top = new Heightmap(heightMapVertCount, heightMapScale);
            terrainTopObject = new GameObject();
            v = top.GenerateVertexArray();

            for(int i = 0;i<v.Length;i++)
            {
                v[i].Color = terrainColor;
            }
      

            return top;
        }

        public void Update(GameTime gameTime, float heightAbovePlanetSurface, Vector3 cameraPos)
        {
            DiffuseLight light = SystemCore.ActiveScene.LightsInScene[0] as DiffuseLight;

            if (groundScatteringHelper != null)
            {
                groundScatteringHelper.Update(heightAbovePlanetSurface, light.LightDirection,
                    cameraPos - CurrentPosition);

                atmosphere.Update(light.LightDirection, cameraPos, heightAbovePlanetSurface);
            }

        }

        public void SetPosition(Vector3 position)
        {
            foreach (GameObject piece in planetPieces)
            {
                piece.Transform.SetPosition(position);
            }
            atmosphere.Transform.WorldMatrix.Translation = position;
            CurrentPosition = position;
        }
    }
}