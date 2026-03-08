using System;
using System.Collections.Generic;
using System.Text;

namespace Manager.Core.CommonDto
{
    public class ApprovalMemberDto
    {
        public long ID { get; set; }
        public string ROLE_NAME { get; set; }
        public long ROLE_ID { get; set; }
        public int SEQUENCE_NUMBER { get; set; }
        public string APPROVER_USER_ID { get; set; }
        public string APPROVER_WALLET { get; set; }
        public int ACTIVITY_TYPE_ID { get; set; }
    }
}
