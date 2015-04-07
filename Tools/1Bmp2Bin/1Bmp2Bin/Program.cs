using System;
using System.Text;
using System.Drawing;
using System.IO;

namespace _1Bmp2Bin {
    class Program {
        static void Main(string[] args) {
            var list = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.bmp");
            foreach (var file in list) {
                ProcessBitmap(file);
            }
        }
        public static void ProcessBitmap(string path) {
            using (var bmp = new Bitmap(path)) {
                if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format1bppIndexed) {
                    Console.Write("Please provide a monochrome bitmap to convert (1 bit depth).");
                    return;
                }
                byte Buffer;
                int Count = 0;
                FileStream bin = new FileStream((string)(path + ".bin"), FileMode.Create, FileAccess.Write);
                for (int row = 0; row < bmp.Height; row++) {
                    Buffer = 0;
                    Count = 0;

                    for (int column = 0; column < bmp.Width; column++) {
                        Color pix = bmp.GetPixel(column, row);
                        if (pix.R != 0 || pix.G != 0 || pix.B != 0) {
                            Buffer |= 0;
                        } else {
                            Buffer |= 1;
                        }

                        if (Count == 7) {
                            Console.Write("0x{0},", Buffer.ToString("x"));
                            bin.WriteByte(Buffer);
                            Buffer = 0;
                            Count = 0;
                        } else {
                            Count++;
                            Buffer <<= 1;
                        }
                    }
                    Console.Write("\r\n");
                }
                bin.Close();
            }
        }
    }
}
