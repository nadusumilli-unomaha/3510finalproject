using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace _3510FinalProject
{
    /// <summary>
    /// An AnimatedModel that can be controlled by player input
    /// </summary>
    public class Player : AnimatedModel
    {
        private float heightOffset = 10f;
        private float? maximumSlopeDistance = 8f;
        private bool jumpEnabled = false;
        private Vector3 velocity;


        public Player(String assetName, Vector3 translation, Game game) : base(assetName, translation, game)
        {

        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            KeyboardState keyState = Keyboard.GetState();
            Vector3 lastPosition = position;
            velocity.X = MathHelper.Clamp(MathHelper.SmoothStep(velocity.X, 0, 0.1f), -2, 2);
            velocity.Z = MathHelper.Clamp(MathHelper.SmoothStep(velocity.Z, 0, 0.1f), -2, 2);
            velocity.Y = MathHelper.Clamp(MathHelper.SmoothStep(velocity.Y, -1, 0.2f), -10, 10);

            Matrix rotationMatrix = Matrix.CreateRotationY(rotation.Y + (float)Math.PI);
            Vector3 forward = Vector3.Transform(Vector3.Forward, rotationMatrix);

            if (keyState.IsKeyDown(Keys.Up) || keyState.IsKeyDown(Keys.W))      //Forward
                velocity += forward;
            if (keyState.IsKeyDown(Keys.Down) || keyState.IsKeyDown(Keys.S))    //Backward
                velocity += -forward;
            if (keyState.IsKeyDown(Keys.Right) || keyState.IsKeyDown(Keys.D))   //Right
                rotation += new Vector3(0, -0.1f, 0);
            if (keyState.IsKeyDown(Keys.Left) || keyState.IsKeyDown(Keys.A))    //Left
                rotation += new Vector3(0, 0.1f, 0);

            if (keyState.IsKeyDown(Keys.Space) && jumpEnabled)
            {
                velocity += Vector3.Transform(Vector3.Up, rotationMatrix) * 5f;
            }

            //Apply velocity
            position += velocity;
            ApplyTerrainCollision(lastPosition);

            base.Update(gameTime);
        }

        private void ApplyTerrainCollision(Vector3 lastTranslation)
        {
            try
            {
                Game1 game = (Game1)base.Game;
                Terrain terrain = game.terrain;

                //Navigation collision
                Vector3 direction = position - lastTranslation;
                direction.Normalize();
                Ray ray = new Ray(lastTranslation, direction);
                float? distance = terrain.Intersects(ray);
                if (distance != null)
                {
                    if (distance <= maximumSlopeDistance)
                        position = lastTranslation;
                }             

                // Height collision
                float height = terrain.GetHeight(position);
                if (position.Y < height + heightOffset)
                {
                    position.Y = height + heightOffset;
                    jumpEnabled = true;
                }
                else
                {
                    jumpEnabled = false;
                }

                //Bounds collision
                if (height < -9999)
                    position = lastTranslation;

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to convert game to Game1 on CheckTerrainCollision()");
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
