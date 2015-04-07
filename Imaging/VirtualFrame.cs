using System;
using System.IO;
using Microsoft.SPOT;
using netduino.helpers.Helpers;

namespace netduino.helpers.Imaging {
    public class VirtualFrame : VirtualMemory {
        public int Height { get; set; }
        private int _width;
        public int Width {
            get { return _width; }
            set { _width = value; ValidateBufferSize(value); }
        }
        private int _bytesPerPixel;
        public int BytesPerPixel {
            get { return _bytesPerPixel; }
            set { _bytesPerPixel = value; ValidateBufferSize(value); }
        }
        private void ValidateBufferSize(int value) {
            if (value * BytesPerPixel > Buffer.Length) {
                Buffer = null;
                Debug.GC(true);
                Buffer = new byte[value * BytesPerPixel];
            }
        }
        public int xOffset { get; set; }
        public int yOffset { get; set; }
        public short TransparentColor { get; set; }

        public const int Solid = -1;

        public VirtualFrame(long capacityInBytes, int segments, string path)
            : base(capacityInBytes, segments, path) {
            TransparentColor = Solid;
            BytesPerPixel = 2;
        }

        public int GetWidthInBytes() {
            return Width * BytesPerPixel;
        }

        public override void Copy(VirtualMemory source) {
            base.Copy(source);
            Height = ((VirtualFrame)source).Height;
            Width = ((VirtualFrame)source).Width;
            xOffset = ((VirtualFrame)source).xOffset;
            yOffset = ((VirtualFrame)source).yOffset;
            BytesPerPixel = ((VirtualFrame)source).BytesPerPixel;
            TransparentColor = ((VirtualFrame)source).TransparentColor;
        }

        //protected string ToHex(byte value) {
        //    var hex = new Char[2];
        //    hex[0] = ToNibble((byte)(value >> 4));
        //    hex[1] = ToNibble((byte)(value & 0x0F));
        //    return new string(hex);
        //}

        //protected Char ToNibble(byte value) {
        //    if (value >= 0 && value <= 9) {
        //        return (Char)('0' + value);
        //    }
        //    return (Char)('A' + value - 10);
        //}

        public VirtualFrame Merge(VirtualFrame background, VirtualFrame sprite) {
            if (sprite.Width > background.Width ||
                sprite.Width > Width ||
                sprite.Height > background.Height ||
                sprite.Height > Height ||
                sprite.BytesPerPixel != BytesPerPixel ||
                sprite.BytesPerPixel != background.BytesPerPixel) {
                throw new IndexOutOfRangeException("sprite");
            }

            var xStartPixels = xOffset + sprite.xOffset;
            if (xStartPixels > Width) return this;

            var yStartPixels = yOffset + sprite.yOffset;
            if (yStartPixels > Height) return this;

            var xEndPixels = xStartPixels + sprite.Width;
            if (xEndPixels > Width) xEndPixels = Width;

            var yEndPixels = yStartPixels + sprite.Height;
            if (yEndPixels > Height) yEndPixels = Height;

            var spriteWidthBytes = (xEndPixels - xStartPixels) * BytesPerPixel;
            var address = (yStartPixels * GetWidthInBytes()) + (xStartPixels * BytesPerPixel);
            var step = GetWidthInBytes();

            if (sprite.TransparentColor == Solid) {
                var spriteLine = 0;
                while(yStartPixels < yEndPixels) {
                    sprite.ReadVM(spriteLine, 0, spriteWidthBytes);
                    base.WriteVM(address, sprite.Buffer, spriteWidthBytes);
                    address += step;
                    spriteLine += spriteWidthBytes;
                    yStartPixels++;
                }
            } else {
                var spriteAddress = 0;
                for (; yStartPixels < yEndPixels; yStartPixels++) {
                    var w = xEndPixels - xStartPixels;
                    for (var x = 0; x < w; x++) {
                        var pixelHigh = sprite.ReadVM(spriteAddress++);
                        var pixelLow = sprite.ReadVM(spriteAddress++);

                        short color = pixelHigh;
                        color <<= 8;
                        color |= (short)pixelLow;

                        if (color == TransparentColor) {
                            // Not yet implemented. Same as solid.
                            base.WriteVM(address++, pixelHigh);
                            base.WriteVM(address++, pixelLow);
                        } else {
                            base.WriteVM(address++, pixelHigh);
                            base.WriteVM(address++, pixelLow);
                        }
                    }
                    address += (step - spriteWidthBytes);
                }
            }

            return this;
        }

        public string BitmapDirectory { get; set; }
        public int MaxMessageLength { get; set; }
        public int MaxCharactersAfterPeriod { get; set; }

        public void Print(VirtualFrame background, string message, int x, int y, int Width, int Height) {
            VirtualFrame spriteCharFrame = null;
            var characterCount = 0;
            long frameSize = 0;
            bool hasPeriod = false;
            int digitsAfterPeriod = 0;
            foreach (Char c in message) {
                characterCount++;
                var path = GetPathFromCharacter(c);
                if (characterCount == 1) {
                    frameSize = GetFrameSize(path);
                    spriteCharFrame = new VirtualFrame(frameSize, 4, path);
                    spriteCharFrame.Height = Height;
                    spriteCharFrame.Width = Width;
                    spriteCharFrame.xOffset = x;
                    spriteCharFrame.yOffset = y;
                } else {
                    spriteCharFrame.ConnectExistingStream(path);
                }
                Merge(background, spriteCharFrame);
                spriteCharFrame.xOffset += Width;
                path = null;
                Debug.GC(true);
                if (characterCount >= MaxMessageLength) break;
                if (c == '.') {
                    hasPeriod = true;
                    continue;
                }
                if (hasPeriod) digitsAfterPeriod++;
                if (hasPeriod && (digitsAfterPeriod >= MaxCharactersAfterPeriod)) break;
            }
            spriteCharFrame.Dispose();
            spriteCharFrame = null;
            Debug.GC(true);
        }

        public string GetPathFromCharacter(Char c) {
            switch (c) {
                case '\\':
                    return BitmapDirectory + "Backslash-Bmp24.bin";
                case '/':
                    return BitmapDirectory + "Forwardslash-Bmp24.bin";
                case ':':
                    return BitmapDirectory + "Colon-Bmp24.bin";
                case '*':
                    return BitmapDirectory + "Asterix-Bmp24.bin";
                case '?':
                    return BitmapDirectory + "Question-Bmp24.bin";
                case '\"':
                    return BitmapDirectory + "DblQuotes-Bmp24.bin";
                case '<':
                    return BitmapDirectory + "LessThan-Bmp24.bin";
                case '>':
                    return BitmapDirectory + "GreaterThan-Bmp24.bin";
                case '|':
                    return BitmapDirectory + "Pipe-Bmp24.bin";
                case ' ':
                    return BitmapDirectory + "Space-Bmp24.bin";
                case '.':
                    return BitmapDirectory + "Period-Bmp24.bin";
                case '-':
                    return BitmapDirectory + "Dash-Bmp24.bin";
            }

            return BitmapDirectory + c + "-Bmp24.bin";
        }

        public long GetFrameSize(string path) {
            if (File.Exists(path)) {
                using (var file = new FileStream(path, FileMode.Open)) {
                    return file.Length;
                }
            }
            throw new ArgumentOutOfRangeException("path");
        }
    }
}
