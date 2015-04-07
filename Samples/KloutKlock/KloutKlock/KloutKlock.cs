using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.NetduinoPlus;
using netduino.helpers.Helpers;
using netduino.helpers.Hardware;
using netduino.helpers.Imaging;

namespace KloutKlock {
    public class KloutKlock {

        public static VirtualFrame vm;
        public static AdaFruitST7735 tft;
        public static readonly string VirtualMemoryFilename = @"SD\vm.bin";
        // Pulses based on http://github.com/adafruit/iCufflinks/blob/master/Firmware/cuff.asm
        public static readonly uint[] BreatheInPulses = new uint[] {0, 1, 1, 1, 1, 1, 1, 1, 1, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4, 7, 7, 7, 7, 7, 7, 9, 9, 9, 12, 12, 12, 14, 14, 16, 16, 16, 16, 21, 21, 21, 21, 24, 24, 26, 28, 28, 28, 31, 36, 33, 36, 36, 40, 40, 43, 43, 45, 48, 52, 55, 55, 55, 57, 62, 62, 64, 67, 72, 74, 79, 81, 86, 86, 86, 88, 93, 96, 98, 100, 112, 115, 117, 124, 127, 129, 129, 136, 141, 144, 148, 160, 165, 170, 175, 184, 189, 194, 199, 208, 213, 220, 237, 244, 252, 255, 255, 255, 255, 255, 255, 255 };
        public static readonly uint[] BreatheOutPulses = new uint[] { 255, 255, 255, 255, 255, 255, 255, 255, 252, 247, 235, 235, 230, 225, 218, 213, 208, 206, 199, 189, 187, 182, 182, 177, 175, 168, 165, 163, 158, 148, 146, 144, 144, 141, 139, 136, 134, 127, 122, 120, 117, 115, 112, 112, 110, 110, 108, 103, 96, 96, 93, 91, 88, 88, 88, 88, 84, 79, 76, 74, 74, 72, 72, 72, 72, 69, 69, 62, 60, 60, 57, 57, 57, 55, 55, 55, 55, 48, 48, 45, 45, 43, 43, 40, 40, 40, 40, 36, 36, 36, 33, 33, 31, 31, 31, 28, 28, 26, 26, 26, 26, 24, 24, 21, 21, 21, 21, 20, 19, 19, 16, 16, 16, 16, 14, 14, 14, 16, 12, 12, 12, 12, 12, 9, 9, 9, 9, 9, 9, 7, 7, 7, 7, 7, 7, 4, 4, 4, 4, 4, 4, 4, 4, 4, 2, 2, 2, 2, 2, 2, 2, 2, 1, 1, 1, 1, 1, 1, 1, 1, 0 };
        public static readonly PWM tftBacklight = new PWM(Pins.GPIO_PIN_D5);

        public static void Main() {
            try {
                tftBacklight.SetPulse(0, 0);

                //ShowGcStats();

                InitializeResources();

                ShowKloutSplashScreen();

                while (true) {
                    SetInternalClock();

                    ShowTime();

                    KloutGet(_host, _port, @"/1/users/show.json", _key, _user, @"SD\Cache\show.txt");
                    KloutGet(_host, _port, @"/1/users/topics.json", _key, _user, @"SD\Cache\topics.txt");
                    KloutGet(_host, _port, @"/1/soi/influenced_by.json", _key, _user, @"SD\Cache\influenced.txt");
                    KloutGet(_host, _port, @"/1/soi/influencer_of.json", _key, _user, @"SD\Cache\influencer.txt");

                    ProcessCachedResults(@"SD\Cache\show.txt", KloutShowHandler);
                    ProcessCachedResults(@"SD\Cache\topics.txt", KloutTopicsHandler);
                    ProcessCachedResults(@"SD\Cache\influenced.txt", KloutInfluencedByHandler);
                    ProcessCachedResults(@"SD\Cache\influencer.txt", KloutInfluencerOfHandler);

                    ShowKlout();
                    
                    ShowTime();

                    //ShowGcStats();

                    Thread.Sleep(30000);
                }
            } catch (OutOfMemoryException e) {
                PowerState.RebootDevice(false);
            }
        }

