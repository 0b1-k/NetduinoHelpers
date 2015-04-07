using System;
using Microsoft.SPOT;

namespace netduino.helpers.Hardware {
    // Based on MicroBuilder's code: http://www.microbuilder.eu/Projects/LPC1343ReferenceDesign/TFTLCDAPI.aspx
    public class LcdProperties {
        public LcdProperties(ushort width, ushort height, bool supportTouch, bool supportHardwareScrolling, bool supportOrientation) {
            Width = width;
            Height = height;
            Touchscreen = supportTouch;
            HwScrolling = supportHardwareScrolling;
            Orientation = supportOrientation;
        }
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public bool Touchscreen { get; set; }
        public bool HwScrolling { get; set; }
        public bool Orientation { get; set; }
    }
    public abstract class LCD {
        public enum Orientation {
            Portrait,
            Landscape
        }
        abstract public ushort Width { get; set; }
        abstract public ushort Height { get; set; }
        abstract public void Initialize();
        abstract public void Reset();
        abstract public void Test();
        abstract public void GetPixel(ushort x, ushort y);
        abstract public void FillRGB(ushort data);
        abstract public void DrawPixel(ushort x, ushort y, ushort color);
        abstract public void DrawPixels(ushort x, ushort y, ushort[] data, int length = 0);
        abstract public void DrawHLine(ushort x0, ushort x1, ushort y, ushort color);
        abstract public void DrawVLine(ushort x, ushort y0, ushort y1, ushort color);
        abstract public void BackLight(uint dutyCycle);
        abstract public void Scroll(ushort pixels, ushort fillColor);
        abstract public void SetOrientation(Orientation orientation);
        abstract public ushort GetControllerID();
        abstract public LcdProperties GetProperties();
        // Protected functions
        abstract protected void Home();
        abstract protected void SetCursor(ushort x, ushort y);
        abstract protected void SetWindow(ushort x0, ushort y0, ushort x1, ushort y1);
    }
}
