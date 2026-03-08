using Core;
using Accounts.Manager.Dto;
using Manager.Core.CommonDto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Accounts.DAL.Entities;

namespace Accounts.Manager.Interfaces
{
    public interface IChequeBookManager
    {
        Task<List<ChequeBookDto>> GetChequeBookList();
        void SaveChanges(ChequeBookDto chequeBookDto);
        void DeleteChequeBook(int chequeBookId);
        Task<ChequeBookDto> GetChequeBook(int chequeBookId);
        Task<List<ChequeBookChild>> GetChequeBookChild(int chequeBookId);
        Task<List<ChequeBookChild>> GetChequeBookLeaf(int chequeBookId);

    }
}
