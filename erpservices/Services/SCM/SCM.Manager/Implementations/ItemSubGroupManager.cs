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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HRMS.Manager.Implementations
{
    public class ItemSubGroupManager : ManagerBase, IItemSubGroupManager
    {

        private readonly IRepository<ItemSubGroup> ItemSubGroupRepo;
        public ItemSubGroupManager(IRepository<ItemSubGroup> itemSubGroupRepo)
        {
            ItemSubGroupRepo = itemSubGroupRepo;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetItemSubGroupListDic()
        {
            string sql = $@"SELECT ISG.*, Dv.ItemGroupName
	                        ,CASE 
		                        WHEN I.ItemSubGroupID IS NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsRemovable
                        FROM ItemSubGroup ISG
                        LEFT JOIN ItemGroup Dv ON ISG.ItemGroupID = Dv.ItemGroupID
                        LEFT JOIN (
	                        SELECT DISTINCT ItemSubGroupID
	                        FROM Item
	                        ) I ON ISG.ItemSubGroupID = I.ItemSubGroupID
                        ORDER BY ISG.ItemSubGroupID DESC";
            var listDict = ItemSubGroupRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<Dictionary<string, object>> GetItemSubGroup(int ItemSubGroupID)
        {

            string sql = $@"SELECT D.*, Dv.ItemGroupName FROM ItemSubGroup D 
                            LEFT JOIN ItemGroup Dv ON D.ItemGroupID = Dv.ItemGroupID WHERE D.ItemSubGroupID={ItemSubGroupID}";

            var reg = ItemSubGroupRepo.GetData(sql);
            return await Task.FromResult(reg);
        }


        public async Task Delete(int ItemSubGroupID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var itemSubGroupEnt = ItemSubGroupRepo.Entities.Where(x => x.ItemSubGroupID == ItemSubGroupID).FirstOrDefault();

                itemSubGroupEnt.SetDeleted();
                ItemSubGroupRepo.Add(itemSubGroupEnt);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public Task<ItemSubGroupDto> SaveChanges(ItemSubGroupDto itemSubGroupDto)
        {
            //check duplicates
            var isExistsName = ItemSubGroupRepo.Entities.FirstOrDefault(x => x.ItemSubGroupID != itemSubGroupDto.ItemSubGroupID && x.ItemSubGroupName.ToLower() == itemSubGroupDto.ItemSubGroupName.ToLower()).MapTo<ItemSubGroup>();
            if (isExistsName.IsNotNull())
            {
                itemSubGroupDto.ItemSubGroupNameError = "ItemSubGroup Name already exists by this division.";
                return Task.FromResult(itemSubGroupDto);
            }


            using (var unitOfWork = new UnitOfWork())
            {
                var existUser = ItemSubGroupRepo.Entities.SingleOrDefault(x => x.ItemSubGroupID == itemSubGroupDto.ItemSubGroupID).MapTo<ItemSubGroup>();

                if (existUser.IsNull() || itemSubGroupDto.ItemSubGroupID.IsZero() )
                {
                    itemSubGroupDto.SetAdded();
                    SetItemSubGroupCode(itemSubGroupDto);
                    SetNewUserID(itemSubGroupDto);
                }
                else
                {
                    itemSubGroupDto.SetModified();
                }

                var itemSubGroupEnt = itemSubGroupDto.MapTo<ItemSubGroup>();
                SetAuditFields(itemSubGroupEnt);
                ItemSubGroupRepo.Add(itemSubGroupEnt);
                unitOfWork.CommitChangesWithAudit();
            }
            return Task.FromResult(itemSubGroupDto);
        }

        private void SetNewUserID(ItemSubGroupDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ItemSubGroup", AppContexts.User.CompanyID);
            obj.ItemSubGroupID = code.MaxNumber;
        }
        private void SetItemSubGroupCode(ItemSubGroupDto obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("ItemSubGroupCode", AppContexts.User.CompanyID);
            obj.ItemSubGroupCode = code.MaxNumber.ToString();
        }
        
    }
}
