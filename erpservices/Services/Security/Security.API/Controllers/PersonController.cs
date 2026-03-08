using API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Security.API.Models;
using Security.Manager.Interfaces;
using System.Threading.Tasks;
using System.Security.Cryptography.Xml;
using System.Collections.Generic;
using Security.DAL.Entities;
using System.IO;
using System;
using Core.Extensions;
using Core.AppContexts;
using Security.Manager.Dto;
using Core;
using Microsoft.AspNetCore.SignalR;
using API.Core.Hubs;
using API.Core.Interface;

namespace Security.API.Controllers
{
    [Authorize, ApiController, Route("[controller]")]
    public class PersonController : BaseController
    {
        private readonly IPersonManager Manager;
        private readonly IHubContext<NotificaitonHub, INotificaitonClient> _notificationHub;

        public PersonController(IPersonManager manager, IHubContext<NotificaitonHub, INotificaitonClient> notificationHub)
        {
            Manager = manager;
            _notificationHub = notificationHub;
        }

        // GET: /Person/GetAll
        [HttpPost("GetAll")]
        public IActionResult GetAll([FromBody] GridParameter parameters)
        {
            var model = Manager.GetPersonListDic(parameters);
            return OkResult(new { parentDataSource = model });
        }
        [HttpPost("GetsMyProfileApprovalList")]
        public IActionResult GetsMyProfileApprovalList([FromBody] GridParameter parameters)
        {
            var model = Manager.GetsMyProfileApprovalList(parameters);
            return OkResult(new { parentDataSource = model });
        }

        [HttpGet("Get/{EPAID:int}/{ApprovalProcessID:int}")]
        public async Task<IActionResult> Get(int EPAID, int ApprovalProcessID)
        {
            var master = await Manager.GetEmployeeUpdateApproval(EPAID);
            var comments = Manager.GetApprovalComment(ApprovalProcessID);
            var rejectedMembers = Manager.GetRejectedMemeberList(ApprovalProcessID).Result;
            var forwardingMembers = Manager.GetForwardingMemberList(ApprovalProcessID).Result;
            var approvalFeedback = Manager.ReportForEPAApprovalFeedback(EPAID);
            var forwardInfoComments = Manager.GetForwardingMemberComments((int)Util.ApprovalType.EmployeeProfileApproval, EPAID);
            return OkResult(new { Master = master, Comments = comments, ApprovalFeedback = approvalFeedback, RejectedMembers = rejectedMembers, ForwardingMembers = forwardingMembers, ForwardInfoComments = forwardInfoComments });
        }

        [HttpGet("GetAllOld")]
        public async Task<IActionResult> GetAllOld()
        {
            var persons = await Manager.GetPersonListDic();
            //if (securityRules.Count.IsZero()) return NoContentResult();
            return OkResult(persons);
        }
        [HttpGet("GetUnemploymentList")]
        public async Task<IActionResult> GetUnemploymentList()
        {
            var persons = await Manager.GetUnemploymentListDic();
            //if (securityRules.Count.IsZero()) return NoContentResult();
            return OkResult(persons);
        }


        // GET: /Person/Get/{primaryID}
        [HttpGet("Get/{PersonID:int}")]
        public async Task<IActionResult> Get(int PersonID)
        {
            //if (!HasViewPrivilege())
            //    return ValidationResult("You have no privilege to view any information.");
            var person = await Manager.GetPersonTableDic(PersonID);
            var PersonImages = await Manager.GetPersonImageList(PersonID);
            var WorkExperience = await Manager.GetPersonWorkExperienceList(PersonID);
            var AcademicQualification = await Manager.GetPersonAcademicInfoList(PersonID);
            var TrainingInfo = await Manager.GetPersonTrainingInfoList(PersonID);
            var AwardInfo = await Manager.GetPersonAwardInfoList(PersonID);
            var ChildrenInfo = await Manager.GetPersonFamilyInfoList(PersonID);
            var ReferenceInfo = await Manager.GetPersonReferenceInfoList(PersonID);
            var EmergencyContact = await Manager.GetPersonEmergencyContactInfo(PersonID);
            var NomineeInfo = await Manager.GeNomineeInfoList(PersonID);
            return OkResult(new { MasterModel = person, PersonImages, WorkExperience, AcademicQualification, TrainingInfo, AwardInfo, ChildrenInfo, ReferenceInfo, EmergencyContact, NomineeInfo });
        }

        // GET: /Person/Get/{primaryID}
        [HttpGet("GetPersonDetailsReport/{PersonID:int}")]
        public async Task<IActionResult> GetPersonDetailsReport(int PersonID)
        {
            var person = await Manager.GetPersonInfoDic(PersonID);
            var personSupervisor = await Manager.GetPersonSupervisorInfoDic(PersonID);
            var PersonImages = await Manager.GetPersonImageList(PersonID);
            var WorkExperience = await Manager.GetPersonWorkExperienceList(PersonID);
            var AcademicQualification = await Manager.GetPersonAcademicInfoList(PersonID);
            var TrainingInfo = await Manager.GetPersonTrainingInfoList(PersonID);
            var AwardInfo = await Manager.GetPersonAwardInfoList(PersonID);
            var ChildrenInfo = await Manager.GetPersonFamilyInfoList(PersonID);
            var ReferenceInfo = await Manager.GetPersonReferenceInfoList(PersonID);
            var EmergencyContact = await Manager.GetPersonEmergencyContactInfo(PersonID);
            var NomineeInfo = await Manager.GeNomineeInfoList(PersonID);
            return OkResult(new { MasterModel = person, PersonSupervisor = personSupervisor, PersonImages, WorkExperience, AcademicQualification, TrainingInfo, AwardInfo, ChildrenInfo, ReferenceInfo, EmergencyContact, NomineeInfo });
        }

