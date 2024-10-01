using System;

namespace ABCRetailApp.Models
{
    public class ContractModel
    {
        public string? FileName { get; set; }
        public long Size { get; set; }
        public DateTimeOffset? UploadedDate { get; set; }
        public string? FilePath { get; set; }

        public string DisplaySize
        {
            get
            {
                if (Size >= 1024 * 1024)
                    return $"{Size / 1024 / 1024} MB";
                if (Size >= 1024)
                    return $"{Size / 1024} KB";
                return $"{Size} Bytes";
            }
        }
    }
}
