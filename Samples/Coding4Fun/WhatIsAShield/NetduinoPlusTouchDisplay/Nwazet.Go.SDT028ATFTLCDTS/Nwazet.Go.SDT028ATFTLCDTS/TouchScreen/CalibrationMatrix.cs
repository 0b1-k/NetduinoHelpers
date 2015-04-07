using System;
using Nwazet.Go.Helpers;
namespace Nwazet.Go.Display.TouchScreen {
    public class CalibrationMatrix {
        public Int32 An;
        public Int32 Bn;
        public Int32 Cn;
        public Int32 Dn;
        public Int32 En;
        public Int32 Fn;
        public Int32 Divider;

        public void Get(BasicTypeDeSerializerContext context) {
            An = BasicTypeDeSerializer.Get(context, An);
            Bn = BasicTypeDeSerializer.Get(context, Bn);
            Cn = BasicTypeDeSerializer.Get(context, Cn);
            Dn = BasicTypeDeSerializer.Get(context, Dn);
            En = BasicTypeDeSerializer.Get(context, En);
            Fn = BasicTypeDeSerializer.Get(context, Fn);
            Divider = BasicTypeDeSerializer.Get(context, Divider);
        }
        public void Put(BasicTypeSerializerContext context) {
            BasicTypeSerializer.Put(context, (Int32)An);
            BasicTypeSerializer.Put(context, (Int32)Bn);
            BasicTypeSerializer.Put(context, (Int32)Cn);
            BasicTypeSerializer.Put(context, (Int32)Dn);
            BasicTypeSerializer.Put(context, (Int32)En);
            BasicTypeSerializer.Put(context, (Int32)Fn);
            BasicTypeSerializer.Put(context, (Int32)Divider);
        }
    }
}
