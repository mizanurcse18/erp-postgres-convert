using DAL.Core.Repository;
using Manager.Core;
using Security.DAL.Entities;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    class MenuManager : ManagerBase, IMenuManager
    {
        private readonly IRepository<Menu> MenuRepo;
        public MenuManager(IRepository<Menu> menuRepository)
        {
            MenuRepo = menuRepository;
        }

        public async Task<List<dynamic>> GetMenus()
        {
            var data = MenuRepo.Entities.Select(x => x).ToList();
            var result = new List<dynamic>();
            string eachItemParentIDName;
            foreach (var eachItem in data)
            {
                var eachItemParentID = eachItem.ParentID;
                if (eachItemParentID == 0)
                {
                    eachItemParentIDName = "Root";
                }
                else
                {
                    eachItemParentIDName = data.FirstOrDefault(item => item.MenuID == eachItemParentID)?.Title ?? "Parent Name Not Found";
                }

                var menuWithParentTitle = new
                {
                    eachItem.MenuID,
                    eachItem.ParentID,
                    eachItem.Title,
                    ParentName = eachItemParentIDName,
                    Url = eachItem.Url ?? " ",
                    eachItem.IsVisible,
                };
                result.Add(menuWithParentTitle);
            }
            return await Task.FromResult(result);
        }
    }
}
