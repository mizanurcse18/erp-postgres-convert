using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Security.Manager.Interfaces
{
    public interface ICommonManager
    {
        Task<List<SystemVariableDto>> GetSystemVariableByEntityTypeID(int entityTypeID);
        void SaveChanges(SystemVariableDto model);
        Task<SystemVariableDto> GetSystemVariable(int systemVariableID);
        void DeleteSystemVariable(int systemVariableID);
        bool DeleteFileFromPath(string folderName, string fileName);

    }
}
