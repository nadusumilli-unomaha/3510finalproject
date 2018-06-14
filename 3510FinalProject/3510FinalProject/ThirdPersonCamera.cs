using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace _3510FinalProject
{
    public class ThirdPersonCamera : GameComponent
    {
        public Matrix view { get; protected set; }
        public Matrix projection { get; protected set; }
        public Vector3 cameraPosition { get; protected set; }
        public Vector3 cameraDirection; //Camera pointing direction
        public AnimatedModel chaseObject; //Object being chased
        Vector3 cameraUp;

        public ThirdPersonCamera(Game1 game, AnimatedModel target, Vector3 up, Viewport viewPort)
            : base(game)
        {
            cameraPosition = target.CameraOffsetPosition;
            cameraDirection = target.CameraOffsetPosition - target.position;
            cameraDirection.Normalize(); 
            cameraUp = up;
            this.chaseObject = target;

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, viewPort.AspectRatio, 0.5f, 1000f);

            CreateLookAt();
        }
            
        //Speed
        float chaseSpeed = 2f;

        // Bounds
        float desiredChaseDistance = 200f;
        float minChaseDistance = 120f;
        float maxChaseDistance = 400f;
        public Vector3 desiredCameraPosition;

        public override void Initialize()
        {
            Mouse.SetPosition(Game.Window.ClientBounds.Width / 2, Game.Window.ClientBounds.Height / 2);
        }

        public override void Update(GameTime gameTime)
        {
            float interpolatedSpeed = MathHelper.Clamp(chaseSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0.0f, 1.0f);
            desiredCameraPosition = chaseObject.CameraOffsetPosition - cameraDirection * desiredChaseDistance;
            desiredCameraPosition = Vector3.Lerp(cameraPosition, desiredCameraPosition, interpolatedSpeed);

            Vector3 targetVector = desiredCameraPosition - chaseObject.CameraOffsetPosition;
            float targetLength = targetVector.Length();
            targetVector.Normalize();

            // Sets desired position if too close from target
            if (targetLength < minChaseDistance)
            {
                desiredCameraPosition = chaseObject.CameraOffsetPosition + targetVector * minChaseDistance;
            }
            // Sets desired position if too distant from target
            else if (targetLength > maxChaseDistance)
            {
                desiredCameraPosition = chaseObject.CameraOffsetPosition + targetVector * maxChaseDistance;
            }

            // Sets camera position to the desired value
            cameraPosition = desiredCameraPosition;
            
            // Sets camera direction to look to the target current position
            cameraDirection = chaseObject.CameraOffsetPosition - cameraPosition;
            cameraDirection.Normalize();

            CreateLookAt();

            base.Update(gameTime);
        }
        
        public void SetChaseParameters(float desired, float min, float max, float speed)
        {
            desiredChaseDistance = desired;
            minChaseDistance = min;
            maxChaseDistance = max;
            chaseSpeed = speed;
        }

        private void CreateLookAt()
        {
            view = Matrix.CreateLookAt(cameraPosition, cameraPosition + cameraDirection, cameraUp);
        }
    }
}
