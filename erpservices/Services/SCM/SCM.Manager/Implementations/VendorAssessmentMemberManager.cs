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
    public class VendorAssessmentMemberManager : ManagerBase, IVendorAssessmentMemberManager
    {

        private readonly IRepository<VendorAssessmentMembers> VendorAssessmentMemberRepo;
        public VendorAssessmentMemberManager(IRepository<VendorAssessmentMembers> vendorAssessmentMemberRepo)
        {
            VendorAssessmentMemberRepo = vendorAssessmentMemberRepo;
        }


        public Task<VendorAssessmentMemberDto> SaveChanges(VendorAssessmentMemberDto VendorAssessmentMemberDto)
        {

            using (var unitOfWork = new UnitOfWork())
            {
                var existVendorAssessmentMembers = VendorAssessmentMemberRepo.Entities.Where(x => x.PrimaryID != 0).ToList();


                existVendorAssessmentMembers.ForEach(x => x.SetDeleted());

                foreach(var item in VendorAssessmentMemberDto.AssessmentMembers)
                {
                    var newObj = new VendorAssessmentMembers();
                    newObj.EmployeeID = item.value;

                    newObj.SetAdded();
                    existVendorAssessmentMembers.Add(newObj);
                }

                
                SetAuditFields(existVendorAssessmentMembers);
                VendorAssessmentMemberRepo.AddRange(existVendorAssessmentMembers);
                unitOfWork.CommitChangesWithAudit();
            }
            return Task.FromResult(VendorAssessmentMemberDto);
        }


    }
}
