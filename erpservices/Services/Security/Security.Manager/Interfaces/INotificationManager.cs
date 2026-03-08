using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface INotificationManager
    {
        Task<int> GetNotificationCount();
        Task<IEnumerable<Dictionary<string, object>>> GetNotificationList(int top = 100000000);
        Task<IEnumerable<Dictionary<string, object>>> GetNotificationByAPType(int id);
        Task<IEnumerable<Dictionary<string, object>>> GetNotificationListForNFA(int top = 100000000);
        Task<List<NotificationDto>> GetNotificationListForNotification();
        Task<IEnumerable<Dictionary<string, object>>> GetAPTypeList(); 
    }
}
