using API.Core;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Accounts.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class ChequeBookController : BaseController
    {
        private readonly IChequeBookManager Manager;

        public ChequeBookController(IChequeBookManager manager)
        {
            Manager = manager;
        }
        [HttpGet("GetAll")]
        public async Task<ActionResult> GetChequeBookList()
        {

            var chequeBooks = await Manager.GetChequeBookList();
            return OkResult(chequeBooks);
        }

        [HttpPost("CreateChequeBook")]
        public IActionResult CreateChequeBook([FromBody] ChequeBookDto chequeBook)
        {
            Manager.SaveChanges(chequeBook);
            return OkResult(chequeBook);
        }

        [HttpGet("Get/{CBID:int}")]
        public async Task<IActionResult> GetChequeBook(int CBID)
        {
            var chequeBookMaster = await Manager.GetChequeBook(CBID);
            var child = await Manager.GetChequeBookChild(CBID);
            return OkResult( new { chequeBooks = chequeBookMaster, childList = child });
        }

        [HttpGet("GetChequeBookLeaf/{CBID:int}")]
        public async Task<IActionResult> GetChequeBookLeaf(int CBID)
        {
            var chequeBooksLeaf = await Manager.GetChequeBookLeaf(CBID);
            return OkResult(chequeBooksLeaf);
        }

        [HttpGet("Delete/{CBID:int}")]
        public IActionResult DeletChequeBook(int CBID)
        {
            Manager.DeleteChequeBook(CBID);
            return Ok(new { Success = true });
        }

    }
}
