using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Core;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.Manager;
using Security.Manager.Interfaces;

namespace Security.API.Controllers
{
    [Authorize,ApiController, Route("[controller]")]
    public class ComboController : BaseController
    {
        private readonly IComboManager Manager;

        public ComboController(IComboManager manager)
        {
            Manager = manager;
        }

        // GET: /Combo/GetSecurityGroupComboSource
        [HttpGet("GetSecurityGroupList")]
        public async Task<ActionResult> GetSecurityGroupList()
        {
            //return OkResult(Manager.GetDemoMasterIndependentComboSource());
            var list = await Manager.GetSecurityGroupList();
            return OkResult(list);
        }


        [HttpGet("GetDivisionOfCountry")]
        public async Task<ActionResult> GetDivisionOfCountry()
        {
            var list = await Manager.GetDivisionOfCountry();
            return OkResult(list);
        }
        // GET: /Combo/GetCompanies
        [HttpGet("GetCompanies")]
        public async Task<ActionResult> GetCompanies()
        {
            var list = await Manager.GetCompanies();
            return OkResult(list);
        }
        // GET: /Combo/GetGenders
        [HttpGet("GetAllSupportRequestType")]
        public async Task<ActionResult> GetAllSupportRequestType()
        {
            var list = await Manager.SystemVariable(46);
            return OkResult(list);
        }
         [HttpGet("GetAllRequisitionSupportType")]
        public async Task<ActionResult> GetAllRequisitionSupportType()
        {
            var list = await Manager.SystemVariable(54);
            return OkResult(list);
        }

        [HttpGet("GetAllSupportRequestLocation")]
        public async Task<ActionResult> GetAllSupportRequestLocation()
        {
            var list = await Manager.GetRequestLocation();
            return OkResult(list);
        }

        [HttpGet("GetAllFacilityOrSupportCategory")]
        public async Task<ActionResult> GetAllFacilityOrSupportCategory()
        {
            var list = await Manager.SystemVariable(47);
            return OkResult(list);
        }
         [HttpGet("GetAllRenovationORMaintenanceCategory")]
        public async Task<ActionResult> GetAlRenovationORMaintenanceCategory()
        {
            var list = await Manager.SystemVariable(47);
            return OkResult(list);
        }

        [HttpGet("GetAllTransportType")]
        public async Task<ActionResult> GetAllTransportType()
        {
            var list = await Manager.SystemVariable(48);
            return OkResult(list);
        }

        [HttpGet("GetAllTransports")]
        public async Task<ActionResult> GetAllTransports()
        {
            var list = await Manager.GetAllTransports();
            return OkResult(list);
        }
        [HttpGet("GetAllDrivers")]
        public async Task<ActionResult> GetAllDrivers()
        {
            var list = await Manager.GetAllDrivers();
            return OkResult(list);
        }

        // GET: /Combo/GetGenders
        [HttpGet("GetGenders")]
        public async Task<ActionResult> GetGenders()
        {
            var list = await Manager.SystemVariable(1);
            return OkResult(list);
        }

        [HttpGet("GetAdminSupportCategoryList")]
        public async Task<ActionResult> GetAdminSupportCategoryList()
        {
            var list = await Manager.SystemVariable(46);
            return OkResult(list);
        }

        [HttpGet("GetSupportRequisitionCategoryList")]
        public async Task<ActionResult> GetSupportRequisitionCategoryList()
        {
            var list = await Manager.SystemVariable(54);
            return OkResult(list);
        }
        // GET: /Combo/GetGenders
        [HttpGet("GetReligions")]
        public async Task<ActionResult> GetReligions()
        {
            var list = await Manager.SystemVariable(2);
            return OkResult(list);
        }

        // GET: /Combo/GetSystemVariables
        [HttpGet("GetSystemVariables/{EntityTypeID:int}")]
        public async Task<ActionResult> GetSystemVariables(int EntityTypeID)
        {
            var list = await Manager.SystemVariable(EntityTypeID);
            return OkResult(list);
        }

