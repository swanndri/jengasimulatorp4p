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

        private String name = "default";

        private float centerX;
        private float centerY;

        private Vector2 lineBetweenBlobs;
        
        
        private float distanceBetweenBlobCentres;
        private float widthOfLargeBlob;


        public BlobPair(TouchPoint bigBlob, TouchPoint smallBlob, Vector2 lineVector)
        {
            this.bigBlob = bigBlob;
            this.smallBlob = smallBlob;
            this.lineBetweenBlobs = lineVector;
            this.distanceBetweenBlobCentres = lineBetweenBlobs.Length();
            this.centerX = (smallBlob.CenterX + bigBlob.CenterX) / 2.0f;
            this.centerY = (smallBlob.CenterY + bigBlob.CenterY) / 2.0f;

            setWidthOfLargeBlob();
            //setName();
            
            //setCentreX();
            //setCentreY();
        }

        private void setWidthOfLargeBlob(){
            //Get vector that is 90 degrees to the line between blobs
            Vector2 perpVector = new Vector2(lineBetweenBlobs.Y,-1 * lineBetweenBlobs.X);

            //Vector2 normPerpVector = perpVector.Normalize();
           // Console.Out.WriteLine(perpVector);
   
        }

        public override string ToString()
        {
            return "Blobpair with"
                //+ "\n\t name: " + name
                + "\n\t center: (X:" + centerX + ", Y:" + centerY + ")";
                //+ "\n\t bounds:"
                //+ "\n\t\t bigBlob: " + bigBlob.
                //+ "\n\t\t smallBlob:" + smallBlob.Bounds.Width;
                //+ "\n\t Distance between blobs: " + distanceBetweenBlobCentres;
            
        }
    }
}
