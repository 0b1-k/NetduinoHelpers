using System;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using netduino.helpers.Imaging;

namespace netduino.helpers.Hardware {
    // Based on https://github.com/adafruit/LPD8806
    public class AdaFruitLPD8806 : IDisposable {

        public int Width { get; private set; }
        public int Height { get; private set; }
        public int PixelCount { get; private set; }
        public int FrameSize { get; private set; }
        public const int BytesPerPixel = 3;
        protected int PixelBufferEnd;

        public AdaFruitLPD8806(int width, int height, Cpu.Pin chipSelect, SPI.SPI_module spiModule = SPI.SPI_module.SPI1, uint speedKHz = 10000) {
            Width = width;
            Height = height;
            PixelCount = Width * Height;
            PixelBufferEnd = (PixelCount - 1) * BytesPerPixel;
            FrameSize = Width * Height * BytesPerPixel;

            var spiConfig = new SPI.Configuration(
                SPI_mod: spiModule,
                ChipSelect_Port: chipSelect,
                ChipSelect_ActiveState: false,
                ChipSelect_SetupTime: 0,
                ChipSelect_HoldTime: 0,
                Clock_IdleState: false,
                Clock_Edge: true,
                Clock_RateKHz: speedKHz
                );

            spi = new SPI(spiConfig);

            pixelBuffer = new byte[PixelCount * BytesPerPixel];

            SetBackgroundColor(0,0,0);
        }

        protected SPI spi;
        protected byte[] attentionSequence = new byte[] { 0, 0, 0, 0 };
        protected byte[] latchSequence = new byte[] { 0, 0, 0 };

        public byte[] pixelBuffer;

        // Sets the background color to black across the strip
        public void Reset() {
            SetColor(0, 0, 0);
        }

        // Sets the color of the entire strip
        public void SetColor(byte red, byte green, byte blue) {
            SetBackgroundColor(red, green, blue);
            for (var pixel = 0; pixel < FrameSize; pixel += BytesPerPixel) {
                Array.Copy(backgroundColor, 0, pixelBuffer, pixel, BytesPerPixel);
            }
        }

        // Send the internal pixel buffer to the strip
        public void Refresh(int delayMS = 0) {
            spi.Write(attentionSequence);
            spi.Write(pixelBuffer);
            var ledLatchCount = PixelCount * 2;
            for (var i = 0; i < ledLatchCount; i++) {
                spi.Write(latchSequence);
            }
            if (delayMS > 0) Thread.Sleep(delayMS);
        }

        // Generates a 32 bit value from RGB values. RGB values must be between 0 and 127 (2,048,383 colors 'only')
        public UInt32 RgbToColor(byte red, byte green, byte blue) {
          // Take the lowest 7 bits of each value and append them end to end
          // We have the top bit set high (its a 'parity-like' bit in the protocol and must be set!
          UInt32 color;
          color = (UInt32)(green | 0x80);
          color <<= 8;
          color |= (UInt32)(red | 0x80);
          color <<= 8;
          color |= (UInt32)(blue | 0x80);
          return color;
        }

        // Sets a pixel at given index with an RGB value
        public void SetPixel(int pixelIndex, byte red, byte green, byte blue) {
            if (pixelIndex >= PixelCount) return;
            pixelBuffer[pixelIndex * 3] = (byte)(green | 0x80);
            pixelBuffer[pixelIndex * 3 + 1] = (byte)(red | 0x80);
            pixelBuffer[pixelIndex * 3 + 2] = (byte)(blue | 0x80);
        }

        // Sets a pixel at given index with a color value. The color parameter needs to come from RgbToColor()
        public void SetPixel(int pixelIndex, UInt32 color) {
            if (pixelIndex >= PixelCount) return;
            pixelBuffer[pixelIndex * 3] = (byte)(color >> 16);
            pixelBuffer[pixelIndex * 3 + 1] = (byte)(color >> 8);
            pixelBuffer[pixelIndex * 3 + 2] = (byte)(color);
        }

        // Set a pixel at a given coordinate with a color value. The color parameter needs to come from RgbToColor()
        public void SetPixel(int x, int y, UInt32 color) {
            if (x < 0 || x >= Width || y < 0 || y > Height) return;
            if (((y + 1) % 2) == 0) {
                SetPixel((y * Width) + (Width - x - 1), color);
            } else {
                SetPixel((y * Width) + x, color);
            }
        }

        protected byte[] backgroundColor = new byte[BytesPerPixel];

        // Sets the default background color. Internally used by Reset, SetColor, Scroll and Shift.
        public void SetBackgroundColor(byte red, byte green, byte blue) {
            backgroundColor[0] = (byte)(green | 0x80);
            backgroundColor[1] = (byte)(red | 0x80);
            backgroundColor[2] = (byte)(blue | 0x80);
        }

