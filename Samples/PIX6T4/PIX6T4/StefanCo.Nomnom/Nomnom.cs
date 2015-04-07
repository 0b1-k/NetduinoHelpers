#define toolboxspeaker

using System.Threading;
using netduino.helpers.Fun;
using netduino.helpers.Imaging;
using Microsoft.SPOT;

// I used my own speaker class due to a small bug in the RTTL class from Nwazet
#if toolboxspeaker
using Toolbox.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoMini;
#else
using netduino.helpers.Sound;
#endif

/*
 * Copyright 2012 Stefan Thoolen (http://www.stefan.co/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace StefanCo.Nomnom
{
    class Nomnom : Game
    {
        // Max. dimensions of the game
        public const int Width = 8;
        public const int Height = 8;

        // Storage for the position of the snake (y * Height + x). The integer size is the max. length of the snake
        private int[] _Snake = new int[40];

        // The position for the food
        private int _FoodPosition = -1;

        // The direction we're moving into (-1 = left, 1 = right, -Width = up, +Width = down)
        private int _Direction = -1;

        // Each tick this will be increased
        private int _Timer = 0;

        // The current game speed
        private int _Speed = 40;

#if toolboxspeaker
        // The speaker
        private Speaker _speaker;
#endif
        
        /// <summary>
        /// Class constructor, creates a new Nom Nom! Game
        /// </summary>
        /// <param name="config">The hardware configuration</param>
        public Nomnom(ConsoleHardwareConfig config)
            : base(config)
        {
            DisplayDelay = 1;
#if toolboxspeaker
            // I used my own speaker class due to a small bug in the RTTL class from Nwazet
            config.Speaker.Dispose();
            this._speaker = new Speaker(Pins.GPIO_PIN_18);
#endif
        }

        /// <summary>
        /// Resets the snake, placing it at a random position
        /// </summary>
        private void _ResetSnake()
        {
            // The rest of the snake pixels is empty
            for (int Counter = 0; Counter < this._Snake.Length; ++Counter)
                this._Snake[Counter] = -1; // No position
            // Starts at the middle
            this._Snake[0] = Height * Width / 2 - Width / 2;
        }

        /// <summary>
        /// Will place food on the matrix
        /// </summary>
        private void _ResetFood()
        {
            int NewPos = -1;
            bool Found;

            // Checks if this position isn't currently occupied by the snake
            do
            {
                NewPos = Random.Next(Height * Width);
                Found = false;
                for (int Counter = 0; Counter < this._Snake.Length; ++Counter)
                    if (this._Snake[Counter] == NewPos) Found = true;
            } while (Found);

            this._FoodPosition = NewPos;
        }

        /// <summary>
        /// Moves the snake one position
        /// </summary>
        private void _DoMovement()
        {
            // The next position
            int NextPos = this._Snake[0] + this._Direction;
            
            // We went out of the screen
            if (NextPos < 0) NextPos += (Height * Width);                                            // At the top
            if (NextPos >= Height * Width) NextPos -= (Height * Width);                              // At the bottom
            if (NextPos / Width < this._Snake[0] / Width && this._Direction == -1) NextPos += Width; // At the left
            if (NextPos / Width > this._Snake[0] / Width && this._Direction == 1) NextPos -= Width;  // At the right

            // Are we eating?
            bool Grow = false;
            if (NextPos == this._FoodPosition)
            {
                Grow = true;
                this._ResetFood();
#if toolboxspeaker
                this._speaker.Sound(6000, 3);
#else
                Beep(6000, 50);
#endif
            }

            // The next list of positions will be stored in here
            int[] NewSnake = (int[])this._Snake.Clone();
            // Some checks
            for (int Counter = 0; Counter < this._Snake.Length; ++Counter)
            {
                // We ate ourself!
                if (this._Snake[Counter] == NextPos) Stop();
                // Is this our length?
                if (this._Snake[Counter] == -1 && !Grow) break;
                // Do we need to grow?
                else if (this._Snake[Counter] == -1) Grow = false;
                // Moves up one spot
                if (Counter > 0) NewSnake[Counter] = this._Snake[Counter - 1];
            }

            // New position
            NewSnake[0] = NextPos;
            this._Snake = NewSnake;

            // Did we win?
            if (this._Snake[this._Snake.Length - 1] != -1)
            {
                ScrollMessage(" Next level!");
                this._ResetSnake();
                this._ResetSnake();
                this._Speed = (int)(this._Speed * .9) - 1;
            }
        }

        /// <summary>
        /// Starts the game
        /// </summary>
        protected override void OnGameStart()
        {
            // Sets the snake for the first time
            this._ResetSnake();
            this._ResetFood();
            // Plays intro tune
#if toolboxspeaker
            this._speaker.Play("MBT128L8O6DDD<B>DEL4D");
#else
            var song = new RttlSong("nom-nom-no:d=4,o=5,b=128:8d6,8d6,8d6,8b,8d6,8e6,d6");
            var thread = song.Play(Hardware.Speaker, true);
#endif
            // Scrolls the intro text
            ScrollMessage(" Nom Nom!");
        }

        /// <summary>
        /// Creates a bitmap for the screen
        /// </summary>
        /// <returns>An array of bytes containing a bitmap</returns>
        private byte[] _ToBitmap()
        {
            // We start with an empty array
            byte[] RetVal = new byte[(Width * Height / 8)];

            // Adds the snake to the byte array
            for (int Counter = 0; Counter < this._Snake.Length; ++Counter)
                if (this._Snake[Counter] != -1)
                {
                    int SnakeByte = this._Snake[Counter] >> 3;
                    int SnakeBit = this._Snake[Counter] & 7;
                    RetVal[SnakeByte] |= (byte)(0x80 >> SnakeBit);
                }

            // Adds the food to the byte array
            int FoodByte = this._FoodPosition >> 3;
            int FoodBit = this._FoodPosition & 7;
            RetVal[FoodByte] |= (byte)(0x80 >> FoodBit);

            return RetVal;
        }

        /// <summary>
        /// The game has been ended
        /// </summary>
        protected override void OnGameEnd()
        {
            ScrollMessage(" Game over!");
        }

        /// <summary>
        /// Generic game loop
        /// </summary>
        public override void Loop()
        {
            // Draws out the display
            Hardware.Matrix.Display(this._ToBitmap());

            // Joystick control changes the direction
            if (Hardware.JoystickLeft.XDirection == netduino.helpers.Hardware.AnalogJoystick.Direction.Negative && this._Direction != 1)
                this._Direction = -1;
            if (Hardware.JoystickLeft.XDirection == netduino.helpers.Hardware.AnalogJoystick.Direction.Positive && this._Direction != -1)
                this._Direction = 1;
            if (Hardware.JoystickLeft.YDirection == netduino.helpers.Hardware.AnalogJoystick.Direction.Negative && this._Direction != 0 - Width)
                this._Direction = Width;
            if (Hardware.JoystickLeft.YDirection == netduino.helpers.Hardware.AnalogJoystick.Direction.Positive && this._Direction != Width)
                this._Direction = 0 - Width;

            // Increases the timer
            this._Timer++;

            // Do we need to move?
            if (this._Timer >= this._Speed)
            {
                this._DoMovement();
                this._Timer = 0;
            }
        }
    }
}
