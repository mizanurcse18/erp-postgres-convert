using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IEmailManager
    {        
        Task SendEmail(EmailDto emailData);
    }
}
