using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using AnimationAux;

namespace _3510FinalProject
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        public ThirdPersonCamera camera;
        public Terrain terrain;

        private Player player = null;
        private AnimatedModel currentAnimation = null;

        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            player = new Player("MainCharacter/model", new Vector3(0,0,0), this);

            currentAnimation = new AnimatedModel("MainCharacter/run_forward", new Vector3(0, 0, 0), this);

            AnimationClip clip = currentAnimation.Clips[0];

            AnimationPlayer animationPlayer = player.PlayClip(clip);
            animationPlayer.Looping = true;
            Components.Add(player);

            terrain = new Terrain(this, 256, 256, 20f, 5f);
            Components.Add(terrain);

            //Creates a Third Person Camera and assigns it to follow player
            camera = new ThirdPersonCamera(this, player, Vector3.Up, new Viewport(0, 0, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight));
            Components.Add(camera);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            
        }

        protected override void UnloadContent()
        {

        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.White);

            base.Draw(gameTime);
        }
    }
}