        public static void ShowKloutSplashScreen() {
            tftBacklight.SetPulse(0, 0);
            var background = new VirtualFrame(40960, 32, @"SD\Bitmaps\KLOUTSplashScreen.bin");
            background.Width = 160;
            background.Height = 128;
            vm.Copy(background);
            tft.Refresh();
            BreatheTFT(BreatheInPulses);
            background.Dispose();
            background = null;
            Debug.GC(true);
            Thread.Sleep(2000);
        }

        public static void BreatheTFT(uint[] pulses) {
            foreach (uint pulse in pulses) {
                tftBacklight.SetPulse(255, pulse);
                Thread.Sleep(17);
            }
        }

        public static void ShowTime() {
            BreatheTFT(BreatheOutPulses);

            var background = new VirtualFrame(40960, 32, @"SD\Bitmaps\KloutBackground.bin");
            background.Width = 160;
            background.Height = 128;
            vm.IsReadOnly = false;
            vm.Copy(background);

            DisplayKloutOrKlockIcon(background, "clock");
            DisplayKloutOrKlock(background, "KLOCK");

            vm.BitmapDirectory = @"SD\DigitsS\";
            vm.MaxCharactersAfterPeriod = 2;
            vm.MaxMessageLength = 10;
            vm.Print(background, GetDate(), 7, 35 + 4, 15, 21);

            vm.BitmapDirectory = @"SD\DigitsM\";
            vm.MaxCharactersAfterPeriod = 2;
            vm.MaxMessageLength = 5;
            vm.Print(background, GetTime(), 0, 64 + 11, 32, 42);

            tft.Refresh();
            BreatheTFT(BreatheInPulses);
            background.Dispose();
            background = null;
            Debug.GC(true);
        }

        public static string GetTime() {
            var parts = GetAdjustedTime().ToLocalTime().ToString().Split(' ');
            return parts[1];
        }

        public static string GetDate() {
            var parts = GetAdjustedTime().ToLocalTime().ToString().Split(' ');
            return parts[0];
        }

        public static DateTime GetAdjustedTime() {
            var timeNow = DateTime.Now;
            TimeSpan timeSpan = new TimeSpan(_timeZone + _dst, 0, 0);
            timeNow = timeNow.Add(timeSpan);
            return timeNow;
        }

        public static void ShowKlout() {
            BreatheTFT(BreatheOutPulses);

            var background = new VirtualFrame(40960, 32, @"SD\Bitmaps\KloutBackground.bin");
            background.Width = 160;
            background.Height = 128;

            vm.IsReadOnly = false;
            vm.Copy(background);

            vm.BitmapDirectory = @"SD\DigitsL\";
            vm.MaxCharactersAfterPeriod = 1;
            vm.MaxMessageLength = 4;
            
            DisplayKloutOrKlockIcon(background, "KloutLogo");
            DisplayKloutOrKlock(background, "KLOUT"); 
            
            DisplayKloutKPI(background, "Class");
            DisplayKloutClass(background, _kclass);
            tft.Refresh();

            BreatheTFT(BreatheInPulses);

            DisplayKloutDataPoint(background, "KloutScore", _kscore);
            DisplayKloutDataPoint(background, "Network", _networkScore);
            DisplayKloutDataPoint(background, "Amplification", _amplificationScore);
            DisplayKloutDataPoint(background, "TrueReach", _trueReach);
            DisplayKloutDataPoint(background, "Influencers", _influencers);
            DisplayKloutDataPoint(background, "Influencees", _influencees);
            DisplayKloutDataPoint(background, "Topics", _topicsCount);
            DisplayKloutDataPoint(background, "Delta1Day", _delta1Day, true);
            DisplayKloutDataPoint(background, "Delta5Days", _delta5Day, true);

            background.Dispose();
            background = null;
            Debug.GC(true);
        }

        public static void DisplayKloutDataPoint(VirtualFrame background, string kpi, float value, bool showArrow = false) {
            DisplayKloutKPI(background, kpi);
            EraseKloutData(background);
            
            string displayValue;

            if ((float) System.Math.Round(value) == 0f) {
                displayValue = " -- ";
                showArrow = false;
            } else if (value >= 1000000f) {
                value = (float) System.Math.Round(value / 1000000f);
                displayValue = value.ToString() + "M";
            } else if (value >= 1000f) {
                value = (float)System.Math.Round(value / 1000f);
                displayValue = value.ToString() + "K";
            } else {
                displayValue = System.Math.Round(value).ToString();
            }

            int xOffset = 0;

            if (showArrow) {
                xOffset += 40;
                if (value < 0f) {
                    DisplayPositiveNegativeIcon(background, "neg");
                } else {
                    DisplayPositiveNegativeIcon(background, "pos");
                }
            }

            if (xOffset == 0) {
                xOffset = (160 - (displayValue.Length * 40)) / 2;
            }

            vm.Print(background, displayValue, xOffset, 64 + 2, 40, 60);
            tft.Refresh();
        }

