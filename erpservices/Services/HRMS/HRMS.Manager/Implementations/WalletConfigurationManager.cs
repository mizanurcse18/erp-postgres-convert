using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class WalletConfigurationManager : ManagerBase, IWalletConfigurationManager
    {

        private readonly IRepository<WalletConfiguration> WalletConfigurationRepo;
        private readonly IRepository<WalletConfiguration> WalletConfigurationExistsRepo;
        public WalletConfigurationManager(IRepository<WalletConfiguration> walletConfigurationRepo, IRepository<WalletConfiguration> walletConfigurationExistsRepo)
        {
            WalletConfigurationRepo = walletConfigurationRepo;
            WalletConfigurationExistsRepo = walletConfigurationExistsRepo;
        }
        

        public async Task<List<WalletConfigurationDto>> GetWalletConfigurationListDic()
        {
            string sql = $@"SELECT wc.*, Dsg.DesignationName, sv.SystemVariableCode as TypeName
	                        
                        FROM WalletConfiguration wc
                        LEFT JOIN Designation Dsg ON Dsg.DesignationID=wc.DesignationID
                        LEFT JOIN Security..SystemVariable sv ON sv.SystemVariableID=wc.TypeID
                       
                        ORDER BY wc.WalletConfigureID DESC";
            var listDict = WalletConfigurationRepo.GetDataModelCollection<WalletConfigurationDto>(sql);

            var customList = listDict.GroupBy(c => new { c.CashOutRate })
                .Select(chld => new WalletConfigurationDto()
                {
                    CashOutRate = chld.Key.CashOutRate,
                    Configurations = chld.ToList()
                }).ToList();
            await Task.CompletedTask;
            return customList;
        }

        public async Task<WalletConfigurationDto> GetWalletConfiguration(decimal cashoutrate)
        {
            var confList = new List<WalletConfigurationDto>();
            string sql = $@"SELECT wc.*, Dsg.DesignationName, sv.SystemVariableCode as TypeName
	                        
                        FROM WalletConfiguration wc
                        LEFT JOIN Designation Dsg ON Dsg.DesignationID=wc.DesignationID
                        LEFT JOIN Security..SystemVariable sv ON sv.SystemVariableID=wc.TypeID
                        WHERE wc.CashOutRate={cashoutrate} AND wc.IsCurrent=1
                        ORDER BY wc.WalletConfigureID DESC";

            confList = WalletConfigurationRepo.GetDataModelCollection<WalletConfigurationDto>(sql);
            var customList = confList.GroupBy(c => new { c.CashOutRate })
                .Select(chld => new WalletConfigurationDto()
                {
                    CashOutRate = chld.Key.CashOutRate,
                    Configurations = chld.ToList()
                }).FirstOrDefault();
            await Task.CompletedTask;
            return customList;
        }


        public async Task Delete(decimal cashoutrate)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var WalletConfigurationEnt = WalletConfigurationRepo.Entities.Where(x => x.CashOutRate == cashoutrate).ToList();
                WalletConfigurationEnt.ForEach(x => x.SetDeleted());

                WalletConfigurationRepo.AddRange(WalletConfigurationEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public void SaveChanges(WalletConfigurationDto WalletConfigurationDto)
        {
            var confs = new List<WalletConfiguration>();
            var existingList = WalletConfigurationRepo.Entities.Where(x => x.CashOutRate == WalletConfigurationDto.CashOutRate).ToList();
            WalletConfigurationDto.Configurations.ForEach(x =>
            {
                confs.Add(new WalletConfiguration
                    {
                        WalletConfigureID = x.WalletConfigureID,
                        CashOutRate = WalletConfigurationDto.CashOutRate,
                        DesignationID = x.DesignationID,
                        TypeID = x.TypeID,
                        Percentage = x.Percentage,
                        ExceptionFlag = x.ExceptionFlag
                    });
            });

            using (var unitOfWork = new UnitOfWork())
            {

                if (confs.Count > 0)
                {
                    confs.ForEach(conf =>
                    {
                        var existConf = WalletConfigurationRepo.Entities.SingleOrDefault(x => x.WalletConfigureID == conf.WalletConfigureID).MapTo<WalletConfiguration>();

                        if (existConf.IsNull())
                        {
                            conf.SetAdded();
                            conf.IsCurrent = true;
                            SetNewWalletConfigurationID(conf);
                        }
                        else if(conf.CashOutRate != existConf.CashOutRate || conf.DesignationID != existConf.DesignationID || conf.Percentage != existConf.Percentage || conf.TypeID != existConf.TypeID || conf.ExceptionFlag != existConf.ExceptionFlag)
                        {
                            conf.SetAdded();
                            conf.IsCurrent = true;
                            SetNewWalletConfigurationID(conf);

                            existConf.SetModified();
                            existConf.IsCurrent = false;

                            SetAuditFields(existConf);
                            WalletConfigurationExistsRepo.Add(existConf);
                        }
                        else
                        {
                            conf.SetModified();
                            conf.IsCurrent = true;
                            conf.CreatedBy = existConf.CreatedBy;
                            conf.CreatedDate = existConf.CreatedDate;
                            conf.CreatedIP = existConf.CreatedIP;
                            conf.RowVersion = existConf.RowVersion;
                        }

                        SetAuditFields(confs);

                    });
                    if (existingList.Count >= WalletConfigurationDto.Configurations.Count)
                    {
                        var willDeleted = existingList.Where(x => !WalletConfigurationDto.Configurations.Select(y => y.WalletConfigureID).Contains(x.WalletConfigureID)).ToList();
                        willDeleted.ForEach(x =>
                        {
                            x.SetDeleted();
                            confs.Add(x);
                        });
                    }


                    //if (confs.Count > 0)
                    //{
                    //    confs.ForEach(conf =>
                    //    {
                    //        var existConf = WalletConfigurationRepo.Entities.SingleOrDefault(x => x.WalletConfigureID == conf.WalletConfigureID).MapTo<WalletConfiguration>();
                    //        if (conf.CashOutRate != existConf.CashOutRate || conf.DesignationID != existConf.DesignationID || conf.Percentage != existConf.Percentage || conf.TypeID != existConf.TypeID || conf.ExceptionFlag != existConf.ExceptionFlag)
                    //        {

                    //            SetAuditFields(existConf);
                    //        }
                    //    });
                    //}
                    //if (confs.Count > 0)
                    //{
                    //    confs.ForEach(conf =>
                    //    {
                    //        var existConf = WalletConfigurationRepo.Entities.SingleOrDefault(x => x.WalletConfigureID == conf.WalletConfigureID).MapTo<WalletConfiguration>();
                    //        if (conf.CashOutRate != existConf.CashOutRate || conf.DesignationID != existConf.DesignationID || conf.Percentage != existConf.Percentage || conf.TypeID != existConf.TypeID || conf.ExceptionFlag != existConf.ExceptionFlag)
                    //        {

                    //            WalletConfigurationExistsRepo.Add(existConf);
                    //        }
                    //    });
                    //}
                    WalletConfigurationRepo.AddRange(confs);
                }
                
                unitOfWork.CommitChangesWithAudit();
            }
        }

        private void SetNewWalletConfigurationID(WalletConfiguration obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("WalletConfiguration", AppContexts.User.CompanyID);
            obj.WalletConfigureID = code.MaxNumber;
        }

    }
}
