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

        public bool _isSelected;
        private bool _isTable;
        Game game;

        public bool getIsTable()
        {
            return _isTable;
        }

        public SolidThing(Game game, Model model)
            : this(game, model, true, false)
        {
        }

        public SolidThing(Game game, Model model, bool isColorRandom, bool isTable)
            : base((RigidBodyModel)model.Tag)
        {
            this.game = game;
            this._isTable = isTable;
            if (isTable)
            {
                this.Freeze();
            }
            _model = model;
            _meshTransforms = new Matrix[_model.Bones.Count];
            _model.CopyAbsoluteBoneTransformsTo(_meshTransforms);

            foreach (var mesh in _model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    //By disabling this we get even lighting
                    effect.EnableDefaultLighting();
                    effect.AmbientLightColor = Vector3.One * 0.6f; 
                    effect.SpecularColor = Vector3.One;
                    effect.PreferPerPixelLighting = true;
                }
            }
            if (_isColorRandom = isColorRandom)
            {
                _diffuseColor = new Vector3((float)_colorRand.NextDouble(),
                    (float)_colorRand.NextDouble(), (float)_colorRand.NextDouble());
                _diffuseColor *= 0.75f; //By Increasing this we get a increase in vibrance of the blocks
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
                    if(_isTable)
                        effect.DiffuseColor *= 0.5f;
                    /*if (!this.IsActive)
                    {
                        effect.DiffuseColor *= 0.5f;
                    }*/
                    if (_isSelected)
                    {
                        effect.DiffuseColor *= 4.0f;
                    }
                }
                mesh.Draw();
            }
        }

        ////For use in setting color of block sides to give affordance to user
        //public void setColour()
        //{
        //    foreach (var mesh in _model.Meshes)
        //    {
        //        foreach (BasicEffect effect in mesh.Effects)
        //        {
        //            //By disabling this we get even lighting
        //            effect.EnableDefaultLighting();
        //            effect.AmbientLightColor = Vector3.One * 0.6f;
        //            effect.SpecularColor = Vector3.One;
        //            effect.PreferPerPixelLighting = true;
        //        }
        //    }
        //}
    }
}
