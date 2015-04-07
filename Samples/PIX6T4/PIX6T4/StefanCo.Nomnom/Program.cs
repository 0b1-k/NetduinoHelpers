//#define dev
#define NETDUINO_MINI

using Microsoft.SPOT.Hardware;
using netduino.helpers.Fun;
using netduino.helpers.Hardware;
using netduino.helpers.Helpers;
using SecretLabs.NETMF.Hardware;

#if NETDUINO_MINI
using SecretLabs.NETMF.Hardware.NetduinoMini;
#else
using SecretLabs.NETMF.Hardware.Netduino;
#endif

namespace StefanCo.Nomnom
{
    public class Program
    {

#if dev
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
#endif
        /// <summary>
        /// During development, Main() acts as the ConsoleBootLoader, making it easy to debug the game.
        /// When game development is complete, comment out the content Main() to remove the overhead
        /// </summary>
        public static void Main()
        {
#if dev
            var args = new object[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.Size];

            var index = 0;
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
            Matrix.SetIntensity(8);

            Run(args);
#endif
        }

        /// <summary>
        /// Entry point called by the ConsoleBootLoader project
        /// </summary>
        /// <param name="args">Array of object references to the hardware features</param>
        public static void Run(object[] args)
        {
            var thread = new Nomnom(new ConsoleHardwareConfig(args)).Run();
            thread.Join();
        }
    }
}
