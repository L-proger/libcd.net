namespace LibCD.ISO9660 {
    public class FileSystem {

        public class File {
            public long LBA;
            public long Size;
            public string Name;

            public override string ToString() {
                return Name;
            }

            public CdByteStream OpenRead(CdSectorStream cd) {
                cd.SeekToSector(LBA);
                return new CdByteStream(cd);
            }
        }

        public class Directory {
            public string Name;
            public List<Directory> Directories = new List<Directory>();
            public List<File> Files = new List<File>();
        }


        public Directory RootDirectory { get; private set; }

        public CdSectorStream SectorStream { get; private set; }
        public PrimaryVolumeDescriptor PrimaryVolumeDescriptor { get; private set; }
        public FileSystem(Stream cdSourceStream) {
            SectorStream = new CdSectorStream(cdSourceStream);

            SectorStream.SeekToSector(16);
            var byteStream = new CdByteStream(SectorStream);
            PrimaryVolumeDescriptor = PrimaryVolumeDescriptor.Read(byteStream);



            RootDirectory = new Directory();
            RootDirectory.Name = "";
            ScanDirectory(RootDirectory, PrimaryVolumeDescriptor.RootDirectoryRecord);
           

            //stream.SeekToSector(volumeDescriptor.RootDirectoryRecord.DataLogicalBlockNumber.Value);
            //var byteStream2 = new CdByteStream(stream);
        }


        private void ScanDirectory(Directory dir, DirectoryRecord dirRecord) {
            var childRecords = ReadDirectoryFile(PrimaryVolumeDescriptor.RootDirectoryRecord);

            if(childRecords.Count < 2) {
                throw new Exception("Unexpected directory structure");
            }



            for(int i = 2; i <  childRecords.Count; ++i) {
                var record = childRecords[i];
                if((record.Flags & DirectoryRecordFlags.IsDirectory) == DirectoryRecordFlags.IsDirectory) {
                    var childDir = new Directory();
                    childDir.Name = record.Name;
                    ScanDirectory(childDir, record);
                    dir.Directories.Add(childDir);
                } else {
                    File childFile = new File();
                    childFile.Name = record.Name;
                    childFile.Size = record.DataSizeInBytes.Value;
                    childFile.LBA = record.DataLogicalBlockNumber.Value;
                    dir.Files.Add(childFile);
                }
            }
        }

        private List<DirectoryRecord> ReadDirectoryFile(DirectoryRecord parent) {
            List<DirectoryRecord> result = new List<DirectoryRecord>();

            SectorStream.SeekToSector(parent.DataLogicalBlockNumber.Value);
            var byteStream = new CdByteStream(SectorStream);


   
            while(byteStream.Position < parent.DataSizeInBytes.Value) {
                if(byteStream.ReadByte() == 0) {
                    byteStream.Seek(byteStream.CurrentSectorDataLength - byteStream.CurrentSectorLocalPosition, SeekOrigin.Current);
                } else {
                    byteStream.Seek(-1, SeekOrigin.Current);
                    result.Add(Serialization.StreamReader.Read<DirectoryRecord>(byteStream));
                }
            }

            return result;
        }

    }
}
