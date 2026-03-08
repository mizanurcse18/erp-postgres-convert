using Core;
using Mail.DAL.Entities;
using Mail.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mail.Manager.Interfaces
{
    public interface IMailGroupSetupManager
    {
        Task<IEnumerable<Dictionary<string, object>>> GetMailGroupSetupListDic();
        Task<Dictionary<string, object>> GetMailGroupSetup(int MailGroupSetupID);

        void SaveChanges(MailGroupSetupDto mailConfigurationDto);
        Task Delete(int mailConfigurationID );
        Task<List<MailSetup>> GetMailSetup(int GroupID);

    }
}
