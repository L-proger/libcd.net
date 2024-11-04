using System.Runtime.InteropServices;

namespace LibCD.Sony.Ps1 {
    public class LicenseStringDescriptor {
        public const long SectorIndex = 4;

        public string LicenseString { get; private set; }
        public LicenseStringDescriptor(string licenseString) {
            LicenseString = licenseString;
        }

        public static LicenseStringDescriptor Read(CdSectorStream stream) {
            stream.SeekToSector(SectorIndex);

            var sector = new CdSector(stream.ReadSector());
            var sectorData = sector.GetData().ToArray();

            var handle = GCHandle.Alloc(sectorData, GCHandleType.Pinned);
            var result = Marshal.PtrToStringAnsi(handle.AddrOfPinnedObject());
            handle.Free();

            return new LicenseStringDescriptor(result);
        }

        public override string ToString() {
            return LicenseString;
        }
    }
}
