using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Hardware;

namespace SharpDistanceSensorTest {
    public class Program {
        public static void Main() {
            // Monitor 6 distance sensors. If an analog input is not connected to a sensor, it must be connected to the Netduino's GND.
            var sensorPins = new Cpu.Pin[6];
            sensorPins[0] = Pins.GPIO_PIN_A0;
            sensorPins[1] = Pins.GPIO_PIN_A1;
            sensorPins[2] = Pins.GPIO_PIN_A2;
            sensorPins[3] = Pins.GPIO_PIN_A3;
            sensorPins[4] = Pins.GPIO_PIN_A4;
            sensorPins[5] = Pins.GPIO_PIN_A5;

            var sensorArray = new SharpGP2Y0A21YK0F(sensorPins);
            sensorArray.Start(VoltageTrigger);
            Debug.Print("Started monitoring " + sensorPins.Length + " distance sensors");
            uint seconds = 60;
            while (--seconds > 0) {
                Thread.Sleep(1000);
            }
            sensorArray.Dispose();
            Debug.Print("Finished monitoring sensors");
        }

        public static void VoltageTrigger(int sensorID, int averagedDistance, float mappedVoltage) {
            Debug.Print("Sensor: " + sensorID + ", avg. dist: " + averagedDistance.ToString() + ", voltage: " + mappedVoltage.ToString());
        }
    }
}
