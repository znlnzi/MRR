using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace MRAnalysis.Common
{
    public class ZipHelper
    {
        public string UnZipFile(string zipFilePath, string directoryPath)
        {
            string fileName = string.Empty;
            if (!File.Exists(zipFilePath))
            {
                Console.WriteLine("Cannot find file '{0}'", zipFilePath);
                return fileName;
            }

            using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFilePath)))
            {

                ZipEntry theEntry;
                while ((theEntry = s.GetNextEntry()) != null)
                {

                    Console.WriteLine(theEntry.Name);

                    string directoryName = directoryPath;
                    fileName = directoryPath + "\\" + theEntry.Name;

                    // create directory
                    if (!Directory.Exists(directoryName))
                    {
                        Directory.CreateDirectory(directoryName);
                    }

                    if (fileName != string.Empty)
                    {
                        using (var streamWriter = File.Create(fileName))
                        {
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                var size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }

            return fileName;
        }
    }
}
