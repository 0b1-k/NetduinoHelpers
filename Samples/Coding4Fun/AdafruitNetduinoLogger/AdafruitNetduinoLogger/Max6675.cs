using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Maxim.Temperature {
    // Driver for the Max6675 cold-junction compensated K-Type thermocouple digital converter: http://www.maxim-ic.com/datasheet/index.mvp/id/3149
    // Author: Fabien Royer | [Nwazet, LLC. | http://nwazet.com | See license.txt for terms of use.
    public class Max6675 : IDisposable {
        protected SPI Spi;
        public void Initialize(Cpu.Pin chipSelect) {
            Spi = new SPI(new SPI.Configuration(chipSelect, false, 1, 0, false, true, 2000, SPI.SPI_module.SPI1));
        }
        public double Celsius {
            get {
                return RawSensorValue * 0.25;
            }
        }
        public double Farenheit {
            get {
                return ((Celsius * 9.0) / 5.0) + 32;
            }
        }
        protected UInt16 RawSensorValue;
        protected byte[] ReadBuffer = new byte[2];
        protected byte[] WriteBuffer = new byte[2];
        public void Read() {
            RawSensorValue = 0;
            Spi.WriteRead(WriteBuffer, ReadBuffer);
            RawSensorValue |= ReadBuffer[0];
            RawSensorValue <<= 8;
            RawSensorValue |= ReadBuffer[1];
            if ((RawSensorValue & 0x4) == 1) {
                throw new ApplicationException("No thermocouple attached.");
            }
            RawSensorValue >>= 3;
        }
        public void Dispose() {
            Spi.Dispose();
        }
        ~Max6675() {
            Dispose();
        }
    }
}
