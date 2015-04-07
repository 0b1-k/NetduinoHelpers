using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

namespace netduino.helpers.Servo
{
    /// <summary>
    /// Driver for the HiTec HS6635HB servo.
    /// http://www.hitecrcd.com/products/digital/digital-sport/hs-6635hb.html
    /// </summary>
    public class HS6635HBServo : IDisposable {
        private PWM _servo;

        public uint MinRangePulse { get; set; }
        public uint CenterRangePulse { get; set; }
        public uint MaxRangePulse { get; set; }
        public uint PulseRefreshRateMs { get; set; }

        public HS6635HBServo(Cpu.Pin pwmPin, uint minPulse = 900, uint centerPulse = 1500, uint maxPulse = 2100) {
            _servo = new PWM((Cpu.Pin)pwmPin);
            _servo.SetDutyCycle(0);
            MinRangePulse = minPulse;
            CenterRangePulse = centerPulse;
            MaxRangePulse = maxPulse;
            PulseRefreshRateMs = 20;
        }

        // Slowly moves the servo from a position to another
        public void Move(uint startDegree, uint endDegree, int delay = 80) {
            if (delay <= 1) {
                delay = 10;
            }

            if (startDegree < endDegree) {
                for (var degree = startDegree; degree <= endDegree; degree++) {
                    Degree = degree;
                    Thread.Sleep(delay);
                }
            } else {
                for (var degree = startDegree; degree > endDegree; degree--) {
                    Degree = degree;
                    Thread.Sleep(delay);
                }
            }

            Release();
        }

        // Positions the servo to its center position
        public void Center() {
            Pulse = CenterRangePulse;
            Thread.Sleep(1000);
        }

        /// <summary>
        /// Positions the servo from 0 to 180 degrees
        /// </summary>
        public uint Degree {
            set {
                if (value > 180) {
                    value = 180;
                } else {
                    if (value < 0) {
                        value = 0;
                    }
                }
                var pulse = (uint) MapRange(
                    (double)0, 
                    (double)180,
                    (double)MinRangePulse,
                    (double)MaxRangePulse,
                    (double)value
                    );
                
                //Debug.Print("Degree: " + value.ToString() + " = " + pulse.ToString());
                
                Pulse = pulse;
            }
        }

        // Positions the servo using an absolute pulse
        public uint Pulse {
            set {
                if (value != 0) {
                    if (value > MaxRangePulse) {
                        value = MaxRangePulse;
                    } else {
                        if (value < MinRangePulse) {
                            value = MinRangePulse;
                        }
                    }
                    _servo.SetPulse(PulseRefreshRateMs * 1000, value);
                } else {
                    _servo.SetPulse(0, 0);
                }
            }
        }

        // Releases the servo from tracking its position
        public void Release() {
            _servo.SetDutyCycle(0);
        }

        /// <summary>
        /// Maps a range of values to another
        /// http://rosettacode.org/wiki/Map_range#C
        /// </summary>
        /// <param name="a1"></param>
        /// <param name="a2"></param>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private double MapRange(double a1, double a2, double b1, double b2, double s) {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        // Disposes of the servo resources
        public void Dispose() {
            Release();
            _servo.Dispose();
            _servo = null;
        }
    }
}
