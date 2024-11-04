using System.Runtime.InteropServices;

namespace LibCD.ISO9660 {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LbeUInt16 {
        public ushort le;
        public ushort be;

        public ushort Value {
            get {
                return le;
            }
            set {
                le = value;
                be = (ushort)((value & 0xff) << 8 | (value >> 8));
            }
        }
        public override string ToString() {
            return le.ToString();
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct LbeUInt32 {
        public uint le;
        public uint be;

        public uint Value {
            get {
                return le;
            }
            set {
                le = value;
                be = (uint)(((value & 0xff) << 24) | ((value & 0xff00) << 8) | ((value & 0xff0000) >> 8) | ((value & 0xff000000) >> 24));
            }
        }

        public override string ToString() {
            return le.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Timestamp {
        public byte year;
        public byte month;
        public byte day;
        public byte hour;
        public byte minute;
        public byte second;
        public sbyte gmtoff;
    };
}
