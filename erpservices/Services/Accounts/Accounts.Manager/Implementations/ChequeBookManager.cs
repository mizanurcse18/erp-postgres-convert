using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Accounts.DAL.Entities;
using Accounts.Manager.Dto;
using Accounts.Manager.Interfaces;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accounts.Manager.Implementations
{

    public class ChequeBookManager : ManagerBase, IChequeBookManager
    {
        private readonly IRepository<ChequeBook> ChequeBookRepo;
        private readonly IRepository<ChequeBookChild> ChequeBookChildRepo;
        private readonly IRepository<GeneralLedger> GeneralLedgerRepo;
        public ChequeBookManager(IRepository<ChequeBook> chequeBookRepo, IRepository<ChequeBookChild> chequeBookChildRepo, IRepository<GeneralLedger> generalLedgerRepo)
        {
            ChequeBookRepo = chequeBookRepo;
            ChequeBookChildRepo = chequeBookChildRepo;
            GeneralLedgerRepo = generalLedgerRepo;
        }

        public async Task<List<ChequeBookDto>> GetChequeBookList()
        {
            //string sql = $@"select gl.GLName, CONCAT(ck.BankID, '-'+gl.GLName) as BankName, gl.GLCode, ck.* from ChequeBook ck left join GeneralLedger gl on ck.GLID = gl.GLID";
            string sql = $@"select gl.GLName, BN.BankName, gl.GLCode, ck.* from ChequeBook ck 
                            left join GeneralLedger gl on ck.GLID = gl.GLID
                            left join Bank BN on BN.BankID = ck.BankID";

            return await Task.FromResult(ChequeBookRepo.GetDataModelCollection<ChequeBookDto>(sql));
        }

        public void SaveChanges(ChequeBookDto chequeBookDto)
        {
            using var unitOfWork = new UnitOfWork();
            var existChequeBook = ChequeBookRepo.Entities.SingleOrDefault(x => x.CBID == chequeBookDto.CBID).MapTo<ChequeBook>();

            if (existChequeBook.IsNull() || existChequeBook.CBID.IsZero() || existChequeBook.IsAdded)
            {
                chequeBookDto.SetAdded();
                SetNewChequeBookID(chequeBookDto);
            }
            else
            {
                chequeBookDto.SetModified();
            }
            var userEnt = chequeBookDto.MapTo<ChequeBook>();
            userEnt.CompanyID = chequeBookDto.CompanyID ?? AppContexts.User.CompanyID;

         

            var childModel = GenerateCBChild(chequeBookDto);
            SetAuditFields(childModel);


            ChequeBookRepo.Add(userEnt);
            ChequeBookChildRepo.AddRange(childModel);

            unitOfWork.CommitChangesWithAudit();
        }

        private void SetNewChequeBookID(ChequeBookDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ChequeBook", AppContexts.User.CompanyID);
            obj.CBID = code.MaxNumber;
        }

        public async Task<ChequeBookDto> GetChequeBook(int chequeBookId)
        {
            //var chequeBook = ChequeBookRepo.Entities.SingleOrDefault(x => x.CBID == chequeBookId).MapTo<ChequeBookDto>();
            var sql = @$"select gl.GLName, BN.BankName ,ck.* from ChequeBook ck
                        left join Bank BN on BN.BankID = ck.BankID
                        left join GeneralLedger gl
                        on ck.GLID = gl.GLID
                        where ck.CBID = {chequeBookId}";
            var chequeBook = ChequeBookRepo.GetModelData<ChequeBookDto>(sql);
            return await Task.FromResult(chequeBook);
        }

        public async Task<List<ChequeBookChild>> GetChequeBookChild(int chequeBookId)
        {
            var chequeBookList = await ChequeBookChildRepo.GetAllListAsync(x => x.CBID == chequeBookId);

            return chequeBookList;
        }

    
        public async Task<List<ChequeBookChild>> GetChequeBookLeaf(int chequeBookId)
        {
            var chequeBookList = await ChequeBookChildRepo.GetAllListAsync(x => x.CBID == chequeBookId && x.IsUsed == false && x.IsActiveLeaf == true);

            return chequeBookList.OrderBy(x => x.LeafNo).ToList();
        }

        public void DeleteChequeBook(int chequeBookId)
        {
            using var unitOfWork = new UnitOfWork();
            var chequeBook = ChequeBookRepo.Entities.SingleOrDefault(x => x.CBID == chequeBookId);
            chequeBook.SetDeleted();
            ChequeBookRepo.Add(chequeBook);

            unitOfWork.CommitChangesWithAudit(); 
        }

        private List<ChequeBookChild> GenerateCBChild(ChequeBookDto chequeBook)
        {
            var existingCBChild = ChequeBookChildRepo.Entities.Where(x => x.CBID == chequeBook.CBID).ToList();
            var childModel = new List<ChequeBookChild>();
            //if (chequeBook.ChequeBookChildItemDetails.IsNotNull())
            //{

            //List<ChequeBookChildItemDetails> listCBItemDetails = new List<ChequeBookChildItemDetails>();
            List<ChequeBookChild> listCBChild = new List<ChequeBookChild>();

            for (int i = 0; i <= (chequeBook.EndLeaf - chequeBook.StartLeaf); i++)
            {
                ChequeBookChild chequeBookChild = new ChequeBookChild();
                chequeBookChild.LeafNo = chequeBook.StartLeaf + i;
                chequeBookChild.CBID = chequeBook.CBID;
                chequeBookChild.IsActiveLeaf = true;
                chequeBookChild.IsUsed = false;
                listCBChild.Add(chequeBookChild);
            }

            if (chequeBook.ChequeBookChildItemDetails?.Count > 0)
            {
                chequeBook.ChequeBookChildItemDetails.ForEach(x =>
                {
                    childModel.Add(new ChequeBookChild
                    {
                        CBCID = x.CBCID,
                        CBID = chequeBook.CBID,
                        LeafNo = x.LeafNo,
                        IsActiveLeaf = x.IsActiveLeaf,
                        IsUsed = x.IsUsed
                    });
                });
            }
            else
            {
                listCBChild.ForEach(x =>
                {
                    childModel.Add(new ChequeBookChild
                    {
                        CBCID = x.CBCID,
                        CBID = chequeBook.CBID,
                        LeafNo = x.LeafNo,
                        IsActiveLeaf = x.IsActiveLeaf,
                        IsUsed = x.IsUsed
                    });
                });
            }



                childModel.ForEach(x =>
                {
                    if (existingCBChild.Count > 0 && x.CBCID > 0)
                    {
                        var existingModelData = existingCBChild.FirstOrDefault(y => y.CBCID == x.CBCID);
                        x.CreatedBy = existingModelData.CreatedBy;
                        x.CreatedDate = existingModelData.CreatedDate;
                        x.CreatedIP = existingModelData.CreatedIP;
                        x.RowVersion = existingModelData.RowVersion;
                        x.SetModified();
                    }
                    else
                    {
                        x.CBID = chequeBook.CBID;
                        x.SetAdded();
                        SetChequeBookChildNewId(x);
                    }
                });

                var willDeleted = existingCBChild.Where(x => !childModel.Select(y => y.CBCID).Contains(x.CBCID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childModel.Add(x);
                });
            //}


            return childModel;
        }

        private void SetChequeBookChildNewId(ChequeBookChild child)
        {
            if (!child.IsAdded) return;
            var code = GenerateSystemCode("ChequeBookChild", AppContexts.User.CompanyID);
            child.CBCID = code.MaxNumber;
        }

    }
}
