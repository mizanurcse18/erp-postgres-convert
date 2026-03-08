using Core;
using DAL.Core.Repository;
using Mail.DAL.Entities;
using Mail.Manager.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mail.Manager.Implementations
{
    public class ComboManager : IComboManager
    {
        private readonly IRepository<MailConfiguration> MailConfigRepo;
        public ComboManager(IRepository<MailConfiguration> objRepo)
        {
            MailConfigRepo = objRepo;
        }

        public async Task<List<ComboModel>> GetMailConfigurations()
        {
            var configList = await MailConfigRepo.GetAllListAsync();
            return configList.Select(x => new ComboModel { value = x.ConfigId, label = x.ConfigName }).ToList();
        }


    }
}
