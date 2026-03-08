using API.Core.Interface;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace API.Core.Hubs
{
    public class RemoteAttendanceNotificaitonHub : Hub<IRemoteAttendanceNotificaitonClient>
    {
    }
}
