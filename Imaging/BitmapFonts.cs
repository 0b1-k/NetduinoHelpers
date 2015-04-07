namespace netduino.helpers.Imaging {
    // Describes a single character's display information
    public class FontCharInfo {
        public int WidthBits;   // width, in bits (or pixels), of the character
        public int Offset;      // offset of the character's bitmap, in bytes, into the the FONT_INFO's data array
    }
    // Describes a single font
    public class FontInfo {
        public readonly int Height;            // height of the font's characters
        public readonly int StartChar;         // the first character in the font (e.g. in charInfo and data)
        public readonly int EndChar;           // the last character in the font (e.g. in charInfo and data)
        public readonly ushort[] CharInfo;     // array of char information
        public readonly byte[] Data;           // generated array of character visual representation
        public readonly string Name;
        public readonly ushort ID;
        public FontInfo(int height, int startChar, int endChar, ushort[] fontDescriptors, byte[] fontBitmaps, string name, ushort id) {
            _fontCharInfo = new FontCharInfo();
            Height = height;
            StartChar = startChar;
            EndChar = endChar;
            CharInfo = fontDescriptors;
            Data = fontBitmaps;
            Name = name;
            ID = id;
        }
        private FontCharInfo _fontCharInfo;
        public const int DefaultCharWidth = 5;
        public FontCharInfo GetFontCharInfo(char characterToOuput) {
            // some fonts have character descriptors, some don't
            if (CharInfo != null) {
                // get correct char offset
                var charInfoIndex = (characterToOuput - StartChar) * 2;
                // get width from char info
                _fontCharInfo.WidthBits = CharInfo[charInfoIndex];
                _fontCharInfo.Offset = CharInfo[charInfoIndex+1];
            } else {
                // if no char info, char width is always 5
                _fontCharInfo.WidthBits = DefaultCharWidth;
                // char offset - assume DefaultCharWidth * letter offset
                _fontCharInfo.Offset = (characterToOuput - StartChar) * DefaultCharWidth;
            } return _fontCharInfo;
        }
    }
}