        public enum ScrollDirection {
            Left,
            Right
        }

        public enum ScrollingType {
            Circular,
            NonCircular
        }

        // Shift the entire strip as a single line, either left or right, circularly or not.
        public void Shift(ScrollDirection direction, ScrollingType scrollingType) {
            if (direction == ScrollDirection.Right) {
                if (scrollingType == ScrollingType.Circular) {
                    Array.Copy(pixelBuffer, PixelBufferEnd, backgroundColor, 0, BytesPerPixel);
                    Array.Copy(pixelBuffer, 0, pixelBuffer, BytesPerPixel, PixelBufferEnd);
                    Array.Copy(backgroundColor, pixelBuffer, BytesPerPixel);
                } else {
                    Array.Copy(pixelBuffer, 0, pixelBuffer, BytesPerPixel, PixelBufferEnd);
                    Array.Copy(backgroundColor, pixelBuffer, BytesPerPixel);
                }
            } else { // Left Shift
                if (scrollingType == ScrollingType.Circular) {
                    Array.Copy(pixelBuffer, 0, backgroundColor, 0, BytesPerPixel);
                    Array.Copy(pixelBuffer, BytesPerPixel, pixelBuffer, 0, PixelBufferEnd);
                    Array.Copy(backgroundColor, 0, pixelBuffer, PixelBufferEnd, BytesPerPixel);
                } else {
                    Array.Copy(pixelBuffer, BytesPerPixel, pixelBuffer, 0, PixelBufferEnd);
                    Array.Copy(backgroundColor, 0, pixelBuffer, PixelBufferEnd, BytesPerPixel);
                }
            }
        }

