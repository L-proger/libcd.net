using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CdReader.ISO9660 {
    public class DirectoryRecordStreamMarshaler : Serialization.IStreamMarshaler {
        public object Read(Stream s) {
            DirectoryRecord result = new DirectoryRecord();

            result.DirectoryRecordLength = Serialization.StreamReader.ReadByte(s);
            result.XARecordLength = Serialization.StreamReader.ReadByte(s);
            result.DataLogicalBlockNumber = Serialization.StreamReader.ReadStruct<LbeUInt32>(s);
            result.DataSizeInBytes = Serialization.StreamReader.ReadStruct<LbeUInt32>(s);
            result.RecordingTimestamp = Serialization.StreamReader.ReadStruct<Timestamp>(s);

            result.Flags = (DirectoryRecordFlags)Serialization.StreamReader.ReadByte(s);
            result.FileUnitSize = Serialization.StreamReader.ReadByte(s);
            result.InterleaveGapSize = Serialization.StreamReader.ReadByte(s);

            result.VolumeSequenceNumber = Serialization.StreamReader.ReadStruct<LbeUInt16>(s);
            result.LengthOfName = Serialization.StreamReader.ReadByte(s);

            result.Name = Serialization.StreamReader.ReadStringAnsi(s, result.LengthOfName);

            if (result.LengthOfName % 2 == 0) {
                Serialization.StreamReader.ReadByte(s);
            }


            var lenSu = result.DirectoryRecordLength - (33 + result.LengthOfName + (result.LengthOfName % 2 == 0 ? 1 : 0));
            if (lenSu != 0) {
                result.SystemUse = new byte[lenSu];
                s.Read(result.SystemUse, 0, result.SystemUse.Length);
            }
            return result;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CdXaOwnerId {
        public ushort GroupID; //MSB first
        public ushort UserID;  //MSB first
    }

    [Flags]
    public enum CdXaDirectoryRecordAttributes1 : byte {
        OwnerRead = 1 << 0,
        Reserved = 1 << 1,
        OwnerExecute = 1 << 2,
        Reserved1 = 1 << 3,
        GroupRead = 1 << 4,
        Reserved2 = 1 << 5,
        GroupExecute = 1 << 6,
        Reserved3 = 1 << 7
    }

    [Flags]
    public enum CdXaDirectoryRecordAttributes2 : byte {
        WorldRead = 1 << 0,
        Reserved4 = 1 << 1,
        WorldExecute = 1 << 2,
        FileContainsForm1Sectors = 1 << 3,
        FileContainsForm2Sectors = 1 << 4,
        FileContainsFnterleavedSectors = 1 << 5,
        CD_DAFile = 1 << 6,
        DirectoryFile = 1 << 7
    }



    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CdXaDirectoryRecordSystemUseInformation {
        public CdXaOwnerId OwnerID;
        public CdXaDirectoryRecordAttributes2 Attributes2;
        public CdXaDirectoryRecordAttributes1 Attributes1;
        public byte Signature1; //'X'
        public byte Signature2; //'A'
        public byte FileNumber;
        public byte Reserved1;
        public byte Reserved2;
        public byte Reserved3;
        public byte Reserved4;
        public byte Reserved5;

        public bool Valid {
            get {
                return Signature1 == (byte)'X' && Signature2 == (byte)'A';
            }
        }

        public static bool IsValidRecord(byte[] data) {
            if(data == null || data.Length != 14) {
                return false;
            }
            return data[6] == (byte)'X' && data[7] == (byte)'A';
        }
    }


    public enum DirectoryRecordFlags : byte {
        Hidden = 1 << 0,
        IsDirectory = 1 << 1,
        IsAssociatedFile = 1 << 2,
        Record = 1 << 3, //TODO: read about it
        ProtectionSpecified = 1 << 4, //Set if permissions / protections / rights specified
        Reserved1 = 1 << 5,
        Reserved2 = 1 << 6,
        HasNextRecord = 1 << 7	//Set if not the final record for the file. This only occurs is the file is split over multiple Extents
    }

    [Serialization.StreamMarshaler(marshalerType: typeof(DirectoryRecordStreamMarshaler))]
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct DirectoryRecord {
        public byte DirectoryRecordLength; //(LEN_DR) (33+LEN_FI+pad+LEN_SU) (0=Pad)
        public byte XARecordLength; //(usually 00h)
        public LbeUInt32 DataLogicalBlockNumber;
        public LbeUInt32 DataSizeInBytes;
        public Timestamp RecordingTimestamp; //(yy-1900,mm,dd,hh,mm,ss,timezone)
        public DirectoryRecordFlags Flags; //(usually 00h=File, or 02h=Directory)

        public byte FileUnitSize; //(usually 00h)
        public byte InterleaveGapSize; //(usually 00h)
        public LbeUInt16 VolumeSequenceNumber; //(2x16bit, usually 0001h)

        public byte LengthOfName; // (LEN_FI)

        public string Name;

        public byte[] SystemUse;

        public CdXaDirectoryRecordSystemUseInformation? CdXaSystemUse {
            get {
                if (!CdXaDirectoryRecordSystemUseInformation.IsValidRecord(SystemUse)) {
                    return null;
                }

                var handle = GCHandle.Alloc(SystemUse, GCHandleType.Pinned);
                var result = Marshal.PtrToStructure<CdXaDirectoryRecordSystemUseInformation>(handle.AddrOfPinnedObject());
                handle.Free();
                return result;
            }
        }
        /*                       
        21h LEN_FI File/Directory Name ("FILENAME.EXT;1" or "DIR_NAME" or 00h or 01h)
        xxh 0..1   Padding Field (00h) (only if LEN_FI is even)
        xxh LEN_SU System Use (LEN_SU bytes) (see below for CD-XA disks)
        */
    }

}
