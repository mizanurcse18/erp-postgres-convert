using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using HRMS.DAL.Entities;
using HRMS.Manager.Dto;
using HRMS.Manager.Interfaces;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Core.Util;

namespace HRMS.Manager.Implementations
{
    public class AnnualLeaveEncashmentWindowManager : ManagerBase, IAnnualLeaveEncashmentWindowManager
    {
        private readonly IRepository<AnnualLeaveEncashmentWindowMaster> AnnualLeaveEncashmentWindowMasterRepo;
        private readonly IRepository<AnnualLeaveEncashmentWindowChild> AnnualLeaveEncashmentWindowChildRepo;
        private readonly IRepository<Employment> EmploymentRepo;
        private readonly IRepository<ShiftingMaster> ShiftingMasterRepo;
        private readonly IRepository<ShiftingChild> ShiftingChildRepo;
        private readonly IRepository<Holiday> HolidayRepo;
        private readonly IRepository<LFADeclaration> LFADeclarationRepo;
        private readonly IRepository<Employee> EmployeeRepo;
        private readonly IRepository<AnnualLeaveEncashmentPolicySettings> AnnualLeaveEncashmentPolicySettingsRepo;
        private readonly IRepository<EmployeeSupervisorMap> EmployeeSupervisorMapRepo;
        private readonly IScheduleManager ScheduleManager;
        public AnnualLeaveEncashmentWindowManager(IRepository<AnnualLeaveEncashmentWindowMaster> annualLeaveEncashmentWindowMasterRepo, IRepository<AnnualLeaveEncashmentWindowChild> annualLeaveEncashmentWindowChildRepo, IRepository<Employment> employmentRepo, IRepository<ShiftingMaster> shiftingMasterRepo, IRepository<ShiftingChild> shiftingChildRepo, IRepository<Holiday> holidayRepo, IRepository<LFADeclaration> lfaRepo, IRepository<Employee> employeeRepo, IRepository<AnnualLeaveEncashmentPolicySettings> leavePolicySettingsRepo, IRepository<EmployeeSupervisorMap> employeeSupervisorMapRepo, IScheduleManager scheduleManager)
        {
            AnnualLeaveEncashmentWindowMasterRepo = annualLeaveEncashmentWindowMasterRepo;
            AnnualLeaveEncashmentWindowChildRepo = annualLeaveEncashmentWindowChildRepo;
            EmploymentRepo = employmentRepo;
            ShiftingMasterRepo = shiftingMasterRepo;
            ShiftingChildRepo = shiftingChildRepo;
            HolidayRepo = holidayRepo;
            LFADeclarationRepo = lfaRepo;
            EmployeeRepo = employeeRepo;
            AnnualLeaveEncashmentPolicySettingsRepo = leavePolicySettingsRepo;
            EmployeeSupervisorMapRepo = employeeSupervisorMapRepo;
            ScheduleManager = scheduleManager;
        }

        public async Task<AnnualLeaveEncashmentPolicySettingsDto> GetAnnualLeaveEncashmentSettings()
        {
            string sql = @$"SELECT ALEPSID, HierarchyLevel, MaximumJobGrade, IncludeHR, S.EmployeeID, ProxyEmployeeIDs, MaxEncashablePercent, MaxEncashableDays, CutOffDate,
                            (
	                            SELECT EmployeeCode+'-'+FullName label, EmployeeID value from Employee
	                            WHERE EmployeeID IN (Select * from dbo.fnReturnStringArray(ProxyEmployeeIDs,','))
	                            FOR JSON PATH
                            ) AS ProxyEmployeeStr 
                            ,J.JobGradeName
                            ,E.FullName EmployeeName
                            FROM AnnualLeaveEncashmentPolicySettings S
                                LEFT JOIN Employee E ON E.EmployeeID=S.EmployeeID
                                LEFT JOIN JobGrade J ON J.JobGradeID=S.MaximumJobGrade";
            var setting = Task.Run(() => AnnualLeaveEncashmentPolicySettingsRepo.GetModelData<AnnualLeaveEncashmentPolicySettingsDto>(sql));

            return await setting;
        }


