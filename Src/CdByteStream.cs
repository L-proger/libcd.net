using CrReader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CdReader {
    public class CdByteStream : System.IO.Stream {
        public CdSectorStream Source { get; private set; }
        public long StartSector { get; private set; }
        public long CurrentSector { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => throw new NotImplementedException();

        public override long Position { get { return _position; } set => throw new NotImplementedException(); }
        private long _position;
        private long _localToSectorPosition;

        private byte[] _sectorData;

        public long CurrentSectorLocalPosition { get { return _localToSectorPosition; } }
        public long CurrentSectorDataLength { get { return _sectorData.Length; } }

        public CdByteStream(CdSectorStream src) {
            Source = src;
            StartSector = src.CurrentSector;
            CurrentSector = src.CurrentSector;
            ReadCurrentSector();
        }

        public override void Flush() {
            
        }

        public byte[] Read(int count) {
            byte[] result = new byte[count];
            Read(result, 0, count);
            return result;
        }

        public override int Read(byte[] buffer, int offset, int count) {
            int totalReadCount = 0;

            while(count > 0) {
                if(_localToSectorPosition == _sectorData.Length) {
                    CurrentSector++;
                    ReadCurrentSector();
                }

                var readChunkSize = (int)Math.Min(count, _sectorData.Length - _localToSectorPosition);
                Array.Copy(_sectorData, _localToSectorPosition, buffer, offset, readChunkSize);
                offset += readChunkSize;
                count -= readChunkSize;
                totalReadCount += readChunkSize;
                _localToSectorPosition += readChunkSize;
                _position += readChunkSize;
            }

            return totalReadCount;
        }

        private void ReadCurrentSector() {
            Source.SeekToSector(CurrentSector);
            _sectorData = new CdSector(Source.ReadSector()).GetData().ToArray();
            _localToSectorPosition = 0;
        }

        public override long Seek(long offset, SeekOrigin origin) {
            if(offset == 0) {
                return _position;
            }

            if(origin == SeekOrigin.Current) {
                if(offset > 0) {
                    while (offset != 0) {
                        if(_localToSectorPosition == _sectorData.Length) {
                            CurrentSector++;
                            ReadCurrentSector();
                        }

                        var skipChunkSize = Math.Min(offset, _sectorData.Length - _localToSectorPosition);
                        offset -= skipChunkSize;
                        _localToSectorPosition += skipChunkSize;
                        _position += skipChunkSize;
                    }
                    
                } else {
                    offset = -offset;
                    while (offset != 0) {
                        if (_localToSectorPosition == 0) {
                            CurrentSector--;
                            ReadCurrentSector();
                            _localToSectorPosition = _sectorData.Length - 1;
                        }

                        var skipChunkSize = Math.Min(offset, _localToSectorPosition);
                        offset -= skipChunkSize;
                        _localToSectorPosition -= skipChunkSize;
                        _position -= skipChunkSize;
                    }
                }
                return _position;

            } else if(origin == SeekOrigin.Begin) {
                throw new NotImplementedException();
            } else {
                throw new NotImplementedException();
            }
        }

        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotImplementedException();
        }
    }
}
