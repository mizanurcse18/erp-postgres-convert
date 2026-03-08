using API.Core;
using Accounts.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ComboController : BaseController
    {
        private readonly IComboManager Manager;

        public ComboController(IComboManager manager)
        {
            Manager = manager;
        }


        [HttpGet("GetGLCombo/{param}")]
        public async Task<ActionResult> GetGLCombo(string param)
        {
            var list = await Manager.GetGLCombo(param);
            return OkResult(list);
        }

        [HttpGet("GetGLComboList")]
        public async Task<ActionResult> GetGLComboList(string param)
        {
            var list = await Manager.GetGLComboList();
            return OkResult(list);
        }

        [HttpGet("GetIOUs")]
        public async Task<ActionResult> GetIOUs()
        {
            var list = await Manager.GetIOUList();
            return OkResult(list);
        }

        [HttpGet("GetBankList")]
        public async Task<ActionResult> GetBankList()
        {

            var banks = await Manager.GetBankList();
            return OkResult(banks);
        }

        [HttpGet("GetAllBankForDropdown")]
        public async Task<ActionResult> GetAllBankForDropdown(string param)
        {
            var list = await Manager.GetBankList();
            return OkResult(list);
        }

        [HttpGet("GetAllChequebookForDropdown")]
        public async Task<ActionResult> GetAllChequebookForDropdown()
        {
            var list = await Manager.GetAllChequebookForDropdown();
            return OkResult(list);
        }
        [HttpGet("GetCOACategoryListCombo")]
        public async Task<ActionResult> GetCOACategoryListCombo()
        {
            var list = await Manager.GetCOACategoryListCombo();
            return OkResult(list);
        }


        [HttpGet("GetCOAGLListCombo/{param}")]
        public async Task<ActionResult> GetCOAGLListCombo(string param)
        {
            var list = await Manager.GetCOAGLListCombo(param);
            return OkResult(list);
        }

        [HttpGet("GetCOAChqBookCombo/{param}")]
        public async Task<ActionResult> GetCOAChqBookCombo(string param)
        {
            var list = await Manager.GetCOAChqBookCombo(param);
            return OkResult(list);
        }

        [HttpGet("GetCOAChqBookPageCombo/{param}")]
        public async Task<ActionResult> GetCOAChqBookPageCombo(string param)
        {
            var list = await Manager.GetCOAChqBookPageCombo(param);
            return OkResult(list);
        }

        [HttpGet("GetWalletList")]
        public async Task<ActionResult> GetWalletList(string param)
        {
            var list = await Manager.GetWalletList();
            return OkResult(list);
        }
    }
}