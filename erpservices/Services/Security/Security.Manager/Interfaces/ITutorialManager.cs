using Core;
using Security.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Security.Manager
{
    public interface ITutorialManager
    {

        List<object> GetTutorial(int tutorialid);
        Task Delete(int tutorialid);
        Task<IEnumerable<Dictionary<string, object>>> GetTutorials();
        Task<List<TutorialDto>> GetTutorialsForList();
        Task<IEnumerable<Dictionary<string, object>>> GetRevenuesForList(); 
        Task<TutorialDto> SaveChanges(TutorialDto tutorial);
    }
}
