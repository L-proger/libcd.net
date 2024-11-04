using System.Runtime.InteropServices;

namespace LibCD {
    namespace XA {

        [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
        public struct DiscLabel {
            public static readonly string ExpectedSignature = "CD-XA001";

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string Signature; //("CD-XA001")
            public ushort Flags; //0-15 == 0, reserved
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
            public string StartupDirectoryName;
            public ulong Reserved;

            public bool Valid => Signature == ExpectedSignature;

            public static bool IsPresent(ref ISO9660.PrimaryVolumeDescriptor descriptor) {
                var data = descriptor.ApplicationUseArea;
                for (int i = 0; i < 8; ++i) {
                    if (data[i] != (byte)ExpectedSignature[i]) {
                        return false;
                    }
                }
                return true;
            }
        }
    }

    public static partial class PrimaryVolumeDescriptorExtensions {
        public static XA.DiscLabel GetXADiscLabel(this ISO9660.PrimaryVolumeDescriptor descriptor) {
            return Serialization.StructFromArray<XA.DiscLabel>(descriptor.ApplicationUseArea, 141);
        }
    }
}
