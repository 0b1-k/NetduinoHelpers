using System;

namespace netduino.helpers.Imaging {
    /// <summary>
    /// Abstracts a 1 bit depth bitmap expressed as a flat array of 8 bit values
    /// </summary>
    public class Bitmap {
        public static readonly int FrameSize = 8;
        public static readonly byte[] ShiftMasks =
            new byte[] { 128, 64, 32, 16, 8, 4, 2, 1 };
        public static readonly byte[] ReverseShiftMasks =
            new byte[] { 0x7F, 0xBF, 0xDF, 0xEF, 0xF7, 0xFB, 0xFD, 0xFE };

        private readonly byte[] _frame = new byte[FrameSize];

        /// <summary>
        /// Bitmap data in hex.
        /// </summary>
        protected byte[] BitmapData;

        public Bitmap(byte[] data, int widthInPixels, int heightInPixels) {
            if ((heightInPixels % 8) != 0 || (widthInPixels % 8) != 0) {
                throw new ArgumentException("height and width must be multiples of 8");
            }
            if (data.Length != heightInPixels * widthInPixels / 8) {
                throw new ArgumentException("Data must be consistent with width and height.", "data");
            }

            WidthModuloSize = widthInPixels >> 3;
            HeightModuloSize = heightInPixels >> 3;
            BitmapData = data;
        }

        protected int HeightModuloSize { get; set; }
        protected int WidthModuloSize { get; set; }

        /// <summary>
        /// Width of the bitmap in pixels
        /// </summary>
        public int Width {
            get {
                return WidthModuloSize << 3;
            }
        }

        /// <summary>
        /// Height of the bitmap in pixels
        /// </summary>
        public int Height {
            get {
                return HeightModuloSize << 3;
            }
        }

        public bool this[int x, int y] {
            get { return GetPixel(x, y); }
            set { SetPixel(x, y, value); }
        }

        public bool GetPixel(int x, int y) {
            if (x >= Width || y >= Height || x < 0 || y < 0) return false;
            var xOffset = x % FrameSize;
            var index = y * WidthModuloSize + (x / FrameSize);
            return (BitmapData[index] & ShiftMasks[xOffset]) != 0;
        }

        public void SetPixel(int x, int y, bool value) {
            if (x >= Width || y >= Height || x < 0 || y < 0) return;
            var xOffset = x % FrameSize;
            var index = y * WidthModuloSize + (x / FrameSize);
            if (value) {
                BitmapData[index] |= ShiftMasks[xOffset];
            }
            else {
                BitmapData[index] &= ReverseShiftMasks[xOffset];
            }
        }

        /// <summary>
        /// Takes x and y coordinates in pixels and returns a corresponding 8*8 frame
        /// </summary>
        /// <returns>An 8*8 frame, whose upper left corner is x and y</returns>
        public byte[] GetFrame(int x, int y) {
            // Reset frame data
            for (var i = 0; i < FrameSize; i++) _frame[i] = 0;
            var bitmapX = x / FrameSize; // Divide x by frameSize to determine where the x coordinate lands in the bitmap
            var xOffset = x % FrameSize; // Determine the amount of horizontal scrolling required to show the frame at this position
            var endLine = (y + FrameSize); // determine the ending line in the bitmap to create the final frame
            for (int line = y, frameLine = 0, index = bitmapX + y * WidthModuloSize;
                 line < endLine;
                 line++, frameLine++, index += WidthModuloSize) { // Build the frame one line at a time
                if (line < 0 || line >= Height) {
                    _frame[frameLine] = 0x00;
                } else if (xOffset == 0) {
                    if (x >= 0 && x + FrameSize <= Width) {
                        // if no scrolling is required, stored the graphics as-is
                        _frame[frameLine] = BitmapData[index];
                    }
                } else {
                    // we need to merge / scroll two graphics to make one line
                    byte merged = 0;
                    if (x >= 0 && x < Width) {
                        merged = BitmapData[index];
                        merged <<= (byte) (xOffset);
                    }
                    byte neighbor = 0;
                    if (x + FrameSize >= 0 && x + FrameSize < Width) {
                        neighbor = BitmapData[index + 1];
                        neighbor >>= (byte) (FrameSize - xOffset);
                    }
                    merged |= neighbor;
                    _frame[frameLine] = merged;
                }
            }
            return _frame;
        }

        public byte[] GetToricFrame(int x, int y) {
            x %= Width;
            y %= Height;
            var xOffset = x % FrameSize; // Determine the amount of horizontal scrolling required to show the frame at this position
            var bitmapX = x / FrameSize; // Divide x by frameSize to determine where the x coordinate lands in the bitmap
            for (int line = y,
                     frameLine = 0,
                     index = bitmapX + y * WidthModuloSize;
                 frameLine < FrameSize;
                 line = (line + 1) % Height,
                 frameLine++,
                 index = (index + WidthModuloSize) % BitmapData.Length) { // Build the frame one line at a time
                if (xOffset == 0) {
                    _frame[frameLine] = BitmapData[index];
                } else {
                    // we need to merge / scroll two graphics to make one line
                    var merged = (byte)(BitmapData[index] << (byte)(xOffset));
                    merged |= (byte)((x + FrameSize >= Width ?
                        BitmapData[index - WidthModuloSize + 1] :
                        BitmapData[index + 1]) >> (byte)(FrameSize - xOffset));
                    _frame[frameLine] = merged;
                }
            }
            return _frame;
        }
    }
}
