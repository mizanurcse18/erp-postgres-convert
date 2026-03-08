using System;
using System.Collections.Generic;
using System.Text;

namespace HRMS.Manager.Dto
{
    public class DocumentUploadResponseDto
    {
        public int DUID { get; set; }
        public Object ApiResponse { get; set; }
        public int ApiStatus { get; set; }
    }
}
