using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JengaSimulator.Source;

namespace JengaSimulator.Source
{
    public class JengaConstants
    {
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
            new Tangible("Jenga Block", 46.63271f, 13.65318f, 30.3775f),
            new Tangible("Jenga Block2", 41.54224f, 15.97482f, 41.32901f),
            new Tangible("Black Parallelogram", 46.0526f, 23.72978f, 39.95073f),
            new Tangible("Blue Triangle", 53.18616f, 21.81044f, 47.44673f)

        };
    }
    
}
