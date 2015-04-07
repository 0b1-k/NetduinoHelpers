using netduino.helpers.Fun;
using netduino.helpers.Imaging;
using System.Threading;

namespace Paddles {
    public class GameOfPaddles : Game {
        private const int ScreenSize = 8;
        private const int StickActiveZoneSize = 300;
        private const int StickRange = 1024;
        private const uint BeepFrequency = 10000;
        private const uint BoopFrequency = 3000;
        private const int PaddleAmplitude = ScreenSize - Paddle.Size;
        private const int StickActiveAmplitude = StickActiveZoneSize - Paddle.Size*StickActiveZoneSize/ScreenSize;
        private const int StickMin = (StickRange - StickActiveAmplitude)/2;
        private const int StickMax = (StickRange + StickActiveAmplitude)/2;
        private const int MaxScore = 9;

        public int LeftScore { get; set; }
        public int RightScore { get; set; }

        public PlayerMissile Ball { get; private set; }
        public Paddle LeftPaddle { get; private set; }
        public Paddle RightPaddle { get; private set; }

        public GameOfPaddles(ConsoleHardwareConfig config) : base(config) {
            World = new Composition(new byte[] { 0, 0, 0, 0, 0, 0, 0, 0 }, ScreenSize, ScreenSize);
            Ball = new PlayerMissile {
                Name = "ball",
                Owner = World,
                IsEnemy = true
            };
            LeftPaddle = new Paddle(Side.Left, this);
            RightPaddle = new Paddle(Side.Right, this);

            World.Coinc +=
                (s, a, b) => {
                    Ball.HorizontalSpeed = -Ball.HorizontalSpeed;
                    return false;
                };
            
            ResetBall(true);
        }

        protected override void OnGameStart() {
            ScrollMessage(" 2 Paddles and a Ball");
        }

        protected override void OnGameEnd() {
            ScrollMessage(" Game over!");
        }

        public override void Loop() {
            var effectiveLeftPaddleY = Hardware.JoystickLeft.Y < StickMin
                                           ? StickMin
                                           : Hardware.JoystickLeft.Y > StickMax
                                                 ? StickMax
                                                 : Hardware.JoystickLeft.Y;
            LeftPaddle.Y = (effectiveLeftPaddleY - StickMin) * PaddleAmplitude / StickActiveAmplitude;
            var effectiveRightPaddleY = Hardware.JoystickRight.Y < StickMin
                                           ? StickMin
                                           : Hardware.JoystickRight.Y > StickMax
                                                 ? StickMax
                                                 : Hardware.JoystickRight.Y;
            RightPaddle.Y = (effectiveRightPaddleY - StickMin) * PaddleAmplitude / StickActiveAmplitude;

            Ball.Move();
            if (Ball.X < 0) {
                RightScore++;
                DisplayScores(LeftScore, RightScore);
                ResetBall(true);
            }
            if (Ball.X >= 8) {
                LeftScore++;
                DisplayScores(LeftScore, RightScore);
                ResetBall(false);
            }
            if (Ball.Y < 0) {
                Ball.Y = 1;
                Ball.VerticalSpeed = 1;
                Beep(BeepFrequency, 50);
            }
            if (Ball.Y >= 8) {
                Ball.Y = 7;
                Ball.VerticalSpeed = -1;
                Beep(BeepFrequency, 50);
            }
            Hardware.Matrix.Display(World.GetFrame(0, 0));
        }

        private void DisplayScores(int leftScore, int rightScore) {
            Hardware.Matrix.Display(SmallChars.ToBitmap(leftScore, rightScore));
            if (leftScore >= MaxScore || rightScore >= MaxScore) {
                WaitForClick();
                Stop();
            }
            else {
                Thread.Sleep(2000);
            }
        }

        private void WaitForClick() {
            ResetButtonClicks();
            while (!(IsLeftButtonClicked|| IsRightButtonClicked)) {
                Thread.Sleep(100);
            }
        }

        public void ResetBall(bool ballGoingRight) {
            Ball.X = ballGoingRight ? 1 : 6;
            Ball.Y = Random.Next(8);
            Ball.HorizontalSpeed = ballGoingRight ? 1 : -1;
            Ball.VerticalSpeed = Random.Next(2) == 0 ? 1 : -1;
            Beep(BoopFrequency, 50);
        }
    }
}
