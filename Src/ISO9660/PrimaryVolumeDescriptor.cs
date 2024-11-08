﻿using System.Runtime.InteropServices;

namespace LibCD.ISO9660 {
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    public struct PrimaryVolumeDescriptor
    {
        public VolumeDescriptorHeader Header;
        public byte Unused; //(00h)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string SystemIdentifier; //(a-characters)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string VolumeIdentifier; //(d-characters) (max 8 chars for PSX?)

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] Unused1; //(00h)

        public LbeUInt32 VolumeSpaceSize; //(2x32bit, number of logical blocks)  LBEF

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] Unused2; //(00h)


        public LbeUInt16 VolumeSetSize; //(2x16bit) (usually 0001h)  LBEF
        public LbeUInt16 VolumeSequenceNumber; //(2x16bit) (usually 0001h)  LBEF
        public LbeUInt16 LogicalBlockSizeInBytes; //(2x16bit) (usually 0800h) (1 sector) LBEF
        public LbeUInt32 PathTableSizeInBytes; //(2x32bit) (max 800h for PSX)  LBEF

        public uint PathTableLBlockNumber; //(32bit little-endian)  LEF
        public uint PathTableLOptionalBlockNumber; //(32bit little-endian) (or 0=None)  LEF
        public uint PathTableMBlockNumber; //(32bit big-endian)  BEF
        public uint PathTableMOptionalBlockNumber; //(32bit big-endian) (or 0=None)  BEF

        public DirectoryRecord RootDirectoryRecord; //(see next chapter)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string VolumeSetIdentifier; //(d-characters) (usually empty)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string PublisherIdentifier; //(a-characters) (company name)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string DataPreparerIdentifier; //(a-characters) (empty or other)
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string ApplicationIdentifier; //(a-characters) 
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
        public string CopyrightFilename; //("FILENAME.EXT;VER") (empty or text)
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
        public string AbstractFilename; //("FILENAME.EXT;VER") (empty)
        
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 37)]
        public string BibliographicFilename; //("FILENAME.EXT;VER") (empty)

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string VolumeCreationTimestamp; //("YYYYMMDDHHMMSSFF",timezone)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string VolumeModificationTimestamp; //("0000000000000000",00h)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string VolumeExpirationTimestamp; //("0000000000000000",00h)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 17)]
        public string VolumeEffectiveTimestamp; //("0000000000000000",00h)

        public byte FileStructureVersion; //(01h=Standard)
        public byte ReservedForFuture; //(00h-filled)

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        public byte[] ApplicationUseArea; 

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 653)]
        public byte[] ReservedForFuture2; //(00h-filled)
    }
}
