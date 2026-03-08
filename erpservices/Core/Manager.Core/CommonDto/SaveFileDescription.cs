using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class SaveFileDescription
    {
        public string FileOriginalName { get; set; }
        public string SavedFileName { get; set; }
        public string FileExtention { get; set; }
        public string FullPath { get; set; }
        public string FileRelativePath { get; set; }
        public long FileSize { get; set; } = 0;
        public string FolderName { get; set; }

    }
}
