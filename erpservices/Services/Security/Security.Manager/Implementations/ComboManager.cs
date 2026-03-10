using Core;
using Core.AppContexts;
using DAL.Core.Repository;
using Security.DAL.Entities;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class ComboManager : IComboManager
    {
        private readonly IRepository<SecurityGroupMaster> SecurityGroupRepo;
        private readonly IRepository<SecurityGroupUserChild> UserGroupRepo;
        private readonly IRepository<Company> CompanyRepo;
        private readonly IRepository<SystemVariable> SystemVariableRepo;
        private readonly IRepository<Person> PersonRepo;
        private readonly IRepository<District> DistrictRepo;
        private readonly IRepository<Thana> ThanaRepo;
        private readonly IRepository<FinancialYear> FinancialYearRepo;
        private readonly IRepository<NFAChild> NFAChildRepo;
        private readonly IRepository<Unit> UnitRepo;
        private readonly IRepository<Division> DivisionRepo;
        private readonly IRepository<Menu> MenuRepo;
        private readonly IRepository<CommonInterfaceFields> CommonInterfaceFieldsRepo;
        private readonly IRepository<BusinessSupportItem> BusinessSupportItemRepo;
        private readonly IRepository<SupportRequisitionItem> SupportRequisitionItemRepo;
        private readonly IRepository<UserProfile> UserProfileRepo;
       

        public ComboManager(IRepository<SecurityGroupMaster> objRepo,
            IRepository<SecurityGroupUserChild> userGroupRepo, IRepository<Company> companyRepo,
            IRepository<SystemVariable> SystemVariableRepo, IRepository<Person> personRepo, IRepository<District> districtRepo, IRepository<Thana> thanaRepo, IRepository<FinancialYear> finYearRepo,
            IRepository<NFAChild> nfaChildRepo, IRepository<Unit> unitRepo, IRepository<Division> divisionRepo, IRepository<Menu> menuRepo, IRepository<CommonInterfaceFields> commonInterfaceFieldsRepo,
            IRepository<BusinessSupportItem> businessSupportItemRepo, IRepository<SupportRequisitionItem> supportRequisitionItemRepo, IRepository<UserProfile> userProfileRepo)
        {
            SecurityGroupRepo = objRepo;
            UserGroupRepo = userGroupRepo;
            CompanyRepo = companyRepo;
            this.SystemVariableRepo = SystemVariableRepo;
            PersonRepo = personRepo;
            DistrictRepo = districtRepo;
            ThanaRepo = thanaRepo;
            FinancialYearRepo = finYearRepo;
            NFAChildRepo = nfaChildRepo;
            UnitRepo = unitRepo;
            DivisionRepo = divisionRepo;
            MenuRepo = menuRepo;
            CommonInterfaceFieldsRepo = commonInterfaceFieldsRepo;
            BusinessSupportItemRepo = businessSupportItemRepo;
            SupportRequisitionItemRepo = supportRequisitionItemRepo;
            UserProfileRepo = userProfileRepo;
        }

        public async Task<List<ComboModel>> GetSecurityGroupList()
        {
            var userGroupList = await SecurityGroupRepo.GetAllListAsync();
            return userGroupList.Select(x => new ComboModel { value = x.SecurityGroupID, label = x.SecurityGroupName }).ToList();
        }


        public async Task<List<ComboModel>> GetDivisionOfCountry()
        {
            var divisionList = await DivisionRepo.GetAllListAsync();
            return divisionList.Select(x => new ComboModel { value = x.DivisionID, label = x.DivisionName }).ToList();
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetCompanies()
        {
            //const string sql = "SELECT CompanyID, CompanyName FROM Company WHERE CompanyGroupID = @p0";
            const string sql = "SELECT CompanyID, CompanyName FROM Company ";
            var result = CompanyRepo.GetDataDictCollection(sql);
            return result;
        }

        public async Task<List<ComboModel>> SystemVariable(int entityTypeID)
        {
            var systemVariableList = await SystemVariableRepo.GetAllListAsync(obj => obj.CompanyID == AppContexts.User.CompanyID.ToString() && obj.EntityTypeID == entityTypeID);
            return systemVariableList.OrderBy(x => x.Sequence).Select(x => new ComboModel { value = x.SystemVariableID, label = x.SystemVariableCode }).ToList();
        }
        //public async Task<List<ComboModel>> GetEmployeeJobStatus()
        //{
        //    var systemVariableList = await SystemVariableRepo.GetAllListAsync(obj => obj.CompanyID == AppContexts.User.CompanyID.ToString() && obj.EntityTypeID == entityTypeID);
        //    return systemVariableList.OrderBy(x => x.Sequence).Select(x => new ComboModel { value = x.SystemVariableID, label = x.SystemVariableCode }).ToList();
        //}

        public async Task<List<ComboModel>> GetPersons()
        {
            string sql = @$"SELECT  psn.PersonID as  value,
                            psn.FirstName  label
	                        ,CASE 
		                        WHEN usr.PersonID IS NULL
			                        THEN CAST(0 AS BIT)
		                        ELSE CAST(1 AS BIT)
		                        END isDisabled
                        FROM Person psn
                        LEFT JOIN (
	                        SELECT PersonID
	                        FROM Users
	                        ) usr ON psn.PersonID = usr.PersonID WHERE psn.CompanyID='{AppContexts.User.CompanyID}'";


            //var personList = await PersonRepo.GetAllListAsync(obj => obj.CompanyID == AppContexts.User.CompanyID.ToString());
            //return personList.Select(x => new ComboModel { value = x.PersonID, label = x.FirstName + " " + x.LastName }).ToList();
            return PersonRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetPersonsWithCode()
        {
            string sql = @$"SELECT
                                psn.person_id AS ""value"",
                                (emp.employee_code || '-' || psn.first_name) AS ""label"",
                                emp.work_email AS ""extraJsonProps"",
                                CASE
                                    WHEN usr.person_id IS NULL THEN FALSE
                                    ELSE TRUE
                                END AS ""isDisabled""
                            FROM person psn
                            LEFT JOIN (
                                SELECT
                                    employee_code,
                                    work_email,
                                    person_id
                                FROM dblink('dbname=hrms user=your_username password=your_password',
                                            'SELECT employee_code, work_email, person_id FROM employee')
                                AS emp(employee_code VARCHAR(50), work_email VARCHAR(100), person_id INTEGER)
                            ) emp ON psn.person_id = emp.person_id
                            LEFT JOIN (
                                SELECT person_id
                                FROM users
                            ) usr ON psn.person_id = usr.person_id
                            WHERE psn.company_id='{AppContexts.User.CompanyID}'";


            //var personList = await PersonRepo.GetAllListAsync(obj => obj.CompanyID == AppContexts.User.CompanyID.ToString());
            //return personList.Select(x => new ComboModel { value = x.PersonID, label = x.FirstName + " " + x.LastName }).ToList();
            return PersonRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetDistricts()
        {
            var districtsList = await DistrictRepo.GetAllListAsync();
            return districtsList.Select(x => new ComboModel { value = x.DistrictID, label = x.DistrictName }).OrderBy(x => x.label).ToList();
        }
        public async Task<List<ComboModel>> GetUnitCombo()
        {
            var unitList = await UnitRepo.GetAllListAsync();
            return unitList.Select(x => new ComboModel { value = x.UnitID, label = x.UnitCode }).OrderBy(x => x.label).ToList();
        }

        public async Task<List<ComboModel>> GetThanas(int districtId)
        {
            var thanaList = await ThanaRepo.GetAllListAsync(x => x.DistrictID == districtId);
            return thanaList.Select(x => new ComboModel { value = x.ThanaID, label = x.ThanaName }).OrderBy(x => x.label).ToList();
        }
        public async Task<List<ComboModel>> GetDistrictsByDivision(int divisionid)
        {
            var districtList = await DistrictRepo.GetAllListAsync(x => x.DivisionID == divisionid);
            return districtList.Select(x => new ComboModel { value = x.DistrictID, label = x.DistrictName }).OrderBy(x => x.label).ToList();
        }

        public async Task<List<ComboModel>> GetMenuURLList()
        {
            var menuList = await MenuRepo.GetAllListAsync(x => !string.IsNullOrEmpty(x.Url));
            return menuList.Select(x => new ComboModel { value = x.MenuID, label = x.Title }).OrderBy(x => x.label).ToList();
        }
        public async Task<List<ComboModel>> GetFinancialYear()
        {
            //var financialYearList = await FinancialYearRepo.GetAllListAsync();
            string sql = @"SELECT 
			                	Fin.FinancialYearID value
			                	,Fin.Year label
                                ,Fin.YearDescription extraJsonProps
	                        FROM 
	                        Security.dbo.FinancialYear Fin
	                        LEFT JOIN (
		                        SELECT DISTINCT FinancialYearID
		                        FROM HRMS..EmployeeLeaveAccount
	                        ) ELA ON ELA.FinancialYearID= Fin.FinancialYearID WHERE ELA.FinancialYearID IS NULL";

            return await Task.Run(() => FinancialYearRepo.GetDataModelCollection<ComboModel>(sql));
            //return financialYearList.Select(x => new ComboModel { value = x.FinancialYearID, label = x.Year.ToString() }).OrderBy(x => x.label).ToList();
        }
        public async Task<List<ComboModel>> GetAllFinancialYear()
        {
            var financialYearList = await FinancialYearRepo.GetAllListAsync();
            string sql = @"SELECT 
                   	Fin.FinancialYearID value
                   	,Fin.Year label
                                ,Fin.YearDescription extraJsonProps
                         FROM 
                             Security.dbo.FinancialYear Fin
                            ORDER BY FinancialYearID desc
                         ";

            return await Task.Run(() => FinancialYearRepo.GetDataModelCollection<ComboModel>(sql));
            //return financialYearList.Select(x => new ComboModel { value = x.FinancialYearID, label = x.Year.ToString() }).OrderBy(x => x.label).ToList();
        }
        public async Task<List<ComboModel>> GetAllAssessmentYear()
        {
            string sql = @"SELECT 
			                	Fin.FinancialYearID value
			                	,Fin.Year label
                                ,Fin.YearDescription extraJsonProps
	                        FROM 
	                            Security.dbo.FinancialYear Fin
                            ORDER BY FinancialYearID desc
	                        ";

            return await Task.Run(() => FinancialYearRepo.GetDataModelCollection<ComboModel>(sql));
        }
        public async Task<List<ComboModel>> GetFinancialYearForTax()
        {
            //var financialYearList = await FinancialYearRepo.GetAllListAsync();
            string sql = @"SELECT 
			                	Fin.FinancialYearID value
			                	,Fin.YearDescription label
                                ,Fin.YearDescription extraJsonProps
	                        FROM 
	                        Security.dbo.FinancialYear Fin
							where Fin.Year NOT IN(2019, 2020)
							ORDER BY Fin.Year DESC";

            return await Task.Run(() => FinancialYearRepo.GetDataModelCollection<ComboModel>(sql));
            //return financialYearList.Select(x => new ComboModel { value = x.FinancialYearID, label = x.Year.ToString() }).OrderBy(x => x.label).ToList();
        }
        public async Task<List<ComboModel>> GetFinancialYearForHoliday()
        {

            string sql = @$"SELECT F.FinancialYearID AS value, F.Year AS label  FROM FinancialYear F
                            WHERE F.FinancialYearID not in (SELECT H.FinancialYearID FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Holiday H)";


            //var personList = await PersonRepo.GetAllListAsync(obj => obj.CompanyID == AppContexts.User.CompanyID.ToString());
            //return personList.Select(x => new ComboModel { value = x.PersonID, label = x.FirstName + " " + x.LastName }).ToList();
            return FinancialYearRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetFinancialYearsForVatTax()
        {

            string sql = @$"SELECT F.FinancialYearID AS value, F.Year AS label  FROM FinancialYear F
                            WHERE F.FinancialYearID not in (SELECT H.FinancialYearID FROM Accounts..VatTaxDeductionSource H)";
            return FinancialYearRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> getAllMonths()
        {
            var monthList = new List<ComboModel>();
            for (int i = 1; i <= 12; i++)
            {
                string monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i);
                monthList.Add(new ComboModel { value = i, label = monthName });
            }
            return monthList;
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetItemNames(string param)
        {
            string sql = @$"SELECT ItemName AS value, ItemName AS label  FROM NFAChild
                            WHERE ItemName Like '%{param}%' GROUP BY ItemName";
            var listDict = NFAChildRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetUnitNames(string param)
        {
            string sql = @$"SELECT DISTINCT * FROM (
                                SELECT UnitCode value,UnitCode label FROM Security..Unit
                                UNION ALL
                                SELECT UnitType value,UnitType label FROM Security..NFAChild
                                ) A WHERE A.value LIKE '%{param}%'";
            var listDict = UnitRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }
        public async Task<List<ComboModel>> GetPurposeList()
        {
            var systemVariableList = await SystemVariableRepo.GetAllListAsync(obj => obj.EntityTypeID == 21);
            return systemVariableList.Select(x => new ComboModel { value = x.SystemVariableID, label = x.SystemVariableDescription }).ToList();
        }


        public async Task<IEnumerable<Dictionary<string, object>>> GetComboItem()
        {
            string sql = @$"SELECT * FROM SCM..Item ORDER BY ItemName";
            var listDict = UnitRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<ComboModel>> GetUnits()
        {
            string sql = @$"SELECT UnitID value,UnitCode label FROM Security..Unit";
            return UnitRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetRequestLocation()
        {
            string sql = @$"SELECT LocationID value,LocationName label FROM Security..Location";
            return UnitRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetAllTransports()
        {
            string sql = @$"SELECT VehicleID value,VehicleRegNo label FROM Security..VehicleDetails where IsActive=1";
            return UnitRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<List<ComboModel>> GetAllDrivers()
        {
            string sql = @$"SELECT DriverID value,DriverName label,ContactNumber extraJsonProps FROM Security..DriverDetails where IsActive=1";
            return UnitRepo.GetDataModelCollection<ComboModel>(sql);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetNFAListCombo(string param, string isScm, int PRMasterID)
        {
            var sql = "";
            if (isScm == "scm")
            {
                sql = @$"SELECT
		                    N.ReferenceNo NFANo
		                    ,N.GrandTotal NFAAmount
		                    ,ISNULL(PRC.AlreadyCreatedAmt, 0) CreatedNFAAmount		
		                    ,N.GrandTotal - ISNULL(PRC.AlreadyCreatedAmt, 0) Balance
		                    ,N.ReferenceNo label
		                    ,N.GrandTotal
		                    ,N.NFAID value
		                    ,N.GrandTotal extraJsonProps					
		                    ,N.NFAID
		                    ,N.ReferenceNo NFAReferenceNo
                            
	                    from NFAMaster N
	                    LEFT JOIN (SELECT SUM(ISNULL(pm.GrandTotal,0)) AlreadyCreatedAmt,M.NFAID FROM 
				                    SCM..PurchaseRequisitionMaster pm
				                    LEFT JOIN SCM..PRNFAMap M ON M.PRMID=pm.PRMasterID
				                    where pm.ApprovalStatusID<>24 AND pm.PRMasterID!={PRMasterID} 
				                    Group By M.NFAID
	                    ) PRC ON PRC.NFAID=N.NFAID
                        where N.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ReferenceNo like '%{param}%'";
            }
            else
            {
                sql = @$"SELECT
		                    N.ReferenceNo NFANo
		                    ,N.GrandTotal NFAAmount
		                    ,ISNULL(PRC.AlreadyCreatedAmt, 0) CreatedNFAAmount					
		                    ,N.GrandTotal - ISNULL(PRC.AlreadyCreatedAmt, 0) Balance
		                    ,N.ReferenceNo label
		                    ,N.GrandTotal
		                    ,N.NFAID value
		                    ,N.GrandTotal extraJsonProps					
		                    ,N.NFAID
		                    ,N.ReferenceNo NFAReferenceNo
                            
	                    from NFAMaster N
                        left join Users U on U.UserID = N.CreatedBy
                        left join hrms..ViewALLEmployee v on v.PersonID = u.PersonID
	                    LEFT JOIN (SELECT SUM(ISNULL(pm.GrandTotal,0)) AlreadyCreatedAmt,M.NFAID FROM 
				                    SCM..PurchaseRequisitionMaster pm
				                    LEFT JOIN SCM..PRNFAMap M ON M.PRMID=pm.PRMasterID
				                    where pm.ApprovalStatusID<>24 AND pm.PRMasterID!={PRMasterID} 
				                    Group By M.NFAID
	                    ) PRC ON PRC.NFAID=N.NFAID
                        where N.ApprovalStatusID={(int)Util.ApprovalStatus.Approved} AND ReferenceNo like '%{param}%' and v.DepartmentID = {AppContexts.User.DepartmentID}";
            }
            var listDict = UnitRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }
        public async Task<IEnumerable<Dictionary<string, object>>> GetNFAListComboManual(string param, string isScm, int PRMasterID)
        {
            var sql = "";
            if (isScm == "scm")
            {
                sql = @$"SELECT	DISTINCT
		                    N.NFAReferenceNo NFANo
		                    ,N.NFAReferenceNo label
		                    ,N.NFAAmount
		                    ,N.NFAID value
		                    ,N.NFAAmount extraJsonProps
		                    ,ISNULL(PRC.AlreadyCreatedAmt, 0) CreatedNFAAmount							
		                    ,N.NFAAmount - ISNULL(PRC.AlreadyCreatedAmt, 0) Balance
                            ,ISNULL(C.NFACount,0) NFACount
                            
	                    from SCM..PRNFAMap N
	                    LEFT JOIN SCM..PurchaseRequisitionMaster PM ON PM.PRMasterID = N.PRMID
	                    LEFT JOIN (SELECT SUM(ISNULL(pm.GrandTotal,0)) AlreadyCreatedAmt, M.NFAReferenceNo FROM 
									SCM..PurchaseRequisitionMaster pm
									LEFT JOIN SCM..PRNFAMap M ON M.PRMID=pm.PRMasterID AND M.IsFromSystem=0
									where pm.ApprovalStatusID<>24 AND pm.PRMasterID!={PRMasterID}
									Group By M.NFAReferenceNo
						) PRC ON PRC.NFAReferenceNo=N.NFAReferenceNo
						LEFT JOIN  (SELECT COUNT(*) NFACount, P.NFAReferenceNo FROM SCM..PRNFAMap P WHERE P.IsFromSystem = 0 GROUP BY P.NFAReferenceNo ) C ON C.NFAReferenceNo = N.NFAReferenceNo

	                    
	                    WHERE PM.ApprovalStatusID <> 24 and N.IsFromSystem = 0 AND  N.NFAReferenceNo like '%{param}%'";
            }
            else
            {
                sql = @$"SELECT	DISTINCT
		                    N.NFAReferenceNo NFANo
		                    ,N.NFAReferenceNo label
		                    ,N.NFAAmount
		                    ,N.NFAID value
		                    ,N.NFAAmount extraJsonProps
		                    ,ISNULL(PRC.AlreadyCreatedAmt, 0) CreatedNFAAmount							
		                    ,N.NFAAmount - ISNULL(PRC.AlreadyCreatedAmt, 0) Balance
                            ,ISNULL(C.NFACount,0) NFACount
                            
	                    from SCM..PRNFAMap N
                        left join Users U on U.UserID = N.CreatedBy
                        left join hrms..ViewALLEmployee v on v.PersonID = u.PersonID
	                    LEFT JOIN SCM..PurchaseRequisitionMaster PM ON PM.PRMasterID = N.PRMID
	                    LEFT JOIN (SELECT SUM(ISNULL(pm.GrandTotal,0)) AlreadyCreatedAmt, M.NFAReferenceNo FROM 
									SCM..PurchaseRequisitionMaster pm
									LEFT JOIN SCM..PRNFAMap M ON M.PRMID=pm.PRMasterID AND M.IsFromSystem=0
									where pm.ApprovalStatusID<>24 AND pm.PRMasterID!={PRMasterID}
									Group By M.NFAReferenceNo
						) PRC ON PRC.NFAReferenceNo=N.NFAReferenceNo
						LEFT JOIN  (SELECT COUNT(*) NFACount, P.NFAReferenceNo FROM SCM..PRNFAMap P WHERE P.IsFromSystem = 0 GROUP BY P.NFAReferenceNo ) C ON C.NFAReferenceNo = N.NFAReferenceNo

	                    WHERE PM.ApprovalStatusID <> 24 and N.IsFromSystem = 0 AND  N.NFAReferenceNo like '%{param}%' and v.DepartmentID = {AppContexts.User.DepartmentID}";
            }
            var listDict = UnitRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetCommonInterfaceFieldsAutocomplete(int RowID)
        {
            //var listDict = new Enumerable<Dictionary<string, object>>();
            var field = CommonInterfaceFieldsRepo.Get(RowID);
            string sql = string.IsNullOrEmpty(field.AutocompleteSource) ? "" : @$"{field.AutocompleteSource}";
            var listDict = string.IsNullOrEmpty(field.AutocompleteSource) ? null : CommonInterfaceFieldsRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetBusinessSupportItems(string param)
        {
            if(param == "0")
            {
                param = "";
            }
            string sql = @$"SELECT * FROM BusinessSupportItem WHERE (ItemName LIKE '%{param}%') ORDER BY ItemName";
            var listDict = BusinessSupportItemRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<ComboModel>> SupportRequisitionItem(int categoryID)
        {
            var systemVariableList = await SupportRequisitionItemRepo.GetAllListAsync(obj => obj.CompanyID == AppContexts.User.CompanyID.ToString() && obj.CategoryID == categoryID);
            return systemVariableList.OrderBy(x => x.ItemID).Select(x => new ComboModel { value = (int)x.ItemID, label = x.ItemName }).ToList();
        }

        public async Task<IEnumerable<Dictionary<string, object>>> SupportRequisitionAllItem(string param, string category)
        {
            if (param == "0")
            {
                param = "";
            } 
            string sql = @$"SELECT * FROM Security..SupportRequisitionItem WHERE CategoryID={category} AND (ItemName LIKE '%{param}%') ORDER BY ItemName";
            var listDict = SupportRequisitionItemRepo.GetDataDictCollection(sql);
            return await Task.FromResult(listDict);
        }

        public async Task<List<ComboModel>> GetDSOWalletComboValue()
        {
            var menuList = await UserProfileRepo.GetAllListAsync(x => x.PositionID == 27);
            return menuList.Take(100).Select(x => new ComboModel { value = x.UserID, label = x.ContactNumber }).OrderBy(x => x.label).ToList();
        }

    

        public async Task<List<ComboModel>> GetUserProfileCommon(int id)
        {
            var menuList = await UserProfileRepo.GetAllListAsync(x => x.PositionID == id);
            return menuList.Take(100).Select(x => new ComboModel { value = x.UserID, label = x.UserFullName + "#" + x.ContactNumber }).OrderBy(x => x.label).ToList();
        }

        public async Task<List<ComboModel>> GetPositionList()
        {
            var userGroupList = await SecurityGroupRepo.GetAllListAsync();

            // Filter the list to include only SecurityGroupID values between 20 and 34
            var filteredList = userGroupList
                .Where(x => x.SecurityGroupID >= 20 && x.SecurityGroupID <= 35)
                .Select(x => new ComboModel { value = x.SecurityGroupID, label = x.SecurityGroupName })
                .ToList();

            return filteredList;
        }
        public async Task<List<ComboModel>> GetPositionsForDHFF()
        {
            var userGroupList = await SecurityGroupRepo.GetAllListAsync();
            var filteredList = userGroupList
                .Where(x => new[] { 20, 24, 25, 27, 37 }.Contains(x.SecurityGroupID))
                .Select(x => new ComboModel { value = x.SecurityGroupID, label = x.SecurityGroupName })
                .ToList();

            return filteredList;
        }




        public async Task<List<ComboModel>> GetDistributionHouses(int positionID)
        {
            string sql = @$"select UserID value,UserFullName label from UserProfile where PositionID= {positionID}";
            return SecurityGroupRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetRSM_ZSM_TM_TO()
        {
            //string sql = @$"select UserID value,UserFullName label from UserProfile where PositionID IN (29,31,34,35)";
            string sql = @$"SELECT DISTINCT U.UserID value ,Emp.FullName label
                            FROM Users U
                            LEFT JOIN Menu M ON U.DefaultMenuID=M.MenuID
							LEFT JOIN Security..SecurityGroupUserChild SGUC ON SGUC.UserID = U.UserID
							LEFT JOIN Security..SecurityGroupMaster SGM ON SGM.SecurityGroupID = SGUC.SecurityGroupID
							LEFT JOIN HRMS..Employee Emp ON Emp.PersonID = U.PersonID
							LEFT JOIN HRMS..Employment Em ON Em.EmployeeID = Emp.EmployeeID AND IsCurrent = 1
		                    LEFT JOIN HRMS..Region R ON EM.RegionID = R.RegionID
							LEFT JOIN HRMS..Cluster C ON EM.ClusterID = C.ClusterID
							Where U.PersonID <> 0 AND   U.IsActive=1 AND SGUC.SecurityGroupID IN (29, 34, 35)";
          
            return SecurityGroupRepo.GetDataModelCollection<ComboModel>(sql);
        }

        public async Task<List<ComboModel>> GetDistinctEntityTypes()
        {

            string sql = @"SELECT DISTINCT EntityTypeID AS value, EntityTypeName AS label FROM SystemVariable";
            return SecurityGroupRepo.GetDataModelCollection<ComboModel>(sql);
        }
    }
}
