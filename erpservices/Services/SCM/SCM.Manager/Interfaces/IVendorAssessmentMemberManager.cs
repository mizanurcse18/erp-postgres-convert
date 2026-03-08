using Core;
using SCM.Manager.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SCM.Manager.Interfaces
{
    public interface IVendorAssessmentMemberManager
    {
        Task<VendorAssessmentMemberDto> SaveChanges(VendorAssessmentMemberDto VendorAssessmentMemberDto);

    }
}
