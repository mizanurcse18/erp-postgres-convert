using Core;
using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Newtonsoft.Json;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Security.Manager.Dto
{
    //[AutoMap(typeof(UserProfile)), Serializable]
    public class UserAndProfileDto
    {
        //public UserAndProfileDto()
        //{
        //    UserDto = new UserDto();
        //    UserProfileDto = new UserProfileDto();
        //}
        public UserDto UserDto { get; set; }       
        public UserProfileDto UserProfileDto { get; set; } 
    }
}
