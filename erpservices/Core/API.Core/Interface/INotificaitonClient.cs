using API.Core.Models;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace API.Core.Interface
{
    public interface INotificaitonClient
    {
        Task SendProgressAsync(string message, double percent);
        Task ExportCompletedAsync(string message, object data);
        Task ReceiveNotification(string message);
        Task ReceiveData(Dictionary<string, string> data);
        Task ReceiveNotificationUserWise(string notificationType, string users);
        Task ReceiveNotificationUserWise(string notificationType, string users, string message,string currentNotificationEmployeeId);
    }
}
