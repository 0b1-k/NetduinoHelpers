using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace netduino.helpers.Hardware {
    // Based on https://github.com/adafruit/ST7565-LCD
    public class AdaFruit7565 : AdaFruitSSD1306 {
        new public enum Command {
            DisplayOff = 0xAE,
            DisplayOn = 0xAF,
            DisplayStartLine = 0x40,
            PageAddress = 0xB0,
            ColumnAddressHigh = 0x10,
            ColumnAddressLow = 0x00,
            AdcSelectNormal = 0xA0, // X axis flip OFF
            AdcSelectReverse = 0xA1, // X axis flip ON
            DisplayVideoNormal = 0xA6,
            DisplayVideoReverse = 0xA7,
            AllPixelsOff = 0xA4,
            AllPixelsOn = 0xA5,
            LcdVoltageBias9 = 0xA2,
            LcdVoltageBias7 = 0xA3,
            EnterReadModifyWriteMode = 0xE0,
            ClearReadModifyWriteMode = 0xEE,
            ResetLcdModule = 0xE2,
            ShlSelectNormal = 0xC0, // Y axis flip OFF
            ShlSelectReverse = 0xC8, // Y axis flip ON
            PowerControl = 0x28,
            RegulatorResistorRatio = 0x20,
            ContrastRegister = 0x81,
            ContrastValue = 0x00,
            NoOperation = 0xE3
        }

        public AdaFruit7565(Cpu.Pin dc, Cpu.Pin reset, Cpu.Pin chipSelect, SPI.SPI_module spiModule = SPI.SPI_module.SPI1, uint speedKHz = 10000)
            : base(dc, reset, chipSelect, spiModule, speedKHz) {
        }

        public void Initialize() {
            resetPin.Write(false);
            Thread.Sleep(50);
            resetPin.Write(true);

            dcPin.Write(DisplayCommand);

            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.LcdVoltageBias7);
            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.AdcSelectNormal);
            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.ShlSelectReverse);
            SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.DisplayStartLine) | 0x00));
            
            SendCommand((AdaFruitSSD1306.Command) ((int)(AdaFruit7565.Command.PowerControl) | 0x04)); // turn on voltage converter (VC=1, VR=0, VF=0)
            Thread.Sleep(50);
            SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.PowerControl) | 0x06)); // turn on voltage regulator (VC=1, VR=1, VF=0)
            Thread.Sleep(50);
            SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.PowerControl) | 0x07)); // turn on voltage follower (VC=1, VR=1, VF=1)
            Thread.Sleep(50);

            SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.RegulatorResistorRatio) | 0x06)); // set lcd operating voltage (regulator resistor, ref voltage resistor)

            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.DisplayOn);
            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.AllPixelsOff);
        }

        public const uint ContrastHigh = 34;
        public const uint ContrastMedium = 24;
        public const uint ContrastLow = 15;

        // 0-63
        public void SetContrast(uint contrast) {
            dcPin.Write(DisplayCommand);
            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.ContrastRegister);
            SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.ContrastValue) | (contrast & 0x3f)));
        }

        new public void InvertDisplay(bool cmd) {
            dcPin.Write(DisplayCommand);
            if (cmd) {
                SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.DisplayVideoReverse);
            } else {
                SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.DisplayVideoNormal);
            }
        }

        public void PowerSaveMode() {
            dcPin.Write(DisplayCommand);
            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.DisplayOff);
            SendCommand((AdaFruitSSD1306.Command)AdaFruit7565.Command.AllPixelsOn);
        }

        protected const int StartColumnOffset = 1;

        public override void Refresh() {
            for (int page = 0; page < 8; page++) {
                dcPin.Write(DisplayCommand);
                SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.PageAddress) | pageReference[page]));
                SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.ColumnAddressLow) | (StartColumnOffset & 0x0F)));
                SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.ColumnAddressHigh) | 0));
                SendCommand((AdaFruitSSD1306.Command)((int)(AdaFruit7565.Command.EnterReadModifyWriteMode)));
                dcPin.Write(Data);
                Array.Copy(displayBuffer, Width * page, pageBuffer, 0, pageSize);
                Spi.Write(pageBuffer);
            }
        }

        protected const int pageSize = 128;
        protected int[] pageReference = new int[] { 4, 5, 6, 7, 0, 1, 2, 3 };
        protected byte[] pageBuffer = new byte[pageSize];
    }
}
