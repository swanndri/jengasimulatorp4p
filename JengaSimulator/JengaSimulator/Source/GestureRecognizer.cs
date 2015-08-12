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
    class GestureRecognizer
    {
        private Game game;
        private IViewManager viewManager;
        private PhysicsManager physics;

        private RigidBody pickedObject;
        private WorldPointConstraint pickedForce;
        private float pickedDistance;

        private TouchPoint touchPosition, lastTouchPosition;


        Manipulator2D point1;
        Manipulator2D point2;
        Manipulator2D[] manipulators;

        private ManipulationProcessor2D manipulationProcessor;

        public GestureRecognizer(Game game, IViewManager viewManager, PhysicsManager physics) {
            this.game = game;
            this.viewManager = viewManager;
            this.physics = physics;

            Manipulations2D enabledManipulations =
                Manipulations2D.Translate | Manipulations2D.Rotate;
            manipulationProcessor = new ManipulationProcessor2D(enabledManipulations);

            manipulationProcessor.Pivot = new ManipulationPivot2D();
            manipulationProcessor.Pivot.Radius = 50;

            manipulationProcessor.MinimumScaleRotateRadius = 10.0f;

            manipulationProcessor.Started += OnManipulationStarted;
            manipulationProcessor.Delta += OnManipulationDelta;
            manipulationProcessor.Completed += OnManipulationCompleted;
        }

        /** Manipulation Events ********/

        #region ManipulationEvents
        private void OnManipulationStarted(object sender, Manipulation2DStartedEventArgs e)
        {
        }

        private void OnManipulationDelta(object sender, Manipulation2DDeltaEventArgs e)
        {
            float toRotate = MathHelper.ToDegrees(e.Delta.Rotation);

            if (pickedObject != null)
            {
                Quaternion q = pickedForce.orientation;

                double yaw = Math.Atan2(2.0 * (q.Y * q.Z + q.W * q.X), q.W * q.W - q.X * q.X - q.Y * q.Y + q.Z * q.Z);
                double pitch = Math.Asin(-2.0 * (q.X * q.Z - q.W * q.Y));
                double roll = Math.Atan2(2.0 * (q.X * q.Y + q.W * q.Z), q.W * q.W + q.X * q.X - q.Y * q.Y - q.Z * q.Z);

                Console.Out.WriteLine(toRotate);

                float currentRotation = MathHelper.ToDegrees((float)roll);
                float finalRotation = currentRotation - toRotate;
                if (finalRotation < 0)
                {
                    finalRotation = 360 + finalRotation;
                }
                if (finalRotation > 360)
                {
                    finalRotation = 360 - finalRotation;
                }

                float totalRotation = MathHelper.ToRadians(finalRotation);
                pickedForce.orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1.0f), totalRotation);

                
            }
        }

        private void OnManipulationCompleted(object sender, Manipulation2DCompletedEventArgs e)
        {
        }
        #endregion

        private long Timestamp
        {
            get
            {
                // Get timestamp in 100-nanosecond units. 
                double nanosecondsPerTick = 1000000000.0 / System.Diagnostics.Stopwatch.Frequency;
                return (long)(System.Diagnostics.Stopwatch.GetTimestamp() / nanosecondsPerTick / 100.0);
            }
        }

        public void processTouchPoints(ReadOnlyTouchPointCollection touches) {
            lastTouchPosition = touchPosition;
            
            int tagID = -1;
            if (touches.Count >= 1)
            {
                for (int i = 0; i < touches.Count; i++ )
                {
                    if (touches[i].IsTagRecognized)
                    {
                        tagID = touches[i].Id;
                        break;
                    }
                }
                if (touches.Count == 2)
                {
                    if (point1 == null)
                    {
                        point1 = new Manipulator2D(0, touches[0].X, touches[0].Y);
                        point2 = new Manipulator2D(1, touches[1].X, touches[1].Y);
                    }
                    point1.X = touches[0].X;
                    point1.Y = touches[0].Y;

                    point2.X = touches[1].X;
                    point2.Y = touches[1].Y;

                    //manipulators = new Manipulator2D[] {point1,point2};
                    manipulators = new Manipulator2D[] {point2 };

                    manipulationProcessor.Pivot.X = touches[0].X;
                    manipulationProcessor.Pivot.Y = touches[0].Y;
                    manipulationProcessor.ProcessManipulators(Timestamp, manipulators);
                }

                
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
                    
                    if (tagID != -1)
                    {
                        pickedForce.orientation = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1.0f), touches.GetTouchPointFromId(tagID).Orientation);
                        
                    }
                }
                else if (pickedObject != null)
                {
                    physics.Remove(pickedForce);
                    pickedObject = null;
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
