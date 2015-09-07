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

        private float smallBlobWidth;
        public float SmallBlobWidth { get { return smallBlobWidth; } }

        private float bigBlobWidth;
        public float BigBlobWidth { get { return bigBlobWidth; } }

        private float distanceBetweenBlobs;
        public float DistanceBetweenBlobs { get { return distanceBetweenBlobs; } }

        public Tangible(String name, float bigBlobWidth, float smallBlobWidth, float distanceBetweenBlobs)
        {
            this.name = name;
            this.smallBlobWidth = smallBlobWidth;
            this.bigBlobWidth = bigBlobWidth;
            this.distanceBetweenBlobs = distanceBetweenBlobs;
        }
    }
}
