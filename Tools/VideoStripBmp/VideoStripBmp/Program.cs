using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace netduino.helpers.tools {
    class Program {
        static void Main(string[] args) {
            var list = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bmp");
            foreach(var file in list) {
                ProcessBitmap(file);
            }
        }

        const int BytesPerPixel = 3;
        private static byte[] pixelBuffer = new byte[BytesPerPixel];
        const int GreenByte = 0;
        const int RedByte = 1;
        const int BlueByte = 2;

        public static void ProcessBitmap(string path) {
            using(var bmp = new Bitmap(path)) {
                if(bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                    Console.WriteLine("Can't process {0}. Please provide a 24-bit depth bitmap to convert.", path);
                    return;
                }
                var periodPosition = path.LastIndexOf('.');
                var filename = path.Substring(0, periodPosition);
                FileStream bin = new FileStream((string)(filename + ".vsbin"), FileMode.Create, FileAccess.Write);
                for(int row = 0; row < bmp.Height; row++) {
                    for(int column = 0; column < bmp.Width; column++) {
                        var pixel = bmp.GetPixel(column, row);
                        pixelBuffer[GreenByte] = (byte)(MapColor(pixel.G) | 0x80);
                        pixelBuffer[RedByte] = (byte)(MapColor(pixel.R) | 0x80);
                        pixelBuffer[BlueByte] = (byte)(MapColor(pixel.B) | 0x80);
                        bin.Write(pixelBuffer, 0, BytesPerPixel);
                        //Console.WriteLine("Dbg: {0}->{1},{2}->{3},{4}->{5}", pixel.G, MapColor(pixel.G), pixel.R, MapColor(pixel.R), pixel.B, MapColor(pixel.B));
                        Console.Write("0x{0:x},0x{1:x},0x{2:x},", pixelBuffer[GreenByte], pixelBuffer[RedByte], pixelBuffer[BlueByte]);
                    }
                    Console.Write("\r\n");
                }
                bin.Close();
                Console.WriteLine("");
            }
        }

        public static byte MapColor(byte color) {
            return (byte)MapRange(0f, 255, 0f, 127f, (float)color);
        }

        // Maps a range of values to another http://rosettacode.org/wiki/Map_range#C
        public static float MapRange(float a1, float a2, float b1, float b2, float s) {
            return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
        }
    }
}