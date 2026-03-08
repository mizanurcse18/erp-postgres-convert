using Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Mail.Manager.Interfaces
{
    public interface IComboManager
    {

        Task<List<ComboModel>> GetMailConfigurations();
    }
}
