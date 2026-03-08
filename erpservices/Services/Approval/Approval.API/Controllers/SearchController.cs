using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Core;
using Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Approval.Manager;
using Approval.Manager.Interfaces;

namespace Approval.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class SearchController : BaseController
    {
        private readonly ISearchManager Manager;
        public SearchController(ISearchManager manager)
        {
            Manager = manager;
        }       
    }
}