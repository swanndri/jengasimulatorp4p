﻿using System;
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

    class GestureRecognizer2
    {
        private Game game;
        private IViewManager viewManager;
        private PhysicsManager physics;

        private RigidBody pickedObject;
        private Vector3 pickedObjectOffset;
        private float pickedDistance;

        private TouchPoint touchPosition, lastTouchPosition;
        private ManipulationProcessor2D manipulationProcessor;

        private float lastOrientation;
        private Vector3 beginPos;
        private Quaternion orientation;

        private const int _bigBlobMinRadius = 21;
        private const int _bigBlobMaxRadius = 60;

        // 2. A small blob with radius of size between smallBlobMinRadius and smallBlobMaxRadius
        private const int _smallBlobMinRadius = 8;
        public const int _smallBlobMaxRadius = 20;

        // 3. The distance between the small blob and the big blob should be between minDistance and maxDistance
        private const int _minDistance = 0;
        private const int _maxDistance = 100;




        public GestureRecognizer2(Game game, IViewManager viewManager, PhysicsManager physics)
        {
            this.game = game;
            this.viewManager = viewManager;
            this.physics = physics;
        }

        public void processTouchPoints(ReadOnlyTouchPointCollection touches)
        {
            List<BlobPair> blobPairs = getBlobPairs(touches);

            foreach (BlobPair bp in blobPairs)
            {
                Console.WriteLine(bp.ToString());
            }
        }

        private List<BlobPair> getBlobPairs(ReadOnlyTouchPointCollection touches)
        {
            List<TouchPoint> bigBlobList = new List<TouchPoint>();
            List<TouchPoint> smallBlobList = new List<TouchPoint>();
            List<BlobPair> blobPairList = new List<BlobPair>();

            //Create blob lists
            for (int i = 0; i < touches.Count; i++)
            {
                //Console.WriteLine(touches.Count);
                TouchPoint touch = touches[i];
                //Console.WriteLine(touch.ToString());
                if (isBlob(touch))
                {
                    //Console.WriteLine("Yep is a blob");
                    if (touch.MajorAxis > _bigBlobMinRadius * 2 && touch.MajorAxis < _bigBlobMaxRadius * 2)
                    {
                        bigBlobList = addBlobToList(touch, bigBlobList);

                        //Console.WriteLine("Yep is a bigblob");
                    }
                    else if (touch.MajorAxis > _smallBlobMinRadius * 2 && touch.MajorAxis < _smallBlobMaxRadius * 2)
                    {
                        smallBlobList = addBlobToList(touch, smallBlobList);
                        //Console.WriteLine("smallblob");
                    }
                }
            }

            //Create blob pairs from bloblists
            foreach (TouchPoint bigBlob in bigBlobList)
            {
                foreach (TouchPoint smallBlob in smallBlobList)
                {
                    Vector2 lineVector = new Vector2(bigBlob.CenterX - smallBlob.CenterX, bigBlob.CenterY - smallBlob.CenterY);

                    //Console.WriteLine(lineVector.Length());
                    if (lineVector.Length() > _minDistance && lineVector.Length() < _maxDistance)
                    {
                        blobPairList.Add(new BlobPair(bigBlob, smallBlob));
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

        private List<TouchPoint> addBlobToList(TouchPoint touch, List<TouchPoint> blobList)
        {
            Boolean alreadyInList = false;

            foreach (TouchPoint touchPoint in blobList)
            {
                if (touchPoint.Id == touch.Id)
                {
                    alreadyInList = true;
                }
            }

            if (!alreadyInList)
            {
                blobList.Add(touch);
            }

            return blobList;
        }
    }

    
}
