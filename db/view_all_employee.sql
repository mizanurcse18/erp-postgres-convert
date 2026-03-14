CREATE OR REPLACE VIEW view_all_employee AS
SELECT
    emp.full_name AS "FullName",
    emp.employee_code AS "EmployeeCode",
    emp.date_of_joining AS "DateOfJoining",
    emp.work_email AS "WorkEmail",
    emp.work_mobile AS "WorkMobile",
    emp.employee_status_id AS "EmployeeStatusID",
    COALESCE(emp_stats.system_variable_code, '') AS "EmployeeStatus",
    emp.discontinue_date AS "DiscontinueDate",
    emp.employment_category_id AS "EmploymentCategoryID",
    emp.confirm_date AS "ConfirmDate",
    dep.department_name AS "DepartmentName",
    des.designation_name AS "DesignationName",
    div.division_name AS "DivisionName",
    p.*,

gen.system_variable_code AS "GenderName",
rel.system_variable_code AS "ReligionName",
bld.system_variable_code AS "BloodGroupName",
msts.system_variable_code AS "MaritalStatusName",
prsn_type.system_variable_code AS "PersonTypeName",
COALESCE(emp.employee_id, 0) AS "EmployeeID",

present.district_name AS "PresentDistrictName",
present.district_id AS "PresentDistrictID",
present.thana_name AS "PresentThanaName",
present.thana_id AS "PresentThanaID",
present.post_code AS "PresentPostCode",
present.address AS "PresentAddress",

permanent.district_name AS "PermanentDistrictName",
permanent.district_id AS "PermanentDistrictID",
permanent.thana_name AS "PermanentThanaName",
permanent.thana_id AS "PermanentThanaID",
permanent.post_code AS "PermanentPostCode",
permanent.address AS "PermanentAddress",

empl.job_grade_id AS "JobGradeID",
empl.band AS "Band",
empl.employee_type_id AS "EmployeeTypeID",
empl.department_id AS "DepartmentID",
empl.designation_id AS "DesignationID",
empl.division_id AS "DivisionID",
cl.cluster_id AS "ClusterID",
rgn.region_id AS "RegionID",
bi.branch_id AS "BranchID",
cl.cluster_name AS "ClusterName",
rgn.region_name AS "RegionName",
bi.branch_name AS "BranchName",
pim.image_path AS "ImagePath",
COALESCE(u.user_id, 0) AS "UserID",
empl.shift_id AS "ShiftID",
empl.employment_id AS "EmploymentID",
psign.sign_path AS "SignPath"

FROM
    employee emp
    LEFT JOIN employment empl ON empl.employee_id = emp.employee_id AND empl.is_current = TRUE
    LEFT JOIN department dep ON dep.department_id = empl.department_id
    LEFT JOIN designation des ON des.designation_id = empl.designation_id
    LEFT JOIN division div ON div.division_id = empl.division_id
    LEFT JOIN cluster cl ON cl.cluster_id = empl.cluster_id
    LEFT JOIN region rgn ON rgn.region_id = empl.region_id
    LEFT JOIN branch_info bi ON bi.branch_id = empl.branch_id
    LEFT JOIN security_remote.system_variable emp_stats ON emp_stats.system_variable_id = empl.employee_type_id
    LEFT JOIN security_remote.person p ON p.person_id = emp.person_id
    LEFT JOIN security_remote.system_variable gen ON gen.system_variable_id = p.gender_id
    LEFT JOIN security_remote.system_variable rel ON rel.system_variable_id = p.religion_id
    LEFT JOIN security_remote.system_variable bld ON bld.system_variable_id = p.blood_group_id
    LEFT JOIN security_remote.system_variable msts ON msts.system_variable_id = p.marital_status_id
    LEFT JOIN security_remote.system_variable prsn_type ON prsn_type.system_variable_id = p.person_type_id
    LEFT JOIN (
        SELECT
            person_address_info.person_id,
            person_address_info.district_id,
            person_address_info.thana_id,
            person_address_info.post_code,
            person_address_info.address,
            district.district_name,
            thana.thana_name
        FROM security_remote.person_address_info
        LEFT JOIN security_remote.district ON person_address_info.district_id = district.district_id
        LEFT JOIN security_remote.thana ON person_address_info.thana_id = thana.thana_id
        WHERE address_type_id = 26
    ) present ON present.person_id = p.person_id
    LEFT JOIN (
        SELECT
            person_address_info.person_id,
            person_address_info.district_id,
            person_address_info.thana_id,
            person_address_info.post_code,
            person_address_info.address,
            district.district_name,
            thana.thana_name
        FROM security_remote.person_address_info
        LEFT JOIN security_remote.district ON person_address_info.district_id = district.district_id
        LEFT JOIN security_remote.thana ON person_address_info.thana_id = thana.thana_id
        WHERE address_type_id = 27
    ) permanent ON permanent.person_id = p.person_id
    LEFT JOIN (
        SELECT person_id, image_path
        FROM security_remote.person_image
        WHERE is_favorite = TRUE
    ) pim ON pim.person_id = emp.person_id
    LEFT JOIN (
        SELECT person_id, image_path AS sign_path
        FROM security_remote.person_image
        WHERE is_signature = TRUE
    ) psign ON psign.person_id = emp.person_id
    LEFT JOIN security_remote.users u ON p.person_id = u.person_id;