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

namespace JengaSimulator
{
    class Tangibles
    {
        private Game game;
        private IViewManager viewManager;
        private PhysicsManager physics;

        private RigidBody pickedObject;
        private WorldPointConstraint pickedForce;
        private float pickedDistance;
        private int _lastSideToTouch;

        private float lastOrientation;

        private TouchPoint touchPosition, lastTouchPosition;

        public Tangibles(Game game, IViewManager viewManager, PhysicsManager physics)
        {
            this.game = game;
            this.viewManager = viewManager;
            this.physics = physics;
            this._lastSideToTouch = 0;
        }

        public void processTouchPoints(ReadOnlyTouchPointCollection touches)
        {
            lastTouchPosition = touchPosition;

            int tagID = -1;
            if (touches.Count >= 1)
            {
                for (int i = 0; i < touches.Count; i++)
                {
                    if (touches[i].IsTagRecognized)
                    {
                        tagID = (int)touches[i].Tag.Value;
                        break;
                    }
                }
                if (tagID != -1)
                {
                    touchPosition = touches[0];
                    //First time touch
                    if (lastTouchPosition == null)
                    {
                        Segment s;
                        s.P1 = game.GraphicsDevice.Viewport.Unproject(new Vector3(touchPosition.CenterX, touchPosition.CenterY, 0f),
                            viewManager.Projection, viewManager.View, Matrix.Identity);
                        s.P2 = game.GraphicsDevice.Viewport.Unproject(new Vector3(touchPosition.CenterX, touchPosition.CenterY, 1f),
                            viewManager.Projection, viewManager.View, Matrix.Identity);
                        float scalar;
                        Vector3 point;
                        var c = physics.BroadPhase.Intersect(ref s, out scalar, out point);

                        if (c != null && c is BodySkin)
                        {
                            pickedObject = ((BodySkin)c).Owner;

                            pickedForce = new WorldPointConstraint(pickedObject, point);
                            physics.Add(pickedForce);
                            pickedDistance = scalar;
                            pickedObject.IsActive = true;
                        }
                        lastOrientation = touches.Count == 1 ? touches[0].Orientation : touches[1].Orientation;
                    }
                    else if (pickedObject != null)
                    {
                        Segment s;
                        s.P1 = game.GraphicsDevice.Viewport.Unproject(new Vector3(touchPosition.CenterX, touchPosition.CenterY, 0f),
                            viewManager.Projection, viewManager.View, Matrix.Identity);
                        s.P2 = game.GraphicsDevice.Viewport.Unproject(new Vector3(touchPosition.CenterX, touchPosition.CenterY, 1f),
                            viewManager.Projection, viewManager.View, Matrix.Identity);
                        Vector3 diff, point;
                        Vector3.Subtract(ref s.P2, ref s.P1, out diff);
                        Vector3.Multiply(ref diff, pickedDistance, out diff);
                        Vector3.Add(ref s.P1, ref diff, out point);
                        pickedForce.WorldPoint = point;
                        pickedObject.IsActive = true;

                        switch (tagID)
                        {

                            //Pin a block
                            case 0:
                                pickedObject.Freeze();
                                break;
                            //unPin a block
                            case 1:
                                pickedObject.Unfreeze();
                                break;
                            //Rotate a block
                            case 2:
                                pickedForce.orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, -1.0f), touchPosition.Orientation);
                                break;
                            //Move a block towards or away from camera
                            case 3:
                                TouchPoint tagPoint = touches[0];
                                float deltaRotation = MathHelper.ToDegrees(lastOrientation) - MathHelper.ToDegrees(tagPoint.Orientation);

                                Vector3 direction = new Vector3(0, 0, 1.0f);
                                direction.Normalize();
                                pickedForce.WorldPoint = Vector3.Add(pickedForce.WorldPoint, Vector3.Multiply(direction, deltaRotation * 0.01f));

                                break;
                            //Rotate stack onto top view
                            case 4:
                                viewManager.rotateToSide(4);
                                break;
                            //Corkscrew closer or further away
                            case 5:
                                viewManager.rotateToSide(5);
                                break;
                            case 6:
                                viewManager.rotateToSide(6);
                                break;
                            case 7:
                                viewManager.rotateToSide(7);
                                break;
                            case 8:
                                viewManager.rotateToSide(8);
                                break;


                        }
                    }
                    else if (pickedObject != null)
                    {
                        physics.Remove(pickedForce);
                        pickedObject = null;
                    }
                }
            }
            else if (pickedObject != null)
            {
                physics.Remove(pickedForce);
                pickedObject = null;
                touchPosition = null;
                lastTouchPosition = null;
            }
            else
            {
                touchPosition = null;
            }

        }
    }
}
