namespace netduino.helpers.Imaging {
    public class SmallChars {
        public static readonly byte[] Blank = new[] {
                                           B.________,
                                           B.________,
                                           B.________,
                                           B.________,
                                           B.________
                                       };
        public static readonly byte[] D0 = new[] {
                                           B._____XXX,
                                           B._____X_X,
                                           B._____X_X,
                                           B._____X_X,
                                           B._____XXX
                                       };
        public static readonly byte[] D1 = new[] {
                                           B.______X_,
                                           B.______X_,
                                           B.______X_,
                                           B.______X_,
                                           B.______X_
                                       };
        public static readonly byte[] D2 = new[] {
                                           B._____XXX,
                                           B._______X,
                                           B._____XXX,
                                           B._____X__,
                                           B._____XXX
                                       };
        public static readonly byte[] D3 = new[] {
                                           B._____XXX,
                                           B._______X,
                                           B._____XXX,
                                           B._______X,
                                           B._____XXX
                                       };
        public static readonly byte[] D4 = new[] {
                                           B._____X_X,
                                           B._____X_X,
                                           B._____XXX,
                                           B._______X,
                                           B._______X
                                       };
        public static readonly byte[] D5 = new[] {
                                           B._____XXX,
                                           B._____X__,
                                           B._____XXX,
                                           B._______X,
                                           B._____XXX
                                       };
        public static readonly byte[] D6 = new[] {
                                           B._____XXX,
                                           B._____X__,
                                           B._____XXX,
                                           B._____X_X,
                                           B._____XXX
                                       };
        public static readonly byte[] D7 = new[] {
                                           B._____XXX,
                                           B._______X,
                                           B._______X,
                                           B._______X,
                                           B._______X
                                       };
        public static readonly byte[] D8 = new[] {
                                           B._____XXX,
                                           B._____X_X,
                                           B._____XXX,
                                           B._____X_X,
                                           B._____XXX
                                       };
        public static readonly byte[] D9 = new[] {
                                           B._____XXX,
                                           B._____X_X,
                                           B._____XXX,
                                           B._______X,
                                           B._____XXX
                                       };

        public static readonly byte[][] Digits = new[] {Blank, D0, D1, D2, D3, D4, D5, D6, D7, D8, D9};

        /// <summary>
        /// Creates a bitmap from two digits.
        /// Digits can be between 0 and 9.
        /// -1 codes a blank.
        /// </summary>
        /// <param name="leftDigit">The digit to display on the left.</param>
        /// <param name="rightDigit">The digit to display on the right.</param>
        /// <returns></returns>
        public static byte[] ToBitmap(int leftDigit, int rightDigit) {
            var result = new byte[8];
            Digits[rightDigit + 1].CopyTo(result, 2);
            for (var i = 0; i < 5; i++) {
                var digit = Digits[leftDigit + 1][i];
                result[i + 2] |= (byte)(digit << 5);
            }
            return result;
        }
    }
}
