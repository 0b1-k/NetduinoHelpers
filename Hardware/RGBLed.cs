using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

namespace netduino.helpers.Hardware {
    public class RGBLed {
        public enum Color {
            Red = 0xFF0000,
            Orange = 0xFF9900,
            Yellow = 0xFFFF00,
            Green = 0x00FF00,
            Blue = 0x0000FF,
            Indigo = 0x6F00FF,
            Violet = 0x9F00FF,
            Cyan = 0x00FFFF,
            Magenta = 0xFF00FF,
            White = 0xFFFFFF,
            Black = 0x000000,
            Pink = 0xFF69B4
        }

        private PWM _red;
        private PWM _green;
        private PWM _blue;

        public bool CommonAnode { get; set; }

        public RGBLed(Cpu.Pin red, Cpu.Pin green, Cpu.Pin blue, bool commonAnode = true) {
            _red = new PWM(red);
            _green = new PWM(green);
            _blue = new PWM(blue);

            CommonAnode = commonAnode;
        }

        public void SetColor(Color rgbValue, int delay = 0) {
            if (CommonAnode == true) {
                _blue.SetDutyCycle((uint)(255 - ((int)rgbValue & 0xFF)));
                _green.SetDutyCycle((uint)(255 - (((int)rgbValue >> 8) & 0xFF)));
                _red.SetDutyCycle((uint)(255 - (((int)rgbValue >> 16) & 0xFF)));
            } else {
                _blue.SetDutyCycle((uint)rgbValue & 0xFF);
                _green.SetDutyCycle((uint)((int)rgbValue >> 8) & 0xFF);
                _red.SetDutyCycle((uint)((int)rgbValue >> 16) & 0xFF);
            }
            if (delay != 0) Thread.Sleep(delay);
        }
    }
}
