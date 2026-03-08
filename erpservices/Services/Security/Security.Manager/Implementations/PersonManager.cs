using Core;
using Core.AppContexts;
using Core.Extensions;
using DAL.Core;
using DAL.Core.Repository;
using Manager.Core;
using Manager.Core.Mapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Myrmec;
using Security.DAL.Entities;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Core.Util;

namespace Security.Manager.Implementations
{
    class PersonManager : ManagerBase, IPersonManager
    {



        readonly IRepository<Person> PersonRepo;
        readonly IRepository<PersonImage> PersonImageRepo;
        readonly IRepository<PersonAddressInfo> PersonAddressInfoRepo;
        readonly IRepository<PersonWorkExperience> PersonWorkExperienceRepo;
        readonly IRepository<PersonAcademicInfo> PersonAcademicInfoRepo;
        readonly IRepository<PersonTrainingInfo> PersonTrainingInfoRepo;
        readonly IRepository<PersonAwardInfo> PersonAwardInfoRepo;
        readonly IRepository<PersonFamilyInfo> PersonFamilyInfoRepo;
        readonly IRepository<PersonReferenceInfo> PersonReferenceInfoRepo;
        readonly IRepository<NomineeInformation> NomineeInformationRepo;
        readonly IRepository<PersonEmergencyContactInfo> PersonEmergencyContactInfoRepo;
        readonly IRepository<OnboardingUser> OnboardingUserRepo;
        readonly IRepository<EmployeeProfileApproval> PersonProfileApprovalRepo;
        public PersonManager(IRepository<Person> personRepo, IRepository<PersonImage> personImageRepo, IRepository<PersonAddressInfo> personAddressInfoRepo, IRepository<PersonWorkExperience> personWorkExperienceRepo, IRepository<PersonAcademicInfo> personAcademicInfoRepo, IRepository<PersonTrainingInfo> personTrainingInfoRepo, IRepository<PersonAwardInfo> personAwardInfoRepo, IRepository<PersonFamilyInfo> personFamilyInfoRepo, IRepository<PersonReferenceInfo> personReferenceInfoRepo, IRepository<PersonEmergencyContactInfo> personEmergencyContactInfoRepo, IRepository<NomineeInformation> nomineeInformationRepo
            , IRepository<OnboardingUser> onboardingUserRepo, IRepository<EmployeeProfileApproval> personProfileApprovalRepo
            )
        {
            PersonRepo = personRepo;
            PersonImageRepo = personImageRepo;
            PersonAddressInfoRepo = personAddressInfoRepo;
            PersonWorkExperienceRepo = personWorkExperienceRepo;
            PersonAcademicInfoRepo = personAcademicInfoRepo;
            PersonTrainingInfoRepo = personTrainingInfoRepo;
            PersonAwardInfoRepo = personAwardInfoRepo;
            PersonFamilyInfoRepo = personFamilyInfoRepo;
            PersonReferenceInfoRepo = personReferenceInfoRepo;
            PersonEmergencyContactInfoRepo = personEmergencyContactInfoRepo;
            NomineeInformationRepo = nomineeInformationRepo;
            OnboardingUserRepo = onboardingUserRepo;
            PersonProfileApprovalRepo = personProfileApprovalRepo;
        }


        public async Task<List<PersonImageDto>> GetPersonImageList(int PersonID)
        {
            var list = await PersonImageRepo.GetAllListAsync(person => person.PersonID.Equals(PersonID));
            var imageList = list.MapTo<List<PersonImageDto>>();
            imageList.ForEach(x => x.AID = x.PIID.ToString());
            return imageList;
            //return list.MapTo<List<PersonImageDto>>();
        }

        public async Task<List<PersonWorkExperienceDto>> GetPersonWorkExperienceList(int PersonID)
        {
            var list = await PersonWorkExperienceRepo.GetAllListAsync(person => person.PersonID.Equals(PersonID));
            return list.MapTo<List<PersonWorkExperienceDto>>();
        }

        public async Task<List<PersonAcademicInfoDto>> GetPersonAcademicInfoList(int PersonID)
        {
            var list = await PersonAcademicInfoRepo.GetAllListAsync(person => person.PersonID.Equals(PersonID));
            return list.MapTo<List<PersonAcademicInfoDto>>();
        }
        public async Task<List<PersonTrainingInfoDto>> GetPersonTrainingInfoList(int PersonID)
        {
            var list = await PersonTrainingInfoRepo.GetAllListAsync(person => person.PersonID.Equals(PersonID));
            return list.MapTo<List<PersonTrainingInfoDto>>();
        }
        public async Task<List<PersonAwardInfoDto>> GetPersonAwardInfoList(int PersonID)
        {
            var list = await PersonAwardInfoRepo.GetAllListAsync(person => person.PersonID.Equals(PersonID));
            return list.MapTo<List<PersonAwardInfoDto>>();
        }
        public async Task<List<PersonFamilyInfoDto>> GetPersonFamilyInfoList(int PersonID)
        {
            var list = await PersonFamilyInfoRepo.GetAllListAsync(person => person.PersonID.Equals(PersonID));
            return list.MapTo<List<PersonFamilyInfoDto>>();
        }
        public async Task<List<PersonReferenceInfoDto>> GetPersonReferenceInfoList(int PersonID)
        {
            var list = await PersonReferenceInfoRepo.GetAllListAsync(person => person.PersonID.Equals(PersonID));
            return list.MapTo<List<PersonReferenceInfoDto>>();
        }
        public async Task<List<PersonDto>> GetPersonSupervisorInfoDic(int PersonID)
        {
            string sql = @$"select sup.*,SV.SystemVariableDescription SupervisorTypeName from HRMS..ViewEmployeeSupervisorMap sup
                            LEFT JOIN SystemVariable SV ON sup.SupervisorType=SV.SystemVariableID
                            where sup.PersonID='{PersonID}'";

            return PersonRepo.GetDataModelCollection<PersonDto>(sql);
        }
        public async Task<PersonEmergencyContactInfoDto> GetPersonEmergencyContactInfo(int personID)
        {
            var contact = await PersonEmergencyContactInfoRepo.FirstOrDefaultAsync(person => person.PersonID.Equals(personID));
            return contact.MapTo<PersonEmergencyContactInfoDto>();
        }

