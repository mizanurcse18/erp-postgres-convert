using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface ISearchManager
    {
        GridModel GetUsers(GridParameter parameters);
        GridModel GetSecurityRules(GridParameter parameters);
        GridModel GetSecurityGroups(GridParameter parameters);
    }
}
