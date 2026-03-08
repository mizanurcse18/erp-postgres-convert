using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Mail.Manager.Interfaces;
using System.Threading.Tasks;

namespace Mail.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ComboController : BaseController
    {
        private readonly IComboManager Manager;

        public ComboController(IComboManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetMailConfigurations")]
        public async Task<ActionResult> GetMailConfigurations()
        {
            var list = await Manager.GetMailConfigurations();
            return OkResult(list);
        }
    }
}