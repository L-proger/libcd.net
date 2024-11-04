using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LibCD.ISO9660 {
    public struct VolumeDescriptorSetTerminator {
        public VolumeDescriptorHeader Header;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2041)]
        public byte[] Reserved;
    }
}
