using System;
using System.IO;
using System.Text;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;

namespace Nwazet.Go.Helpers {
    public class BasicTypeSerializerContext : IDisposable {
        public delegate void OnHighWatermarkEvent();

        public int ContentSize {
            get { return _currentIndex; }
        }
        public const int MinimumBufferSize = 100;
        
        private bool _isLittleEndian = Utility.ExtractValueFromArray(new byte[] { 0xDE, 0xAD }, 0, 2) == 0xADDE;
        public bool IsLittleEndian {
            get {
                return _isLittleEndian;
            }
        }
        
        public int  HighWatermark { get; set; }
        public bool ByteLevelWaterMark { get; set; }

        protected OnHighWatermarkEvent HighWatermarkEvent;

        public BasicTypeSerializerContext(int defaultBufferSize = 1024, int highWatermark = 0, OnHighWatermarkEvent highWatermarkEvent = null, bool byteLevelWatermark = false) {
            if (defaultBufferSize < MinimumBufferSize) throw new ArgumentOutOfRangeException("defaultBufferSize");
            _serializeBuffer = new byte[defaultBufferSize];
            _storeFunction = StoreToBuffer;
            HighWatermarkEvent = highWatermarkEvent;
            HighWatermark = highWatermark;
            ByteLevelWaterMark = byteLevelWatermark;
            InitializeHeader();
        }
        public BasicTypeSerializerContext(FileStream file) {
            if (file == null) throw new ArgumentNullException("file");
            _file = file;
            if (_file.CanWrite == false) throw new ApplicationException("file");
            _storeFunction = StoreToFile;
            InitializeHeader();
        }

        private const int BufferStartOffset = 2;
        private const int NwazetLibSerializerHeaderVersionOffset = BufferStartOffset + 0;
        private const int NwazetLibSerializerHeaderContentSizeOffset = BufferStartOffset + 1;
        private const byte _headerVersion = 3;

        private void InitializeHeader() {
            _currentIndex = BufferStartOffset;
            if (BufferStartOffset > 0 && _file != null) {
                _file.SetLength(BufferStartOffset);
            }
            // Store the version byte
            Store((byte)_headerVersion);
            // Reserve two bytes to track the length of the data in the buffer. Not used with the FileStream.
            Store(0);
            Store(0);
        }
        public void Store(byte data) {
            _storeFunction(data);
            if (ByteLevelWaterMark == true) {
                CheckHighWatermark();
            }
        }
        public void Store(byte[] bytes, ushort offset, ushort count) {
            if (_file == null) {
                var currentSerializeBufferLength = _serializeBuffer.Length;
                if (currentSerializeBufferLength < _currentIndex + count) {
                    var buffer = new byte[(currentSerializeBufferLength * 2) + count];
                    Array.Copy(_serializeBuffer, buffer, currentSerializeBufferLength);
                    _serializeBuffer = buffer;
                    buffer = null;
                    Debug.GC(true);
                }
                Array.Copy(bytes, offset, _serializeBuffer, _currentIndex, count);
            } else {
                _file.Write(bytes, offset, count);
            }
            _currentIndex += count;
            if (ByteLevelWaterMark == true) {
                CheckHighWatermark();
            }
        }
        private void StoreToBuffer(byte data) {
            if (_currentIndex < _serializeBuffer.Length) {
                _serializeBuffer[_currentIndex++] = data;
                return;
            } else { // Attempt to grow the buffer...
                var buffer = new byte[_serializeBuffer.Length * 2];
                Array.Copy(_serializeBuffer, buffer, _serializeBuffer.Length);
                _serializeBuffer = buffer;
                _serializeBuffer[_currentIndex++] = data;
                buffer = null;
                Debug.GC(true);
            }
        }
        private void StoreToFile(byte data) {
            _file.WriteByte(data);
            _currentIndex++;
        }
        public void CheckHighWatermark() {
            if (HighWatermark != 0 && _currentIndex >= HighWatermark) {
                HighWatermarkEvent();
            }
        }
        public byte[] GetBuffer(out int contentSize) {
            if (_serializeBuffer == null) {
                contentSize = 0;
                return null;
            }
            // Finalize the content size in the header
            _serializeBuffer[NwazetLibSerializerHeaderContentSizeOffset] = (byte)(ContentSize >> 8);
            _serializeBuffer[NwazetLibSerializerHeaderContentSizeOffset+1] = (byte)(ContentSize);
            contentSize = _currentIndex;
            _currentIndex = BufferStartOffset + _headerVersion;
            return _serializeBuffer;
        }
        public void Wipe() {
            var length = _serializeBuffer.Length;
            for (var i = 0; i < length; i++) {
                _serializeBuffer[i] = 0;
            }
            InitializeHeader();
        }
        public void Dispose() {
            if (_file != null) {
                _file.Dispose();
            }
            _encoding = null;
            _file = null;
            _serializeBuffer = null;
            _storeFunction = null;
        }
        private delegate void StoreByte(byte data);
        private UTF8Encoding _encoding = new UTF8Encoding();
        private FileStream _file;
        private byte[] _serializeBuffer;
        private int _currentIndex;
        private StoreByte _storeFunction;
    }

