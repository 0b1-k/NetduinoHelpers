using System;
using SecretLabs.NETMF.IO;
using Microsoft.SPOT.Hardware;

namespace Nwazet.Go.SD {
    public class SDCardReader : IDisposable {
        private string _mountPoint;
        public void Initialize(SPI.SPI_module sdCardSpi, Cpu.Pin sdCardChipSelect, string mountPoint = "SD") {
            _mountPoint = mountPoint;
            StorageDevice.MountSD(_mountPoint, sdCardSpi, sdCardChipSelect);
        }
        public void Dispose() {
            StorageDevice.Unmount(_mountPoint);
        }
        ~SDCardReader() {
            Dispose();
        }
    }
}
