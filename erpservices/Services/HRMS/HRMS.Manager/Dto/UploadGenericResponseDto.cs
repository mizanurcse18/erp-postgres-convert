using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class UploadGenericResponseDto<T> where T : class
    {
        public List<T> dList { get; set; }
        public bool uploadStatus { get; set; } = false;
        public string filePath { get; set; } = string.Empty;
        public string Message { get; internal set; }
    }
}
