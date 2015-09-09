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
        public const float BOUNDS_BUFFER_SIZE = 50.0f;              //How close the double tap can be to each other
        public const long MAX_TICK_DIFFERENCE = 6000000;            //How long between tap gestures counts as a double tap

        //Block moving away and towards speed factor. (Greater is faster)
        //In range 0(doesnt move) to infinity (probably crash)
        public const float FORWARD_BACK_BLOCK_SPEED = 5.0f;
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
        public const float PROBABILITY_THRESHOLD = 0.75f;

        public static List<Tangible> REGISTERED_TANGIBLES = new List<Tangible>()
        {   
            new Tangible("Jenga Block", 42f,26f,21f,12f,37f),
            new Tangible("Cork Screw", 24f,16f,9.5f,8.5f, 27f),
            new Tangible("Fine Camera", 54f,46f,11f,8.5f, 55f)
        };


        public const long TIME_BETWEEN_FAKE_TOUCH_AND_TANGIBLE = 70;
        public const long TIME_BETWEEN_BLOCK_TANGIBLE_AND_SELECT = 10;
        public const int HIT_BOX_SIZE = 450;

        public const float TIME_FOR_BLOCK_TO_CENTER = 400.0f;

        //STACK TANGIBLE
        public const long STACK_SIDE_0 = 04;
        public const long STACK_SIDE_1 = 02;
        public const long STACK_SIDE_2 = 01;
        public const long STACK_SIDE_3 = 03;
        public const long STACK_SIDE_4 = 05;

        //Distance tangible can be away from digital block
        public const float MAX_TANGIBLE_DISTANCE = 5f;

        public const float TANGIBLE_ZOOM_SCALE_FACTOR = 0.02f;
        
    }
    
}