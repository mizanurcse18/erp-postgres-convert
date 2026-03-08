using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface IComboManager
    {
        Task<List<ComboModel>> GetSecurityGroupList();
        Task<List<ComboModel>> GetDivisionOfCountry();
        Task<IEnumerable<Dictionary<string, object>>> GetCompanies();
        Task<List<ComboModel>> SystemVariable(int entityTypeID);
        Task<List<ComboModel>> GetPersons();
        Task<List<ComboModel>> GetUnitCombo();
        Task<List<ComboModel>> GetPersonsWithCode();
        Task<List<ComboModel>> GetDistricts();
        Task<List<ComboModel>> GetThanas(int districtId);
        Task<List<ComboModel>> GetDistrictsByDivision(int divisionID);
        Task<List<ComboModel>> getAllMonths();
        Task<List<ComboModel>> GetAllFinancialYear();
        Task<List<ComboModel>> GetAllAssessmentYear();
        Task<List<ComboModel>> GetFinancialYear();
        Task<List<ComboModel>> GetFinancialYearForTax();
        Task<List<ComboModel>> GetMenuURLList();
        Task<List<ComboModel>> GetFinancialYearForHoliday();
        Task<List<ComboModel>> GetFinancialYearsForVatTax();
        Task<IEnumerable<Dictionary<string, object>>> GetItemNames(string param);
        Task<IEnumerable<Dictionary<string, object>>> GetUnitNames(string param);
        Task<List<ComboModel>> GetPurposeList();
        Task<IEnumerable<Dictionary<string, object>>> GetComboItem();
        Task<List<ComboModel>> GetUnits();
        Task<IEnumerable<Dictionary<string, object>>> GetNFAListCombo(string param, string isScm, int PRMasterID);
        Task<IEnumerable<Dictionary<string, object>>> GetNFAListComboManual(string param, string isScm, int PRMasterID);
        Task<List<ComboModel>> GetRequestLocation();
        Task<List<ComboModel>> GetAllTransports();
        Task<List<ComboModel>> GetAllDrivers();
        Task<IEnumerable<Dictionary<string, object>>> GetCommonInterfaceFieldsAutocomplete(int RowID);
        Task<IEnumerable<Dictionary<string, object>>> GetBusinessSupportItems(string param);
        Task<IEnumerable<Dictionary<string, object>>> SupportRequisitionAllItem(string param, string category);

        Task<List<ComboModel>> SupportRequisitionItem(int categoryID);
        Task<List<ComboModel>> GetDSOWalletComboValue();
        Task<List<ComboModel>> GetPositionList();
        Task<List<ComboModel>> GetPositionsForDHFF();
        Task<List<ComboModel>> GetDistributionHouses(int positionID);
        Task<List<ComboModel>> GetRSM_ZSM_TM_TO();
        Task<List<ComboModel>> GetUserProfileCommon(int id);
        Task<List<ComboModel>> GetDistinctEntityTypes();

    }
}
