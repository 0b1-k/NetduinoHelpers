using System.Threading;
using System.Collections;
using SecretLabs.NETMF.Hardware;

namespace netduino.helpers.Sound {
    /// <summary>
    /// This class implements the Ring Tone Transfer Language
    /// http://en.wikipedia.org/wiki/Ring_Tone_Transfer_Language
    /// </summary>
    public class RttlSong {
        /// <summary>
        /// Notes from C1 to B7 organized in groups of 12
        /// </summary>
        public static RttlPitches[] RttlNotes = {
            RttlPitches.C1,RttlPitches.CS1,RttlPitches.D1,RttlPitches.DS1,RttlPitches.E1,RttlPitches.F1,RttlPitches.FS1,RttlPitches.G1,RttlPitches.GS1,RttlPitches.A1,RttlPitches.AS1,RttlPitches.B1,
            RttlPitches.C2,RttlPitches.CS2,RttlPitches.D2,RttlPitches.DS2,RttlPitches.E2,RttlPitches.F2,RttlPitches.FS2,RttlPitches.G2,RttlPitches.GS2,RttlPitches.A2,RttlPitches.AS2,RttlPitches.B2,
            RttlPitches.C3,RttlPitches.CS3,RttlPitches.D3,RttlPitches.DS3,RttlPitches.E3,RttlPitches.F3,RttlPitches.FS3,RttlPitches.G3,RttlPitches.GS3,RttlPitches.A3,RttlPitches.AS3,RttlPitches.B3,
            RttlPitches.C4,RttlPitches.CS4,RttlPitches.D4,RttlPitches.DS4,RttlPitches.E4,RttlPitches.F4,RttlPitches.FS4,RttlPitches.G4,RttlPitches.GS4,RttlPitches.A4,RttlPitches.AS4,RttlPitches.B4,
            RttlPitches.C5,RttlPitches.CS5,RttlPitches.D5,RttlPitches.DS5,RttlPitches.E5,RttlPitches.F5,RttlPitches.FS5,RttlPitches.G5,RttlPitches.GS5,RttlPitches.A5,RttlPitches.AS5,RttlPitches.B5,
            RttlPitches.C6,RttlPitches.CS6,RttlPitches.D6,RttlPitches.DS6,RttlPitches.E6,RttlPitches.F6,RttlPitches.FS6,RttlPitches.G6,RttlPitches.GS6,RttlPitches.A6,RttlPitches.AS6,RttlPitches.B6,
            RttlPitches.C7,RttlPitches.CS7,RttlPitches.D7,RttlPitches.DS7,RttlPitches.E7,RttlPitches.F7,RttlPitches.FS7,RttlPitches.G7,RttlPitches.GS7,RttlPitches.A7,RttlPitches.AS7,RttlPitches.B7
            };

        // Name of the RTTL song
        public string Name { get; set; }
        // Song's duration
        public int Duration { get; set; }
        // Song's octave
        public int Octave { get; set; }
        // Song BPM
        public int Beat { get; set; }
        // Interval to wait between notes
        public int Tempo { get; set; }

        /// <summary>
        /// Cached RTTL notes
        /// </summary>
        private string[] _rttlNotes;

        /// <summary>
        /// Creates a song based on an RTTL string
        /// </summary>
        /// <param name="rttlData">Raw RTTL note string</param>
        public RttlSong(string rttlData) {
            var parts = rttlData.Split(':');
            var header = parts[1].Split(',');
            _rttlNotes = parts[2].Split(',');

            Name = parts[0];
            Duration = int.Parse(header[0].Substring(2, header[0].Length - 2));
            Octave = int.Parse(header[1].Substring(2, header[1].Length - 2));
            Beat = int.Parse(header[2].Substring(2, header[2].Length - 2));
            Tempo = ((1000 * 60) / Beat) * 4;
        }

