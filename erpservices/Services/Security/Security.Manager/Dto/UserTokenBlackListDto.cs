using AutoMapper;
using DAL.Core.EntityBase;
using System;
using Security.DAL.Entities;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    class UserTokenBlackListDto : EntityBase
    {
        public int UTBID { get; set; }
        public int UserID { get; set; }
        public string Token { get; set; }
    }
}
