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


            setWithOfLargeBlob();
            //setName();
            
            setCentreX();
            setCentreY();

            



        }

        private void setWithOfLargeBlob(){
            //Get vector that is 90 degrees to the line between blobs
            Vector2 perpVector = new Vector2(lineBetweenBlobs.Y,-1 * lineBetweenBlobs.X);

            //Vector2 normPerpVector = perpVector.Normalize();
           // Console.Out.WriteLine(perpVector);
   
        }


        private void setCentreX()
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

        private void setCentreY()
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
            return "Blobpair with"
                //+ "\n\t name: " + name
                //+ "\n\t center: (X:" + centerX + ", Y:" + centerY + ")"
                //+ "\n\t bounds:"
                + "\n\t\t bigBlob: " + bigBlob.Bounds.Width;
                //+ "\n\t\t smallBlob:" + smallBlob.Bounds
                //+ "\n\t Distance between blobs: " + distanceBetweenBlobCentres;
            
        }
    }
}
