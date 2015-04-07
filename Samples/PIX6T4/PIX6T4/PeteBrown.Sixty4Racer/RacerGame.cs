using System;
using Microsoft.SPOT;
using netduino.helpers.Fun;
using netduino.helpers.Imaging;
using System.Threading;
using netduino.helpers.Sound;

namespace PeteBrown.Sixty4Racer
{
    class RacerGame : Game
    {
        private Screens _screens;
        private PlayerMissile _ship;

        public RacerGame(ConsoleHardwareConfig config)
            : base(config)
        {
            _screens = new Screens(this);

            DisplayDelay = 25;
        }

        Sprite _explosion = new Sprite(new byte[]
            {
	            0x00, 0x00, 0x00, 0x18, 0x18, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x18, 0x24, 0x24, 0x18, 0x00, 0x00,
	            0x08, 0x3C, 0x72, 0x5B, 0xDE, 0x62, 0x3C, 0x08,
	            0x3C, 0xD2, 0xAD, 0xFE, 0x5F, 0xF5, 0x56, 0x28,
	            0x00, 0x3C, 0x56, 0x7A, 0x6E, 0x72, 0x3C, 0x00,
	            0x00, 0x00, 0x18, 0x3C, 0x3C, 0x18, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x18, 0x18, 0x00, 0x00, 0x00,
	            0x00, 0x00, 0x00, 0x08, 0x10, 0x00, 0x00, 0x00
            });

        protected override void OnGameEnd()
        {
            var song = new RttlSong("SadTrombone:d=4,o=4,b=40:32d4,32c#4,32c4,4b3");
            var thread = song.Play(Hardware.Speaker, true);

            for (int i = 0; i < _explosion.FrameCount; i++)
            {
                Hardware.Matrix.Display(_explosion.GetNextFrame());
                Thread.Sleep(200);
            }

            ScrollMessage(" Game Over");
        }


        protected override void OnGameStart()
        {
            base.OnGameStart();

            _ship = new PlayerMissile()
                {
                    Name = "ship",
                    IsEnemy = false,
                    X = 3,
                    IsVisible = true,
                    VerticalSpeed = 0,
                };

            ScrollMessage(" 6T4Racer!");
        }

        
        private int _currentLevel = 0;
        private float _currentlevelSpeedIncrement = 0f;  
        private const float BaseShipSpeed = 0.3f;           // speed ship moves across the screen
        private float _exactScrollPosition = 0.0f;

        private int CurrentWorldLine
        {
            get { return (int)_exactScrollPosition; }
        }

        private void InitializeLevel()
        {
            World = _screens.GetLevelComposition(_currentLevel);
            World.AddMissile(_ship);

            _exactScrollPosition = _screens.GetLevelPixelHeight(_currentLevel) - 1;
            _currentlevelSpeedIncrement = 0.02f * (float)_currentLevel; // tweak this to change speed

            _ship.Y = CurrentWorldLine + 7; // always be on bottom line
            _ship.X = 3;
        }

        private void CompleteLevel()
        {
            // TODO: Play some music

            Thread.Sleep(1500);
            ScrollMessage(" Level Up!");
        }

        private void IntroduceLevel()
        {
            // show the level number
            Hardware.Matrix.Display(SmallChars.ToBitmap(_currentLevel / 10, _currentLevel % 10));
            Thread.Sleep(1000);
        }

        private bool CheckForCollision()
        {
            return World.Background.GetPixel(_ship.X, _ship.Y);
        }


        private void Blip()
        {
            Beep(200, 30);
        }

        private bool _firstTime = true;
        private int _oldShipX = 0;
        public override void Loop()
        {
            _ship.HorizontalSpeed = (float)Hardware.JoystickLeft.XDirection * BaseShipSpeed;
            _ship.Move();

            if (_ship.X != _oldShipX)
            {
                Blip();
                _oldShipX = _ship.X;
            }


            // make the ship blink so we can see it
            _ship.IsVisible = !_ship.IsVisible;

            if (_ship.X < 0) _ship.X = 0;
            if (_ship.X > 7) _ship.X = 7;

            if (CurrentWorldLine == 0 || _firstTime)
            {
                if (!_firstTime)
                {
                    CompleteLevel();
                }

                // Next Level
                _currentLevel++;
                InitializeLevel();
                IntroduceLevel();

                _firstTime = false;
            }
            else
            {
                _exactScrollPosition -= _currentlevelSpeedIncrement;

                _ship.Y = CurrentWorldLine + 7; // always be on bottom line
            }

            // draw the frame
            Hardware.Matrix.Display(World.GetFrame(0, CurrentWorldLine));

            if (CheckForCollision())
            {
                // game over
                Stop();
            }

        }
    }
}
