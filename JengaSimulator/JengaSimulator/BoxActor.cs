﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Math;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;

namespace JengaSimulator
{
    public class BoxActor : DrawableGameComponent
    {

        private Vector3 position, scale;
        private Model model;

        public Body _body { get; private set; }
        public CollisionSkin _skin { get; private set; }

        public BoxActor(Game game, Vector3 position, Vector3 scale) : base(game)
        {
            this.position = position;
            this.scale = scale;

            _body = new Body();
            _skin = new CollisionSkin(_body);
            _body.CollisionSkin = _skin;

            Box box = new Box(Vector3.Zero, Matrix.Identity, scale);
            _skin.AddPrimitive(box, new MaterialProperties(0.8f, 0.8f, 0.7f));

            Vector3 com = SetMass(1.0f);

            _body.MoveTo(position, Matrix.Identity);
            _skin.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            _body.EnableBody();
        }

        private Vector3 SetMass(float mass){

            PrimitiveProperties primitiveProperties = new PrimitiveProperties(
                    PrimitiveProperties.MassDistributionEnum.Solid,
                    PrimitiveProperties.MassTypeEnum.Mass, mass);

            float junk;
            Vector3 com;
            Matrix it;
            Matrix itCoM;

            _skin.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);
            _body.BodyInertia = itCoM;
            _body.Mass = junk;

            return com;
        }

        protected override void LoadContent()
        {
            this.model = Game.Content.Load<Model>("Models\\Crate");
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


 
