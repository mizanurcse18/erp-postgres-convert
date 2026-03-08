using DAL.Core.EntityBase;
using Manager.Core.Mapper;
using Security.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Security.Manager.Dto
{
    [AutoMap(typeof(UserLogTracker)), Serializable]
    public class UserLogTrackerDto : EntityBase
    {
        private int logedID;
        public int LogedID
        {
            get => logedID;
            set
            {
                if (PropertyChanged(logedID, value))
                    logedID = value;
            }
        }

        private int userID;
        public int UserID
        {
            get => userID;
            set
            {
                if (PropertyChanged(userID, value))
                    userID = value;
            }
        }

        [NonSerialized]
        private DateTime logInDate;
        public DateTime LogInDate
        {
            get => logInDate;
            set
            {
                if (PropertyChanged(logInDate, value))
                    logInDate = value;
            }
        }

        [NonSerialized]
        private DateTime? logOutDate;
        public DateTime? LogOutDate
        {
            get => logOutDate;
            set
            {
                if (PropertyChanged(logOutDate, value))
                    logOutDate = value;
            }
        }

        [NonSerialized]
        private bool? isLive;
        public bool? IsLive
        {
            get => isLive;
            set
            {
                if (PropertyChanged(isLive, value))
                    isLive = value;
            }
        }

        [NonSerialized]
        private string iPAddress;
        public string IPAddress
        {
            get => iPAddress;
            set
            {
                if (PropertyChanged(iPAddress, value))
                    iPAddress = value;
            }
        }
    }
}
