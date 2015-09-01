using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Henge3D;
using Henge3D.Physics;
using System.Windows.Input.Manipulations;
using JengaSimulator.Source.Input.InputProcessors;

namespace JengaSimulator.Source.Input.InputProcessors
{
    class ImprovedProcessor : InputProcessor
    {
        private Game _game;
        private IViewManager _viewManager;
        private PhysicsManager _physics;

        public ImprovedProcessor(Game game, IViewManager viewManager, PhysicsManager physics )
        {
            this._game = game;
            this._viewManager = viewManager;
            this._physics = physics;
        }

        public void processTouchPoints(ReadOnlyTouchPointCollection touches, List<BlobPair> blobPairs)
        {
        }

        #region TouchEvents
        public void TouchDown(object sender, TouchEventArgs e)
        {
            //Console.WriteLine("Touch Down");
            //Console.WriteLine(e.ToString());
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e)
        {
            //Console.WriteLine("Touch Hold");
            //Console.WriteLine(e.ToString());
        }
        public void TouchMove(object sender, TouchEventArgs e)
        {
            //Console.WriteLine("Touch Move");
            //Console.WriteLine(e.ToString());
        }
        public void TouchTapGesture(object sender, TouchEventArgs e)
        {
            //Console.WriteLine("Touch Tap");
            //Console.WriteLine(e.ToString());
        }
        public void TouchUp(object sender, TouchEventArgs e)
        {
            //Console.WriteLine("Touch Up");
            //Console.WriteLine(e.ToString());
        }
        #endregion
    }
}
