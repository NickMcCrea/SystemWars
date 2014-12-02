using System;
using System.Collections.Generic;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.Helper;

namespace MonoGameEngineCore.Procedural
{


    public class Planet : IComponent, IUpdateable
    {
        private readonly IModule module;
        private readonly Effect testEffect;
        private readonly float radius;
        private float splitDistance;
        private float mergeDistance;
        public Matrix customProjection;
        public BasicEffect planetEffect;
        public List<PlanetQuadTreeNode> rootNodes;
        public Color SeaColor;
        public Color LandColor;
        public Color MountainColor;
        public int planetId;
        private static int planetIdList;

       
        public Planet(IModule module, Effect testEffect, float radius, Color sea, Color land, Color mountains)
        {
            planetId = ++planetIdList;
            this.module = module;
            this.testEffect = testEffect;
            this.radius = radius;

            planetEffect = new BasicEffect(SystemCore.GraphicsDevice);
            planetEffect.EnableDefaultLighting();
            planetEffect.VertexColorEnabled = true;

            splitDistance = radius * 4;
            mergeDistance = radius * 4.5f;

            customProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, this.radius * 10);

            this.SeaColor = sea;
            this.LandColor = land;
            this.MountainColor = mountains;
        }

        private void GenerateCustomProjectionMatrix(float far)
        {
            if (far <= 0)
                far = 2;

            customProjection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                SystemCore.GraphicsDevice.Viewport.AspectRatio, 1f, far);
        }

        public GameObject.GameObject ParentObject
        {
            get;
            set;
        }

        public void Initialise()
        {
            rootNodes = new List<PlanetQuadTreeNode>();



            float vectorSpacing = 1f;
            float cubeVerts = 21;
            float sphereSize = radius;


            //top
            PlanetQuadTreeNode n1 = new PlanetQuadTreeNode(1, 1, this, null, new Vector3(-cubeVerts / 2, cubeVerts / 2 - 1, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2 - 1, cubeVerts / 2), vectorSpacing, Vector3.Up, sphereSize);
            n1.StartGeometryGeneration(testEffect, module);


            //bottom
            PlanetQuadTreeNode n2 = new PlanetQuadTreeNode(2, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Down, sphereSize);
            n2.StartGeometryGeneration(testEffect, module);


            //forward
            PlanetQuadTreeNode n3 = new PlanetQuadTreeNode(3, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Forward, sphereSize);
            n3.StartGeometryGeneration(testEffect, module);


            //backward
            PlanetQuadTreeNode n4 = new PlanetQuadTreeNode(4, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, cubeVerts / 2 - 1), new Vector3(cubeVerts / 2, cubeVerts / 2, cubeVerts / 2 - 1), vectorSpacing, Vector3.Backward, sphereSize);
            n4.StartGeometryGeneration(testEffect, module);

            //right
            PlanetQuadTreeNode n5 = new PlanetQuadTreeNode(5, 1, this, null, new Vector3(-cubeVerts / 2, -cubeVerts / 2, -cubeVerts / 2), new Vector3(-cubeVerts / 2, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Right, sphereSize);
            n5.StartGeometryGeneration(testEffect, module);

            //left
            PlanetQuadTreeNode n6 = new PlanetQuadTreeNode(6, 1, this, null, new Vector3(cubeVerts / 2 - 1, -cubeVerts / 2, -cubeVerts / 2), new Vector3(cubeVerts / 2 - 1, cubeVerts / 2, cubeVerts / 2), vectorSpacing, Vector3.Left, sphereSize);
            n6.StartGeometryGeneration(testEffect, module);

            rootNodes.Add(n1);
            rootNodes.Add(n2);
            rootNodes.Add(n3);
            rootNodes.Add(n4);
            rootNodes.Add(n5);
            rootNodes.Add(n6);


            n1.AddNeighbour(Direction.north, n3);
            n1.AddNeighbour(Direction.south, n4);
            n1.AddNeighbour(Direction.west, n6);
            n1.AddNeighbour(Direction.east, n5);


            n1.Planet = this;
            n2.Planet = this;
            n3.Planet = this;
            n4.Planet = this;
            n5.Planet = this;
            n6.Planet = this;

            Enabled = true;
        }

        public bool Enabled
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> EnabledChanged;

        public void Update(GameTime gameTime)
        {
            foreach (PlanetQuadTreeNode n in rootNodes)
            {
                n.Update(gameTime, splitDistance, mergeDistance);

            }

            Vector3 toCenterOfPlanet = ParentObject.Transform.WorldMatrix.Translation;
            float distanceToCenterOfPlanet = toCenterOfPlanet.Length();

            float surfaceDistance = distanceToCenterOfPlanet - radius;


            //we want to draw in the far plane the closer we get to the surface.
            float farPlaneMultiplier = MonoMathHelper.MapFloatRange(radius, radius * 2, 0.3f, 1f, surfaceDistance);
            GenerateCustomProjectionMatrix(distanceToCenterOfPlanet * farPlaneMultiplier);


            //GenerateCustomProjectionMatrix(10000);
            //var frustrum = new BoundingFrustum(SystemCore.GetCamera("main").View * customProjection);
            //DebugShapeRenderer.AddBoundingFrustum(frustrum, Color.Blue);

            FixSeams();

        }

        private void FixSeams()
        {
            //we want to walk down the tree, and ensure that where we have adjacent leaf nodes of different depths
            //that we 'collapse' the vertices of the edge of the greater depth patch to match the coarser patch.

            //walk down tree, if not leaf node, continue.
            //if leaf node, determine neighbours of lower depth (e.g. less detailed). 
            //walk through verts of shared edge and re-position non-shared verts onto shared ones.

            //keep going

        }

        public int UpdateOrder
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> UpdateOrderChanged;


    }


}
