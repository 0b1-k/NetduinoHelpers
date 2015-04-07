#define NETDUINO

using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

#if NETDUINO_MINI
using SecretLabs.NETMF.Hardware.NetduinoMini;
#endif

#if NETDUINO
using SecretLabs.NETMF.Hardware.Netduino;
#endif

#if NETDUINO_PLUS
using SecretLabs.NETMF.Hardware.NetduinoPlus;
#endif

using SecretLabs.NETMF.IO;
using netduino.helpers.Hardware;

namespace AdaFruitVC0706Test {
    public class Program {
        public static AdaFruitVC0706 Camera = new AdaFruitVC0706();
        public static OutputPort OnboardLed = new OutputPort(Pins.ONBOARD_LED, false);
        public static void Main() {
            try {
#if NETDUINO_MINI
                StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_13);
#endif
#if NETDUINO
                StorageDevice.MountSD("SD", SPI.SPI_module.SPI1, Pins.GPIO_PIN_D10);
#endif
                Camera.Initialize("COM1", AdaFruitVC0706.PortSpeed.Baud115200, AdaFruitVC0706.ImageSize.Res640x480);
                Camera.TvOutput(true);
                ShowCameraConfigTest();
                Camera.Initialize("COM1", AdaFruitVC0706.PortSpeed.Baud115200, AdaFruitVC0706.ImageSize.Res160x120);
                TakePictureStressTest(@"SD\StressTestSmall",100);
                Camera.Initialize("COM1", AdaFruitVC0706.PortSpeed.Baud115200, AdaFruitVC0706.ImageSize.Res320x240);
                TakePictureStressTest(@"SD\StressTestMedium",100);
                Camera.Initialize("COM1", AdaFruitVC0706.PortSpeed.Baud115200, AdaFruitVC0706.ImageSize.Res640x480);
                TakePictureStressTest(@"SD\StressTestLarge",100);
                DownSizeTest();
                MotionDetectionTest(3);
                Camera.TvOutput(false);
                Camera.Shutdown();

#if !NETDUINO_PLUS
                StorageDevice.Unmount("SD");
#endif
            } catch ( Exception e ) {
                Debug.Print(e.Message);
                while (true) {
                    OnboardLed.Write(true);
                    Thread.Sleep(500);
                    OnboardLed.Write(false);
                    Thread.Sleep(250);
                }
            }
        }

        public static void TakePictureStressTest(string path, int maxCount = 100) {
            Directory.CreateDirectory(path);
            for (var count = 0; count < maxCount; count++) {
                var imgPath = path + @"\Pic_" + count + ".jpg";
                Debug.Print("Working on " + imgPath);
                OnboardLed.Write(true);
                Camera.TakePicture(imgPath);
                OnboardLed.Write(false);
            }
        }

        public static void DownSizeTest() {
            Camera.SetDownSize(AdaFruitVC0706.Proportion.HalfSize, AdaFruitVC0706.Proportion.HalfSize);
            Thread.Sleep(100);
            Camera.SetDownSize(AdaFruitVC0706.Proportion.NoZoom, AdaFruitVC0706.Proportion.NoZoom);
            Thread.Sleep(2000);

            Camera.Initialize("COM1", AdaFruitVC0706.PortSpeed.Baud115200, AdaFruitVC0706.ImageSize.Res640x480);

            Camera.SetDownSize(AdaFruitVC0706.Proportion.QuarterSize, AdaFruitVC0706.Proportion.QuarterSize);
            Thread.Sleep(100);
            Camera.SetDownSize(AdaFruitVC0706.Proportion.NoZoom, AdaFruitVC0706.Proportion.NoZoom);
            Thread.Sleep(2000);

            Camera.Initialize("COM1", AdaFruitVC0706.PortSpeed.Baud115200, AdaFruitVC0706.ImageSize.Res640x480);            
        }

        public static void ShowDownSizeConfig() {
            AdaFruitVC0706.Proportion width;
            AdaFruitVC0706.Proportion height;
            Camera.GetDownSize(out width, out height);
            Debug.Print("DownSize width: " + width);
            Debug.Print("DownSize height: " + height);
        }

        public static void ShowCameraConfigTest() {
            Debug.Print("Camera version: " + Camera.GetVersion());
            Debug.Print("Compression: " + Camera.GetCompression());
            Debug.Print("Image size: " + Camera.GetImageSize());
            Debug.Print("Motion detection activation: " + Camera.GetMotionDetectionCommStatus());

            ushort width = 0;
            ushort height = 0;
            ushort zoomWidth = 0;
            ushort zoomHeight = 0;
            ushort pan = 0;
            ushort tilt = 0;

            Camera.GetPanTiltZoom(out width, out height, out zoomWidth, out zoomHeight, out pan, out tilt);

            Debug.Print("PTZ width: " + width.ToString());
            Debug.Print("PTZ height: " + height.ToString());
            Debug.Print("PTZ zoomWidth: " + zoomWidth.ToString());
            Debug.Print("PTZ zoomHeight: " + zoomHeight.ToString());
            Debug.Print("PTZ pan: " + pan.ToString());
            Debug.Print("PTZ tilt: " + tilt.ToString());

            ShowCameraColorControlMode();
        }

        public static void ShowCameraColorControlMode() {
            byte showMode = 0;
            AdaFruitVC0706.ColorControl currentColor = AdaFruitVC0706.ColorControl.BlackWhiteColor;
            Camera.GetColorStatus(out showMode, out currentColor);
            Debug.Print("Color status showMode: " + showMode.ToString());
            Debug.Print("Color control currentColor: " + currentColor.ToString());
        }

        public static void MotionDetectionTest(int minutes) {
            Debug.Print("Begin Motion Detection Test");
            Directory.CreateDirectory(@"SD\Motion");
            Camera.StartAutoMotionDetection(0, @"SD\Motion", OnMotion);
            Thread.Sleep(1000 * 60 * minutes);
            Camera.StopAutoMotionDetection();
            Debug.Print("End Motion Detection Test");
        }

        public static bool OnMotion(Exception e, int imageSequenceNumber, string imagePath) {
            if (e == null) {
                Debug.Print("Motion Detection: " + imagePath);
                return true;
            } else Debug.Print("Motion Detection Exception: " + e.Message);
            return false;
        }
    }
}