        // GET: /Combo/GetSystemVariables
        //[HttpGet("GetEmployeeJobStatus")]
        //public async Task<ActionResult> GetEmployeeJobStatus()
        //{
        //    var list = await Manager.GetEmployeeJobStatus();
        //    return OkResult(list);
        //}

        // GET: /Combo/GetGenders
        [HttpGet("GetPersons")]
        public async Task<ActionResult> GetPersons()
        {
            var list = await Manager.GetPersons();
            return OkResult(list);
        }
        
        [HttpGet("GetUnitCombo")]
        public async Task<ActionResult> GetUnitCombo()
        {
            var list = await Manager.GetUnitCombo();
            return OkResult(list);
        }

        [HttpGet("GetPersonsWithEmployeeCode")]
        public async Task<ActionResult> GetPersonsWithEmployeeCode()
        {
            var list = await Manager.GetPersonsWithCode();
            return OkResult(list);
        }

        [HttpGet("GetDistricts")]
        public async Task<ActionResult> GetDistricts()
        {
            var list = await Manager.GetDistricts();
            return OkResult(list);
        }
        [HttpGet("GetThanas/{districtId:int}")]
        public async Task<ActionResult> GetThanas(int districtId)
        {
            var list = await Manager.GetThanas(districtId);
            return OkResult(list);
        }
        [HttpGet("GetDistrictsByDivision/{DivisionID:int}")]
        public async Task<ActionResult> GetDistrictsByDivision(int DivisionID)
        {
            var list = await Manager.GetDistrictsByDivision(DivisionID);
            return OkResult(list);
        }

        [HttpGet("GetAllMonths")]
        public async Task<ActionResult> GetAllMonths()
        {
            var list = await Manager.getAllMonths();
            return OkResult(list);
        }
        
        [HttpGet("GetFinancialYearForTax")]
        public async Task<ActionResult> GetFinancialYearForTax()
        {
            var list = await Manager.GetFinancialYearForTax();
            return OkResult(list);
        }
        [HttpGet("GetFinancialYear")]
        public async Task<ActionResult> GetFinancialYear()
        {
            var list = await Manager.GetFinancialYear();
            return OkResult(list);
        }
        [HttpGet("GetAllFinancialYear")]
        public async Task<ActionResult> GetAllFinancialYear()
        {
            var list = await Manager.GetAllFinancialYear();
            return OkResult(list);
        }
        [HttpGet("GetAllAssessmentYear")]
        public async Task<ActionResult> GetAllAssessmentYear()
        {
            var list = await Manager.GetAllAssessmentYear();
            return OkResult(list);
        }
        [HttpGet("GetMenuURLList")]
        public async Task<ActionResult> GetMenuURLList()
        {
            var list = await Manager.GetMenuURLList();
            return OkResult(list);
        }

        [HttpGet("GetFinancialYearForHoliday")]
        public async Task<ActionResult> GetFinancialYearForHoliday()
        {
            var list = await Manager.GetFinancialYearForHoliday();
            return OkResult(list);
        }
         [HttpGet("GetFinancialYearsForVatTax")]
        public async Task<ActionResult> GetFinancialYearsForVatTax()
        {
            var list = await Manager.GetFinancialYearsForVatTax();
            return OkResult(list);
        }


        [HttpGet("GetItemName/{param}")]
        public async Task<ActionResult> GetItemName(string param)
        {
            var list = await Manager.GetItemNames(param);
            return OkResult(list);
        }
        [HttpGet("GetUnitName/{param}")]
        public async Task<ActionResult> GetUnitName(string param)
        {
            var list = await Manager.GetUnitNames(param);
            return OkResult(list);
        }
        [HttpGet("GetExpensePurpose")]
        public async Task<ActionResult> GetExpensePurpose()
        {
            var list = await Manager.GetPurposeList();
            return OkResult(list);
        }

        [HttpGet("GetComboItem")]
        public async Task<ActionResult> GetComboItem()
        {
            var list = await Manager.GetComboItem();
            return OkResult(list);
        }

        [HttpGet("GetUnitsForStrategicNFA")]
        public async Task<ActionResult> GetUnits()
        {
            var list = await Manager.GetUnits();
            return OkResult(list);
        }

