using API.Core.Interface;
using Core.AppContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace API.Core.Hubs
{
    //[Authorize]
    public class NotificaitonHub : Hub<INotificaitonClient>
    {
        //private static readonly ConcurrentDictionary<string, UserHubModels> Users =
        //new ConcurrentDictionary<string, UserHubModels>(StringComparer.InvariantCultureIgnoreCase);
        //public override Task OnConnectedAsync()
        //{
        //    var username = AppContexts.User.UserName;
        //    var connectId = Context.UserIdentifier;
        //    return base.OnConnectedAsync();
        //}
    }
}
