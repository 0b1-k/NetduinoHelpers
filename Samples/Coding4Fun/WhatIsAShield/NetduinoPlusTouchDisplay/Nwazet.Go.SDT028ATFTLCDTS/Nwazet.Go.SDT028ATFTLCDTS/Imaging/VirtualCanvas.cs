using System;
using System.IO;
using System.Threading;
using System.Collections;
using Nwazet.Go.Helpers;
using Nwazet.Go.Display.TouchScreen;
using Nwazet.BmpImage;
using Microsoft.SPOT.Hardware;

namespace Nwazet.Go.Imaging {
    public class VirtualCanvas : IDisposable {
        protected enum Command {
            DrawTestPattern = 'A',
            DrawPixel = 'B',
            DrawFill = 'C',
            DrawLine = 'D',
            DrawLineDotted = 'E',
            DrawCircle = 'F',
            DrawCircleFilled = 'G',
            DrawCornerFilled = 'H',
            DrawArrow = 'I',
            DrawRectangle = 'J',
            DrawRectangleFilled = 'K',
            DrawRectangleRounded = 'L',
            DrawTriangle = 'M',
            DrawTriangleFilled = 'N',
            DrawProgressBar = 'O',
            DrawButton = 'P',
            DrawIcon16 = 'Q',
            DrawString = 'R',
            DrawImageInitialize = 'S',
            DrawImageData = 'T',
            SetOrientation = 'U',
            Synchronicity = 'V',
            TouchscreenCalibration = 'W',
            TouchscreenShowDialog = 'X',
            TouchscreenWaitForEvent = 'Y',
            Reboot = 'Z',
            TouchscreenGetCalibrationMatrix = 'a',
            TouchscreenSetCalibrationMatrix = 'b'
        }

        protected enum TouchScreenDataType {
            String,
            TouchEvent,
            CalibrationMatrix
        }

        public int Width { get; set; }
        public int Height { get; set; }

        public const int MaxSpiTxBufferSize = 1024 * 8;
        public const int SpiTxBufferHighWatermark = MaxSpiTxBufferSize - 64;
        public const int MaxSpiRxBufferSize = 1024 * 8;

        protected BasicTypeSerializerContext SendContext;
        protected BasicTypeDeSerializerContext ReceiveContext;
        protected SPI Spi;
        protected InterruptPort GoBusIrqPort;
        protected ManualResetEvent GoBusIrqEvent;
        private byte[] _spiRxBuffer;
        private bool _moduleReady;
        
        public ArrayList RegisteredWidgets;

        public event TouchEventHandler Touch;
        public event WidgetClickedHandler WidgetClicked;