        public static void EraseKloutData(VirtualFrame background) {
            var sprite = new VirtualFrame(20480, 16, @"SD\Bitmaps\KloutDataEraser.bin");
            sprite.Width = 160;
            sprite.Height = 64;
            sprite.xOffset = 0;
            sprite.yOffset = 64;
            vm.Merge(background, sprite);
            sprite.Dispose();
            sprite = null;
            Debug.GC(true);
        }

        public static void DisplayKloutClass(VirtualFrame background, string kloutClass) {
            var sprite = new VirtualFrame(6720, 16, @"SD\Bitmaps\" + kloutClass + ".bin");
            sprite.Width = 160;
            sprite.Height = 21;
            sprite.xOffset = 0;
            sprite.yOffset = 64 + 21;
            vm.Merge(background, sprite);
            sprite.Dispose();
            sprite = null;
            Debug.GC(true);
        }

        public static void DisplayPositiveNegativeIcon(VirtualFrame background, string icon) {
            var sprite = new VirtualFrame(4800, 16, @"SD\Bitmaps\" + icon + ".bin");
            sprite.Width = 40;
            sprite.Height = 60;
            sprite.xOffset = 0;
            sprite.yOffset = 64 + 2;
            vm.Merge(background, sprite);
            sprite.Dispose();
            sprite = null;
            Debug.GC(true);
        }

        public static void DisplayKloutOrKlockIcon(VirtualFrame background, string icon) {
            var sprite = new VirtualFrame(4950, 16, @"SD\Bitmaps\" + icon + ".bin");
            sprite.Width = 48;
            sprite.Height = 34;
            sprite.xOffset = 0;
            sprite.yOffset = 0;
            vm.Merge(background, sprite);
            sprite.Dispose();
            sprite = null;
            Debug.GC(true);
        }

        public static void DisplayKloutOrKlock(VirtualFrame background, string filename) {
            var sprite = new VirtualFrame(7548, 16, @"SD\Bitmaps\" + filename + ".bin");
            sprite.Width = 111;
            sprite.Height = 34;
            sprite.xOffset = 44;
            sprite.yOffset = 0;
            vm.Merge(background, sprite);
            sprite.Dispose();
            sprite = null;
            Debug.GC(true);
        }

        public static void DisplayKloutKPI(VirtualFrame background, string filename) {
            var sprite = new VirtualFrame(8960, 16, @"SD\Bitmaps\" + filename + ".bin");
            sprite.Width = 160;
            sprite.Height = 28;
            sprite.xOffset = 0;
            sprite.yOffset = 35;
            vm.Merge(background, sprite);
            sprite.Dispose();
            sprite = null;
            Debug.GC(true);
        }

        //public static void DumpKloutData() {
        //    Debug.Print("_user: " + _user);
        //    Debug.Print("_delta1Day:" + System.Math.Round(_delta1Day).ToString());
        //    Debug.Print("_kscore: " + System.Math.Round(_kscore).ToString());
        //    Debug.Print("_networkScore: " + System.Math.Round(_networkScore).ToString());
        //    Debug.Print("_amplificationScore: " + System.Math.Round(_amplificationScore).ToString());
        //    Debug.Print("_kclassId: " + _kclassId.ToString());
        //    Debug.Print("_trueReach: " + System.Math.Round(_trueReach).ToString());
        //    Debug.Print("_delta5Day: " + System.Math.Round(_delta5Day).ToString());
        //    Debug.Print("_slope: " + System.Math.Round(_slope).ToString());
        //    Debug.Print("_kclass: " + _kclass);
        //    Debug.Print("_topicsCount: " + _topicsCount.ToString());
        //    Debug.Print("_influencers: " + _influencers.ToString());
        //    Debug.Print("_influencees: " + _influencees.ToString());
        //}

        public static void ShowGcStats() {
            Debug.EnableGCMessages(true);
            Debug.GC(true);
            Debug.EnableGCMessages(false);
        }

        private static string _key;
        private static string _user;
        private static string _host;
        private static int _port;
        private static int _timeZone;
        private static int _dst;
        private static string _ntpServers;

        /// <summary>
        /// Configures the application from the 'resources.txt' file place on the microSD card.
        /// See the 'SD Card Resources' folder for a sample to start with.
        /// </summary>
        private static void InitializeResources() {
            var resourceLoader = new SDResourceLoader(); 

            try {
                resourceLoader.Load();
            } catch (IOException e) {
                PowerState.RebootDevice(false);
            }

            _key = (string)resourceLoader.Strings["key"];
            _user = (string)resourceLoader.Strings["user"];
            _host = (string)resourceLoader.Strings["host"];
            _port = int.Parse((string)resourceLoader.Strings["port"]);
            _timeZone = int.Parse((string)resourceLoader.Strings["timeZone"]);
            _ntpServers = (string)resourceLoader.Strings["ntpServers"];
            _dst = int.Parse((string)resourceLoader.Strings["dst"]);

            resourceLoader.Dispose();

            vm = new VirtualFrame(40960, 16, VirtualMemoryFilename);
            vm.IsReadOnly = false;

            tft = new AdaFruitST7735(Pins.GPIO_PIN_D9, Pins.GPIO_PIN_D7, Pins.GPIO_PIN_D8, speedKHz: 40000, vm: vm);
            tft.Orientation = AdaFruitST7735.ScreenOrientation.Landscape;

            Debug.GC(true);
        }

        private static void SetInternalClock() {
            try {
                var servers = _ntpServers.Split(',');
                Trace.Print("Begin SetSystemTimeUTC");
                NTPTime.SetSystemTimeUTC(NTPTime.GetNTPTime(servers));
                Trace.Print("End SetSystemTimeUTC");
            } catch {
                // Failed to set system time due to all NTP servers failing.
                // To clear a possible network stack issue, reboot the device!
                PowerState.RebootDevice(false);
            }
            Debug.GC(true);
        }

        public static bool KloutGet(string host, int port, string kloutCall, string key, string user, string filename) {
            var request = "GET " + kloutCall + "?key=" + key + "&users=" + user + " HTTP/1.1\r\n";
            request += "Host: " + host + "\r\n";
            request += "Connection: close\r\n";
            request += "\r\n";

            IPHostEntry hostEntry = null;

            try {
                hostEntry = Dns.GetHostEntry(host);
            } catch (SocketException e) {
                Debug.Print(e.Message);
                return false;
            }

            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
                socket.ReceiveTimeout = 10000;
                socket.SendTimeout = 10000;

                var requestBytes = Encoding.UTF8.GetBytes(request);

                try {
                    socket.Connect(new IPEndPoint(hostEntry.AddressList[0], port));
                    socket.Send(requestBytes);
                } catch (SocketException e) {
                    Debug.Print(e.Message);
                    return false;
                }

                request = null;
                requestBytes = null;
                hostEntry = null;
                Debug.GC(true);

                var maxTimeout = 3000;
                while (socket.Available == 0 && maxTimeout != 0) {
                    Thread.Sleep(10);
                    maxTimeout -= 10;
                }

                if (maxTimeout == 0) {
                    Debug.Print("Timeout waiting for server data!");
                    return false;
                }

                try {
                    byte[] buffer = new byte[600];
                    using (var cacheFile = new FileStream(filename, FileMode.Create)) {
                        var read = 0;
                        while ((read = socket.Receive(buffer, buffer.Length, SocketFlags.None)) > 0) {
                            cacheFile.Write(buffer, 0, read);
                        }
                    }
                } catch (Exception e) {
                    File.Delete(filename);
                    Debug.Print(e.Message);
                    return false;
                }
            }

            //DumpFile(filename);

            return true;
        }

        //public static void DumpFile(string filename) {
        //    Debug.Print(DateTime.Now.ToString() + " : " + filename);
        //    try {
        //        using (var cacheFile = new StreamReader(filename)) {
        //            while (true) {
        //                var line = cacheFile.ReadLine();
        //                Debug.Print(line);
        //                if (cacheFile.EndOfStream) break;
        //            }
        //        }
        //    } catch (IOException e) {
        //        Debug.Print(e.Message);
        //    }
        //}

        public delegate void JsonHandler(StreamReader reader);

        public static bool ProcessCachedResults(string filename, JsonHandler handler) {
            var rc = false;
            try {
                using (var file = new StreamReader(filename)) {
                    var line = file.ReadLine();
                    var status = line.Split(' ');

                    if (status == null) return rc;
                    if (status.Length != 3) return rc;
                    if (int.Parse(status[1]) != 200) return rc;

                    line = null;
                    status = null;
                    Debug.GC(true);

                    var length = file.BaseStream.Length;
                    while (length-- > 0) {
                        var c = (Char)file.Peek();
                        if (c == '{') {
                            try {
                                handler(file);
                                rc = true;
                            } catch (Exception e) {
                                Debug.Print(e.Message);
                            }
                            break;
                        } else {
                            c = (Char)file.Read();
                        }
                    }
                }
            } catch (IOException e) {
                Debug.Print(e.Message);
            }

            Debug.GC(true);
            return rc;
        }

        private static float _delta1Day;
        private static float _kscore;
        private static float _networkScore;
        private static float _amplificationScore;
        private static float _trueReach;
        private static float _delta5Day;
        private static float _slope;
        private static string _kclass;
        private static int _kclassId;

        public static void KloutShowHandler(StreamReader file) {
            var parser = new JSONParser();
            var results = parser.Parse(file);

            short status;
            ArrayList users;

            _delta1Day = 0.0f;
            _kscore = 0.0f;
            _networkScore = 0.0f;
            _amplificationScore = 0.0f;
            _kclassId = 0;
            _trueReach = 0.0f;
            _delta5Day = 0.0f;
            _slope = 0.0f;

            if (parser.Find("status", results, out status) && parser.Find("users", results, out users)) {
                if (status != 200 || users == null) throw new Exception("KloutShowHandler");
                foreach (Hashtable user in users) {
                    Hashtable score;
                    if (parser.Find("score", user, out score)) {
                        parser.Find("delta_1day", score, out _delta1Day);
                        parser.Find("kscore", score, out _kscore);
                        parser.Find("network_score", score, out _networkScore);
                        parser.Find("amplification_score", score, out _amplificationScore);
                        parser.Find("kclass_id", score, out _kclassId);
                        parser.Find("true_reach", score, out _trueReach);
                        parser.Find("delta_5day", score, out _delta5Day);
                        parser.Find("slope", score, out _slope);
                        parser.Find("kclass", score, out _kclass);
                    }
                }
            }
        }

        private static int _topicsCount;

        public static void KloutTopicsHandler(StreamReader file) {
            var parser = new JSONParser();
            var results = parser.Parse(file);

            short status;
            ArrayList users;
            _topicsCount = 0;

            if (parser.Find("status", results, out status) && parser.Find("users", results, out users)) {
                if (status != 200 || users == null) throw new Exception("KloutTopicsHandler");
                foreach (Hashtable user in users) {
                    ArrayList topics;
                    parser.Find("topics", user, out topics);
                    _topicsCount = topics.Count;
                }
            }
        }

        private static int _influencers;

        public static void KloutInfluencedByHandler(StreamReader file) {
            var parser = new JSONParser();
            var results = parser.Parse(file);

            short status;
            ArrayList users;
            _influencers = 0;

            if (parser.Find("status", results, out status) && parser.Find("users", results, out users)) {
                if (status != 200 || users == null) throw new Exception("KloutInfluencedByHandler");
                foreach (Hashtable user in users) {
                    ArrayList influencers;
                    parser.Find("influencers", user, out influencers);
                    _influencers = influencers.Count;
                }
            }
        }

        private static int _influencees;

        public static void KloutInfluencerOfHandler(StreamReader file) {
            var parser = new JSONParser();
            var results = parser.Parse(file);

            short status;
            ArrayList users;
            _influencees = 0;

            if (parser.Find("status", results, out status) && parser.Find("users", results, out users)) {
                if (status != 200 || users == null) throw new Exception("KloutInfluencerOfHandler");
                foreach (Hashtable user in users) {
                    ArrayList influencees;
                    parser.Find("influencees", user, out influencees);
                    _influencees = influencees.Count;
                }
            }
        }
    }
}
