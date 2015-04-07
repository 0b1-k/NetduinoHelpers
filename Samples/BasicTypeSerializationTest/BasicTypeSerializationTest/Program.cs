using System;
using Microsoft.SPOT;
using SecretLabs.NETMF.Hardware.Netduino;
using netduino.helpers.Helpers;
namespace BasicTypeSerializationTest {
    public class Program {
        public static void Main() {
            var contextSerializer = new BasicTypeSerializerContext();
            BasicTypeSerializer.Put(contextSerializer,(byte)byte.MaxValue);
            BasicTypeSerializer.Put(contextSerializer,(UInt16)UInt16.MaxValue);
            BasicTypeSerializer.Put(contextSerializer,(Int16)Int16.MinValue);
            BasicTypeSerializer.Put(contextSerializer,(UInt32)UInt32.MaxValue);
            BasicTypeSerializer.Put(contextSerializer,(Int32)Int32.MinValue);
            BasicTypeSerializer.Put(contextSerializer,(UInt64)UInt64.MaxValue);
            BasicTypeSerializer.Put(contextSerializer,(Int64)Int64.MinValue);
            BasicTypeSerializer.Put(contextSerializer,(float)float.MaxValue);
            BasicTypeSerializer.Put(contextSerializer,(float)float.MinValue);
            BasicTypeSerializer.Put(contextSerializer,"Unicode String");
            BasicTypeSerializer.Put(contextSerializer,"ASCII String",true);
            BasicTypeSerializer.Put(contextSerializer,new byte[] { byte.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, byte.MaxValue });
            BasicTypeSerializer.Put(contextSerializer,new ushort[] { ushort.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, UInt16.MaxValue });
            BasicTypeSerializer.Put(contextSerializer,new UInt32[] { UInt32.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, UInt32.MaxValue });
            BasicTypeSerializer.Put(contextSerializer,new UInt64[] { UInt64.MinValue, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, UInt64.MaxValue });

            byte byteValue = 0;
            UInt16 UInt16Value = 0;
            Int16 Int16Value = 0;
            UInt32 UInt32Value = 0;
            Int32 Int32Value = 0;
            UInt64 UInt64Value = 0;
            Int64 Int64Value = 0;
            float floatValue = 0.0F;
            string UnicodeString = null;
            string AsciiString = null;
            byte[] byteArray = null;
            ushort[] ushortArray = null;
            UInt32[] UInt32Array = null;
            UInt64[] UInt64Array = null;

            Debug.Print("sizeof(byte)=" + sizeof(byte));
            Debug.Print("sizeof(UInt16)=" + sizeof(UInt16));
            Debug.Print("sizeof(Int16)=" + sizeof(Int16));
            Debug.Print("sizeof(UInt32)=" + sizeof(UInt32));
            Debug.Print("sizeof(Int32)=" + sizeof(Int32));
            Debug.Print("sizeof(UInt64)=" + sizeof(UInt64));
            Debug.Print("sizeof(Int64)=" + sizeof(Int64));
            Debug.Print("sizeof(float)=" + sizeof(float)); 
            Debug.Print("sizeof(double)=" + sizeof(double));

            var contextDeserializer = new BasicTypeDeSerializerContext(contextSerializer.GetBuffer());

            byteValue = BasicTypeDeSerializer.Get(contextDeserializer);
            if (byteValue != byte.MaxValue) throw new ApplicationException("byteValue");

            UInt16Value = BasicTypeDeSerializer.Get(contextDeserializer, UInt16Value);
            if (UInt16Value != UInt16.MaxValue) throw new ApplicationException("UInt16Value");

            Int16Value = BasicTypeDeSerializer.Get(contextDeserializer, Int16Value);
            if (Int16Value != Int16.MinValue) throw new ApplicationException("Int16Value");

            UInt32Value = BasicTypeDeSerializer.Get(contextDeserializer, UInt32Value);
            if (UInt32Value != UInt32.MaxValue) throw new ApplicationException("UInt32Value");

            Int32Value = BasicTypeDeSerializer.Get(contextDeserializer, Int32Value);
            if (Int32Value != Int32.MinValue) throw new ApplicationException("Int32Value");

            UInt64Value = BasicTypeDeSerializer.Get(contextDeserializer, UInt64Value);
            if (UInt64Value != UInt64.MaxValue) throw new ApplicationException("UInt64Value");

            Int64Value = BasicTypeDeSerializer.Get(contextDeserializer, Int64Value);
            if (Int64Value != Int64.MinValue) throw new ApplicationException("Int64Value");

            floatValue = BasicTypeDeSerializer.Get(contextDeserializer, floatValue);
            if (floatValue != float.MaxValue) throw new ApplicationException("floatValue");
            floatValue = BasicTypeDeSerializer.Get(contextDeserializer, floatValue);
            if (floatValue != float.MinValue) throw new ApplicationException("floatValue");

            UnicodeString = BasicTypeDeSerializer.Get(contextDeserializer, "");
            if (UnicodeString != "Unicode String") throw new ApplicationException("UnicodeString");

            AsciiString = BasicTypeDeSerializer.Get(contextDeserializer, "");
            if (AsciiString != "ASCII String") throw new ApplicationException("AsciiString");

            byteArray = BasicTypeDeSerializer.Get(contextDeserializer, byteArray);
            if (byteArray[0] != byte.MinValue || byteArray[15] != byte.MaxValue) throw new ApplicationException("byteArray");

            ushortArray = BasicTypeDeSerializer.Get(contextDeserializer, ushortArray);
            if (ushortArray[0] != ushort.MinValue || ushortArray[15] != ushort.MaxValue) throw new ApplicationException("ushortArray");

            UInt32Array = BasicTypeDeSerializer.Get(contextDeserializer, UInt32Array);
            if (UInt32Array[0] != UInt32.MinValue || UInt32Array[15] != UInt32.MaxValue) throw new ApplicationException("UInt32Array");

            UInt64Array = BasicTypeDeSerializer.Get(contextDeserializer, UInt64Array);
            if (UInt64Array[0] != UInt64.MinValue || UInt64Array[15] != UInt64.MaxValue) throw new ApplicationException("UInt64Array");
        }
    }
}