        public VirtualCanvas() : this(null, null) {}
        public VirtualCanvas(TouchEventHandler touchEventHandler, WidgetClickedHandler widgetClickedHandler) {
            TrackOrientation(Orientation.Portrait);
            _spiRxBuffer = new byte[MaxSpiRxBufferSize];
            SendContext = new BasicTypeSerializerContext(MaxSpiTxBufferSize, SpiTxBufferHighWatermark, OnCanvasBufferNearlyFull);
            ReceiveContext = new BasicTypeDeSerializerContext();
            GoBusIrqEvent = new ManualResetEvent(false);
            RegisteredWidgets = new ArrayList();
            if (widgetClickedHandler != null) {
                WidgetClicked += widgetClickedHandler;
            }
            if (touchEventHandler != null) {
                Touch += touchEventHandler;
            }
        }
        ~VirtualCanvas() {
            Dispose();
        }
        public void Initialize(SPI.SPI_module displaySpi, Cpu.Pin displayChipSelect, Cpu.Pin displayGPIO, uint speedKHz = 25000) {
            if (speedKHz < 5000 || speedKHz > 40000) throw new ArgumentException("speedKHz");

            Spi = new SPI(new SPI.Configuration(
                SPI_mod: displaySpi,
                ChipSelect_Port: displayChipSelect,
                ChipSelect_ActiveState: false,
                ChipSelect_SetupTime: 0,
                ChipSelect_HoldTime: 0,
                Clock_IdleState: false,
                Clock_Edge: false,
                Clock_RateKHz: speedKHz));

            GoBusIrqPort = new InterruptPort(displayGPIO, false, Port.ResistorMode.PullUp, Port.InterruptMode.InterruptEdgeLow);
            GoBusIrqPort.OnInterrupt += OnGoBusIrq;

            WaitUntilModuleIsInitialized();
        }
        protected void OnCanvasBufferNearlyFull() {
            Execute();
        }
        public void RegisterWidget(Widget widget) {
            if (RegisteredWidgets.Contains(widget)) return;
            RegisteredWidgets.Add(widget);
        }
        public void UnRegisterWidget(Widget widget) {
            if (!RegisteredWidgets.Contains(widget)) return;
            RegisteredWidgets.Remove(widget);
        }
        public void UnRegisterAllWidgets() {
            RegisteredWidgets.Clear();
        }
        public void RenderWidgets(Render renderOption = Render.Dirty) {
            if (renderOption == Render.All) {
                foreach (Widget widget in RegisteredWidgets) {
                    widget.Dirty = true;
                    widget.Draw(this);
                }
            } else {
                foreach (Widget widget in RegisteredWidgets) {
                    widget.Draw(this);
                }
            }
        }
        public void ActivateWidgets(bool active) {
            foreach (Widget widget in RegisteredWidgets) {
                widget.Active = active;
            }
        }
        public void Dispose() {
            Spi.Dispose();
            GoBusIrqPort.Dispose();
            SendContext.Dispose();
            ReceiveContext.Dispose();
            _spiRxBuffer = null;
            WidgetClicked = null;
            Touch = null;
            RegisteredWidgets = null;
            GoBusIrqEvent = null;
        }
        private void OnGoBusIrq(UInt32 data1, UInt32 data2, DateTime timestamp) {
            GoBusIrqPort.ClearInterrupt();
            GoBusIrqEvent.Set();
        }
        public void Execute(Synchronicity sync = Synchronicity.Synchronous) {
            SetSynchronicity(sync);
            int contentSize;
            var spiTxBuffer = SendContext.GetBuffer(out contentSize);
            if (contentSize >= MaxSpiTxBufferSize) {
                throw new ApplicationException("contentSize");
            }
            GoBusIrqEvent.Reset();
            Spi.WriteRead(spiTxBuffer, 0, MaxSpiTxBufferSize, _spiRxBuffer, 0, MaxSpiRxBufferSize, 0);
            if (sync == Synchronicity.Synchronous) {
                WaitUntilGoBusIrqIsAsserted();
            }
        }
        private void Receive() {
            Execute();
            ReceiveContext.Bind(_spiRxBuffer, BasicTypeDeSerializerContext.BufferStartOffsetDefault);
        }
        protected void WaitUntilGoBusIrqIsAsserted() {
            GoBusIrqEvent.WaitOne();
        }
        private const byte _identifier8bitCrc = 54;
        protected void WaitUntilModuleIsInitialized() {
            while (!_moduleReady) {
                Execute(Synchronicity.Asynchronous);
                if (_spiRxBuffer[0] == 0x80 && 
                    _spiRxBuffer[1] == '[' && 
                    _spiRxBuffer[2] == 'n' && 
                    _spiRxBuffer[3] == 'w' && 
                    _spiRxBuffer[4] == 'a' && 
                    _spiRxBuffer[5] == 'z' &&
                    _spiRxBuffer[6] == 'e' &&
                    _spiRxBuffer[7] == 't' &&
                    _spiRxBuffer[8] == '.' &&
                    _spiRxBuffer[9] == 'd' &&
                    _spiRxBuffer[10] == 'i' &&
                    _spiRxBuffer[11] == 's' &&
                    _spiRxBuffer[12] == 'p') {
                    if (_spiRxBuffer[17] != _identifier8bitCrc) throw new ApplicationException("SPI data corruption");
                    _moduleReady = true;
                    return;
                }
                Thread.Sleep(200);
            }
        }
        public void DrawTestPattern() {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawTestPattern);
            SendContext.CheckHighWatermark();
        }
        public void DrawPixel(int x, int y, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawPixel);
            BasicTypeSerializer.Put(SendContext,(ushort)x);
            BasicTypeSerializer.Put(SendContext,(ushort)y);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawFill(ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawFill);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawLine(int x0, int y0, int x1, int y1, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawLine);
            BasicTypeSerializer.Put(SendContext,(ushort)x0);
            BasicTypeSerializer.Put(SendContext,(ushort)y0);
            BasicTypeSerializer.Put(SendContext,(ushort)x1);
            BasicTypeSerializer.Put(SendContext,(ushort)y1);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawLineDotted(int x0, int y0, int x1, int y1, int empty, int solid, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawLineDotted);
            BasicTypeSerializer.Put(SendContext,(ushort)x0);
            BasicTypeSerializer.Put(SendContext,(ushort)y0);
            BasicTypeSerializer.Put(SendContext,(ushort)x1);
            BasicTypeSerializer.Put(SendContext,(ushort)y1);
            BasicTypeSerializer.Put(SendContext,(ushort)empty);
            BasicTypeSerializer.Put(SendContext,(ushort)solid);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawCircle(int xCenter, int yCenter, int radius, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawCircle);
            BasicTypeSerializer.Put(SendContext,(ushort)xCenter);
            BasicTypeSerializer.Put(SendContext,(ushort)yCenter);
            BasicTypeSerializer.Put(SendContext,(ushort)radius);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawCircleFilled(int xCenter, int yCenter, int radius, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawCircleFilled);
            BasicTypeSerializer.Put(SendContext,(ushort)xCenter);
            BasicTypeSerializer.Put(SendContext,(ushort)yCenter);
            BasicTypeSerializer.Put(SendContext,(ushort)radius);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawCornerFilled(int xCenter, int yCenter, int radius, CornerPosition position, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawCornerFilled);
            BasicTypeSerializer.Put(SendContext,(ushort)xCenter);
            BasicTypeSerializer.Put(SendContext,(ushort)yCenter);
            BasicTypeSerializer.Put(SendContext,(ushort)radius);
            BasicTypeSerializer.Put(SendContext,(ushort)position);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawArrow(int x, int y, int size, DrawingDirection direction, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawArrow);
            BasicTypeSerializer.Put(SendContext,(ushort)x);
            BasicTypeSerializer.Put(SendContext,(ushort)y);
            BasicTypeSerializer.Put(SendContext,(ushort)size);
            BasicTypeSerializer.Put(SendContext,(ushort)direction);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawRectangle(int x0, int y0, int x1, int y1, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawRectangle);
            BasicTypeSerializer.Put(SendContext,(ushort)x0);
            BasicTypeSerializer.Put(SendContext,(ushort)y0);
            BasicTypeSerializer.Put(SendContext,(ushort)x1);
            BasicTypeSerializer.Put(SendContext,(ushort)y1);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawRectangleFilled(int x0, int y0, int x1, int y1, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawRectangleFilled);
            BasicTypeSerializer.Put(SendContext,(ushort)x0);
            BasicTypeSerializer.Put(SendContext,(ushort)y0);
            BasicTypeSerializer.Put(SendContext,(ushort)x1);
            BasicTypeSerializer.Put(SendContext,(ushort)y1);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawRectangleRounded(int x0, int y0, int x1, int y1, ushort color, int radius, RoundedCornerStyle corners) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawRectangleRounded);
            BasicTypeSerializer.Put(SendContext,(ushort)x0);
            BasicTypeSerializer.Put(SendContext,(ushort)y0);
            BasicTypeSerializer.Put(SendContext,(ushort)x1);
            BasicTypeSerializer.Put(SendContext,(ushort)y1);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            BasicTypeSerializer.Put(SendContext,(ushort)radius);
            BasicTypeSerializer.Put(SendContext,(ushort)corners);
            SendContext.CheckHighWatermark();
        }
        public void DrawTriangle(int x0, int y0, int x1, int y1, int x2, int y2, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawTriangle);
            BasicTypeSerializer.Put(SendContext,(ushort)x0);
            BasicTypeSerializer.Put(SendContext,(ushort)y0);
            BasicTypeSerializer.Put(SendContext,(ushort)x1);
            BasicTypeSerializer.Put(SendContext,(ushort)y1);
            BasicTypeSerializer.Put(SendContext,(ushort)x2);
            BasicTypeSerializer.Put(SendContext,(ushort)y2);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawTriangleFilled(int x0, int y0, int x1, int y1, int x2, int y2, ushort color) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawTriangleFilled);
            BasicTypeSerializer.Put(SendContext,(ushort)x0);
            BasicTypeSerializer.Put(SendContext,(ushort)y0);
            BasicTypeSerializer.Put(SendContext,(ushort)x1);
            BasicTypeSerializer.Put(SendContext,(ushort)y1);
            BasicTypeSerializer.Put(SendContext,(ushort)x2);
            BasicTypeSerializer.Put(SendContext,(ushort)y2);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            SendContext.CheckHighWatermark();
        }
        public void DrawProgressBar(
            int x, int y,
            int width, int height,
            RoundedCornerStyle borderCorners,
            RoundedCornerStyle progressCorners,
            ushort borderColor, ushort borderFillColor,
            ushort progressBorderColor, ushort progressFillColor,
            int progress) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawProgressBar);
            BasicTypeSerializer.Put(SendContext,(ushort)x);
            BasicTypeSerializer.Put(SendContext,(ushort)y);
            BasicTypeSerializer.Put(SendContext,(ushort)width);
            BasicTypeSerializer.Put(SendContext,(ushort)height);
            BasicTypeSerializer.Put(SendContext,(ushort)borderCorners);
            BasicTypeSerializer.Put(SendContext,(ushort)progressCorners);
            BasicTypeSerializer.Put(SendContext,(ushort)borderColor);
            BasicTypeSerializer.Put(SendContext,(ushort)borderFillColor);
            BasicTypeSerializer.Put(SendContext,(ushort)progressBorderColor);
            BasicTypeSerializer.Put(SendContext,(ushort)progressFillColor);
            BasicTypeSerializer.Put(SendContext,(ushort)progress);
            SendContext.CheckHighWatermark();
        }
        public void DrawButton(
            int x, int y,
            int width, int height,
            ushort fontID,
            int fontHeight,
            ushort borderColor,
            ushort fillColor,
            ushort fontColor,
            string text,
            RoundedCornerStyle cornerStyle = RoundedCornerStyle.All) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawButton);
            BasicTypeSerializer.Put(SendContext,(ushort)x);
            BasicTypeSerializer.Put(SendContext,(ushort)y);
            BasicTypeSerializer.Put(SendContext,(ushort)width);
            BasicTypeSerializer.Put(SendContext,(ushort)height);
            BasicTypeSerializer.Put(SendContext,(ushort)fontID);
            BasicTypeSerializer.Put(SendContext,(ushort)fontHeight);
            BasicTypeSerializer.Put(SendContext,(ushort)borderColor);
            BasicTypeSerializer.Put(SendContext,(ushort)fillColor);
            BasicTypeSerializer.Put(SendContext,(ushort)fontColor);
            BasicTypeSerializer.Put(SendContext,text, true);
            BasicTypeSerializer.Put(SendContext,(ushort)cornerStyle);
            SendContext.CheckHighWatermark();
        }
        public void DrawIcon16(int x, int y, ushort color, ushort[] icon) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawIcon16);
            BasicTypeSerializer.Put(SendContext,(ushort)x);
            BasicTypeSerializer.Put(SendContext,(ushort)y);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            BasicTypeSerializer.Put(SendContext,icon);
            SendContext.CheckHighWatermark();
        }
        public void DrawString(int x, int y, ushort color, ushort fontID, string text) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.DrawString);
            BasicTypeSerializer.Put(SendContext,(ushort)x);
            BasicTypeSerializer.Put(SendContext,(ushort)y);
            BasicTypeSerializer.Put(SendContext,(ushort)color);
            BasicTypeSerializer.Put(SendContext,(ushort)fontID);
            BasicTypeSerializer.Put(SendContext,text, true);
            SendContext.CheckHighWatermark();
        }
        
        private const int _maxImageChunkSize = MaxSpiTxBufferSize - 128;

        protected enum BytesPerPixel {
            Two = 2,
            Three = 3
        }

        public void DrawBitmapImage(int x, int y, string filename) {
            using (var bmpStream = new FileStream(filename, FileMode.Open)) {
                var bmpImageInfo = BmpImageHeaderReader.Read(bmpStream);
                if (bmpImageInfo.Width > Width || bmpImageInfo.Height > Height) throw new ArgumentOutOfRangeException("Dimensions");
                var imageChunk = new byte[_maxImageChunkSize];
                DrawImageInitialize(x, y, bmpImageInfo.Width, bmpImageInfo.Height, BytesPerPixel.Three);
                var offset = bmpStream.Length;
                var bytesLeft = (long)bmpImageInfo.ImageSizeInBytes;
                var bmpImageRowLengthInBytes = bmpImageInfo.Width * (int)BytesPerPixel.Three;
                var maxBmpImageChunkSize = (_maxImageChunkSize / bmpImageRowLengthInBytes) * bmpImageRowLengthInBytes;
                while (bytesLeft != 0) {
                    var bytesInChunk = (int)Math.Min(bytesLeft, maxBmpImageChunkSize);
                    offset -= bytesInChunk;
                    bytesLeft -= bytesInChunk;
                    bmpStream.Seek(offset, SeekOrigin.Begin);
                    var bytesRead = bmpStream.Read(imageChunk, 0, bytesInChunk);
                    TransferImage(imageChunk, bytesRead);
                }
            }
        }
        public void DrawBitmapImageRaw(int x, int y, int width, int height, byte[] bytes) {
            DrawImageInitialize(x, y, width, height, BytesPerPixel.Two);
            TransferImage(bytes, bytes.Length);
        }
        protected void TransferImage(byte[] bytes, int imageSize) {
            ushort offset = 0;
            var maxImageChunkSize = _maxImageChunkSize;
            while (offset < imageSize) {
                var count = (ushort)Math.Min(maxImageChunkSize, imageSize - offset);
                DrawImageData(bytes, offset, count);
                Execute();
                offset += count;
            }
        }
        protected void DrawImageInitialize(int x, int y, int width, int height, BytesPerPixel bytesPerPixel) {
            BasicTypeSerializer.Put(SendContext, (byte)Command.DrawImageInitialize);
            BasicTypeSerializer.Put(SendContext, (ushort)x);
            BasicTypeSerializer.Put(SendContext, (ushort)y);
            BasicTypeSerializer.Put(SendContext, (ushort)width);
            BasicTypeSerializer.Put(SendContext, (ushort)height);
            BasicTypeSerializer.Put(SendContext, (ushort)bytesPerPixel);
        }
        protected void DrawImageData(byte[] bytes, ushort offset, ushort count) {
            BasicTypeSerializer.Put(SendContext, (byte)Command.DrawImageData);
            BasicTypeSerializer.Put(SendContext, bytes, offset, count);
        }
        public void SetOrientation(Orientation orientation) {
            BasicTypeSerializer.Put(SendContext,(byte)Command.SetOrientation);
            BasicTypeSerializer.Put(SendContext,(ushort)orientation);
            SendContext.CheckHighWatermark();
            TrackOrientation(orientation);
        }
        private void TrackOrientation(Orientation orientation) {
            if (orientation == Orientation.Portrait) {
                Width = 240;
                Height = 320;
            } else {
                Height = 240;
                Width = 320;
            }
        }
        protected void SetSynchronicity(Synchronicity sync) {
            BasicTypeSerializer.Put(SendContext, (byte)Command.Synchronicity);
            BasicTypeSerializer.Put(SendContext, (byte)sync);
        }
        public void TouchscreenCalibration() {
            BasicTypeSerializer.Put(SendContext, (byte)Command.TouchscreenCalibration);
            Execute();
        }
        public string TouchscreenShowDialog(DialogType dialogType) {
            BasicTypeSerializer.Put(SendContext, (byte)Command.TouchscreenShowDialog);
            BasicTypeSerializer.Put(SendContext, (ushort)dialogType);
            Execute();
            Receive();
            TouchScreenDataType eventType = (TouchScreenDataType) BasicTypeDeSerializer.Get(ReceiveContext);
            if (eventType != TouchScreenDataType.String) {
                throw new ApplicationException("eventType");
            }
            return BasicTypeDeSerializer.Get(ReceiveContext, "");
        }
        public void TouchscreenWaitForEvent(TouchScreenEventMode mode = TouchScreenEventMode.Blocking) {
            BasicTypeSerializer.Put(SendContext, (byte)Command.TouchscreenWaitForEvent);
            BasicTypeSerializer.Put(SendContext, (byte)mode);
            Execute();
            Receive();
            TouchScreenDataType eventType = (TouchScreenDataType)BasicTypeDeSerializer.Get(ReceiveContext);
            if (eventType != TouchScreenDataType.TouchEvent) {
                throw new ApplicationException("eventType");
            }
            var touchEvent = new TouchEvent();
            touchEvent.X = BasicTypeDeSerializer.Get(ReceiveContext, touchEvent.X);
            touchEvent.Y = BasicTypeDeSerializer.Get(ReceiveContext, touchEvent.Y);
            touchEvent.Pressure = BasicTypeDeSerializer.Get(ReceiveContext, touchEvent.Pressure);
            touchEvent.IsValid = BasicTypeDeSerializer.Get(ReceiveContext);
            OnTouch(touchEvent);
            if (WidgetClicked != null && touchEvent.IsValid != 0) {
                foreach (Widget widget in RegisteredWidgets) {
                    widget.OnClickEvent(touchEvent);
                    if (widget.Clicked) {
                        WidgetClicked(this, widget, touchEvent);
                    }
                }
            }
        }
        protected void OnWidgetClicked(Widget widget, TouchEvent touchEvent) {
            if (WidgetClicked != null) {
                WidgetClicked(this, widget, touchEvent);
            }
        }
        protected void OnTouch(TouchEvent touchEvent) {
            if (Touch != null) {
                Touch(this, touchEvent);
            }
        }
        public void Reboot() {
            BasicTypeSerializer.Put(SendContext, (byte)Command.Reboot);
            Execute(Synchronicity.Asynchronous);
        }
        public CalibrationMatrix GetTouchscreenCalibrationMatrix() {
            BasicTypeSerializer.Put(SendContext, (byte)Command.TouchscreenGetCalibrationMatrix);
            Execute();
            Receive();
            TouchScreenDataType eventType = (TouchScreenDataType)BasicTypeDeSerializer.Get(ReceiveContext);
            if (eventType != TouchScreenDataType.CalibrationMatrix) {
                throw new ApplicationException("eventType");
            }
            var matrix = new CalibrationMatrix();
            matrix.Get(ReceiveContext);
            return matrix;
        }
        public void SetTouchscreenCalibrationMatrix(CalibrationMatrix matrix) {
            BasicTypeSerializer.Put(SendContext, (byte)Command.TouchscreenSetCalibrationMatrix);
            matrix.Put(SendContext);
            Execute();
        }
    }
}
