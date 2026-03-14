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
            string sql = $@"SELECT sup.*, sv.system_variable_description AS ""SupervisorTypeName"" FROM hrms.view_employee_supervisor_map sup
                            LEFT JOIN system_variable sv ON sup.supervisor_type = sv.system_variable_id
                            WHERE sup.person_id = '{PersonID}'";

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
                    filter = $@" AND CAST((SELECT approval.dbo.fn_validate_current_ap_employee({AppContexts.User.EmployeeID}, COALESCE(ap.approval_process_id, 0))) AS BOOLEAN) = TRUE";
                    break;
                case "Pending":
                    filter = $@" AND P.ApprovalStatusID = {(int)Util.ApprovalStatus.Pending}";
                    break;
                case "Approved":
                    filter = $@" AND P.ApprovalStatusID = {(int)Util.ApprovalStatus.Approved}";
                    break;
                case "My Approved":
                    filter = $@" AND ap.approval_process_id IN (SELECT * FROM approval.dbo.fn_return_approval_process_id({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Approved}))";
                    break;
                case "MyRejectReturnForwarded":
                    filter = $@" AND ap.approval_process_id IN 
                                (SELECT * FROM approval.dbo.fn_return_approval_process_id({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Rejected})
                                UNION  
                                SELECT * FROM approval.dbo.fn_return_approval_process_id({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Returned})
                                UNION 
                                SELECT * FROM approval.dbo.fn_return_approval_process_id({AppContexts.User.EmployeeID},{(int)Util.ApprovalFeedback.Forwarded}))";
                    break;
                default:
                    break;
            }
            where = filter.IsNotNullOrEmpty() ? "WHERE " : "";
            string sql = $@"SELECT DISTINCT
                             p.*
							 ,ve.employee_code AS ""EmployeeCode"",
	                         ve.department_id AS ""DepartmentID"",
	                         ve.department_name AS ""DepartmentName"",
	                         ve.full_name AS ""EmployeeName"", 
                             ve.division_id AS ""DivisionID"", 
                             ve.division_name AS ""DivisionName"",
                             ve.full_name || ve.employee_code || ve.department_name AS ""EmployeeWithDepartment""
                                ,COALESCE(ap.approval_process_id, 0) AS ""ApprovalProcessID"",
	                            CAST((SELECT approval.dbo.fn_validate_current_ap_employee({AppContexts.User.EmployeeID}, COALESCE(ap.approval_process_id, 0))) AS BOOLEAN) AS ""IsCurrentAPEmployee"",
	                            COALESCE(aef.ap_employee_feedback_id, 0) AS ""APEmployeeFeedbackID"",
	                            COALESCE(ap_forward_info_id, 0) AS ""APForwardInfoID"",
								COALESCE(aef.is_editable, 0) AS ""IsEditable"",
                                CASE WHEN (SELECT approval.dbo.fn_is_ap_creator({AppContexts.User.EmployeeID}, COALESCE(ap.approval_process_id, 0))) = TRUE AND editable_count > 0 THEN TRUE ELSE FALSE END AS ""IsReassessment"",
                                CASE WHEN COALESCE(cntr, 0) > 0 THEN TRUE ELSE FALSE END AS ""IsReturned"",
                                COALESCE(feedback_last_response_date, comment_submit_date) AS ""LastActionDate"",
	                            sv.system_variable_code AS ""ApprovalStatus"",
                                CASE WHEN pending_employee_code IS NOT NULL THEN (SELECT pending_employee_code AS ""EmployeeCode"", pending_employee_name AS ""EmployeeName"", pending_department_name AS ""DepartmentName"" FOR JSON PATH) END AS ""PendingAt""

                            FROM 
                            employee_profile_approval p
							LEFT JOIN hrms.view_all_employee ve ON p.person_id = ve.person_id
                            LEFT JOIN security.system_variable sv ON sv.system_variable_id = p.approval_status_id
                            LEFT JOIN approval.approval_process ap ON ap.reference_id = p.epaid AND ap.ap_type_id = {(int)Util.ApprovalType.EmployeeProfileApproval}
                            LEFT JOIN (
                                        SELECT 
                                              ap_employee_feedback_id,
                                              approval_process_id,
                                              is_editable,
                                              is_scm,
                                              is_multi_proxy  
                                        FROM 
                                            approval.dbo.function_join_list_aef({AppContexts.User.EmployeeID})
                            ) aef ON aef.approval_process_id = ap.approval_process_id
                            LEFT JOIN 
		                            (SELECT 
				                            ap_forward_info_id,
                                            approval_process_id 
			                            FROM 
				                            approval.approval_forward_info  
			                            WHERE 
				                            employee_id = {AppContexts.User.EmployeeID} AND comment_submit_date IS NULL) 
                            ap_forward ON ap_forward.approval_process_id = ap.approval_process_id
                            LEFT JOIN 
									(
									SELECT approval_process_id, employee_id, proxy_employee_id FROM approval.dbo.function_join_list_proxy_employee_f({AppContexts.User.EmployeeID}) 
									)							
							f ON f.approval_process_id = ap.approval_process_id
                            LEFT JOIN (
										SELECT COUNT(cntr) AS editable_count, reference_id FROM 
										(
										SELECT 
											COUNT(ap_employee_feedback_id) AS cntr, reference_id
										FROM 
											approval.approval_employee_feedback  aef
											LEFT JOIN approval.approval_process ap ON ap.approval_process_id = aef.approval_process_id
										WHERE sequence_no = 2 AND ap_feedback_id = 2 AND ap_type_id = {(int)Util.ApprovalType.EmployeeProfileApproval} 
										GROUP BY reference_id

										UNION ALL

										SELECT 
											COUNT(ap_employee_feedback_id) AS cntr, reference_id
										FROM 
											approval.approval_employee_feedback aef
											LEFT JOIN approval.approval_process ap ON ap.approval_process_id = aef.approval_process_id
										WHERE sequence_no = 1 AND ap_feedback_id = 2  AND ap_type_id = {(int)Util.ApprovalType.EmployeeProfileApproval} AND employee_id = {AppContexts.User.EmployeeID}
										GROUP BY reference_id

										) v
										GROUP BY reference_id
										) ea ON ea.reference_id = p.epaid

                                        LEFT JOIN(
										SELECT ap.approval_process_id, COUNT(COALESCE(ap_feedback_id, 0)) AS cntr, ap.reference_id 
										FROM 
											approval.approval_employee_feedback_remarks aefr 
											INNER JOIN approval.approval_process ap ON ap.approval_process_id = aefr.approval_process_id
										WHERE ap_feedback_id = 11 -- Returned
										GROUP BY ap.approval_process_id, ap.reference_id 
									) rej ON rej.reference_id = p.epaid
                                    LEFT JOIN (
                                            SELECT * FROM approval.dbo.function_join_list_proxy_employee_ap_submit_date({AppContexts.User.EmployeeID})                                 
									) ap_submit_date ON ap_submit_date.approval_process_id = ap.approval_process_id
									LEFT JOIN (
													SELECT 
													   MAX(comment_submit_date) AS comment_submit_date,
									                                              approval_process_id 
													FROM 
														approval.approval_forward_info  
													WHERE 
														employee_id = {AppContexts.User.EmployeeID} 
													GROUP BY approval_process_id
																				
														) apf_submit_date ON apf_submit_date.approval_process_id = ap.approval_process_id
                                   LEFT JOIN (
								       SELECT 
										    AEF.EmployeeCode PendingEmployeeCode,
										    EmployeeName PendingEmployeeName,
										    DepartmentName PendingDepartmentName,
										    AEF.ReferenceID PendingReferenceID
									    FROM 
										    Approval..viewApprovalEmployeeFeedback AEF 
									    WHERE AEF.APTypeID = {(int)Util.ApprovalType.EmployeeProfileApproval} AND APFeedbackID = {(int)Util.ApprovalFeedback.FeedbackRequested}
								   ) pending_at ON pending_at.pending_reference_id = p.epaid
							WHERE (ve.employee_id = {AppContexts.User.EmployeeID} OR f.employee_id = {AppContexts.User.EmployeeID} OR COALESCE(f.proxy_employee_id, 0) = {AppContexts.User.EmployeeID}) {filter}";
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
                             p.person_id AS ""PersonID"",
                             p.first_name AS ""FullName"",
                             p.mobile AS ""Mobile"",
                             p.email AS ""Email"",
                                bld.system_variable_code AS ""BloodGroup"",
                                COALESCE(p.mobile || '-' || p.email || '-' || bld.system_variable_code, '') AS ""PersonMobileEmailBG"",
                             gen.system_variable_code AS ""Gender"",
                             p.date_of_birth AS ""DateOfBirth"",
                             rel.system_variable_code AS ""Religion"",
                             p.nationality AS ""Nationality"",
                                COALESCE(gen.system_variable_code || '-' || rel.system_variable_code || '-' || p.nationality, '') AS ""GenRelNationality"",
                             usr.user_name AS ""CreatedBy"",
                             msts.system_variable_code AS ""MaritalStatus"",
                             prsn_type.system_variable_code AS ""PersonType"",
                             p.created_date AS ""CreatedDate"",
                                img.image_path AS ""ImagePath"",
                                COALESCE(emp.employee_id, 0) AS ""EmployeeID"",
                                CASE 
                                  WHEN emp.person_id IS NULL
                                   THEN TRUE
                                  ELSE FALSE
                                  END AS ""IsRemovable""
                            FROM 
                             person p
                                LEFT JOIN (
                             SELECT DISTINCT employee_id, person_id
                             FROM {ConnectionName.HrmsRemote}.employee
                             ) emp ON p.person_id = emp.person_id
                             LEFT JOIN system_variable gen ON gen.system_variable_id = p.gender_id
                             LEFT JOIN system_variable rel ON rel.system_variable_id = p.religion_id
                             LEFT JOIN system_variable bld ON bld.system_variable_id = p.blood_group_id
                             LEFT JOIN system_variable msts ON msts.system_variable_id = p.marital_status_id	
                             LEFT JOIN system_variable prsn_type ON prsn_type.system_variable_id = p.person_type_id
                             LEFT JOIN users usr ON usr.user_id = p.created_by
                                LEFT JOIN (SELECT image_path, person_id FROM person_image WHERE is_favorite = TRUE) img
                                ON img.person_id = p.person_id {filter}";
            var result = PersonRepo.LoadGridModel(parameters, sql);
            return result;
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetPersonListDic()
        {
            string sql = $@"SELECT 
	                            p.person_id AS ""PersonID"",
	                            p.first_name AS ""FullName"",
	                            p.mobile AS ""Mobile"",
	                            p.email AS ""Email"",
	                            gen.system_variable_code AS ""Gender"",
	                            p.date_of_birth AS ""DateOfBirth"",
	                            rel.system_variable_code AS ""Religion"",
	                            p.nationality AS ""Nationality"",
	                            usr.user_name AS ""CreatedBy"",
	                            msts.system_variable_code AS ""MaritalStatus"",
	                            prsn_type.system_variable_code AS ""PersonType"",
	                            p.created_date AS ""CreatedDate"",
                                img.image_path AS ""ImagePath"",
                                COALESCE(emp.employee_id, 0) AS ""EmployeeID"",
                                CASE 
		                        WHEN emp.person_id IS NULL
			                        THEN TRUE
		                        ELSE FALSE
		                        END AS ""IsRemovable""
                            FROM 
	                            person p
                                LEFT JOIN (
	                            SELECT DISTINCT employee_id, person_id
	                            FROM {ConnectionName.HrmsRemote}.employee
	                            ) emp ON p.person_id = emp.person_id
	                            LEFT JOIN system_variable gen ON gen.system_variable_id = p.gender_id
	                            LEFT JOIN system_variable rel ON rel.system_variable_id = p.religion_id
	                            LEFT JOIN system_variable bld ON bld.system_variable_id = p.blood_group_id
	                            LEFT JOIN system_variable msts ON msts.system_variable_id = p.marital_status_id	
	                            LEFT JOIN system_variable prsn_type ON prsn_type.system_variable_id = p.person_type_id
	                            LEFT JOIN users usr ON usr.user_id = p.created_by
                                LEFT JOIN (SELECT image_path, person_id FROM person_image WHERE is_favorite = TRUE) img
								ON img.person_id = p.person_id";
            var listDict = PersonRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }

        public async Task<IEnumerable<Dictionary<string, object>>> GetUnemploymentListDic()
        {
            string sql = $@"SELECT 
	                            p.person_id AS ""PersonID"",
	                            p.first_name AS ""FullName"",
	                            p.mobile AS ""Mobile"",
	                            p.email AS ""Email"",
	                            gen.system_variable_code AS ""Gender"",
	                            p.date_of_birth AS ""DateOfBirth"",
	                            rel.system_variable_code AS ""Religion"",
	                            p.nationality AS ""Nationality"",
	                            usr.user_name AS ""CreatedBy"",
	                            msts.system_variable_code AS ""MaritalStatus"",
	                            prsn_type.system_variable_code AS ""PersonType"",
	                            p.created_date AS ""CreatedDate"",
                                img.image_path AS ""ImagePath""
                            FROM 
	                            person p
								LEFT JOIN (
	                        SELECT DISTINCT person_id
	                        FROM {ConnectionName.HrmsRemote}.employee
	                        ) emp ON p.person_id = emp.person_id
	                            LEFT JOIN system_variable gen ON gen.system_variable_id = p.gender_id
	                            LEFT JOIN system_variable rel ON rel.system_variable_id = p.religion_id
	                            LEFT JOIN system_variable bld ON bld.system_variable_id = p.blood_group_id
	                            LEFT JOIN system_variable msts ON msts.system_variable_id = p.marital_status_id	
	                            LEFT JOIN system_variable prsn_type ON prsn_type.system_variable_id = p.person_type_id
	                            LEFT JOIN users usr ON usr.user_id = p.created_by
                                LEFT JOIN (SELECT image_path, person_id FROM person_image WHERE is_favorite = TRUE) img
								ON img.person_id = p.person_id
								WHERE emp.person_id IS NULL";
            var listDict = PersonRepo.GetDataDictCollection(sql);

            return await Task.FromResult(listDict);
        }
        public async Task<Dictionary<string, object>> GetPersonInfoDic(int primaryID)
        {
            string sql = $@"SELECT e.*, g.job_grade_name AS ""JobGradeName"" FROM hrms.view_all_employee e
                                    LEFT JOIN hrms.job_grade g ON e.job_grade_id = g.job_grade_id
	                            WHERE e.person_id = {primaryID}";
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
	                            p.person_id AS ""PersonID"", 
	                            first_name AS ""FirstName"", 
	                            last_name AS ""LastName"", 
	                            mobile AS ""Mobile"", 
	                            mobile2 AS ""Mobile2"", 
	                            email AS ""Email"", 
	                            gender_id AS ""GenderID"", 
	                            date_of_birth AS ""DateOfBirth"", 
	                            alternate_email AS ""AlternateEmail"", 
	                            religion_id AS ""ReligionID"", 
	                            nationality AS ""Nationality"", 
	                            is_bangladeshi AS ""IsBangladeshi"", 
	                            blood_group_id AS ""BloodGroupID"", 
	                            person_type_id AS ""PersonTypeID"", 
	                            father_name AS ""FatherName"", 
	                            mother_name AS ""MotherName"", 
	                            nidnumber AS ""NidNumber"", 
	                            passport_number AS ""PassportNumber"", 
	                            passport_issue_date AS ""PassportIssueDate"", 
	                            passport_expiry_date AS ""PassportExpiryDate"", 
	                            birth_certificate AS ""BirthCertificate"", 
	                            driving_license AS ""DrivingLicense"", 
	                            tinnumber AS ""TinNumber"", 
	                            tax_zone AS ""TaxZone"", 
	                            marital_status_id AS ""MaritalStatusID"", 
	                            bangla_name AS ""BanglaName"", 
	                            bangla_full_name AS ""BanglaFullName"", 
	                            marriage_date AS ""MarriageDate"", 
	                            father_dob AS ""FatherDob"", 
	                            is_father_alive AS ""IsFatherAlive"", 
	                            mother_dob AS ""MotherDob"", 
	                            is_mother_alive AS ""IsMotherAlive"", 
	                            spouse_name AS ""SpouseName"", 
	                            spouse_dob AS ""SpouseDob"", 
	                            spouse_gender_id AS ""SpouseGenderID"", 
	                            marital_details AS ""MaritalDetails"", 
	                            p.company_id AS ""CompanyID"", 
	                            p.created_by AS ""CreatedBy"", 
	                            p.created_date AS ""CreatedDate"", 
	                            p.created_ip AS ""CreatedIP"", 
	                            p.updated_by AS ""UpdatedBy"", 
	                            p.updated_date AS ""UpdatedDate"", 
	                            p.updated_ip AS ""UpdatedIP"", 
	                            p.row_version AS ""RowVersion"",
	                            gen.system_variable_code AS ""GenderName"",
	                            rel.system_variable_code AS ""ReligionName"",
	                            bld.system_variable_code AS ""BloodGroupName"",
	                            msts.system_variable_code AS ""MaritalStatusName"",
                                sg.system_variable_code AS ""SpouseGenderName"",
	                            prsn_type.system_variable_code AS ""PersonTypeName"",
                                COALESCE(emp.employee_id, 0) AS ""EmployeeID"",

								present.district_name AS ""PresentDistrictName"",
								COALESCE(present.district_id, 0) AS ""PresentDistrictID"",
								present.thana_name AS ""PresentThanaName"",
								COALESCE(present.thana_id, 0) AS ""PresentThanaID"",
								present.post_code AS ""PresentPostCode"",
								present.address AS ""PresentAddress"",

								permanent.district_name AS ""PermanentDistrictName"",
								COALESCE(permanent.district_id, 0) AS ""PermanentDistrictID"",
								permanent.thana_name AS ""PermanentThanaName"",
								COALESCE(permanent.thana_id, 0) AS ""PermanentThanaID"",
								permanent.post_code AS ""PermanentPostCode"",
								permanent.address AS ""PermanentAddress"",
                                COALESCE(present.is_same_as_present_address, FALSE) AS ""IsSameAsPresentAddress"",
								CASE WHEN COALESCE(epa.person_id, 0) = 0 THEN TRUE ELSE FALSE END AS ""CanUpdateProfile""
                            
                            FROM 
                            person p
	                            LEFT JOIN system_variable gen ON gen.system_variable_id = p.gender_id
	                            LEFT JOIN system_variable rel ON rel.system_variable_id = p.religion_id
	                            LEFT JOIN system_variable bld ON bld.system_variable_id = p.blood_group_id
	                            LEFT JOIN system_variable sg ON sg.system_variable_id = p.spouse_gender_id	
	                            LEFT JOIN system_variable msts ON msts.system_variable_id = p.marital_status_id	
	                            LEFT JOIN system_variable prsn_type ON prsn_type.system_variable_id = p.person_type_id
                                LEFT JOIN {ConnectionName.HrmsRemote}.employee emp ON p.person_id = emp.person_id
	                            LEFT JOIN users usr ON usr.user_id = p.created_by

								LEFT JOIN (SELECT DISTINCT person_id FROM employee_profile_approval
									WHERE approval_status_id = 22) epa ON epa.person_id = p.person_id
								
								LEFT JOIN (SELECT
								person_address_info.person_id,
								person_address_info.district_id,
								person_address_info.thana_id,
								person_address_info.post_code,
								person_address_info.address,
								district.district_name,
								thana.thana_name,
                                person_address_info.is_same_as_present_address
								FROM person_address_info
								LEFT JOIN district ON person_address_info.district_id = district.district_id
								LEFT JOIN thana ON person_address_info.thana_id = thana.thana_id
								WHERE address_type_id = {(int)PersonAddressType.Present}) present ON present.person_id = p.person_id
								LEFT JOIN (SELECT 
								person_address_info.person_id,
								person_address_info.district_id,
								person_address_info.thana_id,
								person_address_info.post_code,
								person_address_info.address,
								district.district_name,
								thana.thana_name
								FROM person_address_info
								LEFT JOIN district ON person_address_info.district_id = district.district_id
								LEFT JOIN thana ON person_address_info.thana_id = thana.thana_id
								WHERE address_type_id = {(int)PersonAddressType.Permanent}) permanent ON permanent.person_id = p.person_id
	                            WHERE p.person_id = {primaryID}";
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

            //string sql = $@"UPDATE employee SET full_name='{master.FirstName}' WHERE person_id={master.PersonID}";
            //PersonRepo.ExecuteSqlCommand(sql);
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

            string sql = $@"UPDATE hrms.employee SET full_name = '{master.FirstName}' WHERE person_id = {master.PersonID}";
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
            string sql = $@"SELECT parent.id,
	                            parent.name,
	                            parent.info,
	                            media.type,
	                            media.title,
	                            media.preview
                            FROM 
                            (SELECT 
	                            ROW_NUMBER() OVER (ORDER BY name) AS id,
	                            name,
	                            CAST(SUM(info) AS varchar) || ' Photos' AS info
                            FROM
                            (SELECT
	                            DISTINCT
	                            TO_CHAR(created_date, 'FMMonth YYYY') AS name,
	                            COUNT(pi_id) AS info
                            FROM 
	                            person_image
                            WHERE person_id = {personID}
                            GROUP BY created_date
                            ) media
                            GROUP BY name
							) parent
							LEFT JOIN (
							SELECT
								TO_CHAR(created_date, 'FMMonth YYYY') AS name,
								'photo' AS type,	
								image_name AS title,
								image_path AS preview
							FROM 
								person_image
							WHERE person_id = {personID} ) media ON media.name = parent.name";
            var mediaList = PersonRepo.GetJsonData(sql);
            return await Task.FromResult(mediaList);
        }

        public async Task<Dictionary<string, object>> GetPersonAboutInfo(int personID)
        {
            string sql = $@"SELECT * FROM person_about_info_view WHERE person_id = {personID}";
            var employee = PersonRepo.GetData(sql);
            return await Task.FromResult(employee);
        }
        public async Task<Dictionary<string, object>> GetEmployeeUpdateApproval(int EPAID)
        {
            string sql = $@"SELECT p.*
                            , va.department_id AS ""DepartmentID"", va.department_name AS ""DepartmentName"", va.full_name AS ""EmployeeName"", sv.system_variable_code AS ""ApprovalStatus"", va.image_path AS ""ImagePath"", va.employee_code AS ""EmployeeCode"", va.division_id AS ""DivisionID"", va.division_name AS ""DivisionName"", va.work_mobile AS ""WorkMobile""
                            ,CASE  WHEN (SELECT approval.dbo.fn_is_ap_creator({AppContexts.User.EmployeeID}, COALESCE(ap.approval_process_id, 0))) = TRUE AND editable_count > 0 THEN TRUE ELSE FALSE END AS ""IsReassessment""
							,CASE WHEN COALESCE(cntr, 0) > 0 THEN TRUE ELSE FALSE END AS ""IsReturned""
                            FROM employee_profile_approval p 
                            LEFT JOIN hrms.view_all_employee va ON va.person_id = p.person_id
                            LEFT JOIN system_variable sv ON sv.system_variable_id = p.approval_status_id
                            LEFT JOIN approval.approval_process ap ON ap.reference_id = p.epaid AND ap.ap_type_id = {(int)Util.ApprovalType.EmployeeProfileApproval} 
                            LEFT JOIN (
							    SELECT COUNT(cntr) AS editable_count, reference_id FROM 
							    (
							    SELECT 
								    COUNT(ap_employee_feedback_id) AS cntr, reference_id
							    FROM 
								    approval.approval_employee_feedback aef
								    LEFT JOIN approval.approval_process ap ON ap.approval_process_id = aef.approval_process_id
							    WHERE sequence_no = 2 AND ap_feedback_id = 2 AND ap_type_id = {(int)Util.ApprovalType.EmployeeProfileApproval}
							    GROUP BY reference_id 

							    UNION ALL

							    SELECT 
								    COUNT(ap_employee_feedback_id) AS cntr, reference_id
							    FROM 
								    approval.approval_employee_feedback aef
								    LEFT JOIN approval.approval_process ap ON ap.approval_process_id = aef.approval_process_id
							    WHERE sequence_no = 1 AND ap_feedback_id = 2 AND ap_type_id = {(int)Util.ApprovalType.EmployeeProfileApproval} AND employee_id = {AppContexts.User.EmployeeID}
							    GROUP BY reference_id

							    ) v
							    GROUP BY reference_id
							    ) ea ON ea.reference_id = p.epaid

                                LEFT JOIN(
							    SELECT ap.approval_process_id, COUNT(COALESCE(ap_feedback_id, 0)) AS cntr, ap.reference_id 
							    FROM 
								    approval.approval_employee_feedback_remarks aefr 
								    INNER JOIN approval.approval_process ap ON ap.approval_process_id = aefr.approval_process_id
							    WHERE ap_feedback_id = 11 -- Returned
							    GROUP BY ap.approval_process_id, ap.reference_id 
						    ) rej ON rej.reference_id = p.epaid
LEFT JOIN
                                (
                                    SELECT approval_process_id, employee_id, proxy_employee_id FROM approval.dbo.function_join_list_proxy_employee_f({AppContexts.User.EmployeeID}) 
								)							
						        f ON f.approval_process_id = ap.approval_process_id
                            WHERE p.epaid = {EPAID} AND (va.employee_id = {AppContexts.User.EmployeeID}
                OR f.employee_id = {AppContexts.User.EmployeeID}
                OR COALESCE(f.proxy_employee_id, 0) = {AppContexts.User.EmployeeID})";
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
            string sql = $@"CALL security.sp_rpt_epa_approval_feedback({EPAID})";
            var feedback = PersonRepo.GetDataDictCollection(sql);
            return feedback;
        }
        public IEnumerable<Dictionary<string, object>> GetForwardingMemberComments(int APTypeID, int ReferenceID)
        {
            string sql = $@"SELECT 
	                             full_name AS ""FullName"",
	                             ap_forward_employee_comment AS ""APForwardEmployeeComment"",
	                             CAST(comment_submit_date AS date) AS ""CommentSubmitDate"",
	                             designation_name AS ""DesignationName"",
	                             department_name AS ""DepartmentName""
                            FROM 
	                            {ConnectionName.ApprovalRemote}.approval_forward_info aef 
	                            INNER JOIN approval.approval_process ap ON aef.approval_process_id = ap.approval_process_id
	                            INNER JOIN hrms.view_all_employee ve ON ve.employee_id = aef.employee_id	
                            WHERE ap.ap_type_id = {APTypeID} AND reference_id = {ReferenceID} AND comment_submit_date IS NOT NULL
                            ORDER BY ap_forward_info_id ASC";
            var comments = PersonRepo.GetDataDictCollection(sql);
            return comments;
        }

    }
}
