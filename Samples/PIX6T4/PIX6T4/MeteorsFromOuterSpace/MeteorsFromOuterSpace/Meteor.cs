using netduino.helpers.Imaging;
using netduino.helpers.Math;

namespace Meteors {
    public class Meteor {
        public const float MeteorSpeed = 0.1f;

        private readonly byte[] _rockOffsets =
            new byte[] {
                           0, 0,
                           1, 0,
                           1, 1,
                           0, 1
                       };
        private readonly PlayerMissile[] _rocks = new PlayerMissile[3];
        private readonly byte[] _rockXOffsets = new byte[3];
        private readonly byte[] _rockYOffsets = new byte[3];

        public bool IsExploded { get; set; }
        public int Index { get; private set; }
        public GameOfMeteors Owner { get; private set; }

        public Meteor(GameOfMeteors game, int index) {
            Owner = game;
            Index = index;
            for (var i = 0; i < 3; i++) {
                _rocks[i] = new PlayerMissile {
                                                  Name = "Meteor" + index + ":" + i,
                                                  IsVisible = false,
                                                  Owner = game.World,
                                                  IsEnemy = true
                                              };
            }
        }

        private Vector2D GetRandomSpeed() {
            var dir = (float)Owner.Random.NextDouble() * 2 * Trigo.Pi;
            return new Vector2D {
                                    X = Trigo.Cos(dir)*MeteorSpeed,
                                    Y = Trigo.Sin(dir)*MeteorSpeed
                                };
        }

        public void Move() {
            for(var i = 0; i < _rocks.Length; i++) {
                if (!_rocks[i].IsVisible) continue;
                _rocks[i].Move();
                GameOfMeteors.ApplyToreGeometry(_rocks[i]);
            }
        }

        public bool Has(PlayerMissile someRock) {
            foreach (var rock in _rocks) {
                if (rock == someRock && rock.IsVisible) return true;
            }
            return false;
        }

        public void Explode() {
            _rocks[1].IsVisible = false;
            var speed = GetRandomSpeed();
            _rocks[0].HorizontalSpeed = speed.X;
            _rocks[0].VerticalSpeed = speed.Y;
            speed = GetRandomSpeed();
            _rocks[2].HorizontalSpeed = speed.X;
            _rocks[2].VerticalSpeed = speed.Y;
            IsExploded = true;
        }

        public void Respawn(int x, int y) {
            var speed = GetRandomSpeed();
            var j = 0;
            var skip = Owner.Random.Next(4);
            for (var i = 0; i < 4; i++) {
                if (i == skip) continue;
                _rockXOffsets[j] = _rockOffsets[i * 2];
                _rockYOffsets[j] = _rockOffsets[i * 2 + 1];
                _rocks[j].X = x + _rockXOffsets[j];
                _rocks[j].Y = y + _rockYOffsets[j];
                _rocks[j].HorizontalSpeed = speed.X;
                _rocks[j].VerticalSpeed = speed.Y;
                _rocks[j].IsVisible = true;
                j++;
            }
            IsExploded = false;
        }
    }
}
