using System;
using System.IO;
using Microsoft.SPOT;

namespace netduino.helpers.Helpers {
    public class VirtualMemory: IDisposable {
        public byte[] Buffer;
        public FileStream Stream;
        public bool IsReadOnly { get; set; }

        public VirtualMemory(long capacityInBytes, int segments, string path) {
            IsReadOnly = false;
            if (segments <= 0) throw new ArgumentOutOfRangeException("bufferSize");
            Buffer = new byte[capacityInBytes / segments];
            Stream = new FileStream(path, FileMode.OpenOrCreate);
            if (Stream.Length == 0) {
                Stream.SetLength(capacityInBytes);
            } 
        }

        public void WriteVM(long address, byte data, SeekOrigin origin = SeekOrigin.Begin) {
            if (IsReadOnly) throw new InvalidOperationException("readonly");
            if (address > Stream.Length) throw new ArgumentOutOfRangeException("address");
            Stream.Seek(address, origin);
            Stream.WriteByte(data);
        }
        public void WriteVM(long address, byte[] data, int byteCount = 0, SeekOrigin origin = SeekOrigin.Begin) {
            if (IsReadOnly) throw new InvalidOperationException("readonly");
            Stream.Seek(address, origin);
            if (byteCount == 0) {
                if (address + data.Length > Stream.Length) throw new ArgumentOutOfRangeException("address");
                Stream.Write(data, 0, data.Length);
            } else {
                if (address + byteCount > Stream.Length) throw new ArgumentOutOfRangeException("address");
                Stream.Write(data, 0, byteCount);
            }
        }

        public byte ReadVM(long address, SeekOrigin origin = SeekOrigin.Begin) {
            Stream.Seek(address, origin);
            return (byte) Stream.ReadByte();
        }
        public int ReadVM(long address, int offsetInbuffer, int readBytecount = 0, SeekOrigin origin = SeekOrigin.Begin) {
            Stream.Seek(address, origin);
            if (readBytecount == 0) {
                return Stream.Read(Buffer, 0, Buffer.Length);
            } else {
                if (readBytecount > Buffer.Length) throw new ArgumentOutOfRangeException("readBytecount");
                return Stream.Read(Buffer, offsetInbuffer, readBytecount);
            }
        }

        public void FillFromBuffer() {
            if (IsReadOnly) throw new InvalidOperationException("readonly");
            var address = 0;
            while (address < Stream.Length) {
                WriteVM(address, Buffer);
                address += Buffer.Length;
            }
        }

        public void ConnectExistingStream(string path, bool readOnly = true) {
            Stream.Dispose();
            Stream = null;
            Debug.GC(true);
            Stream = new FileStream(path, FileMode.Open);
            IsReadOnly = readOnly;
        }

        public void RedefineBufferSize(int bufferSize) {
            Buffer = null;
            Debug.GC(true);
            Buffer = new byte[bufferSize];
        }

        public virtual void Copy(VirtualMemory source) {
            if (IsReadOnly) throw new InvalidOperationException("readonly");
            var address = 0;
            Stream.SetLength(source.Stream.Length);
            while(address < source.Stream.Length) {
                var sourceBytesRead = source.ReadVM(address, 0);
                WriteVM(address, source.Buffer, sourceBytesRead);
                address+= sourceBytesRead;
            }
        }

        public void Dispose() {
            Buffer = null;
            Stream.Close();
            Stream.Dispose();
            Stream = null;
            Debug.GC(true);
        }
    }
}