        public GridModel GetsMyProfileApprovalList(GridParameter parameters)
        {
            string filter = "";
            string where = "";
            switch (parameters.ApprovalFilterData)
            {
                case "All":
                    filter = "";
                    break;
                case "My Pending":
                    filter = $@" AND CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) = 1";
                    break;
                case "Pending":
                    filter = $@" AND P.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND P.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND AP.ApprovalProcessID IN (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND AP.ApprovalProcessID IN 
                                (SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM Approval.dbo.fnReturnApprovalProcessID({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT DISTINCT
                             P.*
							 ,VE.EmployeeCode
	                         ,VE.DepartmentID
	                         ,VE.DepartmentName
	                         ,VE.FullName AS EmployeeName, VE.DivisionID, VE.DivisionName
                             ,VE.FullName+VE.EmployeeCode+VE.DepartmentName EmployeeWithDepartment
                                ,ISNULL(AP.ApprovalProcessID, 0) ApprovalProcessID
	                            ,CAST((SELECT Approval.[dbo].[fnValidateCurrentAPEmployee]({AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) AS Bit) IsCurrentAPEmployee
	                            ,ISNULL(AEF.APEmployeeFeedbackID,0) APEmployeeFeedbackID
	                            ,ISNULL(APForwardInfoID,0) APForwardInfoID
								,ISNULL(AEF.IsEditable,0) IsEditable
                                ,CASE WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
                                ,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                                ,ISNULL(FeedbackLastResponseDate,CommentSubmitDate) LastActionDate
	                            ,SV.SystemVariableCode AS ApprovalStatus                            
                                ,CASE WHEN PendingEmployeeCode IS NOT NULL THEN (SELECT PendingEmployeeCode EmployeeCode,PendingEmployeeName EmployeeName,PendingDepartmentName DepartmentName FOR JSON PATH) END PendingAt

                            FROM 
                            EmployeeProfileApproval P
							LEFT JOIN HRMS..ViewALLEmployee VE ON P.PersonID = VE.PersonID
                            LEFT JOIN Security..SystemVariable SV ON SV.SystemVariableID = P.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID =  P.EPAID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval}
                            LEFT JOIN (
                                        SELECT 
                                              APEmployeeFeedbackID,ApprovalProcessID,IsEditable ,IsSCM,IsMultiProxy  
                                        FROM 
                                            Approval.dbo.functionJoinListAEF({AppContexts.User.EmployeeID})
                            )AEF ON AEF.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
		                            (SELECT 
				                            APForwardInfoID,ApprovalProcessID 
			                            FROM 
				                            Approval..ApprovalForwardInfo  
			                            WHERE 
				                            EmployeeID = {AppContexts.User.EmployeeID} AND CommentSubmitDate IS NULL) 
                            APForward ON APForward.ApprovalProcessID = AP.ApprovalProcessID
                            LEFT JOIN 
									(
									SELECT ApprovalProcessID,EmployeeID,ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF({AppContexts.User.EmployeeID}) 
									)							
							F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            LEFT JOIN (
										SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
										(
										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback  AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} 
										GROUP BY ReferenceID

										UNION ALL

										SELECT 
											COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
										FROM 
											Approval..ApprovalEmployeeFeedback AEF
											LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
										where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} AND EmployeeID = {AppContexts.User.EmployeeID}
										GROUP BY ReferenceID

										)V
										GROUP BY ReferenceID
										) EA ON EA.ReferenceID =  P.EPAID

                                        LEFT JOIN(
										SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
										FROM 
											Approval..ApprovalEmployeeFeedbackRemarks AEFR 
											INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
										WHERE APFeedbackID = 11 --Returned
										GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
									) Rej ON Rej.ReferenceID =  P.EPAID
                                    LEFT JOIN (
                                            SELECT * FROM Approval.dbo.functionJoinListProxyEmployeeAPSubmitDate({AppContexts.User.EmployeeID})                                 
									)APSubmitDate ON APSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
									LEFT JOIN (
													SELECT 
													   MAX(CommentSubmitDate)  CommentSubmitDate,ApprovalProcessID 
													FROM 
														Approval..ApprovalForwardInfo  
													WHERE 
														EmployeeID = {AppContexts.User.EmployeeID} 
													GROUP BY ApprovalProcessID
										
														) APFSubmitDate ON APFSubmitDate.ApprovalProcessID = AP.ApprovalProcessID
                                   LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   )PendingAt ON  PendingAt.PendingReferenceID = P.EPAID
							WHERE (VE.EmployeeID = {AppContexts.User.EmployeeID} OR F.EmployeeID = {AppContexts.User.EmployeeID} OR ISNULL(F.ProxyEmployeeID ,0) = {AppContexts.User.EmployeeID}) {filter}
                            ";
            var result = PersonRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public GridModel GetPersonListDic(GridParameter parameters)
        {
            string filter = "";
            switch (parameters.ApprovalFilterData)
            {
                case "unemployment":
                    filter = @$"WHERE P.PersonTypeID = {(int)Util.PersonType.Onboarding}";
                    break;

                default:
                    break;
            }
            string sql = $@"SELECT DISTINCT
                             P.PersonID
                             ,P.FirstName FullName
                             ,P.Mobile
                             ,P.Email
                                ,Bld.SystemVariableCode BloodGroup
        ,ISNULL(P.Mobile + '-' + P.Email+'-'+Bld.SystemVariableCode,'') PersonMobileEmailBG
                             ,Gen.SystemVariableCode Gender
                             ,P.DateOfBirth
                             ,Rel.SystemVariableCode Religion
                             ,P.Nationality
        ,ISNULL(Gen.SystemVariableCode+'-'+Rel.SystemVariableCode+'-'+P.Nationality, '') GenRelNationality
                             ,usr.UserName CreatedBy
                             ,Msts.SystemVariableCode MaritalStatus
                             ,PrsnType.SystemVariableCode PersonType
                             ,P.CreatedDate
                                ,Img.ImagePath
                                ,ISNULL(Emp.EmployeeID, 0) EmployeeID
                                ,CASE 
                          WHEN Emp.PersonID IS NULL
                           THEN CAST(1 AS BIT)
                          ELSE CAST(0 AS BIT)
                          END IsRemovable
                            FROM 
                             Person P
                                LEFT JOIN (
                             SELECT DISTINCT EmployeeID, PersonID
                             FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employee
                             ) Emp ON P.PersonID= Emp.PersonID
                             left join SystemVariable Gen ON Gen.SystemVariableID = P.GenderID
                             left join SystemVariable Rel ON Rel.SystemVariableID = P.ReligionID
                             left join SystemVariable Bld ON Bld.SystemVariableID = P.BloodGroupID
                             left join SystemVariable Msts ON Msts.SystemVariableID = P.MaritalStatusID	
                             left join SystemVariable PrsnType ON PrsnType.SystemVariableID = P.PersonTypeID
                             left join Users Usr ON Usr.UserID = P.CreatedBy
                                left JOIN (SELECT ImagePath,PersonID FROM PersonImage WHERE IsFavorite=1) Img
        ON Img.PersonID = P.PersonID {filter}";
            var result = PersonRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetPersonListDic()
        {
            string sql = $@"SELECT 
	                            P.PersonID
	                            ,P.FirstName FullName
	                            ,P.Mobile
	                            ,P.Email
	                            ,Gen.SystemVariableCode Gender
	                            ,P.DateOfBirth
	                            ,Rel.SystemVariableCode Religion
	                            ,P.Nationality
	                            ,usr.UserName CreatedBy
	                            ,Msts.SystemVariableCode MaritalStatus
	                            ,PrsnType.SystemVariableCode PersonType
	                            ,P.CreatedDate
                                ,Img.ImagePath
                                ,ISNULL(Emp.EmployeeID, 0) EmployeeID
                                ,CASE 
		                        WHEN Emp.PersonID IS NULL
			                        THEN CAST(1 AS BIT)
		                        ELSE CAST(0 AS BIT)
		                        END IsRemovable
                            FROM 
	                            Person P
                                LEFT JOIN (
	                            SELECT DISTINCT EmployeeID, PersonID
	                            FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employee
	                            ) Emp ON P.PersonID= Emp.PersonID
	                            left join SystemVariable Gen ON Gen.SystemVariableID = P.GenderID
	                            left join SystemVariable Rel ON Rel.SystemVariableID = P.ReligionID
	                            left join SystemVariable Bld ON Bld.SystemVariableID = P.BloodGroupID
	                            left join SystemVariable Msts ON Msts.SystemVariableID = P.MaritalStatusID	
	                            left join SystemVariable PrsnType ON PrsnType.SystemVariableID = P.PersonTypeID
	                            left join Users Usr ON Usr.UserID = P.CreatedBy
                                left JOIN (SELECT ImagePath,PersonID FROM PersonImage WHERE IsFavorite=1) Img
								ON Img.PersonID = P.PersonID";
            var listDict = PersonRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetUnemploymentListDic()
        {
            string sql = $@"SELECT 
	                            P.PersonID
	                            ,P.FirstName FullName
	                            ,P.Mobile
	                            ,P.Email
	                            ,Gen.SystemVariableCode Gender
	                            ,P.DateOfBirth
	                            ,Rel.SystemVariableCode Religion
	                            ,P.Nationality
	                            ,usr.UserName CreatedBy
	                            ,Msts.SystemVariableCode MaritalStatus
	                            ,PrsnType.SystemVariableCode PersonType
	                            ,P.CreatedDate
                                ,Img.ImagePath
                            FROM 
	                            Person P
								LEFT JOIN (
	                        SELECT DISTINCT PersonID
	                        FROM {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employee
	                        ) Emp ON P.PersonID= Emp.PersonID
	                            left join SystemVariable Gen ON Gen.SystemVariableID = P.GenderID
	                            left join SystemVariable Rel ON Rel.SystemVariableID = P.ReligionID
	                            left join SystemVariable Bld ON Bld.SystemVariableID = P.BloodGroupID
	                            left join SystemVariable Msts ON Msts.SystemVariableID = P.MaritalStatusID	
	                            left join SystemVariable PrsnType ON PrsnType.SystemVariableID = P.PersonTypeID
	                            left join Users Usr ON Usr.UserID = P.CreatedBy
                                left JOIN (SELECT ImagePath,PersonID FROM PersonImage WHERE IsFavorite=1) Img
								ON Img.PersonID = P.PersonID
								WHERE Emp.PersonID IS NULL";
            var listDict = PersonRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<Dictionary<string, object>> GetPersonInfoDic(int primaryID)
        {
            string sql = $@"select E.*,G.JobGradeName from HRMS..ViewALLEmployee E
                                    LEFT JOIN HRMS..JobGrade G on E.JobGradeID=G.JobGradeID
	                            WHERE E.PersonID = {primaryID}";
            var data = PersonRepo.GetData(sql);

            return await Task.FromResult(data);
        }

        //public async Task<Dictionary<string, object>> GetPersonSupervisorInfoDic(int primaryID)
        //{
        //    string sql = $@"SELECT * from HRMS..ViewEmployeeSupervisorMap
        //                     WHERE PersonID = {primaryID}";
        //    var data = PersonRepo.GetData(sql);

        //    return await Task.FromResult(data);
        //}

        public async Task<Dictionary<string, object>> GetPersonTableDic(int primaryID)
        {
            string sql = $@"SELECT 
	                            P.*,
	                            Gen.SystemVariableCode GenderName,
	                            Rel.SystemVariableCode ReligionName,
	                            Bld.SystemVariableCode BloodGroupName,
	                            Msts.SystemVariableCode MaritalStatusName,
                                sg.SystemVariableCode SpouseGenderName,
	                            PrsnType.SystemVariableCode PersonTypeName,
                                ISNULL(Emp.EmployeeID,0) EmployeeID,

								Present.DistrictName PresentDistrictName,
								ISNULL(Present.DistrictID, 0) PresentDistrictID,
								Present.ThanaName PresentThanaName,
								ISNULL(Present.ThanaID, 0) PresentThanaID,
								Present.PostCode PresentPostCode,
								Present.Address PresentAddress,

								Permanent.DistrictName PermanentDistrictName,
								ISNULL(Permanent.DistrictID, 0) PermanentDistrictID,
								Permanent.ThanaName PermanentThanaName,
								ISNULL(Permanent.ThanaID, 0) PermanentThanaID,
								Permanent.PostCode PermanentPostCode,
								Permanent.Address PermanentAddress,
                                ISNULL(Present.IsSameAsPresentAddress,0) IsSameAsPresentAddress,
								CASE WHEN ISNULL(EPA.PersonID, 0) = 0 THEN 1 ELSE 0 END CanUpdateProfile
                            
                            FROM 
                            Person P
	                            left join SystemVariable Gen ON Gen.SystemVariableID = P.GenderID
	                            left join SystemVariable Rel ON Rel.SystemVariableID = P.ReligionID
	                            left join SystemVariable Bld ON Bld.SystemVariableID = P.BloodGroupID
	                            left join SystemVariable sg ON sg.SystemVariableID = P.SpouseGenderID	
	                            left join SystemVariable Msts ON Msts.SystemVariableID = P.MaritalStatusID	
	                            left join SystemVariable PrsnType ON PrsnType.SystemVariableID = P.PersonTypeID
                                left join {AppContexts.GetDatabaseName(ConnectionName.HRMSContext)}..Employee Emp ON P.PersonID=Emp.PersonID
	                            left join Users Usr ON Usr.UserID = P.CreatedBy

								left join (SELECT DISTINCT PersonID  from EmployeeProfileApproval
									where ApprovalStatusID = 22) EPA ON EPA.PersonID = P.PersonID
								
								left join (SELECT
								PersonAddressInfo.PersonID,
								PersonAddressInfo.DistrictID,
								PersonAddressInfo.ThanaID,
								PersonAddressInfo.PostCode,
								PersonAddressInfo.Address,
								District.DistrictName,
								Thana.ThanaName,
                                PersonAddressInfo.IsSameAsPresentAddress
								FROM PersonAddressInfo
								left join District ON PersonAddressInfo.DistrictID=District.DistrictID
								left join Thana ON PersonAddressInfo.ThanaID=Thana.ThanaID
								WHERE  AddressTypeID={(int)PersonAddressType.Present}) Present on Present.PersonID=P.PersonID
								left join (SELECT 
								PersonAddressInfo.PersonID,
								PersonAddressInfo.DistrictID,
								PersonAddressInfo.ThanaID,
								PersonAddressInfo.PostCode,
								PersonAddressInfo.Address,
								District.DistrictName,
								Thana.ThanaName
								FROM PersonAddressInfo
								left Join District ON PersonAddressInfo.DistrictID=District.DistrictID
								left join Thana ON PersonAddressInfo.ThanaID=Thana.ThanaID
								WHERE  AddressTypeID={(int)PersonAddressType.Permanent}) Permanent on Permanent.PersonID=P.PersonID
	                            WHERE P.PersonID = {primaryID}";
            var data = await PersonRepo.GetDataAsync(sql);

            return data;
        }

        private void SetPersonNewId(Person person)
        {
            if (!person.IsAdded) return;
            var code = GenerateSystemCode("Person", AppContexts.User.CompanyID);
            person.PersonID = code.MaxNumber;
        }

        public async Task RemovePerson(int PersonID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var personEnt = PersonRepo.Entities.Where(x => x.PersonID == PersonID).FirstOrDefault();
                personEnt.SetDeleted();
                var addressList = PersonAddressInfoRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                addressList.ForEach(x => x.SetDeleted());
                var workExperianceList = PersonWorkExperienceRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                workExperianceList.ForEach(x => x.SetDeleted());
                var academicList = PersonAcademicInfoRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                academicList.ForEach(x => x.SetDeleted());
                var trainingInfoList = PersonTrainingInfoRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                trainingInfoList.ForEach(x => x.SetDeleted());
                var awardInfoList = PersonAwardInfoRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                awardInfoList.ForEach(x => x.SetDeleted());
                var childList = PersonFamilyInfoRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                childList.ForEach(x => x.SetDeleted());
                var referenceList = PersonReferenceInfoRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                referenceList.ForEach(x => x.SetDeleted());
                var emergencyContactInfo = PersonEmergencyContactInfoRepo.Entities.Where(x => x.PersonID == PersonID).FirstOrDefault();
                if (emergencyContactInfo.IsNotNull())
                {
                    emergencyContactInfo.SetDeleted();
                }

                var personImages = PersonImageRepo.Entities.Where(x => x.PersonID == PersonID).ToList();
                personImages.ForEach(x => x.SetDeleted());


                var onboardUser = OnboardingUserRepo.Entities.Where(x => x.PersonID == PersonID).FirstOrDefault();
                onboardUser.IsActive = false;
                onboardUser.SetModified();


                PersonRepo.Add(personEnt);
                PersonAddressInfoRepo.AddRange(addressList);
                PersonWorkExperienceRepo.AddRange(workExperianceList);
                PersonAcademicInfoRepo.AddRange(academicList);
                PersonTrainingInfoRepo.AddRange(trainingInfoList);
                PersonAwardInfoRepo.AddRange(awardInfoList);
                PersonFamilyInfoRepo.AddRange(childList);
                PersonReferenceInfoRepo.AddRange(referenceList);
                if (emergencyContactInfo.IsNotNull())
                {
                    PersonEmergencyContactInfoRepo.Add(emergencyContactInfo);
                }
                PersonImageRepo.AddRange(personImages);
                OnboardingUserRepo.Add(onboardUser);

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public async Task UpdateOnBoardFlag(int PersonID)
        {
            using (var unitOfWork = new UnitOfWork())
            {
                var onBoardUser = OnboardingUserRepo.Entities.Where(x => x.PersonID == PersonID).SingleOrDefault();

                if (
                    onBoardUser.IsNotNull())
                {
                    onBoardUser.SetModified();
                    onBoardUser.IsActive = false;
                    onBoardUser.IsSubmit = true;

                }

                //Set Audti Fields Data
                SetAuditFields(onBoardUser);

                OnboardingUserRepo.Add(onBoardUser);
                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }
        public async Task RemovePersonImage(int PersonImageID)
        {
            using (var unitOfWork = new UnitOfWork())
            {

                var personImageEnt = PersonImageRepo.Entities.Where(x => x.PIID == PersonImageID).FirstOrDefault();

                if (personImageEnt.IsNotNull())
                {
                    int personid = personImageEnt.PersonID;
                    string fileName = personImageEnt.ImageOriginalName;

                    var person = PersonRepo.Entities.Where(x => x.PersonID == personid).FirstOrDefault();

                    personImageEnt.SetDeleted();
                    PersonImageRepo.Add(personImageEnt);


                    string imageFolder = "upload\\images";
                    string folderName = "person";
                    IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                    string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + imageFolder + "\\" + folderName + "\\" + personid + " - " + person.FirstName.Replace(" ", ""));
                    File.Delete(str + "\\" + fileName);
                }

                unitOfWork.CommitChangesWithAudit();
            }

            await Task.CompletedTask;
        }

        private void SetPersonImageNewId(PersonImage img)
        {
            if (!img.IsAdded) return;
            var code = GenerateSystemCode("PersonImage", AppContexts.User.CompanyID);
            img.PIID = code.MaxNumber;
        }

        private void SetPersonAddressInfoNewId(PersonAddressInfo adress)
        {
            if (!adress.IsAdded) return;
            var code = GenerateSystemCode("PersonAddressInfo", AppContexts.User.CompanyID);
            adress.PAIID = code.MaxNumber;
        }

        private void SetPersonWorkExperienceNewId(PersonWorkExperience experience)
        {
            if (!experience.IsAdded) return;
            var code = GenerateSystemCode("PersonWorkExperience", AppContexts.User.CompanyID);
            experience.PWEID = code.MaxNumber;
        }

        private void SetPersonAcademicInfoNewId(PersonAcademicInfo academicInfo)
        {
            if (!academicInfo.IsAdded) return;
            var code = GenerateSystemCode("PersonAcademicInfo", AppContexts.User.CompanyID);
            academicInfo.PAIID = code.MaxNumber;
        }
        private void SetPersonTrainingInfoNewId(PersonTrainingInfo trainingInfo)
        {
            if (!trainingInfo.IsAdded) return;
            var code = GenerateSystemCode("PersonTrainingInfo", AppContexts.User.CompanyID);
            trainingInfo.PTIID = code.MaxNumber;
        }
        private void SetPersonAwardInfoNewId(PersonAwardInfo awardInfo)
        {
            if (!awardInfo.IsAdded) return;
            var code = GenerateSystemCode("PersonAwardInfo", AppContexts.User.CompanyID);
            awardInfo.PAIID = code.MaxNumber;
        }

        private void SetPersonFamilyInfoNewId(PersonFamilyInfo familyInfo)
        {
            if (!familyInfo.IsAdded) return;
            var code = GenerateSystemCode("PersonFamilyInfo", AppContexts.User.CompanyID);
            familyInfo.PFIID = code.MaxNumber;
        }
        private void SetPersonReferenceInfoNewId(PersonReferenceInfo referenceInfo)
        {
            if (!referenceInfo.IsAdded) return;
            var code = GenerateSystemCode("PersonReferenceInfo", AppContexts.User.CompanyID);
            referenceInfo.PRIID = code.MaxNumber;
        }
        private void SetNomineeInfoNewId(NomineeInformation nomineeInfo)
        {
            if (!nomineeInfo.IsAdded) return;
            var code = GenerateSystemCode("NomineeInformation", AppContexts.User.CompanyID);
            nomineeInfo.NIID = code.MaxNumber;
        }
        private void SetPersonEmergencyContactInfoNewId(PersonEmergencyContactInfo contactInfo)
        {
            if (!contactInfo.IsAdded) return;
            var code = GenerateSystemCode("PersonEmergencyContactInfo", AppContexts.User.CompanyID);
            contactInfo.PECIID = code.MaxNumber;
        }

        private void SetPersonProfileApprovalNewId(EmployeeProfileApproval obj)
        {
            if (!obj.IsAdded) return;
            var code = GenerateSystemCode("PersonProfileApproval", AppContexts.User.CompanyID);
            obj.EPAID = code.MaxNumber;
        }
        public async Task<PersonDto> SaveChanges(PersonSaveModel personSaveModel)
        {
            Dictionary<string, object> changedPropertiesValues = new Dictionary<string, object>();
            Dictionary<string, object> changedPropertiesOldValues = new Dictionary<string, object>();
            Dictionary<string, string> finalDict = new Dictionary<string, string>();

            Tuple<string, object, object> approvalTuple = new Tuple<string, object, object>("", null, null);

            if (personSaveModel.UpdatedFieldTrackerObj.IsNotNullOrEmpty() && personSaveModel.UpdatedFieldTrackerObj.Length > 0)
            {
                var objectValues = JsonSerializer.Deserialize<Dictionary<string, object>>(personSaveModel.UpdatedFieldTrackerObj);
                var stringValues = objectValues.Select(o => new KeyValuePair<string, string>(o.Key, o.Value?.ToString()));
                finalDict = stringValues.ToDictionary(pair => pair.Key, pair => pair.Value);
            }


            var master = personSaveModel.MasterModel.MapTo<Person>();
            var personProfile = personSaveModel.EmployeeProfileApproval.MapTo<PersonProfileApprovalDto>();

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (master.PersonID > 0 && master.PersonID > 0)
            {
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)master.PersonID, personSaveModel.EmployeeProfileApproval.IsNull() ? 0 : personProfile.ApprovalProcessID, (int)Util.ApprovalType.EmployeeProfileApproval);
            }

            if (personSaveModel.MasterModel.FirstName.StartsWith(" "))
            {
                return master.MapTo<PersonDto>();
            }

            string ApprovalProcessID = "0";

            using (var unitOfWork = new UnitOfWork())
            {
                var existsPerson = PersonRepo.Entities.Where(x => x.PersonID == master.PersonID).SingleOrDefault();
                if (
                    master.PersonID.IsZero() && existsPerson.IsNull())
                {
                    master.SetAdded();
                    SetPersonNewId(master);
                }
                else
                {
                    foreach (var property in finalDict)
                    {

                        var name = property.Key;
                        var val = property.Value;
                        if (val.ToBoolean() == true)
                        {

                            var childInfo = GetChildrenInfo(personSaveModel).MapTo<List<PersonFamilyInfoDto>>();
                            var nomineeInfo = GetNomineeInfo(personSaveModel).MapTo<List<NomineeDto>>();
                            childInfo.ForEach(x =>
                            {
                                x.PFIID = x.IsModified || x.IsDeleted ? x.PFIID : 0;
                                x.isDelete = x.IsDeleted;
                            });
                            nomineeInfo.ForEach(x =>
                            {
                                x.NIID = x.IsModified || x.IsDeleted ? x.NIID : 0;
                                x.isDelete = x.IsDeleted;
                            });

                            string propertyName = "";
                            object propertyValue = null;
                            object propertyOldValue = null;
                            bool isChildGenerated = false;
                            bool isNomineeGenerated = false;

                            List<PersonFamilyInfo> childInfoOld = new List<PersonFamilyInfo>();
                            List<NomineeInformation> nomineeInfoOld = new List<NomineeInformation>();
                            if (name == "ChildrenInfo" || name == "NomineeInfo")
                            {
                                propertyName = personSaveModel.GetType().GetProperty(name).Name;
                                //propertyValue = personSaveModel.GetType().GetProperty(propertyName).GetValue(personSaveModel, null);

                                if (name == "ChildrenInfo")
                                {
                                    isChildGenerated = true;
                                    var existsChildrenInfo = PersonFamilyInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();
                                    childInfoOld = existsChildrenInfo;
                                }
                                if (name == "NomineeInfo")
                                {
                                    isNomineeGenerated = true;
                                    var existsNomineenfoList = NomineeInformationRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();
                                    nomineeInfoOld = existsNomineenfoList;
                                }
                            }
                            else
                            {
                                propertyName = master.GetType().GetProperty(name).Name;
                                propertyValue = getValueByPropertyName(master, propertyName); //propertyName == "SpouseGenderID" ? master.GetType().GetProperty(propertyName).GetValue(master, null) : master.GetType().GetProperty(propertyName).GetValue(master, null).ToString();
                                propertyOldValue = getValueByPropertyName(existsPerson, propertyName);// existsPerson.GetType().GetProperty(propertyName).GetValue(existsPerson, null) != null ? existsPerson.GetType().GetProperty(propertyName).GetValue(existsPerson, null).ToString() : null;

                                //keep existing value for this field
                                if (propertyName == "NIDNumber")
                                    master.NIDNumber = existsPerson.NIDNumber;
                                if (propertyName == "PassportNumber")
                                    master.PassportNumber = existsPerson.PassportNumber;
                                if (propertyName == "FatherName")
                                    master.FatherName = existsPerson.FatherName;
                                if (propertyName == "MotherName")
                                    master.MotherName = existsPerson.MotherName;
                                if (propertyName == "SpouseName")
                                    master.SpouseName = existsPerson.SpouseName;
                                if (propertyName == "PassportIssueDate")
                                    master.PassportIssueDate = existsPerson.PassportIssueDate;
                                if (propertyName == "FatherDOB")
                                    master.FatherDOB = existsPerson.FatherDOB;
                                if (propertyName == "MotherDOB")
                                    master.MotherDOB = existsPerson.MotherDOB;
                                if (propertyName == "SpouseDOB")
                                    master.SpouseDOB = existsPerson.SpouseDOB;
                                if (propertyName == "MarriageDate")
                                    master.MarriageDate = existsPerson.MarriageDate;
                                if (propertyName == "IsFatherAlive")
                                    master.IsFatherAlive = existsPerson.IsFatherAlive;
                                if (propertyName == "IsMotherAlive")
                                    master.IsMotherAlive = existsPerson.IsMotherAlive;
                                if (propertyName == "SpouseGenderID")
                                    master.SpouseGenderID = existsPerson.SpouseGenderID;
                            }
                            if (name == "ChildrenInfo" || name == "NomineeInfo")
                            {
                                if (childInfo.IsNotNull() && childInfo.Count > 0 && isChildGenerated)
                                {
                                    changedPropertiesValues.Add(propertyName, childInfo);
                                    changedPropertiesOldValues.Add(propertyName, childInfoOld);
                                }
                                if (nomineeInfo.IsNotNull() && nomineeInfo.Count > 0 && isNomineeGenerated)
                                {
                                    changedPropertiesValues.Add(propertyName, nomineeInfo);
                                    changedPropertiesOldValues.Add(propertyName, nomineeInfoOld);
                                }
                            }
                            else
                            {
                                changedPropertiesValues.Add(propertyName, propertyValue);
                                changedPropertiesOldValues.Add(propertyName, propertyOldValue);
                            }
                        }
                    }

                    master.CreatedBy = existsPerson.CreatedBy;
                    master.CreatedDate = existsPerson.CreatedDate;
                    master.CreatedIP = existsPerson.CreatedIP;
                    master.RowVersion = existsPerson.RowVersion;
                    master.SetModified();
                }

                EmployeeProfileApproval pfa = new EmployeeProfileApproval();
                pfa = new EmployeeProfileApproval
                {
                    PersonID = master.PersonID,
                    OldValue = JsonSerializer.Serialize(changedPropertiesOldValues),
                    NewValue = JsonSerializer.Serialize(changedPropertiesValues),
                    ApprovalStatusID = (int)Util.ApprovalStatus.Pending
                };

                pfa.SetAdded();
                SetPersonProfileApprovalNewId(pfa);


                PersonDto personDto = new PersonDto();
                personSaveModel.MasterModel.PersonID = master.PersonID;
                var imageList = GetPersonImages(personSaveModel);
                if (imageList.Count > 0 && imageList[0].ImageTypeValidation.IsNotNullOrEmpty())
                {
                    personDto.PermissionError = imageList[0].ImageTypeValidation;
                    return await Task.FromResult(personDto);
                }

                var adressList = GetPersonAddressInfo(personSaveModel.MasterModel);
                var experienceList = GetPersonWorkExperience(personSaveModel);
                var academicList = GetAcademicQualification(personSaveModel);
                var trainingList = GetTrainingInfo(personSaveModel);
                var awardList = GetAwardInfo(personSaveModel);
                var referenceList = GetReferenceInfo(personSaveModel);
                var emergencyContact = GetEmergencyContact(personSaveModel);
                var childrenList = GetChildrenInfo(personSaveModel);
                var nomineeList = GetNomineeInfo(personSaveModel);

                //Set Audti Fields Data
                SetAuditFields(master);
                SetAuditFields(imageList);
                SetAuditFields(adressList);
                SetAuditFields(experienceList);
                SetAuditFields(academicList);
                SetAuditFields(trainingList);
                SetAuditFields(awardList);
                SetAuditFields(childrenList);
                SetAuditFields(referenceList);
                SetAuditFields(emergencyContact);
                SetAuditFields(nomineeList);

                SetAuditFields(pfa);

                PersonRepo.Add(master);
                PersonImageRepo.AddRange(imageList.MapTo<List<PersonImage>>());
                PersonAddressInfoRepo.AddRange(adressList);
                PersonWorkExperienceRepo.AddRange(experienceList);
                PersonAcademicInfoRepo.AddRange(academicList);
                PersonTrainingInfoRepo.AddRange(trainingList);
                PersonAwardInfoRepo.AddRange(awardList);
                PersonReferenceInfoRepo.AddRange(referenceList);
                PersonEmergencyContactInfoRepo.Add(emergencyContact);

                if (!changedPropertiesValues.ContainsKey("ChildrenInfo"))
                {
                    PersonFamilyInfoRepo.AddRange(childrenList);
                }
                if (!changedPropertiesValues.ContainsKey("NomineeInfo"))
                {
                    NomineeInformationRepo.AddRange(nomineeList);
                }

                if (pfa.NewValue != "{}")
                {
                    PersonProfileApprovalRepo.Add(pfa);
                }

                //unitOfWork.CommitChangesWithAudit();

                if (finalDict.Where(x => x.Value == "True").Any())

                {
                    if (master.IsModified && approvalProcessFeedBack.Count == 0)
                    {
                        string approvalTitle = $"{Util.EmployeeProfileApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Person Name:{master.FirstName + " " + master.LastName}";
                        ApprovalProcessID = CreateApprovalProcessForEmployeeProfileUpdate(pfa.EPAID, Util.AutoPersonApprovalDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.ApprovalPanel.EmployeeProfileApproval);
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                                (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                                $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                                (int)approvalProcessFeedBack["APTypeID"],
                                (int)approvalProcessFeedBack["ReferenceID"], 0);

                            ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                        }
                    }
                }

                unitOfWork.CommitChangesWithAudit();

                if (ApprovalProcessID.ToInt() > 0)
                    await SendMailFromManagerBase(ApprovalProcessID, false, pfa.EPAID, (int)Util.MailGroupSetup.EmployeeProfileInitiatedMail, (int)Util.ApprovalType.EmployeeProfileApproval);

            }

            string sql = $@"UPDATE HRMS..Employee SET FullName='{master.FirstName}' WHERE PersonID={master.PersonID}";
            PersonRepo.ExecuteSqlCommand(sql);
            await Task.CompletedTask;

            return master.MapTo<PersonDto>();
        }

        public async Task<PersonDto> ProfileUpdateWithApproval(PersonSaveModel personSaveModel)
        {
            personSaveModel.MasterModel.PersonID = AppContexts.User.PersonID;

            Dictionary<string, object> changedPropertiesValues = new Dictionary<string, object>();
            Dictionary<string, object> changedPropertiesOldValues = new Dictionary<string, object>();
            Dictionary<string, string> finalDict = new Dictionary<string, string>();

            Tuple<string, object, object> approvalTuple = new Tuple<string, object, object>("", null, null);

            if (personSaveModel.UpdatedFieldTrackerObj.Length > 0)
            {
                var objectValues = JsonSerializer.Deserialize<Dictionary<string, object>>(personSaveModel.UpdatedFieldTrackerObj);
                var stringValues = objectValues.Select(o => new KeyValuePair<string, string>(o.Key, o.Value?.ToString()));
                finalDict = stringValues.ToDictionary(pair => pair.Key, pair => pair.Value);
            }


            var master = personSaveModel.MasterModel.MapTo<Person>();
            var personProfile = personSaveModel.EmployeeProfileApproval.MapTo<PersonProfileApprovalDto>();

            var approvalProcessFeedBack = new Dictionary<string, object>();
            if (master.PersonID > 0 && master.PersonID > 0)
            {
                approvalProcessFeedBack = GetApprovalProcessFeedback((int)master.PersonID, personSaveModel.EmployeeProfileApproval.IsNull() ? 0 : personProfile.ApprovalProcessID, (int)Util.ApprovalType.EmployeeProfileApproval);
            }

            string ApprovalProcessID = "0";

            using (var unitOfWork = new UnitOfWork())
            {
                var existsPerson = PersonRepo.Entities.Where(x => x.PersonID == master.PersonID).SingleOrDefault();
                if (
                    master.PersonID.IsZero() && existsPerson.IsNull())
                {
                    master.SetAdded();
                    SetPersonNewId(master);
                }
                else
                {
                    foreach (var property in finalDict)
                    {

                        var name = property.Key;
                        var val = property.Value;
                        if (val.ToBoolean() == true)
                        {

                            var childInfo = GetChildrenInfo(personSaveModel).MapTo<List<PersonFamilyInfoDto>>();
                            var nomineeInfo = GetNomineeInfo(personSaveModel).MapTo<List<NomineeDto>>();
                            childInfo.ForEach(x =>
                            {
                                x.PFIID = x.IsModified || x.IsDeleted ? x.PFIID : 0;
                                x.isDelete = x.IsDeleted;
                            });
                            nomineeInfo.ForEach(x =>
                            {
                                x.NIID = x.IsModified || x.IsDeleted ? x.NIID : 0;
                                x.isDelete = x.IsDeleted;
                            });

                            string propertyName = "";
                            object propertyValue = null;
                            object propertyOldValue = null;
                            bool isChildGenerated = false;
                            bool isNomineeGenerated = false;

                            List<PersonFamilyInfo> childInfoOld = new List<PersonFamilyInfo>();
                            List<NomineeInformation> nomineeInfoOld = new List<NomineeInformation>();
                            if (name == "ChildrenInfo" || name == "NomineeInfo")
                            {
                                propertyName = personSaveModel.GetType().GetProperty(name).Name;
                                //propertyValue = personSaveModel.GetType().GetProperty(propertyName).GetValue(personSaveModel, null);

                                if (name == "ChildrenInfo")
                                {
                                    isChildGenerated = true;
                                    var existsChildrenInfo = PersonFamilyInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();
                                    childInfoOld = existsChildrenInfo;
                                }
                                if (name == "NomineeInfo")
                                {
                                    isNomineeGenerated = true;
                                    var existsNomineenfoList = NomineeInformationRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();
                                    nomineeInfoOld = existsNomineenfoList;
                                }
                            }
                            else
                            {
                                propertyName = master.GetType().GetProperty(name).Name;
                                propertyValue = getValueByPropertyName(master, propertyName); //propertyName == "SpouseGenderID" ? master.GetType().GetProperty(propertyName).GetValue(master, null) : master.GetType().GetProperty(propertyName).GetValue(master, null).ToString();
                                propertyOldValue = getValueByPropertyName(existsPerson, propertyName);// existsPerson.GetType().GetProperty(propertyName).GetValue(existsPerson, null) != null ? existsPerson.GetType().GetProperty(propertyName).GetValue(existsPerson, null).ToString() : null;

                                //keep existing value for this field
                                if (propertyName == "NIDNumber")
                                    master.NIDNumber = existsPerson.NIDNumber;
                                if (propertyName == "PassportNumber")
                                    master.PassportNumber = existsPerson.PassportNumber;
                                if (propertyName == "FatherName")
                                    master.FatherName = existsPerson.FatherName;
                                if (propertyName == "MotherName")
                                    master.MotherName = existsPerson.MotherName;
                                if (propertyName == "SpouseName")
                                    master.SpouseName = existsPerson.SpouseName;
                                if (propertyName == "PassportIssueDate")
                                    master.PassportIssueDate = existsPerson.PassportIssueDate;
                                if (propertyName == "FatherDOB")
                                    master.FatherDOB = existsPerson.FatherDOB;
                                if (propertyName == "MotherDOB")
                                    master.MotherDOB = existsPerson.MotherDOB;
                                if (propertyName == "SpouseDOB")
                                    master.SpouseDOB = existsPerson.SpouseDOB;
                                if (propertyName == "MarriageDate")
                                    master.MarriageDate = existsPerson.MarriageDate;
                                if (propertyName == "IsFatherAlive")
                                    master.IsFatherAlive = existsPerson.IsFatherAlive;
                                if (propertyName == "IsMotherAlive")
                                    master.IsMotherAlive = existsPerson.IsMotherAlive;
                                if (propertyName == "SpouseGenderID")
                                    master.SpouseGenderID = existsPerson.SpouseGenderID;
                            }
                            if (name == "ChildrenInfo" || name == "NomineeInfo")
                            {
                                if (childInfo.IsNotNull() && childInfo.Count > 0 && isChildGenerated)
                                {
                                    changedPropertiesValues.Add(propertyName, childInfo);
                                    changedPropertiesOldValues.Add(propertyName, childInfoOld);
                                }
                                if (nomineeInfo.IsNotNull() && nomineeInfo.Count > 0 && isNomineeGenerated)
                                {
                                    changedPropertiesValues.Add(propertyName, nomineeInfo);
                                    changedPropertiesOldValues.Add(propertyName, nomineeInfoOld);
                                }
                            }
                            else
                            {
                                changedPropertiesValues.Add(propertyName, propertyValue);
                                changedPropertiesOldValues.Add(propertyName, propertyOldValue);
                            }
                        }
                    }

                    master.CreatedBy = existsPerson.CreatedBy;
                    master.CreatedDate = existsPerson.CreatedDate;
                    master.CreatedIP = existsPerson.CreatedIP;
                    master.RowVersion = existsPerson.RowVersion;
                    master.SetModified();
                }

                EmployeeProfileApproval pfa = new EmployeeProfileApproval();
                pfa = new EmployeeProfileApproval
                {
                    PersonID = master.PersonID,
                    OldValue = JsonSerializer.Serialize(changedPropertiesOldValues),
                    NewValue = JsonSerializer.Serialize(changedPropertiesValues),
                    ApprovalStatusID = (int)Util.ApprovalStatus.Pending
                };

                pfa.SetAdded();
                SetPersonProfileApprovalNewId(pfa);

                PersonDto personDto = new PersonDto();
                personSaveModel.MasterModel.PersonID = master.PersonID;
                var imageList = GetPersonImages(personSaveModel);
                if (imageList.Count > 0 && imageList[0].ImageTypeValidation.IsNotNullOrEmpty())
                {
                    personDto.PermissionError = imageList[0].ImageTypeValidation;
                    return await Task.FromResult(personDto);
                }
                var adressList = GetPersonAddressInfo(personSaveModel.MasterModel);
                var experienceList = GetPersonWorkExperience(personSaveModel);
                var academicList = GetAcademicQualification(personSaveModel);
                var trainingList = GetTrainingInfo(personSaveModel);
                var awardList = GetAwardInfo(personSaveModel);
                var referenceList = GetReferenceInfo(personSaveModel);
                var emergencyContact = GetEmergencyContact(personSaveModel);
                var childrenList = GetChildrenInfo(personSaveModel);
                var nomineeList = GetNomineeInfo(personSaveModel);

                //Set Audti Fields Data
                SetAuditFields(master);
                SetAuditFields(imageList);
                SetAuditFields(adressList);
                SetAuditFields(experienceList);
                SetAuditFields(academicList);
                SetAuditFields(trainingList);
                SetAuditFields(awardList);
                SetAuditFields(childrenList);
                SetAuditFields(referenceList);
                SetAuditFields(emergencyContact);
                SetAuditFields(nomineeList);

                SetAuditFields(pfa);

                PersonRepo.Add(master);
                PersonImageRepo.AddRange(imageList.MapTo<List<PersonImage>>());
                PersonAddressInfoRepo.AddRange(adressList);
                PersonWorkExperienceRepo.AddRange(experienceList);
                PersonAcademicInfoRepo.AddRange(academicList);
                PersonTrainingInfoRepo.AddRange(trainingList);
                PersonAwardInfoRepo.AddRange(awardList);
                PersonReferenceInfoRepo.AddRange(referenceList);
                PersonEmergencyContactInfoRepo.Add(emergencyContact);

                if (!changedPropertiesValues.ContainsKey("ChildrenInfo"))
                {
                    PersonFamilyInfoRepo.AddRange(childrenList);
                }
                if (!changedPropertiesValues.ContainsKey("NomineeInfo"))
                {
                    NomineeInformationRepo.AddRange(nomineeList);
                }

                if (pfa.NewValue != "{}")
                {
                    PersonProfileApprovalRepo.Add(pfa);
                }

                //unitOfWork.CommitChangesWithAudit();

                if (finalDict.Where(x => x.Value == "True").Any())

                {
                    if (master.IsModified && approvalProcessFeedBack.Count == 0)
                    {
                        string approvalTitle = $"{Util.EmployeeProfileApprovalTitle} {AppContexts.User.FullName}-{AppContexts.User.EmployeeCode}||{AppContexts.User.DivisionName}||{AppContexts.User.DepartmentName}, Person Name:{master.FirstName + " " + master.LastName}";
                        ApprovalProcessID = CreateApprovalProcessForEmployeeProfileUpdate(pfa.EPAID, Util.AutoPersonApprovalDesc, approvalTitle, AppContexts.User.EmployeeID.Value, (int)Util.ApprovalType.EmployeeProfileApproval, (int)Util.ApprovalPanel.EmployeeProfileApproval);
                    }
                    else
                    {
                        if (approvalProcessFeedBack.Count > 0)
                        {
                            UpdateApprovalProcessFeedback((int)approvalProcessFeedBack["ApprovalProcessID"],
                                (int)approvalProcessFeedBack["APEmployeeFeedbackID"], (int)Util.ApprovalFeedback.Approved,
                                $@"Reviewed And Resubmited by {AppContexts.User.FullName} - {AppContexts.User.EmployeeCode}",
                                (int)approvalProcessFeedBack["APTypeID"],
                                (int)approvalProcessFeedBack["ReferenceID"], 0);

                            ApprovalProcessID = approvalProcessFeedBack["ApprovalProcessID"].ToString();
                        }
                    }
                }

                unitOfWork.CommitChangesWithAudit();

                if (ApprovalProcessID.ToInt() > 0)
                    await SendMailFromManagerBase(ApprovalProcessID, false, pfa.EPAID, (int)Util.MailGroupSetup.EmployeeProfileInitiatedMail, (int)Util.ApprovalType.EmployeeProfileApproval);

            }

            string sql = $@"UPDATE HRMS..Employee SET FullName='{master.FirstName}' WHERE PersonID={master.PersonID}";
            PersonRepo.ExecuteSqlCommand(sql);
            await Task.CompletedTask;

            return master.MapTo<PersonDto>();
        }

        public object getValueByPropertyName(Person master, string propertyName)
        {
            object val = "";
            if (propertyName == "SpouseGenderID")
            {
                val = master.GetType().GetProperty(propertyName).GetValue(master, null);
            }
            else if (propertyName == "PassportIssueDate" || propertyName == "SpouseDOB" || propertyName == "FatherDOB" || propertyName == "MotherDOB" || propertyName == "MarriageDate")
            {
                val = master.GetType().GetProperty(propertyName).GetValue(master, null) != null ? Convert.ToDateTime(master.GetType().GetProperty(propertyName).GetValue(master, null).ToString()).ToString("yyyy-MM-dd") : null;
            }
            else
            {
                val = master.GetType().GetProperty(propertyName).GetValue(master, null) != null ? master.GetType().GetProperty(propertyName).GetValue(master, null).ToString() : "";
            }
            return val;
        }
        public async Task<string> GetMediaList(int personID)
        {
            string sql = $@"SELECT Parent.id,
	                            Parent.name,
	                            Parent.info,
	                            media.type,
	                            media.title,
	                            media.preview
                            FROM 
                            (SELECT 
	                            Row_Number()OVER(order by name) id,
	                            name,
	                            Cast(sum(info) as varchar) + ' Photos'info
                            FROM
                            (SELECT
	                            distinct
	                            FORMAT(CreatedDate, 'MMMM yyyy') name,
	                            COUNT(PIID) info
                            FROM 
	                            PersonImage
                            WHERE PersonID = {personID}
                            GROUP BY CreatedDate
                            )Media
                            GROUP BY name
							)Parent
							LEFT JOIN (
							SELECT
								FORMAT(CreatedDate, 'MMMM yyyy') name,
								'photo' type,	
								ImageName title,
								ImagePath preview
							FROM 
								PersonImage
							WHERE PersonID = {personID} ) media ON media.name = Parent.name
							FOR JSON AUTO";
            var mediaList = PersonRepo.GetJsonData(sql);
            return await Task.FromResult(mediaList);
        }

        public async Task<Dictionary<string, object>> GetPersonAboutInfo(int personID)
        {
            string sql = @$"SELECT * FROM PersonAboutInfoView WHERE PersonID = {personID}";
            var employee = PersonRepo.GetData(sql);
            return await Task.FromResult(employee);
        }
        public async Task<Dictionary<string, object>> GetEmployeeUpdateApproval(int EPAID)
        {
            string sql = @$"SELECT P.*
                            , VA.DepartmentID, VA.DepartmentName, VA.FullName AS EmployeeName, SV.SystemVariableCode AS ApprovalStatus, VA.ImagePath, VA.EmployeeCode, VA.DivisionID, VA.DivisionName ,VA.WorkMobile
                            ,CASE  WHEN (SELECT Approval.dbo.[fnIsAPCreator]( {AppContexts.User.EmployeeID},ISNULL(AP.ApprovalProcessID, 0))) = 1 AND EditableCount > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReassessment
							,CASE WHEN ISNULL(Cntr ,0) > 0 THEN CAST(1 as bit) ELSE CAST(0 as bit) END IsReturned
                            FROM EmployeeProfileApproval P 
                            LEFT JOIN HRMS..ViewAllEmployee VA ON VA.PersonID=P.PersonID
                            LEFT JOIN {AppContexts.GetDatabaseName(ConnectionName.SecurityContext)}..SystemVariable SV ON SV.SystemVariableID = P.ApprovalStatusID
                            LEFT JOIN Approval..ApprovalProcess AP ON AP.ReferenceID = P.EPAID AND AP.APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} 
                            LEFT JOIN (
							    SELECT COUNT(cntr) EditableCount, ReferenceID FROM 
							    (
							    SELECT 
								    COUNT(APEmployeeFeedbackID) Cntr ,ReferenceID
							    FROM 
								    Approval..ApprovalEmployeeFeedback  AEF
								    LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
							    where SequenceNo = 2 AND APFeedbackID = 2 AND APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval}
							    GROUP BY ReferenceID 

							    UNION ALL

							    SELECT 
								    COUNT(APEmployeeFeedbackID) Cntr, ReferenceID
							    FROM 
								    Approval..ApprovalEmployeeFeedback AEF
								    LEFT JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEF.ApprovalProcessID
							    where SequenceNo = 1 AND APFeedbackID = 2  AND APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} AND EmployeeID = {AppContexts.User.EmployeeID}
							    GROUP BY ReferenceID

							    )V
							    GROUP BY ReferenceID
							    ) EA ON EA.ReferenceID = P.EPAID

                                LEFT JOIN(
							    SELECT AP.ApprovalProcessID,COUNT(ISNULL(APFeedbackID,0)) Cntr,AP.ReferenceID 
							    FROM 
								    Approval..ApprovalEmployeeFeedbackRemarks AEFR 
								    INNER JOIN Approval..ApprovalProcess AP ON AP.ApprovalProcessID = AEFR.ApprovalProcessID
							    WHERE APFeedbackID = 11 --Returned
							    GROUP BY AP.ApprovalProcessID,AP.ReferenceID 
						    ) Rej ON Rej.ReferenceID = P.EPAID
LEFT JOIN
                                (
                                    SELECT ApprovalProcessID, EmployeeID, ProxyEmployeeID FROM Approval.dbo.functionJoinListProxyEmployeeF( {AppContexts.User.EmployeeID}) 
								)							
						        F ON F.ApprovalProcessID = Ap.ApprovalProcessID
                            WHERE P.EPAID = {EPAID} AND(VA.EmployeeID =  {AppContexts.User.EmployeeID}
                OR F.EmployeeID =  {AppContexts.User.EmployeeID}
                OR ISNULL(F.ProxyEmployeeID ,0) =  {AppContexts.User.EmployeeID})";
            var employee = await PersonRepo.GetDataAsync(sql);
            return await Task.FromResult(employee);
        }

        private List<PersonAddressInfo> GetPersonAddressInfo(PersonDto masterModelDto)
        {
            var adressList = new List<PersonAddressInfo>();

            var existsPersonAddress = PersonAddressInfoRepo.Entities.Where(x => x.PersonID == masterModelDto.PersonID).ToList();

            if ((masterModelDto.PresentDistrictID.HasValue && masterModelDto.PresentDistrictID.Value > 0) || (masterModelDto.PresentThanaID.HasValue && masterModelDto.PresentThanaID.Value > 0) || (masterModelDto.PresentPostCode.HasValue && masterModelDto.PresentPostCode.Value > 0) || !string.IsNullOrWhiteSpace(masterModelDto.PresentAddress))
            {
                var presentAdress = new PersonAddressInfo
                {
                    AddressTypeID = (int)PersonAddressType.Present,
                    DistrictID = masterModelDto.PresentDistrictID ?? 0,
                    ThanaID = masterModelDto.PresentThanaID ?? 0,
                    PostCode = masterModelDto.PresentPostCode,
                    Address = masterModelDto.PresentAddress,
                    PersonID = masterModelDto.PersonID,
                    IsSameAsPresentAddress = masterModelDto.IsSameAsPresentAddress
                };
                adressList.Add(presentAdress);
            }

            var permanentAdress = new PersonAddressInfo();
            if (masterModelDto.IsSameAsPresentAddress)
            {
                if ((masterModelDto.PresentDistrictID.HasValue && masterModelDto.PresentDistrictID.Value > 0) || (masterModelDto.PresentThanaID.HasValue && masterModelDto.PresentThanaID.Value > 0) || (masterModelDto.PresentPostCode.HasValue && masterModelDto.PresentPostCode.Value > 0) || !string.IsNullOrWhiteSpace(masterModelDto.PresentAddress))
                {
                    permanentAdress.AddressTypeID = (int)PersonAddressType.Permanent;
                    permanentAdress.DistrictID = masterModelDto.PresentDistrictID ?? 0;
                    permanentAdress.ThanaID = masterModelDto.PresentThanaID ?? 0;
                    permanentAdress.PostCode = masterModelDto.PresentPostCode;
                    permanentAdress.Address = masterModelDto.PresentAddress;
                    permanentAdress.PersonID = masterModelDto.PersonID;
                    permanentAdress.IsSameAsPresentAddress = masterModelDto.IsSameAsPresentAddress;
                    adressList.Add(permanentAdress);
                }
            }
            else
            {
                if ((masterModelDto.PermanentDistrictID.HasValue && masterModelDto.PermanentDistrictID.Value > 0) || (masterModelDto.PermanentThanaID.HasValue && masterModelDto.PermanentThanaID.Value > 0) || (masterModelDto.PermanentPostCode.HasValue && masterModelDto.PermanentPostCode.Value > 0) || !string.IsNullOrWhiteSpace(masterModelDto.PermanentAddress))
                {
                    permanentAdress.AddressTypeID = (int)PersonAddressType.Permanent;
                    permanentAdress.DistrictID = masterModelDto.PermanentDistrictID ?? 0;
                    permanentAdress.ThanaID = masterModelDto.PermanentThanaID ?? 0;
                    permanentAdress.PostCode = masterModelDto.PermanentPostCode;
                    permanentAdress.Address = masterModelDto.PermanentAddress;
                    permanentAdress.PersonID = masterModelDto.PersonID;
                    permanentAdress.IsSameAsPresentAddress = masterModelDto.IsSameAsPresentAddress;
                    adressList.Add(permanentAdress);
                }
            }


            adressList.ForEach(x =>
            {
                var address = existsPersonAddress.SingleOrDefault(y => y.AddressTypeID == x.AddressTypeID);
                if (address.IsNotNull())
                {
                    x.PAIID = address.PAIID;
                    x.CreatedBy = address.CreatedBy;
                    x.CreatedDate = address.CreatedDate;
                    x.CreatedIP = address.CreatedIP;
                    x.RowVersion = address.RowVersion;
                    x.SetModified();
                }
                else
                {
                    x.SetAdded();
                    SetPersonAddressInfoNewId(x);
                }
            });

            return adressList;
        }
        private List<PersonWorkExperience> GetPersonWorkExperience(PersonSaveModel personSaveModel)
        {
            var experienceList = new List<PersonWorkExperience>();
            if (personSaveModel.WorkExperience.IsNotNull())
            {
                experienceList = personSaveModel.WorkExperience.MapTo<List<PersonWorkExperience>>();
                var existsPersonWorkExperience = PersonWorkExperienceRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();

                experienceList.ForEach(x =>
                {
                    x.Responsibilities = "";
                    if (existsPersonWorkExperience.Count > 0 && x.PWEID > 0)
                    {
                        x.SetModified();
                    }
                    else
                    {
                        x.PersonID = personSaveModel.MasterModel.PersonID;
                        x.StartDate = x.StartDate.Date;
                        x.EndDate = x.EndDate.Date;
                        x.SetAdded();
                        SetPersonWorkExperienceNewId(x);
                    }
                });

                var willDeleted = existsPersonWorkExperience.Where(x => !experienceList.Select(y => y.PWEID).Contains(x.PWEID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    experienceList.Add(x);
                });

            }

            return experienceList;
        }
        private List<PersonAcademicInfo> GetAcademicQualification(PersonSaveModel personSaveModel)
        {
            var academicInfoList = new List<PersonAcademicInfo>();
            if (personSaveModel.AcademicQualification.IsNotNull())
            {
                academicInfoList = personSaveModel.AcademicQualification.MapTo<List<PersonAcademicInfo>>();
                var existsPersonAcademicInfo = PersonAcademicInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();

                academicInfoList.ForEach(x =>
                {
                    if (existsPersonAcademicInfo.Count > 0 && x.PAIID > 0)
                    {
                        x.SetModified();
                    }
                    else
                    {
                        x.PersonID = personSaveModel.MasterModel.PersonID;
                        x.SetAdded();
                        SetPersonAcademicInfoNewId(x);
                    }
                });


                var willDeleted = existsPersonAcademicInfo.Where(x => !academicInfoList.Select(y => y.PAIID).Contains(x.PAIID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    academicInfoList.Add(x);
                });

            }

            return academicInfoList;
        }
        private List<PersonTrainingInfo> GetTrainingInfo(PersonSaveModel personSaveModel)
        {
            var trainingInfoList = new List<PersonTrainingInfo>();
            if (personSaveModel.TrainingInfo.IsNotNull())
            {
                trainingInfoList = personSaveModel.TrainingInfo.MapTo<List<PersonTrainingInfo>>();
                var existsTrainingInfo = PersonTrainingInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();

                trainingInfoList.ForEach(x =>
                {
                    if (existsTrainingInfo.Count > 0 && x.PTIID > 0)
                    {
                        x.SetModified();
                    }
                    else
                    {
                        x.PersonID = personSaveModel.MasterModel.PersonID;
                        x.SetAdded();
                        SetPersonTrainingInfoNewId(x);
                    }
                });

                var willDeleted = existsTrainingInfo.Where(x => !trainingInfoList.Select(y => y.PTIID).Contains(x.PTIID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    trainingInfoList.Add(x);
                });

            }

            return trainingInfoList;
        }

        private List<PersonAwardInfo> GetAwardInfo(PersonSaveModel personSaveModel)
        {
            var awardInfoList = new List<PersonAwardInfo>();
            if (personSaveModel.TrainingInfo.IsNotNull())
            {
                awardInfoList = personSaveModel.AwardInfo.MapTo<List<PersonAwardInfo>>();
                var existsAwardInfo = PersonAwardInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();

                awardInfoList.ForEach(x =>
                {
                    if (existsAwardInfo.Count > 0 && x.PAIID > 0)
                    {
                        x.SetModified();
                    }
                    else
                    {
                        x.PersonID = personSaveModel.MasterModel.PersonID;
                        x.SetAdded();
                        SetPersonAwardInfoNewId(x);
                    }
                });

                var willDeleted = existsAwardInfo.Where(x => !awardInfoList.Select(y => y.PAIID).Contains(x.PAIID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    awardInfoList.Add(x);
                });

            }

            return awardInfoList;
        }

        private List<PersonFamilyInfo> GetChildrenInfo(PersonSaveModel personSaveModel)
        {
            var childrenInfoList = new List<PersonFamilyInfo>();
            if (personSaveModel.TrainingInfo.IsNotNull())
            {
                childrenInfoList = personSaveModel.ChildrenInfo.MapTo<List<PersonFamilyInfo>>();
                var existsChildrenInfo = PersonFamilyInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();

                childrenInfoList.ForEach(x =>
                {
                    if (existsChildrenInfo.Count > 0 && x.PFIID > 0)
                    {
                        x.SetModified();
                    }
                    else
                    {
                        x.PersonID = personSaveModel.MasterModel.PersonID;
                        x.SetAdded();
                        SetPersonFamilyInfoNewId(x);
                    }
                });


                var willDeleted = existsChildrenInfo.Where(x => !childrenInfoList.Select(y => y.PFIID).Contains(x.PFIID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    childrenInfoList.Add(x);
                });
            }

            return childrenInfoList;
        }

        private List<PersonReferenceInfo> GetReferenceInfo(PersonSaveModel personSaveModel)
        {
            var referenceInfoList = new List<PersonReferenceInfo>();
            if (personSaveModel.TrainingInfo.IsNotNull())
            {
                referenceInfoList = personSaveModel.ReferenceInfo.MapTo<List<PersonReferenceInfo>>();
                var existsReferenceInfo = PersonReferenceInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();

                referenceInfoList.ForEach(x =>
                {
                    if (existsReferenceInfo.Count > 0 && x.PRIID > 0)
                    {
                        x.SetModified();
                    }
                    else
                    {
                        x.PersonID = personSaveModel.MasterModel.PersonID;
                        x.SetAdded();
                        SetPersonReferenceInfoNewId(x);
                    }
                });

                var willDeleted = existsReferenceInfo.Where(x => !referenceInfoList.Select(y => y.PRIID).Contains(x.PRIID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    referenceInfoList.Add(x);
                });

            }

            return referenceInfoList;
        }
        private PersonEmergencyContactInfo GetEmergencyContact(PersonSaveModel personSaveModel)
        {
            var emergencyContact = personSaveModel.EmergencyContact.MapTo<PersonEmergencyContactInfo>();
            var existsEmergencyContactInfo = PersonEmergencyContactInfoRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).FirstOrDefault();

            if (emergencyContact.Name.IsNotNull() && emergencyContact.ContactNo.IsNotNull())
            {
                if (existsEmergencyContactInfo.IsNotNull())
                {
                    emergencyContact.PECIID = existsEmergencyContactInfo.PECIID;
                    emergencyContact.PersonID = existsEmergencyContactInfo.PersonID;
                    emergencyContact.CreatedBy = existsEmergencyContactInfo.CreatedBy;
                    emergencyContact.CreatedDate = existsEmergencyContactInfo.CreatedDate;
                    emergencyContact.CreatedIP = existsEmergencyContactInfo.CreatedIP;
                    emergencyContact.RowVersion = existsEmergencyContactInfo.RowVersion;
                    emergencyContact.SetModified();
                }
                else
                {
                    emergencyContact.PersonID = personSaveModel.MasterModel.PersonID;
                    emergencyContact.SetAdded();
                    SetPersonEmergencyContactInfoNewId(emergencyContact);
                }

            }
            return emergencyContact;
        }

        private List<PersonImageDto> GetPersonImages(PersonSaveModel personSaveModel)
        {

            List<PersonImage> allImages = new List<PersonImage>();
            List<PersonImageDto> validateImages = new List<PersonImageDto>();

            foreach (var item in personSaveModel.PersonImages)
            {

                string ext = "";
                bool fileValid = false;
                string fileValidError = "";
                if (item.PIID > 0)
                {
                    ext = item.ImageType.Remove(0, 1);
                }
                else
                {
                    string result = item.ImageFile.Split(',')[1];

                    var bytes = System.Convert.FromBase64String(result);

                    fileValid = UploadUtil.IsFileValid(bytes, item.ImageName);


                    string err = CheckValidFileExtensionsForAttachment(ext, item.ImageName);
                    if (fileValid == false)
                    {
                        fileValidError = "Uploaded file extension is not allowed.";
                    }
                    if (!fileValidError.IsNullOrEmpty())
                    {
                        err = fileValidError;
                    }
                    if (!err.IsNullOrEmpty())
                    {
                        PersonImageDto piDto = new PersonImageDto();
                        piDto.ImageTypeValidation = err;
                        validateImages.Add(piDto);
                        return validateImages;
                    }
                }


            }

            var removeList = RemovePersonImages(personSaveModel);
            removeList.ForEach(x => x.SetDeleted());
            var addedList = AddImages(personSaveModel);
            if (addedList.IsNotNull() && addedList.Count > 0)
            {
                addedList.ForEach(x =>
                {
                    x.SetAdded();
                    SetPersonImageNewId(x);
                });
            }

            var updatedImages = UpdatePersonImages(personSaveModel);

            allImages = addedList.Concat(removeList).Concat(updatedImages).ToList();

            return allImages.MapTo<List<PersonImageDto>>();
        }

        private List<NomineeInformation> GetNomineeInfo(PersonSaveModel personSaveModel)
        {
            var nomineenfoList = new List<NomineeInformation>();
            if (personSaveModel.NomineeInfo.IsNotNull())
            {
                nomineenfoList = personSaveModel.NomineeInfo.MapTo<List<NomineeInformation>>();
                var existsNomineenfoList = NomineeInformationRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();

                nomineenfoList.ForEach(x =>
                {
                    if (existsNomineenfoList.Count > 0 && x.NIID > 0)
                    {
                        x.SetModified();
                    }
                    else
                    {
                        x.PersonID = personSaveModel.MasterModel.PersonID;
                        x.SetAdded();
                        SetNomineeInfoNewId(x);
                    }
                });

                var willDeleted = existsNomineenfoList.Where(x => !nomineenfoList.Select(y => y.NIID).Contains(x.NIID)).ToList();
                willDeleted.ForEach(x =>
                {
                    x.SetDeleted();
                    nomineenfoList.Add(x);
                });

            }

            return nomineenfoList;
        }

        public async Task<List<NomineeDto>> GeNomineeInfoList(int personID)
        {
            var list = await NomineeInformationRepo.GetAllListAsync(person => person.PersonID.Equals(personID));
            return list.MapTo<List<NomineeDto>>();
        }

        private List<PersonImage> RemovePersonImages(PersonSaveModel personSaveModel)
        {
            var personImageList = new List<PersonImage>();

            var prevImages = PersonImageRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();
            var removeList = prevImages.Where(x => !personSaveModel.PersonImages.Where(z => z.PIID != 0).Select(y => y.PIID).Contains(x.PIID)).ToList();

            if (removeList.Count > 0)
            {
                foreach (var data in removeList)
                {
                    string imageFolder = "upload\\images";
                    string folderName = "person";
                    IWebHostEnvironment instance1 = AppContexts.GetInstance<IWebHostEnvironment>();
                    string str = Path.Combine(((IHostEnvironment)instance1).ContentRootPath, "wwwroot\\" + data.ImagePath);
                    //imageFolder + "\\" + folderName + "\\" + data.PersonID + " - " + (personSaveModel.MasterModel.FirstName + personSaveModel.MasterModel.LastName).Replace(" ", ""));
                    if (File.Exists(str))
                        //File.Delete(str + "\\" + data.ImageName);
                        File.Delete(str);

                }
            }
            return removeList;
        }
        private List<PersonImage> UpdatePersonImages(PersonSaveModel personSaveModel)
        {
            var personImageList = new List<PersonImage>();
            if (personSaveModel.PersonImages.Count > 0)
            {
                var prevImages = PersonImageRepo.Entities.Where(x => x.PersonID == personSaveModel.MasterModel.PersonID).ToList();
                var updateList = prevImages.Where(x => personSaveModel.PersonImages.Where(z => z.PIID != 0).Select(y => y.PIID).Contains(x.PIID)).ToList();
                if (updateList.Count > 0)
                {
                    updateList.ForEach(x =>
                    {
                        x.IsFavorite = personSaveModel.PersonImages.Where(y => y.PIID == x.PIID).Select(x => x.IsFavorite).DefaultIfEmpty(false).SingleOrDefault();
                        x.SetModified();
                    });
                }

                return updateList;
            }
            return personImageList;
        }

        private List<PersonImage> AddImages(PersonSaveModel personSaveModel)
        {
            var personImageList = new List<PersonImage>();
            var willAdded = personSaveModel.PersonImages.Where(x => x.PIID == 0).ToList();
            if (willAdded.Count > 0)
            {
                int sl = 1;
                foreach (var img in willAdded)
                {
                    if (img.ImageFile.IsNotNull())
                    {
                        string imagename = $"{(img.IsSignature ? "Signature" : "Profile")}-{DateTime.Now.ToString("ddMMyyHHmmss")}-{sl}{Path.GetExtension(img.ImageName)}";
                        var imgByte = UploadUtil.Base64ToByteArray(img.ImageFile);
                        var imagePath = UploadUtil.SaveImageInDisk(imgByte, imagename, "Person\\" + personSaveModel.MasterModel.PersonID + " - " + (personSaveModel.MasterModel.FirstName + personSaveModel.MasterModel.LastName).Replace(" ", ""));
                        personImageList.Add(new PersonImage
                        {
                            PersonID = personSaveModel.MasterModel.PersonID,
                            ImagePath = imagePath,
                            ImageOriginalName = img.ImageName,
                            IsSignature = img.IsSignature,
                            ImageName = imagename,
                            IsFavorite = img.IsFavorite,
                            ImageType = Path.GetExtension(img.ImageName),
                        });

                        sl++;
                    }
                }
                return personImageList;
            }
            return personImageList;
        }


        public IEnumerable<Dictionary<string, object>> GetApprovalComment(int approvalProcessID)
        {
            var comments = GetApprovalComments(approvalProcessID, (int)Util.ApprovalType.EmployeeProfileApproval);
            return comments.Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ReferenceID, int APTypeID, int APPanelID)
        {
            return GetApprovalForwardingMembers(ReferenceID, APTypeID, APPanelID).Result;
        }
        public async Task<List<ComboModel>> GetForwardingMemberList(int ApprovalProcessID)
        {
            return GetApprovalForwardingMembers(ApprovalProcessID).Result;
        }
        public async Task<List<ComboModel>> GetRejectedMemeberList(int aprovalProcessId)
        {
            return GetApprovalRejectedMembers(aprovalProcessId).Result;
        }

        public IEnumerable<Dictionary<string, object>> ReportForEPAApprovalFeedback(int EPAID)
        {
            string sql = $@" EXEC Security..spRPTEPAApprovalFeedback {EPAID}";
            var feedback = PersonRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID)
        {
            string sql = $@"SELECT 
	                             FullName,
	                             APForwardEmployeeComment,
	                             CAST(CommentSubmitDate as Date) CommentSubmitDate,
	                             DesignationName,
	                             DepartmentName
                            FROM 
	                            {AppContexts.GetDatabaseName(ConnectionName.ApprovalContext)}..ApprovalForwardInfo AEF 
	                            INNER JOIN Approval..ApprovalProcess  AP ON AEF.ApprovalProcessID = AP.ApprovalProcessID
	                            INNER JOIN HRMS..ViewALLEmployee VE ON VE.EmployeeID = AEF.EmployeeID	
                            WHERE AP.APTypeID = {APTypeID} AND ReferenceID = {ReferenceID} AND CommentSubmitDate IS NOT NULL
                            ORDER BY APForwardInfoID asc";
            var comments = PersonRepo.GetDataDictCollection(sql);
            return comments;
        }

    }
}