        [HttpGet("GetPersonSelfProfile/{PersonID:int}")]
        public async Task<IActionResult> GetPersonSelfProfile(int PersonID)
        {

            PersonID = AppContexts.User.PersonID;
            var person = await Manager.GetPersonTableDic(PersonID);
            var PersonImages = await Manager.GetPersonImageList(PersonID);
            var WorkExperience = await Manager.GetPersonWorkExperienceList(PersonID);
            var AcademicQualification = await Manager.GetPersonAcademicInfoList(PersonID);
            var TrainingInfo = await Manager.GetPersonTrainingInfoList(PersonID);
            var AwardInfo = await Manager.GetPersonAwardInfoList(PersonID);
            var ChildrenInfo = await Manager.GetPersonFamilyInfoList(PersonID);
            var ReferenceInfo = await Manager.GetPersonReferenceInfoList(PersonID);
            var EmergencyContact = await Manager.GetPersonEmergencyContactInfo(PersonID);
            var NomineeInfo = await Manager.GeNomineeInfoList(PersonID);
            return OkResult(new { MasterModel = person, PersonImages, WorkExperience, AcademicQualification, TrainingInfo, AwardInfo, ChildrenInfo, ReferenceInfo, EmergencyContact, NomineeInfo });
        }

        // POST: /Person/CreatePerson
        [HttpPost("SavePerson")]
        public async Task<IActionResult> SavePerson([FromBody] PersonSaveModel personModel)
        {

            var model = await Manager.SaveChanges(personModel);

            if (!string.IsNullOrEmpty(model.PermissionError))
            {
                personModel.PermissionError = model.PermissionError;
                return OkResult(personModel);
            }

            _notificationHub.Clients.All.ReceiveNotification("Person");
            return await Get(model.PersonID);
        }
        [HttpPost("ProfileUpdateWithApproval")]
        public async Task<IActionResult> ProfileUpdateWithApproval([FromBody] PersonSaveModel personModel)
        {

            var model = await Manager.ProfileUpdateWithApproval(personModel);
            if (!string.IsNullOrEmpty(model.PermissionError))
            {
                personModel.PermissionError = model.PermissionError;
                return OkResult(personModel);
            }

            _notificationHub.Clients.All.ReceiveNotification("Person");
            return await Get(model.PersonID);
        }


        // GET: /Person/RemoveSecurityGroup
        [HttpGet("RemovePerson/{PersonID:int}")]
        public async Task<IActionResult> RemovePerson(int PersonID)
        {
            //await Manager.UpdateAsync(user);
            await Manager.RemovePerson(PersonID);
            return OkResult(new { success = true });
        }

        [HttpGet("RemovePersonImage")]
        public async Task<IActionResult> RemovePersonImage(int PersonImageID, int PersonID)
        {
            //await Manager.UpdateAsync(user);
            await Manager.RemovePersonImage(PersonImageID);
            //return OkResult(new { success = true });
            return await Get(PersonID);
        }

        // GET: /Person/RemoveSecurityGroup
        [HttpGet("GetMediaList/{PersonID:int}")]
        public async Task<IActionResult> GetMediaList(int PersonID)
        {
            //await Manager.UpdateAsync(user);
            var medialist = await Manager.GetMediaList(PersonID);
            return OkResult(medialist);
        }

        // GET: /Person/RemoveSecurityGroup
        [HttpGet("GetPersonAboutInfo/{PersonID:int}")]
        public async Task<IActionResult> GetPersonAboutInfo(int PersonID)
        {
            //await Manager.UpdateAsync(user);
            PersonID = PersonID.IsZero() ? AppContexts.User.PersonID : PersonID;
            var aboutInfo = await Manager.GetPersonAboutInfo(PersonID);
            return OkResult(aboutInfo);
        }

        // GET: /Person/GetPersonAboutInfoForEmpDirectory
        [HttpGet("GetPersonAboutInfoForEmpDirectory/{PersonID:int}")]
        public async Task<IActionResult> GetPersonAboutInfoForEmpDirectory(int PersonID)
        {
            var aboutInfo = await Manager.GetPersonAboutInfo(PersonID);
            return OkResult(aboutInfo);
        }

        [HttpGet("FinalSubmissionOfOnboardUser/{PersonID:int}")]
        public async Task<IActionResult> FinalSubmissionOfOnboardUser(int PersonID)
        {
            await Manager.UpdateOnBoardFlag(PersonID);
            return OkResult(new { success = true });
        }


    }
}
