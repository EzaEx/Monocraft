using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Monocraft
{
    class NumberEntry : TextBox, IUpdateable
    {  
        //numbers being pressed
        private bool[] numsDown;
        private bool[] oldNumsDown;
        private int _seed;
        
        public NumberEntry(Vector2 position, int scale, SpriteFont font, GraphicsDevice graphics, SpriteBatch spriteBatch) : base("", position, scale, font, graphics, spriteBatch)
        { 
            //set up arrays of pressed numbers for entry
            numsDown = new bool[10];
            oldNumsDown = new bool[10];
            
        }

        public int Seed { get => _seed; set => _seed = value; }

        public void Update(GameTime gameTime)
        { 
            //set all pressed nums to false
            for (int i = 0; i < numsDown.Length; i++)
            {
                numsDown[i] = false;
            } 
            //set to true if respective num is being pressed
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad1))
            {
                numsDown[1] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad2))
            {
                numsDown[2] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad3))
            {
                numsDown[3] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad4))
            {
                numsDown[4] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad5))
            {
                numsDown[5] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad6))
            {
                numsDown[6] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad7))
            {
                numsDown[7] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad8))
            {
                numsDown[8] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad9))
            {
                numsDown[9] = true;
            }
            if (Keyboard.GetState().IsKeyDown(Keys.NumPad0))
            {
                numsDown[0] = true;
            } 
            //if num is released, add to running string
            for (int i = 0; i < numsDown.Length; i++)
            {
                if (numsDown[i] == false && oldNumsDown[i] == true)
                {
                    _text += i.ToString();
                    _seed = int.Parse(_text);
                };
                oldNumsDown[i] = numsDown[i];
            }
        }

    }
}
