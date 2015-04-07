#define NETDUINO_MINI

using System;
using System.IO;
using System.Threading;
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.IO;
using netduino.helpers.Hardware;
using netduino.helpers.Helpers;
using netduino.helpers.Fun;

#if NETDUINO_MINI
using SecretLabs.NETMF.Hardware.NetduinoMini;
#else
using SecretLabs.NETMF.Hardware.Netduino;
#endif

namespace ConsoleBootLoader {
    public class Program {
#if NETDUINO_MINI
        // Use this document to see the pin map of the mini: http://www.netduino.com/netduinomini/schematic.pdf
        public static AnalogJoystick JoystickLeft = new AnalogJoystick(Pins.GPIO_PIN_5, Pins.GPIO_PIN_6, minYRange: 1023, maxYRange: 0, centerDeadZoneRadius: 80);
        public static AnalogJoystick JoystickRight = new AnalogJoystick(Pins.GPIO_PIN_7, Pins.GPIO_PIN_8, minYRange: 1023, maxYRange: 0, centerDeadZoneRadius: 80);
        public static Max72197221 Matrix = new Max72197221(chipSelect: Pins.GPIO_PIN_17);
        public static PWM Speaker = new PWM(Pins.GPIO_PIN_18);
        public static PushButton ButtonLeft = new PushButton(Pins.GPIO_PIN_19, Port.InterruptMode.InterruptEdgeLevelLow, null, Port.ResistorMode.PullUp);
        public static PushButton ButtonRight = new PushButton(Pins.GPIO_PIN_20, Port.InterruptMode.InterruptEdgeLevelLow, null, Port.ResistorMode.PullUp);
        public static OutputPort max7219PowerTransistor = new OutputPort(Pins.GPIO_PIN_9,false);
#else
        public static AnalogJoystick JoystickLeft = new AnalogJoystick(Pins.GPIO_PIN_A0, Pins.GPIO_PIN_A1, centerDeadZoneRadius: 20);
        public static AnalogJoystick JoystickRight = new AnalogJoystick(Pins.GPIO_PIN_A2, Pins.GPIO_PIN_A3, centerDeadZoneRadius: 20);
        public static Max72197221 Matrix = new Max72197221(chipSelect: Pins.GPIO_PIN_D8);
        public static PWM Speaker = new PWM(Pins.GPIO_PIN_D5);
        public static PushButton ButtonLeft = new PushButton(Pins.GPIO_PIN_D0, Port.InterruptMode.InterruptEdgeLevelLow, null, Port.ResistorMode.PullUp);
        public static PushButton ButtonRight = new PushButton(Pins.GPIO_PIN_D1, Port.InterruptMode.InterruptEdgeLevelLow, null, Port.ResistorMode.PullUp);
        public static OutputPort max7219PowerTransistor = new OutputPort(Pins.GPIO_PIN_D7,false);
#endif
        public static SDResourceLoader ResourceLoader = new SDResourceLoader();
        public static readonly string SDMountPoint = @"SD";
        
        private static bool _leftButtonClicked = false;

        public static object[] args = new object[(int) CartridgeVersionInfo.LoaderArgumentsVersion100.Size];

        public static void Main() {
            try {
                while (true) {
                    int index = 0;
                    args[index++] = CartridgeVersionInfo.CurrentVersion;
                    args[index++] = JoystickLeft;
                    args[index++] = JoystickRight;
                    args[index++] = Matrix;
                    args[index++] = Speaker;
                    args[index++] = ResourceLoader;
                    args[index++] = ButtonLeft;
                    args[index] = ButtonRight;

                    Matrix.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
                    Matrix.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
                    Matrix.SetDigitScanLimit(7);
                    Matrix.SetIntensity(3);

                    Matrix.Display(new byte[] { 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55, 0xAA, 0x55 });
#if NETDUINO_MINI
                    StorageDevice.MountSD(SDMountPoint, SPI.SPI_module.SPI1, Pins.GPIO_PIN_13);
#else
                    StorageDevice.MountSD(SDMountPoint, SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
#endif
                    ResourceLoader.Path = SelectCartridge();
                    ResourceLoader.Load(resourceManifest: "cartridge.txt", args: new object[] { args });
                    ResourceLoader.Dispose();

#if NETDUINO_MINI || NETDUINO
                    StorageDevice.Unmount(SDMountPoint);
#endif
                    Debug.GC(true);

                    ResourceLoader = new SDResourceLoader();
                }
            } catch (IOException) {
                Matrix.Display(new byte[] { 0x81, 0x42, 0x3c, 0x5a, 0x7e, 0x24, 0x5a, 0x81 });
                Wait();
            }
        }

        private static string SelectCartridge() {

            ButtonLeft.Input.DisableInterrupt();
            ButtonLeft.Input.OnInterrupt += OnLeftButtonClick;
            ButtonLeft.Input.EnableInterrupt();

            var folders = ResourceLoader.GetFolderList(SDMountPoint);
            var foldersWithGames = new string[folders.Length];
            var count = 0;
            ArrayList iconFiles = new ArrayList();
            foreach (string folder in folders) {
                var game = folder.Substring(folder.LastIndexOf(@"\"));
                if (File.Exists(folder + @"\cartridge.txt")) {
                    foldersWithGames[count++] = folder;
                    var iconFile = folder + game + @".bmp.bin";
                    iconFiles.Add(iconFile);
                }
            }

            var current = 0;
            var previous = current;
            var matrixFrame = new byte[8];
            var questionMarkIcon = new byte[] { 0x1c, 0x22, 0x22, 0x04, 0x08, 0x08, 0x00, 0x08 };

            DisplayIcon((string)iconFiles[current], matrixFrame, questionMarkIcon);

            _leftButtonClicked = false;

            while (!_leftButtonClicked) {
                switch(JoystickLeft.YDirection) {
                    case AnalogJoystick.Direction.Negative:
                        if (current > 0) {
                            current--;
                        }
                        break;
                    case AnalogJoystick.Direction.Positive:
                        if (current < iconFiles.Count - 1) {
                            current++;
                        }
                        break;
                }
                if (previous != current) {
                    previous = current;
                    DisplayIcon((string)iconFiles[current], matrixFrame, questionMarkIcon);
                }
                Thread.Sleep(200);
            }

            ButtonLeft.Input.DisableInterrupt();
            ButtonLeft.Input.OnInterrupt -= OnLeftButtonClick;
            ButtonLeft.Input.EnableInterrupt();

            return foldersWithGames[current];
        }

        private static void DisplayIcon(string iconFilePath, byte[] matrixFrame, byte[] defaultIcon) {
            try{
                using (var bmpfile = new FileStream(iconFilePath, FileMode.Open, FileAccess.Read, FileShare.None)) {
                    bmpfile.Read(matrixFrame, 0, (int)matrixFrame.Length);
                    Matrix.Display(matrixFrame);
                }
            }
            catch(Exception e){
                Matrix.Display(defaultIcon);
            }
        }

        private static void OnLeftButtonClick(UInt32 port, UInt32 state, DateTime time) {
            _leftButtonClicked = true;
        }

        private static void Wait() {
            while (true) {
                Thread.Sleep(1000);
            }
        }
    }
}
