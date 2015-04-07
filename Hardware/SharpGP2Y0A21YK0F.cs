using System;
using System.Threading;
using Microsoft.SPOT.Hardware;
using SecretLabs.NETMF.Hardware;

namespace netduino.helpers.Hardware {
    public delegate void VoltageTriggerCallback(int sensorID, int averagedDistance, float mappedVoltage);

    // Datasheet: http://sharp-world.com/products/device/lineup/data/pdf/datasheet/gp2y0a21yk_e.pdf
    public class SharpGP2Y0A21YK0F : IDisposable {
        protected AnalogInput[] DistanceSensors;
        protected VoltageTriggerCallback VoltageTriggerProcedure;
        protected Thread DistanceSampler;
        protected bool DistanceSamplerStop;

        private float _voltageThreshold = 1.09f;
        public float VoltageThreshold { 
            get { return _voltageThreshold; } 
            set { if (value < 0.25 || value > 3.5) throw new ArgumentOutOfRangeException("value");
            _voltageThreshold = value;
            } 
        }

        private int _averageMeasurementCount = 2;
        public int AverageMeasurementCount { 
            get { return _averageMeasurementCount; } 
            set { if (value == 0) throw new ArgumentOutOfRangeException("value");
            _averageMeasurementCount = value;
            } 
        }

        public SharpGP2Y0A21YK0F(Cpu.Pin[] analogPins) {
            var sensorId = 0;
            DistanceSensors = new AnalogInput[analogPins.Length];
            foreach (var analogPin in analogPins) {
                DistanceSensors[sensorId] = new AnalogInput(analogPin);
                DistanceSensors[sensorId].SetRange(70, 970);
                sensorId++;
            }
        }

        public void Start(VoltageTriggerCallback callback) {
            if (DistanceSampler == null) {
                VoltageTriggerProcedure = callback;
                DistanceSamplerStop = false;
                DistanceSampler = new Thread(DistanceSampling);
                DistanceSampler.Start();
            }
        }

        public void Stop() {
            if (DistanceSampler != null) {
                DistanceSamplerStop = true;
                DistanceSampler.Join();
                DistanceSampler = null;
            }
        }

        protected void DistanceSampling() {
            while (!DistanceSamplerStop) {
                for (var sensorId = 0; sensorId < DistanceSensors.Length; sensorId++ ) {
                    var averagedDistance = ReadAverageDistance(DistanceSensors[sensorId]);
                    var mappedVoltage = MapRange(970f, 70f, 3.3f, 0.4f, averagedDistance);
                    if (mappedVoltage >= VoltageThreshold) VoltageTriggerProcedure(sensorId, averagedDistance, mappedVoltage);
                }
            }
        }

        protected int ReadAverageDistance(AnalogInput analogInput) {
            var count = AverageMeasurementCount;
            var total = 0;
            while (--count >= 0) {
                total += analogInput.Read();
            }
            return total / AverageMeasurementCount;
        }

        // Maps a range of values to another http://rosettacode.org/wiki/Map_range#C
        public float MapRange(float a1, float a2, float b1, float b2, float s) {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }

        public void Dispose() {
            Stop();
            DistanceSensors = null;
            VoltageTriggerProcedure = null;
        }
    }
}
