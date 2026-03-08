using Accounts.Manager.Dto;
using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Interfaces
{
    public interface IComboManager
    {
        Task<List<ComboModel>> GetIOUList();
        Task<IEnumerable<Dictionary<string, object>>> GetGLCombo(string param);
        Task<List<ComboModel>> GetGLComboList();
        Task<List<ComboModel>> GetWalletList();

        Task<List<BankDto>> GetBankList();

        //Task<IEnumerable<Dictionary<string, object>>> GetAllBankForDropdown();
        Task<IEnumerable<Dictionary<string, object>>> GetAllChequebookForDropdown();
        Task<List<ComboModel>> GetCOACategoryListCombo();
        Task<List<ComboModel>> GetCOAGLListCombo(string param);
        Task<List<ComboModel>> GetCOAChqBookCombo(string param);
        Task<List<ComboModel>> GetCOAChqBookPageCombo(string param);


    }
}
