using System;
using System.Collections.Generic;
using System.Text;

namespace Monocraft
{
    //A copy of Vector 2 & 3 I made so they are serializable
    [Serializable]
    public class Vector2Ser 
    {
        private float _x, _y; 

        public Vector2Ser(float x, float y)
        {
            _x = x;
            _y = y;
        }

        public float X { get => _x; set => _x = value; }
        public float Y { get => _y; set => _y = value; }
    }

    [Serializable]
    public class Vector3Ser
    {
        private float _x, _y, _z;

        public Vector3Ser(float x, float y, float z)
        {
            _x = x;
            _y = y;
            _z = z;
        }

        public float X { get => _x; set => _x = value; }
        public float Y { get => _y; set => _y = value; }
        public float Z { get => _z; set => _z = value; }
    }
}
