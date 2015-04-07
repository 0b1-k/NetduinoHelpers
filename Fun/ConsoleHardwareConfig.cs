using System;
using netduino.helpers.Hardware;
using netduino.helpers.Fun;
using netduino.helpers.Helpers;
using SecretLabs.NETMF.Hardware;

namespace netduino.helpers.Fun {
    public class ConsoleHardwareConfig {
        public int Version { get; private set; }
        public AnalogJoystick JoystickLeft { get; set; }
        public AnalogJoystick JoystickRight { get; set; }
        public Max72197221 Matrix { get; set; }
        public PWM Speaker { get; set; }
        public SDResourceLoader Resources { get; set; }
        public PushButton LeftButton { get; set; }
        public PushButton RightButton { get; set; }

        public ConsoleHardwareConfig(object[] args) {
            Version = (int)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.Version];

            if (100 == Version) {
                JoystickLeft = (AnalogJoystick)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.JoystickLeft];
                JoystickRight = (AnalogJoystick)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.JoystickRight];
                Matrix = (Max72197221)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.Matrix];
                Speaker = (PWM)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.Speaker];
                Resources = (SDResourceLoader)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.SDResourceLoader];
                LeftButton = (PushButton)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.ButtonLeft];
                RightButton = (PushButton)args[(int)CartridgeVersionInfo.LoaderArgumentsVersion100.ButtonRight];
            }
            else {
                throw new ArgumentException("args");
            }
        }
    }
}
