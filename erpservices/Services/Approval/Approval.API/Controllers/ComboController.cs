using API.Core;
using Approval.Manager.Interfaces;
using Core.AppContexts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ComboController : BaseController
    {
        private readonly IComboManager Manager;

        public ComboController(IComboManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetApprovalPanelCombo")]
        public async Task<ActionResult> GetApprovalPanelCombo()
        {
            var list = await Manager.GetApprovalPanelCombo();
            return OkResult(list);
        }

        [HttpGet("GetDynamicApprovalPanelCombo")]
        public async Task<ActionResult> GetDynamicApprovalPanelCombo()
        {
            var list = await Manager.GetDynamicApprovalPanelCombo();
            return OkResult(list);
        }


        [HttpGet("GetApprovalPanelByEmployeeID/{EmployeeID:int}")]
        public async Task<ActionResult> GetApprovalPanelByEmployeeID(int EmployeeID)
        {
            int empId = EmployeeID > 0 ? EmployeeID : AppContexts.User.EmployeeID.Value;
            var list = await Manager.GetApprovalPanelByEmployeeCombo(empId);
            return OkResult(list);
        }
        [HttpGet("GetApprovalTypesList")]
        public async Task<ActionResult> GetApprovalTypesList()
        {
            var list = await Manager.GetApprovalTypesList();
            return OkResult(list);
        }
        [HttpGet("GetsTemplateCombo/{isHR:int}")]
        public async Task<ActionResult> GetsTemplateCombo(int isHR)
        {
            var list = await Manager.GetsTemplateCombo(isHR);
            return OkResult(list);
        }

    }
}