        public async Task<AnnualLeaveEncashmentWindowMasterDto> GetAnnualLeaveEncashmentWindowMaster(int id)
        {
            if (id == 0)
            {
                string sql = @$"SELECT ALEWM.* FROM AnnualLeaveEncashmentWindowMaster ALEWM WHERE ALEWM.ALEWMasterID={id}";
                var setting = Task.Run(() => AnnualLeaveEncashmentWindowMasterRepo.GetModelData<AnnualLeaveEncashmentWindowMasterDto>(sql));

                return await setting;
            }
            else
            {
                string sql = @$"SELECT AL.ALEWMasterID, AL.FinancialYearID, AL.StartDate, AL.EndDate, FY.Year, AL.RowVersion
                            , (SELECT DivisionName label, DivisionID value from Division
	                            WHERE DivisionID IN (Select * from dbo.fnReturnStringArray(AL.DivisionIDs,','))
                            FOR JSON PATH) DivisionIDsStr
                            , (SELECT DepartmentName label, DepartmentID value from Department
	                            WHERE DepartmentID IN (Select * from dbo.fnReturnStringArray(AL.DepartmentIDs,','))
                            FOR JSON PATH) DepartmentIDsStr
                            , (SELECT SystemVariableCode label, SystemVariableID value from Security..SystemVariable
	                            WHERE SystemVariableID IN (Select * from dbo.fnReturnStringArray(AL.EmployeeTypeIDs,','))
                            FOR JSON PATH) EmployeeTypeIDsStr
                            FROM AnnualLeaveEncashmentWindowMaster  AL
							LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID = AL.FinancialYearID 
                            WHERE ALEWMasterID={id}";
                var setting = Task.Run(() => AnnualLeaveEncashmentWindowMasterRepo.GetModelData<AnnualLeaveEncashmentWindowMasterDto>(sql));

                return await setting;

            }
        }

        public async Task<List<AnnualLeaveEncashmentWindowChildDto>> GetAnnualLeaveEncashmentWindowChild(int id)
        {
            string sql = @$"SELECT ALEWC.*, CONCAT(EmployeeCode,'-', FullName) EmployeeName, DivisionName, DepartmentName, DesignationName
                            FROM AnnualLeaveEncashmentWindowChild ALEWC 
                            LEFT JOIN ViewALLEmployee VA ON VA.EmployeeID = ALEWC.EmployeeID
                            WHERE ALEWC.ALEWMasterID={id}";

            var childs = AnnualLeaveEncashmentWindowChildRepo.GetDataModelCollection<AnnualLeaveEncashmentWindowChildDto>(sql);
            return childs;
        }

        private void SetMasterNewId(AnnualLeaveEncashmentWindowMaster master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("AnnualLeaveEncashmentWindowMaster", AppContexts.User.CompanyID);
            master.ALEWMasterID = code.MaxNumber;
        }
        private void SetPanelNewId(AnnualLeaveEncashmentPolicySettings master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("AnnualLeaveEncashmentPolicySettings", AppContexts.User.CompanyID);
            master.ALEPSID = code.MaxNumber;
        }
        private void SetChildId(AnnualLeaveEncashmentWindowChild master)
        {
            if (!master.IsAdded) return;
            var code = GenerateSystemCode("AnnualLeaveEncashmentWindowChild", AppContexts.User.CompanyID);
            master.ALEChildID = code.MaxNumber;
        }


