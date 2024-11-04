using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrReader {
    public class CdSectorStream {
        public const int PhysicalSectorSize = 2352;
        public long Length { get; private set; }
        public long SectorsCount { get; private set; }
        public long CurrentSector { get; private set; }

        private Stream _dataStream;


        public CdSectorStream(Stream dataStream) {
            _dataStream = dataStream;
            Length = dataStream.Seek(0, SeekOrigin.End);
            dataStream.Seek(0, SeekOrigin.Begin);
            if (Length % PhysicalSectorSize != 0) {
                throw new Exception("Unexpected image size: not aligned to physical sector size");
            }

            SectorsCount = Length / PhysicalSectorSize;
        }

        public void SeekToSector(long sector) {
            if(sector < 0 || sector >= SectorsCount) {
                throw new Exception("Invalid sector index");
            }

            _dataStream.Seek(sector * PhysicalSectorSize, SeekOrigin.Begin);
            CurrentSector = sector;
        }

        public byte[] ReadSector() {
            byte[] sectorData = new byte[PhysicalSectorSize];
            _dataStream.Read(sectorData);
            CurrentSector++;
            return sectorData;
        }

        public void ReadSector(byte[] sectorDataBuffer) {
            if(sectorDataBuffer.Length != PhysicalSectorSize) {
                throw new Exception("Invalid sector buffer size");
            }
            int readLength = _dataStream.Read(sectorDataBuffer);
            if(readLength != sectorDataBuffer.Length) {
                _dataStream.Seek(-readLength, SeekOrigin.Current);
                throw new Exception("Failed to read sector");
            }
            CurrentSector++;
        }

    }
}
