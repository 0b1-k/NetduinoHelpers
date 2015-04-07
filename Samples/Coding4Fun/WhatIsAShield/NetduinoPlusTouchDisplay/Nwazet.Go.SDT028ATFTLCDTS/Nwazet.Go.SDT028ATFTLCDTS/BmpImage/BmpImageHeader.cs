using System;
using System.IO;

namespace Nwazet.BmpImage {
    public class BmpImageHeader {
        public static void Read(FileStream bmpStream) {
            var _header = new byte[14];
            bmpStream.Read(_header,0,_header.Length);
            if (_header[0] != 'B' && _header[1] != 'M') {
                throw new ApplicationException("Not a .bmp");
            }
        }
    }
}
