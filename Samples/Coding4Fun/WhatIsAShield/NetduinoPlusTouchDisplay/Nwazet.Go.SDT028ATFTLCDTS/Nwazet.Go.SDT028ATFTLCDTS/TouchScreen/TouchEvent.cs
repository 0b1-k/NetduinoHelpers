using Microsoft.SPOT;
using Nwazet.Go.Imaging;
namespace Nwazet.Go.Display.TouchScreen {
    public delegate void TouchEventHandler(VirtualCanvas canvas, TouchEvent touchEvent);

    public class TouchEvent : EventArgs {
        public ushort X;
        public ushort Y;
        public uint Pressure;
        public byte IsValid;
    }
}
