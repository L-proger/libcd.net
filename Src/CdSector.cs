using System.Runtime.InteropServices;

namespace LibCD {
    [Flags]
    public enum CdRomXaSubModeFlags : byte {
        EndOfRecord = 0x01,
        Video = 0x02,
        Audio = 0x04,
        Data = 0x08,
        Trigger = 0x010,
        Form = 0x20,
        RealTimeSector = 0x40,
        EndOfFile = 0x80
    }

    [StructLayout(layoutKind: LayoutKind.Sequential)]
    public struct CdRomXaSubHeader {
        public byte FileNumber;
        public byte ChannelNumber;
        public CdRomXaSubModeFlags SubModeFlags;
        public byte CodingInformation;
        public byte FileNumber1;
        public byte ChannelNumber1;
        public CdRomXaSubModeFlags SubModeFlags1;
        public byte CodingInformation1;

        public static bool operator ==(CdRomXaSubHeader a, CdRomXaSubHeader b) {
            return (a.FileNumber == b.FileNumber) && (a.ChannelNumber == b.ChannelNumber) && (a.SubModeFlags == b.SubModeFlags) && (a.CodingInformation == b.CodingInformation);
        }

        public static bool operator !=(CdRomXaSubHeader a, CdRomXaSubHeader b) {
            return (a.FileNumber != b.FileNumber) || (a.ChannelNumber != b.ChannelNumber) || (a.SubModeFlags != b.SubModeFlags) || (a.CodingInformation != b.CodingInformation);
        }
    }

    public class CdSector {
        private byte[] _data;
        private static byte[] ReferenceSyncPattern = new byte[] { 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00 };

        public bool SyncPatternValid {
            get {
                for(int i = 0; i < ReferenceSyncPattern.Length; ++i) {
                    if (_data[i] != ReferenceSyncPattern[i]) {
                        return false;
                    }
                }
                return true;
            }
        }

        public byte Minute { get { return _data[12]; } }
        public byte Second { get { return _data[13]; } }
        public byte Sector { get { return _data[14]; } }
        public byte Mode { get { return _data[15]; } }

        public CdRomXaSubHeader SubHeader {
            get {
                byte[] d = new byte[8];
                Array.Copy(_data, 16, d, 0, d.Length);
                var h = GCHandle.Alloc(d, GCHandleType.Pinned);
                var result = Marshal.PtrToStructure<CdRomXaSubHeader>(h.AddrOfPinnedObject());
                return result;
            }
        }

        public byte CdXaForm { 
            get {
                return ((SubHeader.SubModeFlags & CdRomXaSubModeFlags.Form) == CdRomXaSubModeFlags.Form) ? (byte)2 : (byte)1;
      
               //if(Mode != 2) {
               //    throw new Exception("CD-XA form defined only in mode 2");
               //}
               //
               //return (byte)(((_data[18] & 0x20) == 0) ? 1 : 2);
            } 
        }

        public CdSector(byte[] data) {
            _data = data;
        }

        public ReadOnlySpan<byte> GetData() {
            if(Mode == 1) {
                return new ReadOnlySpan<byte>(_data, 0x10, 2048);
            }else if(Mode == 2) {

                if(CdXaForm == 1) {
                    return new ReadOnlySpan<byte>(_data, 0x18, 2048);
                } else {
                    return new ReadOnlySpan<byte>(_data, 0x18, 2324);
                }

            } else {
                throw new Exception("Unknown sector type");
            }
        }
    }
}
