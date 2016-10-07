using System;

namespace Haste.Interfaces
{
    public interface IDownloader
    {
        Uri UniformResourecIndicator { get; set; }

        string Url { get; set; }
        string FileName { get; set; }
        string DownloadPath { get; set; }

        long TotalSize { get; set; }
        long StartPoint { get; set; }
        long EndPoint { get; set; }
        long DownloadedSize { get; set; }

        DownloadStatus Status { get; }
        
        bool IsRangeSupported { get; set; }
        bool HasChecked { get; set; }

        void CheckUrl();
        void BeginDownload();
    }
}