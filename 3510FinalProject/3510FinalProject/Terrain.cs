using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace _3510FinalProject
{
    public class Terrain : DrawableGameComponent
    {
        //Number of vertices of Vertex Grid in X direction, equivilent to the width of the height map image in pixels
        int vertexCountX;

        //Number of vertices of Vertex Grid in Z direction, equivilent to the height of the height map image in pixels
        int vertexCountZ;

        //Space in both X and Z direction between verteces in Vertex Grid
        float blockScale;

        //Heights provided by height map will be values 0 – 255, amount at which to scale these values
        float heightScale;

        //Top-Left cornor position in world
        Vector2 startPosition;

        //Array of heights from height map
        byte[] heightmap;

        //Number of vertices in vertex grid
        int numVertices;

        //Number of triangles in vertex grid
        int numTriangles;

        //All the vertices that make up the vertex grid
        VertexBuffer vb;

        //Indices of the vertices that make up the primitives that make up the terrain mesh
        IndexBuffer ib;

        //Used for drawing the terrain
        BasicEffect effect;

        //Texture placed over the terrain mesh
        Texture2D texture;

        public Terrain(Game game, int vertexCountX, int vertexCountZ, float blockScale, float heightScale)
            : base(game)
        {
            this.vertexCountX = vertexCountX;
            this.vertexCountZ = vertexCountZ;
            this.blockScale = blockScale;
            this.heightScale = heightScale;
            this.startPosition = new Vector2((vertexCountX - 1) * blockScale * .5f, (vertexCountZ - 1) * blockScale * .5f) * -1;


            Load("terrain");
        }

        public float GetHeight(Vector2 position)
        {
            return GetHeight(position.X, position.Y);
        }

        public float GetHeight(Vector3 position)
        {
            return GetHeight(position.X, position.Z);
        }

        public float GetHeight(float positionX, float positionZ)
        {
            Vector2 positionInGrid = new Vector2(positionX - startPosition.X, positionZ - startPosition.Y);
            Vector2 blockPosition = new Vector2(positionInGrid.X / blockScale, positionInGrid.Y / blockScale);

            if (blockPosition.X >= 0 && blockPosition.X < (vertexCountX - 1) && blockPosition.Y >= 0 && blockPosition.Y < (vertexCountZ - 1))
            {
                Vector2 blockOffset = new Vector2(blockPosition.X - (int)blockPosition.X, blockPosition.Y - (int)blockPosition.Y);
                int vertexIndex = (int)blockPosition.X + (int)blockPosition.Y * 		     vertexCountX;
                float height1 = heightmap[vertexIndex + 1];
                float height2 = heightmap[vertexIndex];
                float height3 = heightmap[vertexIndex + vertexCountX + 1];
                float height4 = heightmap[vertexIndex + vertexCountX];

                float heightIncX, heightIncY;
                //Top triangle
                if (blockOffset.X > blockOffset.Y)
                {
                    heightIncX = height1 - height2;
                    heightIncY = height3 - height1;
                }
                //Bottom triangle
                else
                {
                    heightIncX = height3 - height4;
                    heightIncY = height4 - height2;
                }

                float lerpHeight = height2 + heightIncX * blockOffset.X + heightIncY * blockOffset.Y;
                return lerpHeight * heightScale;
            }
            else
                return -999999;
        }

        public float? Intersects(Ray ray)
        {
            //This won't be changed if the Ray doesn't collide with terrain            
            float? collisionDistance = null;
            //Size of step is half of blockScale
            Vector3 rayStep = ray.Direction * blockScale * 0.5f;
            //Need to save start position to find total distance once collision point is found
            Vector3 rayStartPosition = ray.Position;

            Vector3 lastRayPosition = ray.Position;
            ray.Position += rayStep;
            float height = GetHeight(ray.Position);
            while (ray.Position.Y > height && height >= 0)
            {
                lastRayPosition = ray.Position;
                ray.Position += rayStep;
                height = GetHeight(ray.Position);
            }

            if (height >= 0) //Lowest possible point of terrain
            {
                Vector3 startPosition = lastRayPosition;
                Vector3 endPosition = ray.Position;
                // Binary search. Find the exact collision point
                for (int i = 0; i < 32; i++)
                {
                    // Binary search pass
                    Vector3 middlePoint = (startPosition + endPosition) * 0.5f;
                    if (middlePoint.Y < height)
                        endPosition = middlePoint;
                    else
                        startPosition = middlePoint;
                }
                Vector3 collisionPoint = (startPosition + endPosition) * 0.5f;
                collisionDistance = Vector3.Distance(rayStartPosition, collisionPoint);
            }//end if
            return collisionDistance;
        }



        public override void Initialize()
        {
            base.Initialize();
        }

        //Non-overridden Load(), includes everything to generate terrain mesh
        public void Load(string heightmapFileName)
        {
            Initialize();

            texture = Game.Content.Load<Texture2D>("Terrain/terrain_base_text");
            effect = new BasicEffect(Game.GraphicsDevice);

            int heightmapSize = vertexCountX * vertexCountZ;
            heightmap = new byte[heightmapSize];

            FileStream filestream = File.OpenRead(Game.Content.RootDirectory + "/Terrain/" + heightmapFileName + ".raw");
            filestream.Read(heightmap, 0, heightmapSize);
            filestream.Close();

            GenerateTerrainMesh();
        }

        //Called by Load(), creates the Mesh of the terrain
        private void GenerateTerrainMesh()
        {
            numVertices = vertexCountX * vertexCountZ;
            numTriangles = (vertexCountX - 1) * (vertexCountZ - 1) * 2;

            int[] indices = GenerateTerrainIndices();
            VertexPositionTexture[] vertices = GenerateTerrainVertices(indices);

            vb = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTexture), numVertices, BufferUsage.WriteOnly);
            vb.SetData<VertexPositionTexture>(vertices);

            ib = new IndexBuffer(GraphicsDevice, typeof(int), numTriangles * 3, BufferUsage.WriteOnly);
            ib.SetData<int>(indices);


        }

        //Creates the indices of the vertices that make up the triangle primitives that make up the mesh
        private int[] GenerateTerrainIndices()
        {
            int numIndices = numTriangles * 3;
            int[] indices = new int[numIndices];

            int indicesCount = 0;
            for (int i = 0; i < (vertexCountZ - 1); i++) //All Rows except last
                for (int j = 0; j < (vertexCountX - 1); j++) //All Columns except last
                {
                    int index = j + i * vertexCountZ; //2D coordinates to linear
                    //First Triangle Vertices
                    indices[indicesCount++] = index;
                    indices[indicesCount++] = index + 1;
                    indices[indicesCount++] = index + vertexCountX + 1;

                    //Second Triangle Vertices
                    indices[indicesCount++] = index + vertexCountX + 1;
                    indices[indicesCount++] = index + vertexCountX;
                    indices[indicesCount++] = index;
                }
            return indices;
        }

        //Creates the actual vertices that make up the vertex grid, setting their place in the 3D world based upon size of terrain, block scale, height map and height scale
        //Also determines texturing maping
        private VertexPositionTexture[] GenerateTerrainVertices(int[] terrainIndeces)
        {
            float halfTerrainWidth = (vertexCountX - 1) * blockScale * .5f;
            float halfTerrainDepth = (vertexCountZ - 1) * blockScale * .5f;
            float tuDerivative = 1.0f / (vertexCountX - 1);
            float tvDerivative = 1.0f / (vertexCountZ - 1);

            VertexPositionTexture[] vertices = new VertexPositionTexture[vertexCountX * vertexCountZ];
            ushort vertexCount = 0;
            float tu = 0;
            float tv = 0;

            for (float i = -halfTerrainDepth; i <= halfTerrainDepth; i += blockScale)
            {
                tu = 0.0f;
                for (float j = -halfTerrainWidth; j <= halfTerrainWidth; j += blockScale)
                {
                    vertices[vertexCount].Position = new Vector3(j, heightmap[vertexCount] * heightScale, i);
                    vertices[vertexCount].TextureCoordinate = new Vector2(tu, tv);

                    tu += tuDerivative;
                    vertexCount++;
                }
                tv += tvDerivative;
            }

            return vertices;
        }

        //Draws the terrain
        public override void Draw(GameTime gameTime)
        {
            Game1 myGame = (Game1)Game;

            effect.World = Matrix.Identity; //No transformation of the terrain
            effect.View = myGame.camera.view;
            effect.Projection = myGame.camera.projection;
            effect.Texture = texture;
            effect.TextureEnabled = true;

            GraphicsDevice.SetVertexBuffer(vb); //Set vertices
            GraphicsDevice.Indices = ib; //Set indices

            foreach (EffectPass CurrentPass in effect.CurrentTechnique.Passes)
            {
                CurrentPass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, 0, numTriangles); //Draw all triangles that make up the mesh

            }

            base.Draw(gameTime);
        }
    }
}
