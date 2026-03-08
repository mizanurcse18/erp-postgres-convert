using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    public class PrismUser_Dto
    {
        public int UserPositionID { get; set; }
        public IFormFile File { get; set; }
    }
}
