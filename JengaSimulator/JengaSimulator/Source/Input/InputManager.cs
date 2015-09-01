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
	public class InputManager
	{
        private Game game;
        private IViewManager viewManager;
        private PhysicsManager physics;

        List<BlobPair> blobPairs = new List<BlobPair>();

        public InputManager(Game game, IViewManager viewManager, PhysicsManager physics)
		{
            this.game = game;
            this.viewManager = viewManager;
            this.physics = physics;
		}

        public void processTouchPoints(ReadOnlyTouchPointCollection touches)
        {
            blobPairs = getBlobPairListFromTouchCollection(touches);

            foreach (BlobPair b in blobPairs)
            {
                //printAverageBlobStats(b);
                //Console.WriteLine(b);
            }         
        }

        #region TouchEvents
        public void TouchDown(object sender, TouchEventArgs e){
            //Console.WriteLine("Touch Down");
            //Console.WriteLine(e.ToString());
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e) {
            //Console.WriteLine("Touch Hold");
            //Console.WriteLine(e.ToString());
        }
        public void TouchMove(object sender, TouchEventArgs e) {
            //Console.WriteLine("Touch Move");
            //Console.WriteLine(e.ToString());
        }
        public void TouchTapGesture(object sender, TouchEventArgs e) {
            //Console.WriteLine("Touch Tap");
            //Console.WriteLine(e.ToString());
        }
        public void TouchUp(object sender, TouchEventArgs e) {
            //Console.WriteLine("Touch Up");
            //Console.WriteLine(e.ToString());
        }
        #endregion

        #region Helper Methods

        public List<BlobPair> getBlobPairListFromTouchCollection(ReadOnlyTouchPointCollection touches)
        {
            List<TouchPoint> bigBlobList = new List<TouchPoint>();
            List<TouchPoint> smallBlobList = new List<TouchPoint>();
            List<BlobPair> blobPairList = new List<BlobPair>();

            //---------------------------------------------------------
            //Add blobs to two lists: big blob list and small blob list            
            for (int i = 0; i < touches.Count; i++)
            {
                TouchPoint touch = touches[i];
                if (isBlob(touch))
                {
                    if (touch.MajorAxis > JengaConstants.BIG_BLOB_MIN_WIDTH && touch.MajorAxis < JengaConstants.BIG_BLOB_MAX_WIDTH)
                        bigBlobList.Add(touch);
                    else if (touch.MajorAxis > JengaConstants.SMALL_BLOB_MIN_WIDTH && touch.MajorAxis < JengaConstants.SMALL_BLOB_MAX_WIDTH)
                        smallBlobList.Add(touch);
                }
            }

            //---------------------------------------------------------
            //Create blob pairs from bloblists.
            foreach (TouchPoint bigBlob in bigBlobList)
            {
                foreach (TouchPoint smallBlob in smallBlobList)
                {
                    Vector2 lineVector = new Vector2(bigBlob.CenterX - smallBlob.CenterX, bigBlob.CenterY - smallBlob.CenterY);

                    if (lineVector.Length() > JengaConstants.BLOB_MIN_DISTANCE && lineVector.Length() < JengaConstants.BLOB_MAX_DISTANCE)
                    {
                        blobPairList.Add(new BlobPair(bigBlob, smallBlob, lineVector));
                        continue;
                    }
                }
            }
            return blobPairList;
        }
        private Boolean isBlob(TouchPoint t)
        {
            return (!(t.IsFingerRecognized || t.IsTagRecognized));
        }

        float averageBigBlobSize = 0;
        float averageSmallBlobSize = 0;
        float averageDistance = 0;
        int count = 0;
        private void printAverageBlobStats(BlobPair b)
        {
            averageBigBlobSize += b.BigBlob.MajorAxis;
            averageSmallBlobSize += b.SmallBlob.MajorAxis;
            averageDistance += b.DistanceBetweenBlobCentres;
            count++;
            String output =     "BIG BLOB: " + (averageBigBlobSize /count) 
                            +   "SMALL BLOB: " + (averageSmallBlobSize /count)
                            + "DISTNACE: " + (averageDistance / count);
            Console.WriteLine(output);   
        }

        #endregion
    }

}
