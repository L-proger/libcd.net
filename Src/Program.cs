using System.Runtime.CompilerServices;

namespace LibCD {
    class App {
        static string CalledFileDir([CallerFilePath] string path = null) {
            return System.IO.Path.GetDirectoryName(path);
        }

        static string TestDataDir {
            get {
                return Path.Combine(CalledFileDir(), "..", "TestData");

            }
        }

        static string ImageFilePath {
            get { return Path.Combine(TestDataDir, "Image.bin"); }
        }


        static void PrintFileSystem(ISO9660.FileSystem.Directory dir, string fullPath) {
            fullPath += dir.Name;
            Console.WriteLine(fullPath);

            foreach(var file in dir.Files) {
                Console.WriteLine($"{fullPath}/{file.Name} | LBA: {file.LBA}  SIZE: {file.Size}");
            }

            foreach(var childDir in dir.Directories) {
                PrintFileSystem(childDir, fullPath + "/");
            }
        }

        public static void Main() {
            Console.WriteLine(ImageFilePath);


            var fs = new ISO9660.FileSystem(File.OpenRead(ImageFilePath));

            PrintFileSystem(fs.RootDirectory, "");



            CdRomXaSubHeader? lastSubHeader = null;
            
            var stream = fs.SectorStream;
            
            var of = File.OpenWrite("E:/CdLog.txt");
            var wr = new StreamWriter(of);
            
            stream.SeekToSector(0); 
            for (int i = 0; i < fs.PrimaryVolumeDescriptor.VolumeSpaceSize.Value; ++i) {
                var s = new CdSector(stream.ReadSector());
                if(!lastSubHeader.HasValue || lastSubHeader.Value != s.SubHeader) {

                   

                    wr.WriteLine($"SubHeader changed at sector: {i} Flags:{s.SubHeader.SubModeFlags.ToString()}, FileNumber:{s.SubHeader.FileNumber}, ChannelNumber:{s.SubHeader.ChannelNumber}, CodingInformation:{s.SubHeader.CodingInformation}");
                    Console.WriteLine($"SubHeader changed at sector: {i} NewFlags:{s.SubHeader.SubModeFlags.ToString()}");
                    Console.Out.Flush();

                    if (((s.SubHeader.SubModeFlags & CdRomXaSubModeFlags.Data) == CdRomXaSubModeFlags.Data) && ((s.SubHeader.SubModeFlags & CdRomXaSubModeFlags.Form) == CdRomXaSubModeFlags.Form)) {
                        throw new Exception("Invalid submode");
                    }
                   //if (((s.SubHeader.SubModeFlags & CdRomXaSubModeFlags.Audio) == CdRomXaSubModeFlags.Audio) ) {
                   //    throw new Exception("Invalid submode");
                   //}

                        //if (((s.SubHeader.SubModeFlags & CdRomXaSubModeFlags.Audio) == CdRomXaSubModeFlags.Audio) && ((s.SubHeader.SubModeFlags & CdRomXaSubModeFlags.Form) != CdRomXaSubModeFlags.Form)) {
                        //    throw new Exception("Invalid submode");
                        //}


                        lastSubHeader = s.SubHeader;
                }
            
                if(i == 39359) {
                    Console.Out.Flush();
                    Console.WriteLine();
                }
            }
            wr.Flush();
            of.Close();


            foreach (var file in fs.RootDirectory.Files) {
                var outDir = @"E:\CdContent\Extracted";

                var fn = file.Name;

                if (!fn.StartsWith("HILL")) {
                    continue;
                }

                if (fn.EndsWith(";1")) {
                    fn = fn.Substring(0, fn.Length - 2);
                }
                var outPath = Path.Combine(outDir, file.Name);

                var inStream = file.OpenRead(fs.SectorStream);

                Console.WriteLine($"Start reading file {file.Name} at sector {inStream.CurrentSector}:{inStream.CurrentSectorLocalPosition}");
                var writeSize = file.Size;

                var outStream = System.IO.File.OpenWrite(outPath);

                byte[] blockBuffer = new byte[2048];
                while(writeSize > 0) {
                    var chunkSize = Math.Min(writeSize, blockBuffer.Length);
                    inStream.Read(blockBuffer, 0, (int)chunkSize);

                    outStream.Write(blockBuffer, 0, (int)chunkSize);
                    writeSize -= chunkSize;
                }

                Console.WriteLine($"  End reading file {file.Name} at sector {inStream.CurrentSector}:{inStream.CurrentSectorLocalPosition}");
                outStream.Close();
            }

            Console.WriteLine("Done");

        }

    }

}