using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Math;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;

namespace JengaSimulator
{
    public class Block : DrawableGameComponent
    {

        private Vector3 position, scale, sideLengths;
        private Model model;

        public Body _body { get; private set; }
        public CollisionSkin _skin { get; private set; }

        private bool isTable;

        public Block(Game game, Vector3 sideLengths, Matrix orientation, Vector3 position, bool isTable) : base(game)
        {
            this.isTable = isTable;
            this.position = position;

            this._body = new Body();
            this._skin = new CollisionSkin(_body);

            Box box = new Box(-0.5f * sideLengths, orientation, sideLengths);
            this._skin.AddPrimitive(box, new MaterialProperties(0.8f, 0.8f, 0.7f));
            this._body.CollisionSkin = _skin;
          
            Vector3 com = SetMass(1.0f);
            this._body.MoveTo(position, Matrix.Identity);
            this._skin.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            this._body.EnableBody();
            this.scale = sideLengths;
        }

        private Vector3 SetMass(float mass){

            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                    PrimitiveProperties.MassDistributionEnum.Solid,
                    PrimitiveProperties.MassTypeEnum.Mass, mass);

            float junk;
            Vector3 com;
            Matrix it, itCoM;

            this._skin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);
            this._body.BodyInertia = itCoM;
            this._body.Mass = junk;

            return com;
        }

        protected override void LoadContent()
        {
            this.model = Game.Content.Load<Model>("models/box");
        }

        Matrix[] boneTransforms = null;
        int boneCount = 0;

        public override void Draw(GameTime gameTime)
        {
            App1 game = (App1)Game;

            if (boneTransforms == null || boneCount != model.Bones.Count)
            {
                boneTransforms = new Matrix[model.Bones.Count];
                boneCount = model.Bones.Count;
            }

            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    // the body has an orientation but also the primitives in the collision skin
                    // owned by the body can be rotated!
                    if (this._body.CollisionSkin != null)
                        effect.World = boneTransforms[mesh.ParentBone.Index] * Matrix.CreateScale(scale) * this._body.CollisionSkin.GetPrimitiveLocal(0).Transform.Orientation * this._body.Orientation * Matrix.CreateTranslation(this._body.Position);
                    else
                        effect.World = boneTransforms[mesh.ParentBone.Index] * Matrix.CreateScale(scale) * this._body.Orientation * Matrix.CreateTranslation(this._body.Position);

                    effect.View = game.View;
                    effect.Projection = game.Projection;

                    if (this.isTable == false)
                    {
                        effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.2f);
                    }
                    else
                    {
                        effect.DiffuseColor = new Vector3(0.0f, 0.2f, 0.2f);
                    }

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;                  
                    
                }
                mesh.Draw();
            }   
        }                    

    }
}


 

