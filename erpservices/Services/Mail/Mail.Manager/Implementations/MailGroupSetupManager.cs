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
    public class MailGroupSetupManager : ManagerBase, IMailGroupSetupManager
    {

        private readonly IRepository<MailGroupSetup> MailGroupSetupRepo;
        private readonly IRepository<MailSetup> MailSetupRepo;
        public MailGroupSetupManager(IRepository<MailGroupSetup> mailConfigurationRepo, IRepository<MailSetup> mailSetupRepo)
        {
            MailGroupSetupRepo = mailConfigurationRepo;
            MailSetupRepo = mailSetupRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetMailGroupSetupListDic()
        {
            string sql = $@"SELECT mc.*
	                        
                        FROM MailGroupSetup mc
                       
                        ORDER BY mc.GroupId DESC";
            var listDict = MailGroupSetupRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<Dictionary<string, object>> GetMailGroupSetup(int MailGroupSetupID)
        {

            string sql = $@"select MGS.*, SV1.SystemVariableCode AS PriorityName, SV2.SystemVariableCode AS SensitivityName, MC.ConfigName AS ConfigName
                        from MailGroupSetup MGS
                        LEFT JOIN Security..SystemVariable SV1 ON MGS.Priority = SV1.SystemVariableID
                        LEFT JOIN Security..SystemVariable SV2 ON MGS.Sensitivity = SV2.SystemVariableID
                        LEFT JOIN MailConfiguration MC ON MGS.ConfigId = MC.ConfigId
                        WHERE MGS.GroupId={MailGroupSetupID}";

            //MailGroupSetupDto data = new MailGroupSetupDto();
            var data = MailGroupSetupRepo.GetData(sql);
            
            return await Task.FromResult(data);

            //var dept = MailGroupSetupRepo.SingleOrDefault(x => x.GroupId == MailGroupSetupID).MapTo<MailGroupSetupDto>();

            //return await Task.FromResult(dept);
        }


        public async Task Delete(int MailGroupSetupID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var mailConfigurationEnt = MailGroupSetupRepo.Entities.Where(x => x.GroupId == MailGroupSetupID).FirstOrDefault();

                mailConfigurationEnt.SetDeleted();
                MailGroupSetupRepo.Add(mailConfigurationEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(MailGroupSetupDto mailConfigurationDto)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = MailGroupSetupRepo.Entities.SingleOrDefault(x => x.GroupId == mailConfigurationDto.GroupId).MapTo<MailGroupSetup>();

                if (existUser.IsNull() || mailConfigurationDto.GroupId.IsZero() )
                {
                    mailConfigurationDto.SetAdded();
                    SetNewUserID(mailConfigurationDto);
                }
                else
                {
                    mailConfigurationDto.SetModified();
                }

                var mailConfigurationEnt = mailConfigurationDto.MapTo<MailGroupSetup>();
                var childModel = GenerateMailSetup(mailConfigurationDto);
                SetAuditFields(mailConfigurationEnt);
                SetAuditFields(childModel);

                MailGroupSetupRepo.Add(mailConfigurationEnt);
                MailSetupRepo.AddRange(childModel);

                unitOfWork.CommitChangesWithAudit();
            }
        }
        private List<MailSetup> GenerateMailSetup(MailGroupSetupDto mailSetupDto)
        {
            var existingMailSetup = MailSetupRepo.Entities.Where(x => x.GroupId == mailSetupDto.GroupId).ToList();
            var childModel = new List<MailSetup>();
            if (mailSetupDto.MailSetups.IsNotNull())
            {
                mailSetupDto.MailSetups.ForEach(x =>
                {
                    if (x.Email.IsNotNullOrEmpty())
                    {
                        childModel.Add(new MailSetup
                        {
                            MailId = x.MailId,
                            GroupId = mailSetupDto.GroupId,
                            To_CC_BCC = x.To_CC_BCC,
                            Email = x.Email
                        });
                    }

                });

                childModel.ForEach(x =>
                {
                    if (existingMailSetup.Count > 0 && x.MailId > 0)
                    {
                        if (x.Email.IsNullOrEmpty())
                        {
                            var existingModelData = existingMailSetup.FirstOrDefault(y => y.MailId == x.MailId);
                            x.CreatedBy = existingModelData.CreatedBy;
                            x.CreatedDate = existingModelData.CreatedDate;
                            x.CreatedIP = existingModelData.CreatedIP;
                            x.RowVersion = existingModelData.RowVersion;
                            x.SetModified();
                        }
                        else x.SetDeleted();
                        
                    }
                    else
                    {
                        x.GroupId = mailSetupDto.GroupId;
                        x.SetAdded();
                        SetMailSetupNewId(x);
                    }
                });

                var willDeleted = existingMailSetup.Where(x => !childModel.Select(y => y.MailId).Contains(x.MailId)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }


            return childModel;
        }
        private void SetMailSetupNewId(MailSetup child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("MailSetup", AppContexts.User.CompanyID);
            child.MailId = code.MaxNumber;
        }
        private void SetNewUserID(MailGroupSetupDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("MailGroupSetup", AppContexts.User.CompanyID);
            obj.GroupId = code.MaxNumber;
        }

        public async Task<List<MailSetup>> GetMailSetup(int GroupID)
        {
            var lists =  MailSetupRepo.Entities.Where(x => x.GroupId == GroupID).ToList<MailSetup>();
            return await Task.FromResult(lists);
        }
    }
}
