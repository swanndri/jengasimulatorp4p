using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Henge3D.Physics;
using Henge3D.Pipeline;

namespace JengaSimulator
{
    public class SolidThing : RigidBody, IVisible
    {
        private static Random _colorRand = new Random();

        private Model _model;
        private Matrix[] _meshTransforms;
        private bool _isColorRandom;
        private Vector3 _diffuseColor;
        Game game;

        public Model Model
        {
            get
            {
                return _model;
            }
            set { this._model = value; }
        }

        public SolidThing(Game game, Model model)
            : this(game, model, true)
        {
        }

        public SolidThing(Game game, Model model, bool isColorRandom)
            : base((RigidBodyModel)model.Tag)
        {
            this.game = game;
            _model = model;
            _meshTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_meshTransforms);

            foreach (var mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //By Disabling this we can make everything evenly lit. It all just looks quite dull tho.
                    //effect.EnableDefaultLighting();
                    effect.AmbientLightColor = Vector3.One * 0.6f; 
                    effect.SpecularColor = Vector3.One;
                    effect.PreferPerPixelLighting = true;              
                }
            }
            if (_isColorRandom = isColorRandom)
            {
                _diffuseColor = new Vector3((float)_colorRand.NextDouble(),
                    (float)_colorRand.NextDouble(), (float)_colorRand.NextDouble());
                _diffuseColor *= 2f; //By setting this to a value higher value we can make the blocks brighter.
            }
        }

        public void Draw(IViewManager view)
        {
            foreach (var mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = _meshTransforms[mesh.ParentBone.Index] * Transform.Combined;
                    effect.View = view.View;                    
                    effect.Projection = view.Projection;
                    if (_isColorRandom) effect.DiffuseColor = _diffuseColor;
                    if (!this.IsActive)
                    {
                        effect.DiffuseColor *= 0.6f;
                    }
                }
                mesh.Draw();
            }
        }
    }
}
