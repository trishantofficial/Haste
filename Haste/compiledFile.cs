using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Haste
{
    class compiledFile
    {
        String name;
        List<Haste.File> downloadFiles;
        public compiledFile(String name, List<Haste.File> downloadFiles)
        {
            setName(name);
            setDownloadFiles(downloadFiles);
            Console.WriteLine(getName());
        }
        public void compile()
        {
            foreach (Haste.File downloadFile in this.downloadFiles)
            {
                try
                {
                    byte[] fileData = System.IO.File.ReadAllBytes(downloadFile.getName());
                    var stream = new FileStream(this.getName(), FileMode.Append);
                    try
                    {
                        stream.Write(fileData, 0, fileData.Length);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Couldn't write file " + downloadFile.getPartNumber());
                    }
                    finally
                    {
                        stream.Close();
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Couldn't read file " + downloadFile.getPartNumber());
                }
            }
        }
        public void createCompiledFile()
        {
            if (!System.IO.File.Exists(this.name))
            {
                try
                {
                    FileStream fs = System.IO.File.Create(this.name);
                }
                catch(Exception e)
                {
                    Console.WriteLine("Error creating File...");
                }
            }
            else
            {
                Console.WriteLine("File already exists....");
                Console.WriteLine("Rewrite??");
                String rewrite = Console.ReadLine();
                if(rewrite == "y" || rewrite == "Y")
                {
                    System.IO.File.Delete(name);
                    FileStream fs = System.IO.File.Create(this.name);
                }
            }
        }
        public void setName(String name)
        {
            this.name = name;
        }
        public String getName()
        {
            return this.name;
        }
        public void setDownloadFiles(List<Haste.File> downloadFiles)
        {
            this.downloadFiles = downloadFiles;
        }
        public List<Haste.File> getDownloadFiles()
        {
            return this.downloadFiles;
        }
    }
}