        // Scroll the entire frame by x pixels either left or right, circularly or not.
        public void Scroll(ScrollDirection direction, ScrollingType scrollingType, int pixelCount) {
            var length = (Width * BytesPerPixel) - BytesPerPixel;
            while (pixelCount != 0) {
                for (var row = 0; row < Height; row += 2) {
                    var offset = Width * row * BytesPerPixel;
                    if (direction == ScrollDirection.Right) {
                        if (scrollingType == ScrollingType.NonCircular) {
                            Array.Copy(pixelBuffer, offset, pixelBuffer, offset + BytesPerPixel, length);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset, BytesPerPixel);
                        } else { // Circular
                            Array.Copy(pixelBuffer, offset + length, backgroundColor, 0, BytesPerPixel);
                            Array.Copy(pixelBuffer, offset, pixelBuffer, offset + BytesPerPixel, length);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset, BytesPerPixel);
                        }
                    } else { // Scrolling Left
                        if (scrollingType == ScrollingType.NonCircular) {
                            Array.Copy(pixelBuffer, offset + BytesPerPixel, pixelBuffer, offset, length);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset + length, BytesPerPixel);
                        } else { // Circular
                            Array.Copy(pixelBuffer, offset, backgroundColor, 0, BytesPerPixel);
                            Array.Copy(pixelBuffer, offset + BytesPerPixel, pixelBuffer, offset, length + BytesPerPixel);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset + length, BytesPerPixel);
                        }
                    }
                }
                for (var row = 1; row < Height; row += 2) {
                    var offset = Width * row * BytesPerPixel;
                    if (direction == ScrollDirection.Right) {
                        if (scrollingType == ScrollingType.NonCircular) {
                            Array.Copy(pixelBuffer, offset + BytesPerPixel, pixelBuffer, offset, length);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset + length, BytesPerPixel);
                        } else { // Circular
                            Array.Copy(pixelBuffer, offset, backgroundColor, 0, BytesPerPixel);
                            Array.Copy(pixelBuffer, offset + BytesPerPixel, pixelBuffer, offset, length);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset + length, BytesPerPixel);
                        }
                    } else { // Scrolling Left
                        var lengthLeftScrolling = Width * BytesPerPixel;
                        if (scrollingType == ScrollingType.NonCircular) {
                            ScrollEvenLineLeft(offset, lengthLeftScrolling);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset, BytesPerPixel);
                        } else { // Circular
                            Array.Copy(pixelBuffer, offset + (lengthLeftScrolling - BytesPerPixel), backgroundColor, 0, BytesPerPixel);
                            ScrollEvenLineLeft(offset, lengthLeftScrolling);
                            Array.Copy(backgroundColor, 0, pixelBuffer, offset, BytesPerPixel);
                        }
                    }
                }
                Refresh();
                pixelCount--;
            }
        }

        // Scroll function helper
        private void ScrollEvenLineLeft(int offset, int length) {
            var pixelCounter = length - BytesPerPixel;
            while (pixelCounter >= 0) {
                Array.Copy(pixelBuffer, offset + pixelCounter - BytesPerPixel, pixelBuffer, offset + pixelCounter, BytesPerPixel);
                pixelCounter -= BytesPerPixel;
            }
        }

        // Generates a gradient between two colors, between two pixels indices
        public void Gradient(int startRed, int startGreen, int startBlue, int endRed, int endGreen, int endBlue, int pixelIndexStart, int pixelIndexEnd) {
            var pixelRange = pixelIndexEnd - pixelIndexStart;
            for (var pixel = pixelIndexStart; pixel <= pixelIndexEnd; pixel++) {
                var ratio = (float)pixel / (float)pixelRange;
                SetPixel(pixel,
                    (byte)(endRed * ratio + startRed * (1 - ratio)),
                    (byte)(endGreen * ratio + startGreen * (1 - ratio)),
                    (byte)(endBlue * ratio + startBlue * (1 - ratio))
                    );
            }
        }

        // Fade a bitmap into view using the bitmap currently loaded in the LED strip buffer as the point of reference
        public void FadeIn(byte[] bitmap, int sourceBufferOffset = 0, byte Speed = 1) {
            var complete = false;
            var length = Width * BytesPerPixel; 
            while (!complete) {
                complete = true;
                for (var row = 0; row < Height; row += 2) {
                    var offSet = Width * row * BytesPerPixel;
                    for (var i = 0; i < length; i++) {
                        var source = bitmap[offSet + i + sourceBufferOffset] & 0x7F;
                        var destination = pixelBuffer[offSet + i] & 0x7F;

                        if (destination == source) continue;
                        if (destination < source) {
                            destination += Speed;
                            if (destination >= source) {
                                pixelBuffer[offSet + i] = bitmap[offSet + i + sourceBufferOffset];
                                complete = true;
                            } else {
                                pixelBuffer[offSet + i] = (byte) (destination | 0x80);
                                complete = false;
                            }
                        } else if (destination > source) {
                            destination -= Speed;
                            if (destination <= source) {
                                pixelBuffer[offSet + i] = bitmap[offSet + i + sourceBufferOffset];
                                complete = true;
                            } else {
                                pixelBuffer[offSet + i] = (byte)(destination | 0x80);
                                complete = false;
                            }
                        }
                    }
                }
                for (var row = 1; row < Height; row += 2) {
                    var offSet = Width * row * BytesPerPixel;
                    var targetCount = length - BytesPerPixel;
                    var sourceCount = 0;
                    while (targetCount >= 0) {
                        if (!FadeInPixelPart(0, offSet, targetCount, bitmap, sourceBufferOffset, sourceCount, Speed)) complete = false;
                        if (!FadeInPixelPart(1, offSet, targetCount, bitmap, sourceBufferOffset, sourceCount, Speed)) complete = false;
                        if (!FadeInPixelPart(2, offSet, targetCount, bitmap, sourceBufferOffset, sourceCount, Speed)) complete = false;
                        targetCount -= BytesPerPixel;
                        sourceCount += BytesPerPixel;
                    }
                }
                Refresh();
            }
        }

        // FadeIn helper function
        private bool FadeInPixelPart(int pixelComponent, int offSet, int targetCount, byte[] bitmap, int sourceBufferOffset, int sourceCount, byte Speed) {
            var source = bitmap[sourceBufferOffset + offSet + sourceCount + pixelComponent] & 0x7F;
            var destination = pixelBuffer[offSet + targetCount + pixelComponent] & 0x7F;

            if (destination == source) return true;

            if (destination < source) {
                destination += Speed;
                if (destination >= source) {
                    pixelBuffer[offSet + targetCount + pixelComponent] = bitmap[sourceBufferOffset + offSet + sourceCount + pixelComponent];
                    return true;
                }
            } else if (destination > source) {
                destination -= Speed;
                if (destination <= source) {
                    pixelBuffer[offSet + targetCount + pixelComponent] = bitmap[sourceBufferOffset + offSet + sourceCount + pixelComponent];
                    return true;
                }
            }
            pixelBuffer[offSet + targetCount + pixelComponent] = (byte)(destination | 0x80);
            return false;
        }

        // Copies a source bitmap to the LED strip buffer. Source and destinations must be the same size.
        public void Copy(byte[] bitmap, int sourceBufferOffset = 0) {
            var length = Width * BytesPerPixel;
            for (var row = 0; row < Height; row += 2) {
                var offset = Width * row * BytesPerPixel;
                Array.Copy(bitmap, offset + sourceBufferOffset, pixelBuffer, offset, length);
            }
            for (var row = 1; row < Height; row += 2) {
                var offset = Width * row * BytesPerPixel;
                var targetCount = length - BytesPerPixel;
                var sourceCount = 0;
                while (targetCount >= 0) {
                    Array.Copy(bitmap, sourceBufferOffset + offset + sourceCount, pixelBuffer, offset + targetCount, BytesPerPixel);
                    targetCount -= BytesPerPixel;
                    sourceCount += BytesPerPixel;
                }
            }
        }

        // Copies a source bitmap at the given coordinates into the LED strip buffer.
        // Does not yet handle the case where the source bitmap is smaller than the target frame size.
        public void Copy(byte[] bitmap, int x, int y, int width, int height, int bitmapWidth, int bitmapHeight) {
            var srcOffset = ((y * bitmapWidth) + x) * BytesPerPixel;
            var srcStep = bitmapWidth * BytesPerPixel * 2;
            //var fillerLength = 0;
            //var fillerHeight = 0;
            
            var srcLength = Width;
            if(x + width > bitmapWidth) srcLength = bitmapWidth - x;
            if(srcLength > Width) {
                srcLength = Width;
            }
            // else {
            //    fillerLength = Width - srcLength;
            //}

            srcLength *= BytesPerPixel;
            //fillerLength *= BytesPerPixel;

            var srcHeight = Height;
            if(y + height > bitmapHeight) srcHeight = bitmapHeight - y;
            if(srcHeight > Height) {
                srcHeight = Height;
            }
            // else {
            //    fillerHeight = Height - srcHeight;
            //}
           
            var offset = srcOffset;
            for (var row = 0; row < Height; row+=2) {
                var targetOffset = Width * row * BytesPerPixel;
                Array.Copy(bitmap, offset, pixelBuffer, targetOffset, srcLength);
                offset += srcStep;
            }

            offset = srcOffset + (bitmapWidth * BytesPerPixel);
            for (var row = 1; row < Height; row += 2) {
                var targetOffset = Width * row * BytesPerPixel;
                var targetCount = srcLength - BytesPerPixel;
                var sourceCount = 0;
                while(targetCount >= 0) {
                    Array.Copy(bitmap, offset + sourceCount, pixelBuffer, targetOffset + targetCount, BytesPerPixel);
                    targetCount -= BytesPerPixel;
                    sourceCount += BytesPerPixel;
                }
                offset += srcStep;
            }
        }

        // Draws a rectangle of a given color
        public void DrawRectangle(int x, int y, int width, int height, UInt32 color) {
            for (int i = x; i < x + width; i++) {
                SetPixel(i, y, color);
                SetPixel(i, y + height - 1, color);
            }
            for (int i = y; i < y + height; i++) {
                SetPixel(x, i, color);
                SetPixel(x + width - 1, i, color);
            }
        }

        public byte[] BuildMarquee(string text, CharSet charSet, byte redBackground, byte greenBackground, byte blueBackground, byte redText, byte greenText, byte blueText) {
            var tempChar = new byte[8];
            var textColor = new byte[3];
            var backgroundColor = new byte[3];
            var marquee = new byte[text.Length * 8 * AdaFruitLPD8806.BytesPerPixel * Height];
            var lineLengthInBytes = text.Length * 8 * AdaFruitLPD8806.BytesPerPixel;
            textColor[0] = (byte)(greenText | 0x80);
            textColor[1] = (byte)(redText | 0x80);
            textColor[2] = (byte)(blueText | 0x80);
            backgroundColor[0] = (byte)(greenBackground | 0x80);
            backgroundColor[1] = (byte)(redBackground | 0x80);
            backgroundColor[2] = (byte)(blueBackground | 0x80);
            var index = 0;
            foreach (char c in text) {
                charSet.CopyBitmapChar(c, tempChar, 0, 1);
                BuildMarqueeCharacter(index++, lineLengthInBytes, marquee, tempChar, textColor, backgroundColor);
            }
            return marquee;
        }

        protected void BuildMarqueeCharacter(int index, int lineLengthInBytes, byte[] marquee, byte[] tempChar, byte[] textColor, byte[] backgroundColor) {
            var row = 0;
            var charOffset = index * 8 * AdaFruitLPD8806.BytesPerPixel;
            for (var byteIndex = 0; byteIndex < tempChar.Length; byteIndex++) {
                var col = 0;
                var rowOffset = (row * lineLengthInBytes) + charOffset;
                while (col < 8) {
                    var pixelOffset = rowOffset + (col * AdaFruitLPD8806.BytesPerPixel);
                    if ((tempChar[byteIndex] & 0x80) != 0) {
                        Array.Copy(textColor, 0, marquee, pixelOffset, AdaFruitLPD8806.BytesPerPixel);
                    } else {
                        Array.Copy(backgroundColor, 0, marquee, pixelOffset, AdaFruitLPD8806.BytesPerPixel);
                    }
                    tempChar[byteIndex] <<= 1;
                    col++;
                }
                row++;
            }
        }

        // Releases all resources used by the driver
        public void Dispose() {
            backgroundColor = null;
            pixelBuffer = null;
            attentionSequence = null;
            latchSequence = null;
            spi = null;
        }
    }
}
