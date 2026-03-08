using Core;
using Manager.Core.CommonDto;
using Approval.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Approval.Manager.Interfaces
{
    public interface IDocumentApprovalTemplateManager
    {
        Task<(bool, string)> SaveChanges(DocumentApprovalTemplateDto MR);

        Task<IEnumerable<Dictionary<string, object>>> GetDocumentApprovalTemplateList();
        Task<DocumentApprovalTemplateDto> GetDocumentApprovalTemplate(int DATID);
        Task<DocumentApprovalTemplateDto> GetTemplateWithReplacedData(int DATID); 
         Task Delete(int id);



    }
}