        public async Task<(bool, string)> Save(AnnualLeaveEncashmentPolicySettingsDto settings)
        {
            var existingWindowMaster = AnnualLeaveEncashmentWindowMasterRepo.Entities.SingleOrDefault(x => x.ALEWMasterID == settings.ALEWMasterID);

            var existingPanel = AnnualLeaveEncashmentPolicySettingsRepo.Entities.SingleOrDefault(x => x.ALEPSID == settings.ALEPSID);

            int tmpStatus = (int)Util.LeaveEncashmentStatus.Initiated;
            var stDate = DateTime.ParseExact(settings.StartDate.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var endDate = DateTime.ParseExact(settings.EndDate.ToString("yyyy-MM-dd"), "yyyy-MM-dd", CultureInfo.InvariantCulture);

            var todaysDate = DateTime.Today;
            //if (stDate < todaysDate)
            //{
            //    tmpStatus = (int)Util.LeaveEncashmentStatus.Initiated;
            //}
            //else if (stDate >= todaysDate && stDate <= endDate)
            //{
            //    tmpStatus = (int)Util.LeaveEncashmentStatus.Ongoing;
            //}
            //else if (stDate > endDate)
            //{
            //    tmpStatus = (int)Util.LeaveEncashmentStatus.Expired;
            //}

            if( stDate > endDate)
            {
               return (false, $"End date can't be before start date");
            }

            
            if (stDate <= todaysDate && todaysDate <= endDate)
            {
                tmpStatus = (int)Util.LeaveEncashmentStatus.Ongoing;
            }
            else if (stDate >= todaysDate && stDate <= endDate)
            {
                tmpStatus = (int)Util.LeaveEncashmentStatus.Initiated;
            }
            else if (stDate > endDate || endDate < todaysDate)
            {
                tmpStatus = (int)Util.LeaveEncashmentStatus.Expired;
            }

            var masterModel = new AnnualLeaveEncashmentWindowMaster
            {
                FinancialYearID = settings.FinancialYearID,
                StartDate = settings.StartDate,
                EndDate = settings.EndDate,
                Status = tmpStatus,
                DivisionIDs = settings.DivisionIDs,
                DepartmentIDs = settings.DepartmentIDs,
                EmployeeTypeIDs = settings.EmployeeTypeIDs,
            };


            var panelModel = new AnnualLeaveEncashmentPolicySettings
            {
                HierarchyLevel = settings.HierarchyLevel,
                MaximumJobGrade = settings.MaximumJobGrade,
                IncludeHR = settings.IncludeHR,
                EmployeeID = settings.EmployeeID,
                ProxyEmployeeIDs = settings.ProxyEmployeeIDs,
                MaxEncashablePercent = settings.MaxEncashablePercent,
                MaxEncashableDays = settings.MaxEncashableDays,
                CutOffDate = settings.CutOffDate,
            };

            using (var unitOfWork = new UnitOfWork())
            {
                if (settings.ALEWMasterID.IsZero() && existingWindowMaster.IsNull())
                {
                    masterModel.SetAdded();
                    SetMasterNewId(masterModel);
                    settings.ALEWMasterID = (int)masterModel.ALEWMasterID;
                }
                else
                {
                    masterModel.ALEWMasterID = existingWindowMaster.ALEWMasterID;
                    masterModel.CreatedBy = existingWindowMaster.CreatedBy;
                    masterModel.CreatedDate = existingWindowMaster.CreatedDate;
                    masterModel.CreatedIP = existingWindowMaster.CreatedIP;
                    masterModel.RowVersion = existingWindowMaster.RowVersion;
                    masterModel.SetModified();
                }
                var childModel = GenerateChild(settings);


                //Panel
                if (settings.ALEPSID.IsZero() && existingPanel.IsNull())
                {
                    panelModel.SetAdded();
                    SetPanelNewId(panelModel);
                }
                else
                {
                    panelModel.ALEPSID = existingPanel.ALEPSID;
                    panelModel.CreatedBy = existingPanel.CreatedBy;
                    panelModel.CreatedDate = existingPanel.CreatedDate;
                    panelModel.CreatedIP = existingPanel.CreatedIP;
                    panelModel.RowVersion = existingPanel.RowVersion;
                    panelModel.SetModified();
                }


                SetAuditFields(masterModel);
                SetAuditFields(childModel);
                SetAuditFields(panelModel);



                AnnualLeaveEncashmentWindowMasterRepo.Add(masterModel);
                AnnualLeaveEncashmentWindowChildRepo.AddRange(childModel);
                AnnualLeaveEncashmentPolicySettingsRepo.Add(panelModel);


                unitOfWork.CommitChangesWithAudit();

                //List<AnnualLeaveEncashmentWindowChildDto> childListForMail = new List<AnnualLeaveEncashmentWindowChildDto>();
                //childListForMail = AnnualLeaveEncashmentWindowChildRepo.Entities.Where(x => x.ALEWMasterID == settings.ALEWMasterID && x.IsMailSent == false).ToList().MapTo<List<AnnualLeaveEncashmentWindowChildDto>>();

                var mailData = new List<Dictionary<string, object>>();
                var mdata = new Dictionary<string, object>
                {
                    { "StartDate", settings.StartDate.ToString("dd/MM/yyyy") },
                    { "EndDate", settings.EndDate.ToString("dd/MM/yyyy") },
                    { "FinancialYear", settings.FinancialYearName }
                };
                mailData.Add(mdata);



                var toMail = new List<string>();
                foreach (var item in settings.ChildList.Where(x => x.IsMailSent == false))
                {
                    string email = EmployeeRepo.Entities.Where(x => x.EmployeeID == item.EmployeeID).Select(y => y.WorkEmail).FirstOrDefault();
                    toMail.Add(email);
                }
                BasicMail((int)Util.MailGroupSetup.LeaveEncashmentWindowMail, toMail, false, null, null, mailData);


            }

            await Task.CompletedTask;

            return (true, $"Window with Panel Saved Successfully");

        }

        private List<AnnualLeaveEncashmentWindowChild> GenerateChild(AnnualLeaveEncashmentPolicySettingsDto En)
        {
            var existingAnnualLeaveEncashmentWindowChild = AnnualLeaveEncashmentWindowChildRepo.Entities.Where(x => x.ALEWMasterID == En.ALEWMasterID).ToList();
            var childModel = new List<AnnualLeaveEncashmentWindowChild>();
            if (En.ChildList.IsNotNull())
            {
                En.ChildList.ForEach(x =>
                {
                    
                        childModel.Add(new AnnualLeaveEncashmentWindowChild
                        {
                            ALEWMasterID = En.ALEWMasterID,
                            EmployeeID = x.EmployeeID,
                            IsMailSent = true
                        });

                });

                childModel.ForEach(x =>
                {
                    if (existingAnnualLeaveEncashmentWindowChild.Count > 0 && x.ALEChildID > 0)
                    {
                        var existingModelData = existingAnnualLeaveEncashmentWindowChild.FirstOrDefault(y => y.ALEChildID == x.ALEChildID);
                        x.IsMailSent = existingModelData.IsMailSent;
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.ALEWMasterID = En.ALEWMasterID;
                        x.SetAdded();
                        SetChildId(x);
                    }
                });

                var willDeleted = existingAnnualLeaveEncashmentWindowChild.Where(x => !childModel.Select(y => y.ALEChildID).Contains(x.ALEChildID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            }

            return childModel;
        }

        public async Task<List<AnnualLeaveEncashmentWindowMasterDto>> GetAll()
        {
            string sql = @$"SELECT
                        ALEWMasterID,
                        Year,
                        StartDate,
                        EndDate,
                        SystemVariableCode StatusName
                        FROM AnnualLeaveEncashmentWindowMaster ALEWM
                        LEFT JOIN Security..FinancialYear FY ON FY.FinancialYearID=ALEWM.FinancialYearID
						LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = ALEWM.Status";
            var data = AnnualLeaveEncashmentWindowMasterRepo.GetDataModelCollection<AnnualLeaveEncashmentWindowMasterDto>(sql);

            return data;
        }

        public void UpdateLeaveEncashmentStatus(long ALEWMasterID, int Status)
        {
            var existingAnnualLeaveEncashmentWindowMaster = AnnualLeaveEncashmentWindowMasterRepo.Get(ALEWMasterID);
            if (existingAnnualLeaveEncashmentWindowMaster.IsNotNull())
            {
                existingAnnualLeaveEncashmentWindowMaster.SetModified();
                existingAnnualLeaveEncashmentWindowMaster.Status = Status;


                SetAuditFields(existingAnnualLeaveEncashmentWindowMaster);
                using (var unitOfWork = new UnitOfWork())
                {
                    AnnualLeaveEncashmentWindowMasterRepo.Add(existingAnnualLeaveEncashmentWindowMaster);

                    unitOfWork.CommitChangesWithAudit();
                }
            }
            
        }

    }
}