        [HttpGet("GetComboNfa")]
        public async Task<ActionResult> GetComboNfa()
        {
            var list = await Manager.SystemVariable(17);
            return OkResult(list);
        }
        [HttpPost("GetNFAListCombo")]
        public async Task<ActionResult> GetNFAListCombo([FromBody] ComboModel param)
        {
            var list = await Manager.GetNFAListCombo(param.label, param.extraJsonProps, param.value);
            return OkResult(list);
        }
        [HttpPost("GetNFAListComboManual")]
        public async Task<ActionResult> GetNFAListComboManual([FromBody] ComboModel param)
        {
            var list = await Manager.GetNFAListComboManual(param.label, param.extraJsonProps, param.value);
            return OkResult(list);
        }

        [HttpGet("GetCommonInterfaceFieldsAutocomplete/{RowID:int}")]
        public async Task<ActionResult> GetCommonInterfaceFieldsAutocomplete(int RowID)
        {
            var list = await Manager.GetCommonInterfaceFieldsAutocomplete(RowID);
            return OkResult(list);
        }

        [HttpGet("GetAllCOAModeOfPaymentType")]
        public async Task<ActionResult> GetAllCOAModeOfPaymentType()
        {
            var list = await Manager.SystemVariable(53);
            return OkResult(list);
        }

        [HttpGet("GetBusinessSupportItem/{param}")]
        public async Task<ActionResult> GetBusinessSupportItem(string param)
        {
            var list = await Manager.GetBusinessSupportItems(param);
            return OkResult(list);
        }


        [HttpGet("GetAllAccessType")]
        public async Task<ActionResult> GetAllAccessType()
        {
            var list = await Manager.SupportRequisitionItem(1);
            return OkResult(list);
        }
        [HttpGet("GetAllRequisitionSupportItem/{param}/{category}")]
        public async Task<ActionResult> GetAllRequisitionSupportItem(string param, string category)
        {
            var list = await Manager.SupportRequisitionAllItem(param, category);
            return OkResult(list);
        }

        [HttpGet("GetDSOWalletCombo")]
        public async Task<ActionResult> GetDSOWalletCombo()
        {
            var list = await Manager.GetDSOWalletComboValue();
            return OkResult(list);
        }

        [HttpGet("GetPositionList")]
        public async Task<ActionResult> GetPositionList()
        {
            //return OkResult(Manager.GetDemoMasterIndependentComboSource());
            var list = await Manager.GetPositionList();
            return OkResult(list);
        }
        [HttpGet("GetPositionsForDHFF")]
        public async Task<ActionResult> GetPositionsForDHFF()
        {
            var list = await Manager.GetPositionsForDHFF();
            return OkResult(list);
        }
        [HttpGet("GetDistributionHouses/{PositionID:int}")]
        public async Task<ActionResult> GetDistributionHouses(int PositionID)
        {
            var list = await Manager.GetDistributionHouses(PositionID);
            return OkResult(list);
        }

        [HttpGet("GetRSM_ZSM_TM_TO")]
        public async Task<ActionResult> GetRSM_ZSM_TM_TO()
        {
            var list = await Manager.GetRSM_ZSM_TM_TO();
            return OkResult(list);
        }


        [HttpGet("GetDSONameNumber")]
        public async Task<ActionResult> GetDSONameNumber()
        {
            var list = await Manager.GetUserProfileCommon(27);
            return OkResult(list);
        }
         [HttpGet("GetDSSNameNumber")]
        public async Task<ActionResult> GetDSSNameNumber()
        {
            var list = await Manager.GetUserProfileCommon(24);
            return OkResult(list);
        }      
        [HttpGet("GetDSONumber")]
        public async Task<ActionResult> GetDSONumber()
        {
            var list = await Manager.GetDSOWalletComboValue();
            return OkResult(list);
        }

        [HttpGet("GetDistinctEntityTypes")]
        public async Task<ActionResult> GetDistinctEntityTypes()
        {
            var list = await Manager.GetDistinctEntityTypes();
            return OkResult(list);
        }

    }
}