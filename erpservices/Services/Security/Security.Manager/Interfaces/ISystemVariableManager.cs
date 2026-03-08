using Security.Manager.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface ISystemVariableManager
    {
        Task<List<SystemVariableDto>> GetSystemVariableList();
        void SaveChanges(SystemVariableDto systemVariableDto);
        //void SaveChangesNew(SystemVariableDto systemVariableDto);
        Task<SystemVariableDto> GetSystemVariable(int systemVariableId);
        void DeleteSystemVariable(int systemVariableId);
    }
} 