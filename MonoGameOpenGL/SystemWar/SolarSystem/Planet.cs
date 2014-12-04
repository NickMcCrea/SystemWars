using System;
using System.Collections.Generic;
using BEPUutilities;
using LibNoise;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGameEngineCore.GameObject;
using MonoGameEngineCore.GameObject.Components;
using MonoGameEngineCore.Helper;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using Matrix = Microsoft.Xna.Framework.Matrix;
using Ray = Microsoft.Xna.Framework.Ray;
using Vector3 = Microsoft.Xna.Framework.Vector3;

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
        public int BuildCountPerSecond;
        public int BuildTally;
        private TimeSpan lastClearTime;
        public bool visualisePatches = true;

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


            DefineAdjacency(n1, n3, n4, n6, n5, n2);

            n1.Planet = this;
            n2.Planet = this;
            n3.Planet = this;
            n4.Planet = this;
            n5.Planet = this;
            n6.Planet = this;

            Enabled = true;
        }

        private static void DefineAdjacency(PlanetQuadTreeNode top, PlanetQuadTreeNode bottom, PlanetQuadTreeNode forward, PlanetQuadTreeNode backward,
            PlanetQuadTreeNode left, PlanetQuadTreeNode right)
        {
            top.AddNeighbour(Direction.north, forward);
            top.AddNeighbour(Direction.south, backward);
            top.AddNeighbour(Direction.west, left);
            top.AddNeighbour(Direction.east, right);

            bottom.AddNeighbour(Direction.north, forward);
            bottom.AddNeighbour(Direction.south, backward);
            bottom.AddNeighbour(Direction.west, left);
            bottom.AddNeighbour(Direction.east, right);

            forward.AddNeighbour(Direction.north, top);
            forward.AddNeighbour(Direction.south, bottom);
            forward.AddNeighbour(Direction.west, right);
            forward.AddNeighbour(Direction.east, left);

            backward.AddNeighbour(Direction.north, top);
            backward.AddNeighbour(Direction.south, bottom);
            backward.AddNeighbour(Direction.west, left);
            backward.AddNeighbour(Direction.east, right);

            right.AddNeighbour(Direction.north, top);
            right.AddNeighbour(Direction.south, bottom);
            right.AddNeighbour(Direction.west, backward);
            right.AddNeighbour(Direction.east, forward);

            left.AddNeighbour(Direction.north, top);
            left.AddNeighbour(Direction.south, bottom);
            left.AddNeighbour(Direction.west, forward);
            left.AddNeighbour(Direction.east, backward);
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

            lastClearTime += new TimeSpan(0, 0, 0, 0, gameTime.ElapsedGameTime.Milliseconds);
            if (lastClearTime.TotalMilliseconds > 1000)
            {
                lastClearTime = new TimeSpan();
                BuildCountPerSecond = BuildTally;
                BuildTally = 0;
            }

        }

        

        public bool RayCast(Vector3 pos, Vector3 dir, float distance, out Vector3 hitLocation)
        {
            List<PlanetQuadTreeNode> potentialCollisions = new List<PlanetQuadTreeNode>();
            Ray ray = new Ray(pos, dir);

            for (int i = 0; i < rootNodes.Count; i++)
            {
                BroadPhaseRayCast(rootNodes[i], ray, ref potentialCollisions);
            }

            foreach (PlanetQuadTreeNode node in potentialCollisions)
            {
                var collider = node.gameObject.GetComponent<MeshColliderComponent>();
                if (collider != null)
                {
                    RayHit hit;
                    if (collider.RayCollision(pos, dir, distance, out hit))
                    {
                        hitLocation = hit.Location.ToXNAVector();
                        return true;
                    }
                }
                else
                {
                  
                }
            }

            hitLocation = Vector3.Zero;
            return false;
        }

        private void BroadPhaseRayCast(PlanetQuadTreeNode node, Ray ray, ref List<PlanetQuadTreeNode> potentialCollisions)
        {
            if (node.isLeaf)
            {
                if (node.boundingSphere.Intersects(ray).HasValue == true)
                    potentialCollisions.Add(node);
            }
            else
            {
                foreach (PlanetQuadTreeNode child in node.Children)
                {
                    BroadPhaseRayCast(child, ray, ref potentialCollisions);
                }
            }
        }

        public int UpdateOrder
        {
            get;
            set;
        }

        public event EventHandler<EventArgs> UpdateOrderChanged;


    }


}
