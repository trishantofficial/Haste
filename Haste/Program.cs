using System;
using System.Diagnostics;
using System.IO;
using static System.Console;

namespace Haste
{
    class Program
    {
        string _path;
        private Download _downloader;
        void BeginProgram()
        {
            WriteLine("Starting program!");
            Write("Enter the URL: ");
            try
            {
                _downloader = new Download(ReadLine());
                WriteLine("Please wait while we prep the download!");
                _downloader.CheckUrl();
                _path = string.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    _downloader.FileName);
                WriteLine("Current Path: {0}", _path);
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
            try
            {
                if (File.Exists(_path.Trim()))
                {
                    WriteLine("File already exists!\nDo you want to delete the file\n1.Yes\n2.No");
                    int choice = int.Parse(ReadLine());
                    if (choice == 1)
                        File.Delete(_path.Trim());
                    else
                    {
                        Write("Specify other location: ");
                        _path = ReadLine();
                    }
                }

                if (_path != null && File.Exists(_path.Trim() + ".tmp"))
                    File.Delete(_path.Trim() + ".tmp");
                if (_path != null)
                {
                    // Set the download path
                    _downloader.DownloadPath = _path.Trim() + ".tmp";

                    // Start the download
                    _downloader.BeginDownload();

                    // Once the download completes. Remove the .tmp extension
                    if (_downloader.Status == DownloadStatus.Completed)
                        File.Move(_path.Trim() + ".tmp", _path.Trim());
                }
            }
            catch (Exception e)
            {
                WriteLine(e.Message);
                Debug.WriteLine(e.StackTrace);
            }
        }
        static void Main()
        {
            new Program().BeginProgram();
        }
    }
}
