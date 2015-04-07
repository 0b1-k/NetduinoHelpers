using System.Threading;
using netduino.helpers.Fun;
using netduino.helpers.Imaging;
using netduino.helpers.Sound;
namespace Tunes {
    public class Player : Game {
        public Player(ConsoleHardwareConfig config)
            : base(config) {
        }
        protected override void OnGameStart() {
            ScrollMessage(" Name that tune...");
        }
        protected override void OnGameEnd() {
            ScrollMessage(" Bye!");
        }
        public override void Loop() {
            var song = new RttlSong("PacMan:d=4,o=5,b=90:32b,32p,32b6,32p,32f#6,32p,32d#6,32p,32b6,32f#6,16p,16d#6,16p,32c6,32p,32c7,32p,32g6,32p,32e6,32p,32c7,32g6,16p,16e6,16p,32b,32p,32b6,32p,32f#6,32p,32d#6,32p,32b6,32f#6,16p,16d#6,16p,32d#6,32e6,32f6,32p,32f6,32f#6,32g6,32p,32g6,32g#6,32a6,32p,32b.6");
            var thread = song.Play(Hardware.Speaker, false);
            Stop();
        }
    }
}
