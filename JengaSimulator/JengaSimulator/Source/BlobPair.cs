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


namespace JengaSimulator
{
    public class BlobPair
    {
        private TouchPoint bigBlob;
        private TouchPoint smallBlob;

        private float centerX;
        private float centerY;

        public BlobPair(TouchPoint bigBlob, TouchPoint smallBlob) {
            this.bigBlob = bigBlob;
            this.smallBlob = smallBlob;

            centerX = bigBlob.X > smallBlob.X ? bigBlob.X - smallBlob.X : smallBlob.X - bigBlob.X;
            centerY = bigBlob.Y > smallBlob.Y ? bigBlob.Y - smallBlob.Y : smallBlob.Y - bigBlob.Y;
        }

        

        public override string ToString()
        {
            return "Blobpair with center: " + centerX + ", " + centerY;
        }
    }
}
