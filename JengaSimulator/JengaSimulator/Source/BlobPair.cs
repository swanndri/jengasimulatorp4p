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
        public int BlobPairID { get; set; }
        private TouchPoint bigBlob;
        private TouchPoint smallBlob;

        private String name = "default";

        private float centerX;
        private float centerY;

        //In radians
        private float orientation;
        private Vector2 lineBetweenBlobs;
        
        private float distanceBetweenBlobCentres;
        private float widthOfLargeBlob;


        public BlobPair(TouchPoint bigBlob, TouchPoint smallBlob, Vector2 lineVector)
        {
            this.bigBlob = bigBlob;
            this.smallBlob = smallBlob;
            this.BlobPairID = bigBlob.Id;
            this.lineBetweenBlobs = lineVector;
            this.distanceBetweenBlobCentres = lineBetweenBlobs.Length();
            
            //Determine center location for the blob pair
            this.centerX = (smallBlob.CenterX + bigBlob.CenterX) / 2.0f;
            this.centerY = (smallBlob.CenterY + bigBlob.CenterY) / 2.0f;

            //Determine orientation for the blob pair. Orientation is angle from vertical position.
            Vector2 v1 = Vector2.UnitY;
            Vector2 v2 = new Vector2(lineVector.X, lineVector.Y) ;
            v2.Normalize();
            float dot = (v1.X * v2.X) + (v1.Y * v2.Y);
            float det = (v2.X * v2.Y) - (v1.Y * v2.X);
            this.orientation = (float)Math.PI + ((float)Math.Atan2(det, dot));
    
            setWidthOfLargeBlob();
            //setName();
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
                + "\n\t ID: " + this.BlobPairID
                + "\n\t Center: (X:" + centerX + ", Y:" + centerY + ")"
                +"\n\t Orientation: " + MathHelper.ToDegrees(this.orientation);
                //+ "\n\t bounds:"
                //+ "\n\t\t bigBlob: " + bigBlob.
                //+ "\n\t\t smallBlob:" + smallBlob.Bounds.Width;
                //+ "\n\t Distance between blobs: " + distanceBetweenBlobCentres;            
        }
    }
}
