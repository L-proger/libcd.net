using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibCD.ISO9660 {
    public struct BootRecordDescriptor {
        public VolumeDescriptorHeader Header;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string BootSystemIdentifier; //a-characters

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string BootIdentifier; //a-characters

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1977)]
        public byte[] BootSystemUse; 
    }
}