        /// <summary>
        /// Plays a raw RTTL string by converting it into PWN tones
        /// Derived from http://code.google.com/p/rogue-code/source/browse/Arduino/libraries/Tone/trunk/examples/RTTTL/RTTTL.pde
        /// and Ian Lintner's port to C# https://github.com/ianlintner/Netduino-Ring-Tone-Player
        /// </summary>
        public void Play(PWM channel) {
            char[] charParserArray; //used for parsing the current section
            const int defaultDuration = 4;
            const int defaultOctave = 6;
            var tone = new RttlTone();

            foreach (var rttlNote in _rttlNotes) {
                charParserArray = rttlNote.ToLower().ToCharArray();

                // Parse each note... and play it!
                for (var i = 0; i < rttlNote.Length; i++) {
                    var durationParseNumber = 0;
                    int currentScale;
                    var currentNote = 0;
                    const int octaveOffset = 0;

                    // first, get note duration, if available
                    while (i < charParserArray.Length && IsDigit(charParserArray[i])) {
                        //construct the duration
                        durationParseNumber = (durationParseNumber * 10) + (charParserArray[i++] - '0');
                    }

                    var currentDuration = durationParseNumber > 0 ? durationParseNumber : defaultDuration;

                    // c is first note i.e. c = 1
                    // b = 12
                    // pause or undefined = 0
                    if (i < charParserArray.Length) {
                        switch (charParserArray[i]) {
                            case 'c':
                                currentNote = 1;
                                break;
                            case 'd':
                                currentNote = 3;
                                break;
                            case 'e':
                                currentNote = 5;
                                break;
                            case 'f':
                                currentNote = 6;
                                break;
                            case 'g':
                                currentNote = 8;
                                break;
                            case 'a':
                                currentNote = 10;
                                break;
                            case 'b':
                                currentNote = 12;
                                break;
                            case 'p':
                                currentNote = 0;
                                break;
                            default:
                                currentNote = 0;
                                break;
                        }
                    }

                    i++;

                    // process whether the note is sharp
                    if (i < charParserArray.Length && charParserArray[i] == '#') {
                        currentNote++;
                        i++;
                    }

                    // is it dotted note, divide the duration in half
                    if (i < charParserArray.Length && charParserArray[i] == '.') {
                        currentDuration += currentDuration / 2;
                        i++;
                    }

                    // now, get octave
                    if (i < charParserArray.Length && IsDigit(charParserArray[i])) {
                        currentScale = charParserArray[i] - '0';
                        i++;
                    } else {
                        currentScale = defaultOctave;
                    }

                    //offset if necessary
                    currentScale += octaveOffset;

                    // Setup the tone by calculating the note's location in the RTTTL note array
                    tone.SetTone((uint)RttlNotes[(currentScale - 1) * 12 + currentNote], (uint)currentDuration);
                    
                    // Play the tone
                    PlayTone(tone, channel);
                }
            }
        }

        /// <summary>
        /// Play an single tone on a given channel 
        /// </summary>
        /// <param name="tone">A RttlTone object</param>
        /// <param name="channel">Any PWN pin</param>
        public void PlayTone(RttlTone tone, PWM channel) {
            if (tone.Note != 0) {
                channel.SetPulse(tone.Period, tone.Period / 2);
                Thread.Sleep(tone.GetDelay(Tempo));
                channel.SetDutyCycle(0);
            } else {
                channel.SetDutyCycle(0);
                Thread.Sleep(tone.GetDelay(Tempo));
            }
        }


        /// <summary>
        /// Plays the song
        /// </summary>
        /// <param name="channel">The PWN pin connected to the speaker</param>
        /// <param name="asynchronous">True: play the song on a separate thread and return immediately. False: play the song and return when done.</param>
        /// <returns>If asynchronous == true, returns a reference to the thread playing the song or a null reference if asynchronous == false.</returns>
        public Thread Play(PWM channel, bool asynchronous = false) {
            if (asynchronous) {
                var thread = new Thread(() => Play(channel));
                thread.Start();
                return thread;
            } 

            Play(channel);
            return null;
        }

        /// <summary>
        /// Determines if a character is a digit between 0-9
        /// </summary>
        /// <param name="c">Character to test</param>
        /// <returns>True if the character is a digit</returns>
        private static bool IsDigit(char c) {
            return c >= '0' && c <= '9';
        }
    }
}
