using System;
using System.Drawing;
using System.IO;
using System.Text;

namespace _24BitBmpToBigEndianBinary {
    class Program {
        static void Main(string[] args) {
            var list = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bmp");
            foreach (var file in list) {
                ProcessBitmap(file);
            }
        }

        public static void ProcessBitmap(string path) {
            using (var bmp = new Bitmap(path)) {
                if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format24bppRgb) {
                    Console.WriteLine("Please provide a 24-bit depth bitmap to convert.");
                    return;
                }

                var periodPosition = path.LastIndexOf('.');
                var filename = path.Substring(0, periodPosition);

                FileStream bin = new FileStream((string)(filename + ".bin"), FileMode.Create, FileAccess.Write);

                for (int row = 0; row < bmp.Height; row++) {
                    for (int column = 0; column < bmp.Width; column++) {
                        var pixel = bmp.GetPixel(column, row);
                        
                        // Convert from 888 to 565 format
                        ushort pixelOut = (byte) (pixel.R >> 3);
                        pixelOut <<= 6;
                        pixelOut |= (byte) (pixel.G >> 2);
                        pixelOut <<= 5;
                        pixelOut |= (byte) (pixel.B >> 3);

                        bin.WriteByte((byte) (pixelOut >> 8));
                        Console.Write("{0:x}", (byte)(pixelOut >> 8));
                        bin.WriteByte((byte) pixelOut);
                        Console.Write("{0:x}", (byte)pixelOut);
                    }
                    Console.Write("\r\n");
                }
                bin.Close();
                Console.WriteLine("Done.");
            }
        }
    }
}
