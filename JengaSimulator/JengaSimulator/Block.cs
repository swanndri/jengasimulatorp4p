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

        private Vector3 position, scale;
        private Model model;

        public Body _body { get; private set; }
        public CollisionSkin _skin { get; private set; }
        
        private bool isTable;

        public Block(Game game, Vector3 position, Matrix orientation, Vector3 scale, bool isTable) : base(game)
        {
            this.isTable = isTable;
            this.position = position;
            this.scale = scale;

            this._body = new Body();
            this._skin = new CollisionSkin(_body);

            this._body.CollisionSkin = _skin;

            //Vector3 blckScale = new Vector3(0.5f * scale.X, 1.0f * scale.Y, 3.0f * scale.Z);

            //Position, Orientation, Scale
            Box box = new Box(Vector3.Zero, orientation, scale);
            this._skin.AddPrimitive(box, new MaterialProperties(0.8f, 0.8f, 0.7f));

            Vector3 com = SetMass(1.0f);

            this._body.MoveTo(position, Matrix.Identity);

            this._skin.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            this._body.EnableBody();
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
            if (isTable)
            {
                this.model = Game.Content.Load<Model>("models/table");
            }
            else
            {
                this.model = Game.Content.Load<Model>("models/block");
            }
            
         
        }

        private Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(scale) * _skin.GetPrimitiveLocal(0).Transform.Orientation * _body.Orientation * Matrix.CreateTranslation(_body.Position);
        }
        public override void Draw(GameTime gameTime)
        {
            App1 game = (App1)Game;

            Matrix[] transforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix worldMatrix = GetWorldMatrix();

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                    
                    effect.World = transforms[mesh.ParentBone.Index] * worldMatrix;
                    effect.View = game.View;
                    effect.Projection = game.Projection;
                }
                mesh.Draw();
            }   
        }                    

    }
}


 

