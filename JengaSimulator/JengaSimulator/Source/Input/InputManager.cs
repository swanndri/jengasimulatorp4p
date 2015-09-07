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
using JengaSimulator.Source.Input.InputProcessors;

namespace JengaSimulator
{
	public class InputManager
	{
        private Game _game;
        private IViewManager _viewManager;
        private PhysicsManager _physics;
        private InputProcessor _inputProcessor;

        List<BlobPair> blobPairs = new List<BlobPair>();

        public InputManager(Game game, IViewManager viewManager, PhysicsManager physics)
		{
            this._game = game;
            this._viewManager = viewManager;
            this._physics = physics;
            //_inputProcessor = new DefaultProcessor(game, viewManager, physics);
            _inputProcessor = new ImprovedProcessor(game, viewManager, physics);
		}

        public void processTouchPoints(ReadOnlyTouchPointCollection touches, GameTime gameTime)
        {
            blobPairs = getBlobPairListFromTouchCollection(touches);            
            _inputProcessor.processTouchPoints(touches, blobPairs, gameTime);

            /*
            foreach (BlobPair bp in blobPairs)
            {
                Console.WriteLine("LARGE BLOB: " + bp.BigBlob.MajorAxis + " : " + bp.BigBlob.MinorAxis + 
                    " SMALL BLOB: " + bp.SmallBlob.MajorAxis + " : " + bp.SmallBlob.MinorAxis
                    + " DISTANCE: " + bp.DistanceBetweenBlobCentres);
            }
            */
        }

        #region TouchEvents
        public void TouchDown(object sender, TouchEventArgs e){
            _inputProcessor.TouchDown(sender, e);
        }
        public void TouchHoldGesture(object sender, TouchEventArgs e) {
            _inputProcessor.TouchHoldGesture(sender, e);
        }
        public void TouchMove(object sender, TouchEventArgs e) {
            _inputProcessor.TouchMove(sender, e);
        }
        public void TouchTapGesture(object sender, TouchEventArgs e) {
            _inputProcessor.TouchTapGesture(sender, e);
        }
        public void TouchUp(object sender, TouchEventArgs e) {
            _inputProcessor.TouchUp(sender, e);
        }
        #endregion

        #region Helper Methods

        public List<BlobPair> getBlobPairListFromTouchCollection(ReadOnlyTouchPointCollection touches)
        {
            List<TouchPoint> blobTouchPointList = new List<TouchPoint>();

            List<TouchPoint> bigBlobList = new List<TouchPoint>();
            List<TouchPoint> smallBlobList = new List<TouchPoint>();
            List<BlobPair> blobPairList = new List<BlobPair>();
            
            //Firstly get every touch point that is a blob and put it in a list.
            foreach (TouchPoint t in touches)
            {
                if (isBlob(t))
                {
                    blobTouchPointList.Add(t);
                }
            }
            //Order the touchpoints from largest major axis to smallest.
            blobTouchPointList = blobTouchPointList.OrderByDescending(x => x.MajorAxis).ToList();

            for (int i = 0; i < blobTouchPointList.Count; i++)
            {
                if (i >= blobTouchPointList.Count / 2){
                    smallBlobList.Add(blobTouchPointList.ElementAt(i));
                }else{
                    bigBlobList.Add(blobTouchPointList.ElementAt(i));
                }
            }

            if (smallBlobList.Count != bigBlobList.Count)
            {
                return blobPairList;
            }

            List<Tuple<float, TouchPoint, TouchPoint>> vectorDistances = new List<Tuple<float, TouchPoint, TouchPoint>>();

            for (int i = 0; i < bigBlobList.Count; i++) {
                for (int j = 0; j < smallBlobList.Count; j++) { 
                    TouchPoint one = bigBlobList.ElementAt(i);
                    TouchPoint two = smallBlobList.ElementAt(j);
                    Vector2 lineBetweenBlobs = new Vector2(one.X - two.X, one.Y - two.Y);

                    vectorDistances.Add(new Tuple<float,TouchPoint,TouchPoint>(lineBetweenBlobs.Length(), one, two));
                }
            }
            vectorDistances = vectorDistances.OrderBy(x => x.Item1).ToList();

            for (int i = 0; i < bigBlobList.Count; i++ )
            {
                Vector2 lineVector = new Vector2(vectorDistances[i].Item2.CenterX - vectorDistances[i].Item3.CenterX,
                    vectorDistances[i].Item2.CenterY - vectorDistances[i].Item3.CenterY);
                
                blobPairList.Add(new BlobPair(vectorDistances[i].Item2, vectorDistances[i].Item3, lineVector));
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
