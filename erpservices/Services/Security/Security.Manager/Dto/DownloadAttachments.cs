using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    public class DownloadAttachments
    {
        public string AID { get; set; }
        public int FUID { get; set; }
        public int ID
        {
            get
            {
                int fuid;
                if (int.TryParse(AID, out fuid))
                {
                    return fuid;
                }
                else
                {
                    return 0;
                }
            }
        }
        public string AttachedFile { get; set; }
        public string Type { get; set; }
        public string OriginalName { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public int ReferenceId { get; set; }
        public decimal Size { get; set; }
        public string Description { get; set; }
        public string TableName { get; set; }
        public string BaseUrl { get; set; }
        public string Module { get; set; }
    }
}
