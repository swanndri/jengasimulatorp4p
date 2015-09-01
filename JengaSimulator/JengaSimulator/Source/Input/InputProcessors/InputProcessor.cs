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
using JengaSimulator.Source;

namespace JengaSimulator.Source.Input.InputProcessors
{
    interface InputProcessor
    {
        void processTouchPoints(ReadOnlyTouchPointCollection touches, List<BlobPair> blobPairs);

        void TouchDown(object sender, TouchEventArgs e);
        void TouchHoldGesture(object sender, TouchEventArgs e);
        void TouchMove(object sender, TouchEventArgs e);
        void TouchTapGesture(object sender, TouchEventArgs e);
        void TouchUp(object sender, TouchEventArgs e);
    }
}
