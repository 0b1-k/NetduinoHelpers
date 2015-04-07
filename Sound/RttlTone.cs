namespace netduino.helpers.Sound {
    public class RttlTone {
        public uint Note { get; set; }
        public int Duration { get; set; }
        public uint Period { get; set; }

        public void SetTone(uint note, uint duration) {
            Note = note;
            Duration = (int) duration;
            if (note > 0) {
                Period = 1000000 / note; // 1000000 = 1 sec. Period = 1 sec / frequency
            } else {
                Period = 0;
            }
        }

        /// <summary>
        /// Calculate the duration of the note based on the song's tempo
        /// </summary>
        /// <param name="tempo">Song's tempo variable</param>
        /// <returns>Duration of the note in milliseconds</returns>
        public int GetDelay(int tempo) {
            return tempo / Duration;
        }
    }
}
