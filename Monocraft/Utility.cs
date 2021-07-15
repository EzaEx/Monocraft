using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;


namespace Monocraft
{ 
    //general useful methods class
    static class Utility
    { 
        //get coords of face textures from texturesheet based on variation, ID and face orientation
        public static Vector2[] GetTexCoord(byte blockID, Face face, byte variationID)
        { 
            //values on texture sheet
            byte blockTotal = 20;
            byte variationTotal = 15; 
            //array of coords for two tris (to make a square)
            Vector2[] coord = new Vector2[6];
            variationID *= 3;
            switch (face)
            {
                case (Face.top):
                    coord[0] = new Vector2(blockID / (float)blockTotal, variationID / (float)variationTotal);
                    coord[1] = new Vector2((blockID + 1) / (float)blockTotal, variationID / (float)variationTotal);
                    coord[2] = new Vector2(blockID / (float)blockTotal, (variationID + 1) / (float)variationTotal);
                    coord[3] = coord[1];
                    coord[4] = new Vector2((blockID + 1) / (float)blockTotal, (variationID + 1) / (float)variationTotal);
                    coord[5] = coord[2];
                    break;
                case (Face.down):
                    coord[0] = new Vector2(blockID / (float)blockTotal, (variationID + 1) / (float)variationTotal);
                    coord[1] = new Vector2((blockID + 1) / (float)blockTotal, (variationID + 1) / (float)variationTotal);
                    coord[2] = new Vector2(blockID / (float)blockTotal, (variationID + 2) / (float)variationTotal);
                    coord[3] = coord[1];
                    coord[4] = new Vector2((blockID + 1) / (float)blockTotal, (variationID + 2) / (float)variationTotal);
                    coord[5] = coord[2];
                    break;
                case (Face.side):
                    coord[0] = new Vector2(blockID / (float)blockTotal, (variationID + 2) / (float)variationTotal);
                    coord[1] = new Vector2((blockID + 1) / (float)blockTotal, (variationID + 2) / (float)variationTotal);
                    coord[2] = new Vector2(blockID / (float)(blockTotal), (variationID + 3) / (float)variationTotal);
                    coord[3] = coord[1];
                    coord[4] = new Vector2((blockID + 1) / (float)blockTotal, (variationID + 3) / (float)variationTotal);
                    coord[5] = coord[2];
                    break;
            }
           
            return coord;
        }


        public static void WriteToScreen(Vector3 vector, SpriteFont font, SpriteBatch spriteBatch, Vector2 position)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font, String.Format("X: {0:0.000}\nY: {1:0.000}\nZ: {2:0.000}", vector.X, vector.Y, vector.Z), position, Color.White);
            spriteBatch.End();
        }

        //better mod (than Math.Mod)
        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        } 

        //used when proper collider not available
        public static bool PlaceholderCollider(Vector3 position)
        {
            return false;
        }

        //Floors vector3
        public static Vector3 Floor(this Vector3 vector)
        {
            return new Vector3((float)Math.Floor(vector.X),(float)Math.Ceiling(vector.Y),(float)Math.Floor(vector.Z));
        }

        //2D loop in a spiral
        public static Vector2 Spiral(int X, int Y, int C) {
            int x = 0;
            int y = 0;
            int dx = 0;
            int dy = -1;
            C += 1;
            Vector2 ans = new Vector2();
            for (int i = 0; i < C; i++) {
                if (-X / 2 < x && x <= X / 2 && -Y / 2 < y && y <= Y / 2) {
                    ans = new Vector2(x, y);
                }
                if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1 - y)) {
                    int t = dx;
                    dx = -dy;
                    dy = t;
                }
                x = x + dx;
                y = y + dy;
             } 
            return ans;
        }
    } 
}
