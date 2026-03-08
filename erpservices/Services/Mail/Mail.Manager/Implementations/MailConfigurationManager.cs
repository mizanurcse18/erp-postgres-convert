using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Mail.DAL.Entities;
using Mail.Manager.Dto;
using Mail.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mail.Manager.Implementations
{
    public class MailConfigurationManager : ManagerBase, IMailConfigurationManager
    {

        private readonly IRepository<MailConfiguration> MailConfigurationRepo;
        public MailConfigurationManager(IRepository<MailConfiguration> mailConfigurationRepo)
        {
            MailConfigurationRepo = mailConfigurationRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetMailConfigurationListDic()
        {
            string sql = $@"SELECT mc.*
	                        
                        FROM MailConfiguration mc
                       
                        ORDER BY mc.ConfigId DESC";
            var listDict = MailConfigurationRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<MailConfigurationDto> GetMailConfiguration(int MailConfigurationID)
        {

            var dept = MailConfigurationRepo.SingleOrDefault(x => x.ConfigId == MailConfigurationID).MapTo<MailConfigurationDto>();

            return await Task.FromResult(dept);
        }


        public async Task Delete(int MailConfigurationID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var mailConfigurationEnt = MailConfigurationRepo.Entities.Where(x => x.ConfigId == MailConfigurationID).FirstOrDefault();

                mailConfigurationEnt.SetDeleted();
                MailConfigurationRepo.Add(mailConfigurationEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public async Task<bool> GetActiveConfig(int configId)
        {
            bool isExists = MailConfigurationRepo.Entities.Where(p=> p.ConfigId != configId).Count(x => x.IsActive == true) > 0;
            await Task.CompletedTask;
            return isExists;
        }
        public void SaveChanges(MailConfigurationDto mailConfigurationDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = MailConfigurationRepo.Entities.SingleOrDefault(x => x.ConfigId == mailConfigurationDto.ConfigId).MapTo<MailConfiguration>();

                if (existUser.IsNull() || mailConfigurationDto.ConfigId.IsZero() )
                {
                    mailConfigurationDto.SetAdded();
                    SetNewUserID(mailConfigurationDto);
                }
                else
                {
                    mailConfigurationDto.SetModified();
                }

                var mailConfigurationEnt = mailConfigurationDto.MapTo<MailConfiguration>();
                SetAuditFields(mailConfigurationEnt);
                MailConfigurationRepo.Add(mailConfigurationEnt);
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewUserID(MailConfigurationDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("MailConfiguration", AppContexts.User.CompanyID);
            obj.ConfigId = code.MaxNumber;
        }

    }
}
