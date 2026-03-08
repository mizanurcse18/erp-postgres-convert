using API.Core;
using Approval.Manager.Dto;
using Core.AppContexts;
using Core.Extensions;
using HRMS.Manager.Interfaces;
using Manager.Core.CommonDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class APViewController : BaseController
    {
        private readonly IAPViewManager Manager;

        public APViewController(IAPViewManager manager)
        {
            Manager = manager;
        }

        [HttpGet("GetAPViews")]
        public async Task<IActionResult> GetAPViews(int APTypeID, int ReferenceID)
        {
            var APViews = await Manager.GetAPViewListDic(APTypeID, ReferenceID);
            return OkResult(APViews);
        }
        [HttpGet("GetAPViewForAll")]
        public async Task<IActionResult> GetAPViewForAll(int APTypeID, int ReferenceID)
        {
            return await GetAllAppView(APTypeID, ReferenceID);
        }
        [HttpGet("GetAPViewAllForPO")]
        public async Task<IActionResult> GetAPViewAllForPO(int APTypeID, int ReferenceID)
        {
            return await GetAllAppView(APTypeID, ReferenceID);
        }
        [HttpGet("GetAPViewAllForQC")]
        public async Task<IActionResult> GetAPViewAllForQC(int APTypeID, int ReferenceID)
        {
            return await GetAllAppView(APTypeID, ReferenceID);
        }
        [HttpGet("GetAPViewAllForGRN")]
        public async Task<IActionResult> GetAPViewAllForGRN(int APTypeID, int ReferenceID)
        {
            return await GetAllAppView(APTypeID, ReferenceID);
        }
        private async Task<IActionResult> GetAllAppView(int APTypeID, int ReferenceID)
        {
            var APViews = await Manager.GetAPViewForAll(APTypeID, ReferenceID);
            return OkResult(APViews);
        }
    }
}