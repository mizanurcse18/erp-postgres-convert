using Core;
using Mail.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mail.Manager.Interfaces
{
    public interface IMailConfigurationManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetMailConfigurationListDic();
        Task<MailConfigurationDto> GetMailConfiguration(int mailConfigurationId);

        void SaveChanges(MailConfigurationDto mailConfigurationDto);
        Task Delete(int mailConfigurationID );

        Task<bool> GetActiveConfig(int ConfigId);
    }
}
