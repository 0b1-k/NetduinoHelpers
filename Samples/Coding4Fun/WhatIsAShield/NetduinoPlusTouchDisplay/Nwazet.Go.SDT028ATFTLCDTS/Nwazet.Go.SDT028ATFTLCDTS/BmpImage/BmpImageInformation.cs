using System;
using System.IO;
using Nwazet.Go.Imaging;

namespace Nwazet.BmpImage {
    public class BmpImageInformation {
        enum CompressionType {
            None,
            RLE8,
            RLE4,
            RGBMask
        };

        // Width of the image
        public Int32 Width { get; set; }
        // Height of image
        public Int32 Height { get; set; }
        // Bits per pixel
        public UInt16 Bits { get; set; }
        // Compression type
        public UInt32 Compression { get; set; }
        // Image size
        public UInt32 ImageSizeInBytes {
            get {
                return (UInt32)((Width * Height) * (Bits / 8));
            }
        }

        private const int _widthOffset = 4;
        private const int _heightOffset = 8;
        private const int _bitsPerPixelOffset = 14;
        private const int _compressionOffset = 16;

        private byte[] _bmpInformationheader = new byte[40];

        public BmpImageInformation() {
        }
        public void Read(FileStream bmpStream) {
            DetermineEndianness();
            BmpImageHeader.Read(bmpStream);
            bmpStream.Read(_bmpInformationheader, 0, _bmpInformationheader.Length);
            Width = Get(_widthOffset, Width);
            Height = Get(_heightOffset, Height);
            Bits = Get(_bitsPerPixelOffset, Bits);
            Compression = Get(_compressionOffset, Compression);
            if (Bits != 24) throw new ApplicationException("Bits");
            if ((CompressionType)Compression != CompressionType.None) throw new ApplicationException("Compression");
        }
        private Int32 Get(int offset, Int32 value) {
            return (Int32)Get(offset, (UInt32)value);
        }
        private UInt32 Get(int offset, UInt32 value) {
            if (_isLittleEndian) {
                value |= (byte)_bmpInformationheader[offset + 0];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 1];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 2];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 3];
            } else {
                value |= (byte)_bmpInformationheader[offset + 3];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 2];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 1];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 0]; 
            }
            return value;
        }
        private UInt16 Get(int offset, UInt16 value) {
            if (_isLittleEndian) {
                value |= (byte)_bmpInformationheader[offset + 0];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 1];
            } else {
                value |= (byte)_bmpInformationheader[offset + 1];
                value <<= 8;
                value |= (byte)_bmpInformationheader[offset + 0];
            }
            return value;
        }
        private bool _isLittleEndian;
        private void DetermineEndianness() {
            var test = new byte[] { 0xBE, 0xBF };
            UInt16 Endianness = (byte) test[0];
            Endianness <<= 8;
            Endianness |= (byte)test[1];
            _isLittleEndian = (Endianness == 0xBFBE) ? true : false;
        }
    }
}
