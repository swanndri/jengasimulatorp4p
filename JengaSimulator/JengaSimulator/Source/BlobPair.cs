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

            getCentreX(bigBlob, smallBlob);
            getCentreY(bigBlob, smallBlob);

        }

        private void getCentreX(TouchPoint bigBlob, TouchPoint smallBlob)
        {

            if (bigBlob.CenterX > smallBlob.CenterX)
            {
                centerX = bigBlob.CenterX - (bigBlob.CenterX - smallBlob.CenterX);
            }
            else
            {
                centerX = smallBlob.CenterX - (smallBlob.CenterX - bigBlob.CenterX);
            }
        }

        private void getCentreY(TouchPoint bigBlob, TouchPoint smallBlob)
        {

            if (bigBlob.CenterY > smallBlob.CenterY)
            {
                centerY = bigBlob.CenterY - (bigBlob.CenterY - smallBlob.CenterY);
            }
            else
            {
                centerY = smallBlob.CenterY - (smallBlob.CenterY - bigBlob.CenterY);
            }
        }
        

        public override string ToString()
        {
            //test
            return "Blobpair with center: " + centerX + ", " + centerY;
        }
    }
}
