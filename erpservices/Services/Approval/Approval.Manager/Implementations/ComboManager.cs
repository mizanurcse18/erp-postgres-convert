using Approval.DAL.Entities;
using Approval.Manager.Interfaces;
using Core;
using Core.AppContexts;
using DAL.Core.Repository;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
namespace Approval.Manager.Implementations
{
    public class ComboManager : IComboManager
    {

        private readonly IRepository<ApprovalPanel> ApprovalPanelRepo;
        private readonly IRepository<ApprovalType> ApprovalTypeRepo;
        private readonly IRepository<DocumentApprovalTemplate> DocumentApprovalTemplateRepo;
        public ComboManager(IRepository<ApprovalPanel> approvalpanelRepo, IRepository<ApprovalType> approvalTypeRepo
            , IRepository<DocumentApprovalTemplate> documentApprovalTemplateRepo)
        {
            ApprovalPanelRepo = approvalpanelRepo;
            ApprovalTypeRepo = approvalTypeRepo;
            DocumentApprovalTemplateRepo = documentApprovalTemplateRepo;
        }
        public async Task<List<ComboModel>> GetApprovalPanelCombo()
        {
            var approvalpanelList = await ApprovalPanelRepo.GetAllListAsync();
            return approvalpanelList.Select(x => new ComboModel { value = x.APPanelID, label = x.Name, extraJsonProps = x.IsParallelApproval.ToString() }).ToList();
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetDynamicApprovalPanelCombo()
        {
            string sql = $@"SELECT DISTINCT AP.IsDynamicApproval extraJsonProps
                                ,AP.APPanelID value
                                ,AP.Name label
                            FROM ApprovalPanel AP
                            WHERE AP.IsDynamicApproval=1";
            //var approvalpanelList = await ApprovalPanelRepo.GetAllListAsync();
            //return approvalpanelList.Select(x => new ComboModel { value = x.APPanelID, label = x.Name, extraJsonProps = x.IsParallelApproval.ToString() }).ToList();
            var listDict = ApprovalPanelRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetApprovalPanelByEmployeeCombo(int EmployeeID)
        {
            string sql = $@"SELECT DISTINCT APE.EmployeeID extraJsonProps
                                ,AP.APPanelID value
                                ,AP.Name label
                            FROM ApprovalPanelEmployee APE
                            INNER JOIN ApprovalPanel AP ON AP.APPanelID = APE.APPanelID
                            INNER JOIN HRMS..ViewALLEmployee VAE ON APE.EmployeeID = VAE.EmployeeID
                            WHERE APE.EmployeeID = {EmployeeID}";
            //var approvalpanelList = await ApprovalPanelRepo.GetAllListAsync();
            //return approvalpanelList.Select(x => new ComboModel { value = x.APPanelID, label = x.Name, extraJsonProps = x.IsParallelApproval.ToString() }).ToList();
            var listDict = ApprovalPanelRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<ComboModel>> GetApprovalTypesList()
        {
            var approvalTypeList = await ApprovalTypeRepo.GetAllListAsync();
            return approvalTypeList.Select(x => new ComboModel { value = x.APTypeID, label = x.Name }).ToList();
        }
        public async Task<List<ComboModel>> GetsTemplateCombo(int isHR)
        {
            var templateList = new List<DocumentApprovalTemplate>();
            if (isHR == 1)
            {
                templateList = await DocumentApprovalTemplateRepo.GetAllListAsync(x => x.CategoryType == (int)Util.DocApprovalCategory.HR);
            }
            else
            {
                templateList = await DocumentApprovalTemplateRepo.GetAllListAsync(x => x.CategoryType != (int)Util.DocApprovalCategory.HR);
            }
            return templateList.Select(x => new ComboModel { value = Convert.ToInt32(x.DATID), label = x.DATName }).ToList();
        }


    }
}
