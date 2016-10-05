using System;
using System.Collections.Generic;
using System.IO;

namespace Haste
{
    class CompiledFile
    {
        public string Name { get; private set; }
        public List<HasteFile> DownloadFiles { get; private set; }


        public CompiledFile(string name, List<HasteFile> downloadFiles)
        {
            Name = name;
            DownloadFiles = downloadFiles;
            Console.WriteLine(Name);
        }
        public void Compile()
        {
            foreach (HasteFile downloadFile in DownloadFiles)
            {
                try
                {
                    FileMode fileToken = (File.Exists(Name) ? FileMode.Append : FileMode.Create);
                    byte[] fileData = File.ReadAllBytes(downloadFile.Name);
                    var stream = new FileStream(Name, fileToken);
                    try
                    {
                        stream.Write(fileData, 0, fileData.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Couldn't write file " + downloadFile.PartNumber);
                    }
                    finally
                    {
                        stream.Close();
                        File.Delete(downloadFile.Name);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Couldn't read file " + downloadFile.PartNumber);
                }
            }
        }
    }
}
