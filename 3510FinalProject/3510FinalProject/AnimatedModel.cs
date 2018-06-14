using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Content;
using AnimationAux;

namespace _3510FinalProject
{
    /// <summary>
    /// An encloser for an XNA model that we will use that includes support for
    /// bones, animation, and some manipulations.
    /// </summary>
    public class AnimatedModel : DrawableGameComponent
    {
        #region Fields

        /// <summary>
        /// The actual underlying XNA model
        /// </summary>
        private Model model = null;

        /// <summary>
        /// Extra data associated with the XNA model
        /// </summary>
        private ModelExtra modelExtra = null;

        /// <summary>
        /// The model bones
        /// </summary>
        private List<Bone> bones = new List<Bone>();

        /// <summary>
        /// The model asset name
        /// </summary>
        private string assetName = "";

        /// <summary>
        /// An associated animation clip player
        /// </summary>
        private AnimationPlayer player = null;

        private Vector3 cameraOffset = new Vector3(0, 150, 0);

        public Vector3 position = new Vector3(0, 100, 5);
        public Vector3 rotation = new Vector3(0, 0, 0);
        public Vector3 scale = new Vector3(1f, 1f, 1f);

        #endregion

        #region Properties

        /// <summary>
        /// The actual underlying XNA model
        /// </summary>
        public Model Model
        {
            get { return model; }
        }

        public Vector3 CameraOffsetPosition
        {
            get { return cameraOffset + position; }
        }

        /// <summary>
        /// The underlying bones for the model
        /// </summary>
        public List<Bone> Bones { get { return bones; } }

        /// <summary>
        /// The model animation clips
        /// </summary>
        public List<AnimationClip> Clips { get { return modelExtra.Clips; } }

        #endregion

        #region Construction and Loading

        /// <summary>
        /// Constructor. Creates the model from an XNA model
        /// </summary>
        /// <param name="assetName">The name of the asset for this model</param>
        public AnimatedModel(string assetName, Vector3 translation, Game game) : base(game)
        {
            this.assetName = assetName;

            LoadContent();
        }

        /// <summary>
        /// Custom Load Content
        /// </summary>
        /// <param name="content"></param>
        private void LoadContent()
        {
            this.model = base.Game.Content.Load<Model>(assetName);
            modelExtra = model.Tag as ModelExtra;
            System.Diagnostics.Debug.Assert(modelExtra != null);

            ObtainBones();
        }


        #endregion

        #region Bones Management

        /// <summary>
        /// Get the bones from the model and create a bone class object for
        /// each bone. We use our bone class to do the real animated bone work.
        /// </summary>
        private void ObtainBones()
        {
            bones.Clear();
            foreach (ModelBone bone in model.Bones)
            {
                // Create the bone object and add to the heirarchy
                Bone newBone = new Bone(bone.Name, bone.Transform, bone.Parent != null ? bones[bone.Parent.Index] : null);

                // Add to the bones for this model
                bones.Add(newBone);
            }
        }

        /// <summary>
        /// Find a bone in this model by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Bone FindBone(string name)
        {
            foreach(Bone bone in Bones)
            {
                if (bone.Name == name)
                    return bone;
            }

            return null;
        }

        #endregion

        #region Animation Management

        /// <summary>
        /// Play an animation clip
        /// </summary>
        /// <param name="clip">The clip to play</param>
        /// <returns>The player that will play this clip</returns>
        public AnimationPlayer PlayClip(AnimationClip clip)
        {
            // Create a clip player and assign it to this model
            player = new AnimationPlayer(clip, this);
            return player;
        }

        #endregion

        #region Updating

        /// <summary>
        /// Update animation for the model.
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (player != null)
            {
                player.Update(gameTime);
            }

            base.Update(gameTime);
        }

        #endregion

        #region Drawing

        /// <summary>
        /// Draw the model
        /// </summary>
        /// <param name="graphics">The graphics device to draw on</param>
        /// <param name="camera">A camera to determine the view</param>
        /// <param name="world">A world matrix to place the model</param>
        public override void  Draw(GameTime gameTime)
        {
            if (model == null)
                return;

            Game1 _game = (Game1)base.Game;

            //
            // Compute all of the bone absolute transforms
            //

            Matrix[] boneTransforms = new Matrix[bones.Count];

            for (int i = 0; i < bones.Count; i++)
            {
                Bone _bone = bones[i];
                _bone.ComputeAbsoluteTransform();

                boneTransforms[i] = _bone.AbsoluteTransform;
            }

            //
            // Determine the skin transforms from the skeleton
            //

            Matrix[] skeleton = new Matrix[modelExtra.Skeleton.Count];
            for (int s = 0; s < modelExtra.Skeleton.Count; s++)
            {
                Bone bone = bones[modelExtra.Skeleton[s]];
                skeleton[s] = bone.SkinTransform * bone.AbsoluteTransform;
            }

            // Draw the model.
            foreach (ModelMesh modelMesh in model.Meshes)
            {
                foreach (Effect effect in modelMesh.Effects)
                {
                    if (effect is BasicEffect)
                    {
                        BasicEffect beffect = effect as BasicEffect;
                        beffect.World = boneTransforms[modelMesh.ParentBone.Index] * GetWorld();
                        beffect.View = _game.camera.view;
                        beffect.Projection = _game.camera.projection;
                        beffect.EnableDefaultLighting();
                        beffect.PreferPerPixelLighting = true;
                    }

                    if (effect is SkinnedEffect)
                    {
                        SkinnedEffect seffect = effect as SkinnedEffect;
                        seffect.World = boneTransforms[modelMesh.ParentBone.Index] * GetWorld();
                        seffect.View = ((Game1)base.Game).camera.view;
                        seffect.Projection = ((Game1)base.Game).camera.projection;
                        seffect.EnableDefaultLighting();
                        seffect.PreferPerPixelLighting = true;
                        seffect.SetBoneTransforms(skeleton);
                    }
                }

                modelMesh.Draw();
            }
        }

        private Matrix GetWorld()
        {
            Matrix worldRotation = Matrix.Identity * Matrix.CreateRotationZ(rotation.Z) * Matrix.CreateRotationX(rotation.X) * Matrix.CreateRotationY(rotation.Y);
            return Matrix.Identity * Matrix.CreateScale(scale) * worldRotation * Matrix.CreateTranslation(position);
        }


        #endregion

    }
}
