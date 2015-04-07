using System;
using System.IO;

namespace Nwazet.BmpImage {
    public class BmpImageHeaderReader {
        public static BmpImageInformation Read(FileStream bmpFileStream) {
            var bmpInfo = new BmpImageInformation();
            bmpInfo.Read(bmpFileStream);
            return bmpInfo;
        }
    }
}
