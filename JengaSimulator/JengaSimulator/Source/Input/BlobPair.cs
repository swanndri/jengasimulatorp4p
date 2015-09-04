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

namespace JengaSimulator
{
    public class BlobPair
    {
        public Tangible thisBlobPairTangible;

        public int BlobPairID { get; set; }

        private TouchPoint bigBlob, smallBlob;
        public TouchPoint BigBlob { get { return bigBlob; } }
        public TouchPoint SmallBlob { get { return smallBlob; } }

        private float centerX, centerY;
        private float distanceBetweenBlobCentres;
        public float DistanceBetweenBlobCentres { get { return distanceBetweenBlobCentres; } }
        private float orientation;                      //NOTE: In radians

        private Vector2 lineBetweenBlobs;

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

            determineTangible();
        }

        public override string ToString()
        {
            return "Blobpair with"
                + "\n\t ID: " + this.BlobPairID
                + "\n\t Center: (X:" + centerX + ", Y:" + centerY + ")"
                +"\n\t Orientation: " + MathHelper.ToDegrees(this.orientation)
                +"\n\t\t BigBlobWidth: " + bigBlob.MajorAxis
                + "\n\t\t SmallBlobWidth:" + smallBlob.MajorAxis
                + "\n\t Distance between blobs: " + distanceBetweenBlobCentres;            
        }

        //Helper Methods

        private void determineTangible() {     
            List<Tuple<Tangible, float>> probabilities = new List<Tuple<Tangible, float>>();            

            for (int i = 0; i < JengaConstants.REGISTERED_TANGIBLES.Count; i++)
            {
                float bigBlobCloseness = (1.0f - Math.Abs(((this.BigBlob.MajorAxis - JengaConstants.REGISTERED_TANGIBLES[i].BigBlobWidth) / this.BigBlob.MajorAxis)));
                float smallBlobCloseness = (1.0f - Math.Abs(((this.SmallBlob.MajorAxis - JengaConstants.REGISTERED_TANGIBLES[i].SmallBlobWidth) / this.SmallBlob.MajorAxis)));
                float distanceCloseness = (1.0f - Math.Abs(((this.distanceBetweenBlobCentres - JengaConstants.REGISTERED_TANGIBLES[i].DistanceBetweenBlobs) / this.distanceBetweenBlobCentres)));

                float totalWeighting = JengaConstants.SMALL_BLOB_WEIGHTING + JengaConstants.BIG_BLOB_WEIGHTING + JengaConstants.DISTANCE_WEIGHTING;

                float probability =
                    +(JengaConstants.BIG_BLOB_WEIGHTING / totalWeighting) * bigBlobCloseness
                    + (JengaConstants.SMALL_BLOB_WEIGHTING / totalWeighting) * smallBlobCloseness
                    + (JengaConstants.DISTANCE_WEIGHTING / totalWeighting) * distanceCloseness;                
                probabilities.Add(new Tuple<Tangible, float>(JengaConstants.REGISTERED_TANGIBLES[i], probability));               
            }
            probabilities = probabilities.OrderByDescending(x => x.Item2).ToList();
            
            
            Console.WriteLine("---------------BEGIN---------------------");
            foreach (Tuple<Tangible, float> t in probabilities){
                Console.WriteLine(t.Item1.Name + " : " + t.Item2 + "%"); 
            }
            Console.WriteLine("---------------Final---------------------");
            Console.WriteLine("Tangible is: " + probabilities[0].Item1.Name + " with " + (probabilities[0].Item2 * 100) +
                " percent certainty.");

            Console.WriteLine("---------------END-----------------------");
            

            
            thisBlobPairTangible = probabilities[0].Item1;
        } 
    }
}
