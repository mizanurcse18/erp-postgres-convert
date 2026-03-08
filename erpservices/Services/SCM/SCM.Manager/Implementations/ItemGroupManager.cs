using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.CommonDto;
using Manager.Core.Mapper;
using SCM.DAL.Entities;
using SCM.Manager.Dto;
using SCM.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{

    public class ItemGroupManager : ManagerBase, IItemGroupManager
    {
        private readonly IRepository<ItemGroup> ItemGroupRepo;
        public ItemGroupManager(IRepository<ItemGroup> itemGroupRepo)
        {
            ItemGroupRepo = itemGroupRepo;
        }

        public async Task<List<ItemGroupDto>> GetItemGroupList()
        {
            //var itemGroup = await ItemGroupRepo.GetAllListAsync();
            //return itemGroup.MapTo<List<ItemGroupDto>>();
            string sql = $@"SELECT IG.*
	                        ,CASE 
		                        WHEN ISG.ItemGroupID IS NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsRemovable
                        FROM ItemGroup IG
                        LEFT JOIN (
	                        SELECT DISTINCT ItemGroupID
	                        FROM ItemSubGroup
	                        ) ISG ON IG.ItemGroupID = ISG.ItemGroupID ORDER BY IG.ItemGroupID DESC";

            return await Task.FromResult(ItemGroupRepo.GetDataModelCollection<ItemGroupDto>(sql));
        }

        public void SaveChanges(ItemGroupDto itemGroupDto)
        {
            using var unitOfWork = new UnitOfWork();
            var existItemGroup = ItemGroupRepo.Entities.SingleOrDefault(x => x.ItemGroupID == itemGroupDto.ItemGroupID).MapTo<ItemGroup>();

            if (existItemGroup.IsNull() || existItemGroup.ItemGroupID.IsZero() || existItemGroup.IsAdded)
            {
                itemGroupDto.SetAdded();
                SetNewItemGroupCode(itemGroupDto);
                SetNewItemGroupID(itemGroupDto);
            }
            else
            {
                itemGroupDto.SetModified();
            }
            var userEnt = itemGroupDto.MapTo<ItemGroup>();
            userEnt.CompanyID = itemGroupDto.CompanyID ?? AppContexts.User.CompanyID;


            ItemGroupRepo.Add(userEnt);
            unitOfWork.CommitChangesWithAudit();
        }

        private void SetNewItemGroupID(ItemGroupDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ItemGroup", AppContexts.User.CompanyID);
            obj.ItemGroupID = code.MaxNumber;
        }

        public async Task<ItemGroupDto> GetItemGroup(int itemGroupId)
        {
            var itemGroup = ItemGroupRepo.Entities.SingleOrDefault(x => x.ItemGroupID == itemGroupId).MapTo<ItemGroupDto>();
            return await Task.FromResult(itemGroup);
        }

        public async Task DeleteItemGroup(int itemGroupId)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var itemGroupEnt = ItemGroupRepo.Entities.Where(x => x.ItemGroupID == itemGroupId).FirstOrDefault();

                itemGroupEnt.SetDeleted();
                ItemGroupRepo.Add(itemGroupEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;

        }
        private void SetNewItemGroupCode(ItemGroupDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ItemGroupCode", AppContexts.User.CompanyID);
            obj.ItemGroupCode = code.MaxNumber.ToString();
        }
    }
}
