#define NETDUINO_MINI

using System;
using System.IO;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

#if NETDUINO || NETDUINO_PLUS
using SecretLabs.NETMF.Hardware.Netduino;
#endif

#if NETDUINO_MINI
using SecretLabs.NETMF.Hardware.NetduinoMini;
#endif

using netduino.helpers.Hardware;
using netduino.helpers.Imaging;
using netduino.helpers.Helpers;

namespace DualStickTest
{
    public class Program
    {
        public static void Main()
        {
#if NETDUINO || NETDUINO_PLUS
            var LeftJoystick = new AnalogJoystick(Pins.GPIO_PIN_A0, Pins.GPIO_PIN_A1);
            var RightJoystick = new AnalogJoystick(Pins.GPIO_PIN_A2, Pins.GPIO_PIN_A3);
            var matrix = new Max72197221(chipSelect: Pins.GPIO_PIN_D8);
#endif
#if NETDUINO_MINI
            var LeftJoystick = new AnalogJoystick(Pins.GPIO_PIN_5, Pins.GPIO_PIN_6, minYRange: 1023, maxYRange: 0, centerDeadZoneRadius: 30);
            var RightJoystick = new AnalogJoystick(Pins.GPIO_PIN_7, Pins.GPIO_PIN_8, minYRange: 1023, maxYRange: 0, centerDeadZoneRadius: 30);
            var matrix = new Max72197221(chipSelect: Pins.GPIO_PIN_17);
#endif

            matrix.Shutdown(Max72197221.ShutdownRegister.NormalOperation);
            matrix.SetDecodeMode(Max72197221.DecodeModeRegister.NoDecodeMode);
            matrix.SetDigitScanLimit(7);
            matrix.SetIntensity(4);

            var comp = new Composition(new byte[8], 8, 8);

            var leftBall = new PlayerMissile("leftBall", 0, 0);
            var rightBall = new PlayerMissile("rightBall", 0, 0);

            comp.AddMissile(leftBall);
            comp.AddMissile(rightBall);

            while (true)
            {
                leftBall.X = LeftJoystick.X / 128;
                leftBall.Y = LeftJoystick.Y / 128;
                rightBall.X = RightJoystick.X / 128;
                rightBall.Y = RightJoystick.Y / 128;

                Debug.Print("LEFT: (X=" + LeftJoystick.X + " (" + LeftJoystick.XDirection + ")" + ", Y=" + LeftJoystick.Y + " (" + LeftJoystick.YDirection + "), RIGHT: (X=" + RightJoystick.X + " (" + RightJoystick.XDirection + ")" + ", Y=" + RightJoystick.Y + " (" + RightJoystick.YDirection + ")");

                matrix.Display(comp.GetFrame(0, 0));

                Thread.Sleep(80);
            }
        }
    }
}