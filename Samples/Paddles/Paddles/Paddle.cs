using netduino.helpers.Imaging;

namespace Paddles {
    public class Paddle {
        public const int Size = 3;

        public int Y {
            get { return _pixels[0].Y; }
            set {
                for (var i = 0; i < Size; i++) {
                    _pixels[i].Y = value + i;
                }
            }
        }

        private readonly Side _side;
        private readonly PlayerMissile[] _pixels;

        public Paddle(Side side, GameOfPaddles game) {
            _side = side;
            var world = game.World;
            _pixels = new PlayerMissile[Size];
            for(var i = 0; i < Size; i++) {
                _pixels[i] = new PlayerMissile(
                    "paddle" +
                    (_side == Side.Right ? 'R' : 'L') +
                    i,
                    _side == Side.Right ? 7 : 0,
                    i,
                    world);
            }
        }
    }

    public enum Side {
        Left,
        Right
    }
}
