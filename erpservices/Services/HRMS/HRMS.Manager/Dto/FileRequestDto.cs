using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class FileRequestDto
    {
        public IFormFile File { get; set; }
        public bool IsContinue { get; set; } = false;
    }
}
