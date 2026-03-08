using API.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace API.Core.Interface
{
    public interface IRemoteAttendanceNotificaitonClient
    {
        Task ReceiveRemoteAttendance();
    }
}