    public static class BasicTypeSerializer {
        public static void Put(BasicTypeSerializerContext context, UInt16 data) {
            Put(context, (Int16) data);
        }
        public static void Put(BasicTypeSerializerContext context, Int16 data) {
            Put(context, (byte)(data >> 8));
            Put(context, (byte)data);
        }
        public static void Put(BasicTypeSerializerContext context, UInt32 data) {
            Put(context, (byte)(data >> 24));
            Put(context, (byte)(data >> 16));
            Put(context, (byte)(data >> 8));
            Put(context, (byte)data);
        }
        public static void Put(BasicTypeSerializerContext context, Int32 data) {
            Put(context, (UInt32)data);
        }
        public static void Put(BasicTypeSerializerContext context, UInt64 data) {
            Put(context, (byte)(data >> 56));
            Put(context, (byte)(data >> 48));
            Put(context, (byte)(data >> 40));
            Put(context, (byte)(data >> 32));
            Put(context, (byte)(data >> 24));
            Put(context, (byte)(data >> 16));
            Put(context, (byte)(data >> 8));
            Put(context, (byte)data);
        }
        public static void Put(BasicTypeSerializerContext context, Int64 data) {
            Put(context, (UInt64)data);
        }
        public static unsafe void Put(BasicTypeSerializerContext context, float data) {
            var temp = new byte[4];
            Utility.InsertValueIntoArray(temp, 0, 4, *((uint*)&data));
            if (context.IsLittleEndian) {
                // Store the float in network byte order (Big Endian)
                Put(context, temp[3]);
                Put(context, temp[2]);
                Put(context, temp[1]);
                Put(context, temp[0]);
            } else {
                // Already in network byte order
                Put(context, temp[0]);
                Put(context, temp[1]);
                Put(context, temp[2]);
                Put(context, temp[3]);
            }
        }
        public static void Put(BasicTypeSerializerContext context, string text, bool ConvertToASCII = false) {
            Put(context, (byte)(ConvertToASCII ? 1 : 0));
            if (ConvertToASCII) {
                Put(context, Encoding.UTF8.GetBytes(text));
                Put(context, (byte) 0); // terminate the string with a null byte
            } else {
                Put(context, (ushort)text.Length);
                foreach (var c in text) {
                    Put(context, c);
                }
                Put(context, (ushort)0); // terminate the unicode string with a null short
            }
        }
        public static void Put(BasicTypeSerializerContext context, byte[] bytes) {
            Put(context, (ushort)bytes.Length);
            foreach (var b in bytes) {
                Put(context, b);
            }
        }
        public static void Put(BasicTypeSerializerContext context, byte[] bytes, ushort offset, ushort count) {
            Put(context, (ushort)count);
            context.Store(bytes, offset, count);
        }
        public static void Put(BasicTypeSerializerContext context, ushort[] array) {
            Put(context, (ushort)array.Length);
            foreach (var e in array) {
                Put(context, e);
            }
        }
        public static void Put(BasicTypeSerializerContext context, UInt32[] array) {
            Put(context, (ushort)array.Length);
            foreach (var e in array) {
                Put(context, e);
            }
        }
        public static void Put(BasicTypeSerializerContext context, UInt64[] array) {
            Put(context, (ushort)array.Length);
            foreach (var e in array) {
                Put(context, e);
            }
        }
        public static void Put(BasicTypeSerializerContext context, byte data) {
            context.Store(data);
        }
    }
}
