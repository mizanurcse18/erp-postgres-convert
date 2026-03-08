using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using API.Core;
using API.Core.Hubs;
using API.Core.Interface;
using Core;
using Core.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class CustodianWalletController : BaseController
    {
        private readonly ICustodianWalletManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;
        public CustodianWalletController(ICustodianWalletManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }
        [HttpGet("Get/{CWID:int}")]
        public async Task<IActionResult> Get(int CWID)
        {

            var wallets = await Manager.Get(CWID);
            var transactionDetails = await Manager.TransactionDetailsByCWID(CWID);
            var attachments = Manager.GetAttachments(CWID);
            return OkResult(new { Master = wallets, TransactionDetails = transactionDetails, Attachments = attachments });
        }
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] CustodianWalletDto wallets)
        {
            var response = await Manager.Save(wallets);
            return OkResult(new { status = response.Item1, message = response.Item2 });
        }


        [HttpPost("GetAll")]
        public IActionResult GetAll([FromBody] GridParameter parameters)
        {
            var model = Manager.GetAll(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("Delete/{CWID:int}")]
        public IActionResult DeletBank(int CWID)
        {
            bool status = Manager.DeleteWallet(CWID);
            return OkResult(new { Success = status });
        }
    }
}
