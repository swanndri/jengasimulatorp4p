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

        private Boolean firstTime = false;
        
        
        private float distanceBetweenBlobCentres;

        
        
        private float angleOfTangible;


        public BlobPair(TouchPoint bigBlob, TouchPoint smallBlob, Vector2 lineVector)
        {
            this.bigBlob = bigBlob;
            this.smallBlob = smallBlob;
            this.distanceBetweenBlobCentres = lineVector.Length();


            

            //setName();
            
            setCentreX();
            setCentreY();

            



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


        //private void setTangibleAngle()
        //{
        //    float deltaX = Math.Abs(bigBlob.CenterX - smallBlob.CenterX);
        //    Console.Out.WriteLine(deltaX);
        //    float deltaY = Math.Abs(bigBlob.CenterY - smallBlob.CenterY);
        //    Console.Out.WriteLine(deltaY);

        //    distanceBetweenBlobCentres = (float)Math.Sqrt(Math.Pow(deltaX, 2) + Math.Pow(deltaY, 2));

        //}
        //private void setName()
        //{
        //    if(
        //}


        public override string ToString()
        {
            return "Blobpair with"
                + "\n\t name: " + name
                + "\n\t center: (X:" + centerX + ", Y:" + centerY + ")"
                + "\n\t bounds:"
                + "\n\t\t bigBlob: " + bigBlob.Bounds
                + "\n\t\t smallBlob:" + smallBlob.Bounds
                + "\n\t Distance between blobs: " + distanceBetweenBlobCentres;
            
        }
    }
}
