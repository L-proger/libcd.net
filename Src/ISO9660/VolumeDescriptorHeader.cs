using System.Runtime.InteropServices;

namespace CdReader.ISO9660 {
    public enum VolumeDescriptorType : byte {
        BootRecord = 0,
        PrimaryVolumeDescriptor = 1,
        SupplementaryVolumeDescriptor = 2,
        VolumePartitionDescriptor = 3,
        //Reserved
        VolumeDescriptorSetTerminator = 0xff
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct VolumeDescriptorHeader {
        public VolumeDescriptorType VolumeDescriptorType;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 5)]
        public string StandardIdentifier; //("CD001")
        public byte VolumeDescriptorVersion; //(01h=Standard)
    }
}
