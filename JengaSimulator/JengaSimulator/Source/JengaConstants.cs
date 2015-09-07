using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JengaSimulator.Source;
using Microsoft.Xna.Framework;

namespace JengaSimulator.Source
{
    public class JengaConstants
    {
        /*******************************************************************
         * Gestures Tweaks
         */
        //DoubleTap Config
        public const float BOUNDS_BUFFER_SIZE = 50.0f;
        public const long MAX_TICK_DIFFERENCE = 8000000;
        public const long MIN_TICK_DIFFERENCE = 8000000;
        //Block moving away and towards speed factor. (Greater is faster)
        //In range 0(doesnt move) to infinity (probably crash)
        public const float FORWARD_BACK_BLOCK_SPEED = 4.0f;
        //This constant makes the two zoom gestures the same fineness. larger means less movement
        //Range: 0.9 - 1
        public const float FORWARD_BACK_SCALE_CONSTANT = 0.999f;     
        /*******************************************************************
         * CAMERA
         */
        public const int CAMERA_HEIGHT = 20;
        public static readonly Vector3 MAIN_CAMERA_LOOKAT = new Vector3(0, 0, 3.5f);

        public const float PAN_SPEED_DIVISOR = 2.5f;        //Used only in improvedProcessor class
        public const float HEIGHT_ANGLE_MAX = 1.7f;
        public const float HEIGHT_ANGLE_MIN = 0.01f;
        public const float ROTATE_REVERSED = -1.0f;         //1 and -1 only 
        public const float HEIGHT_REVERSED = -1.0f;         //1 and -1 only 
        /*******************************************************************
         * Blobs
         */
        // 1. A big blob with WIDTH of size between bigBlobMinWIDTH and bigBlobMaxWIDTH
        public const int BIG_BLOB_MIN_WIDTH = 39;
        public const int BIG_BLOB_MAX_WIDTH = 120;
        // 2. A small blob with WIDTH of size between smallBlobMinWIDTH and smallBlobMaxWIDTH
        public const int SMALL_BLOB_MIN_WIDTH = 10;
        public const int SMALL_BLOB_MAX_WIDTH = 37;
        // 3. The distance between the small blob and the big blob should be between minDistance and maxDistance
        public const int BLOB_MIN_DISTANCE = 0;
        public const int BLOB_MAX_DISTANCE = 100;

        /*******************************************************************
         * Tangibles
         */
        //The following weightings are relative to each other. If they all have 1 that means
        //they are given equal weighting to decide what tangible has been placed on the table.
        //Equal Weighting recommended.
        public const float BIG_BLOB_WEIGHTING = 1.0f;
        public const float SMALL_BLOB_WEIGHTING = 1.0f;
        public const float DISTANCE_WEIGHTING = 1.0f;

        public static List<Tangible> REGISTERED_TANGIBLES = new List<Tangible>()
        {   
            new Tangible("Jenga Block", 41.41071f, 19.84369f, 35.39117f),
            new Tangible("SIDE 5", 26f, 54f, 49f),
            //new Tangible("Cork Screw", 46.0526f, 23.72978f, 39.95073f),
            //new Tangible("Freeze", 53.18616f, 21.81044f, 47.44673f),
            //new Tangible("Un-Freeze", 67.18616f, 15.81044f, 35.44673f),
            new Tangible("SIDE 4", 47f, 19f, 42f)
        };
    }
    
}
