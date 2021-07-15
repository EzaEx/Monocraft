using System;
using System.Collections.Generic;
using System.Text;

namespace Monocraft
{
    //A class that holds world data so it is serializable
    [Serializable]
    public class WorldSave
    { 
        //essential world data
        private Dictionary<Vector2Ser, byte[,,,]> _knownChunks;
        private int seed;
        private Vector3Ser dimensions;

        public Dictionary<Vector2Ser, byte[,,,]> KnownChunks { get => _knownChunks; set => _knownChunks = value; }
        public int Seed { get => seed; set => seed = value; }
        public Vector3Ser Dimensions { get => dimensions; set => dimensions = value; }
    }
}
