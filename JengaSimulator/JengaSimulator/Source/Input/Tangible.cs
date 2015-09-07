using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JengaSimulator.Source
{
    public class Tangible
    {
        private String name;
        public String Name { get { return name; } }

        private float smallBlobMajor;
        public float SmallBlobMajor { get { return smallBlobMajor; } }
        private float smallBlobMinor;
        public float SmallBlobMinor { get { return smallBlobMinor; } }

        private float bigBlobMajor;
        public float BigBlobMajor { get { return bigBlobMajor; } }
        private float bigBlobMinor;
        public float BigBlobMinor { get { return bigBlobMinor; } }

        private float distanceBetweenBlobs;
        public float DistanceBetweenBlobs { get { return distanceBetweenBlobs; } }

        public Tangible(String name, float bigBlobMajor, float bigBlobMinor, float smallBlobMajor, float smallBlobMinor, float distanceBetweenBlobs)
        {
            this.name = name;
            this.smallBlobMajor = smallBlobMajor;
            this.smallBlobMinor = smallBlobMinor;
            this.bigBlobMinor = bigBlobMinor;
            this.bigBlobMajor = bigBlobMajor;
            this.distanceBetweenBlobs = distanceBetweenBlobs;
        }
    }
}
