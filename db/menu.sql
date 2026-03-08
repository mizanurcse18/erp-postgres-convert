USE [Security]
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (3, 0, 1, N'applications', N'Applications', N'APPLICATIONS', N'group', N'apps', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (5, 3, 1, N'dashboards', N'Dashboards', N'DASHBOARDS', N'collapse', N'dashboard', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (6, 24, 1, N'attendance-dashboard', N'Attendance Details', N'Attendance Details', N'item', NULL, N'/apps/dashboards/attendance-dashboard', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (8, 3, 1, N'calendar', N'Calendar', N'CALENDAR', N'item', N'today', N'/apps/calendar', NULL, NULL, 1, NULL, NULL, 1, CAST(2.01 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (9, 11, 1, N'users', N'Users', N'Users', N'item', N'account_box', N'/apps/users/all', NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (11, 3, 1, N'access-control', N'Access Control', N'Access Control', N'collapse', N'security', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.16 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (12, 11, 1, N'access-rule', N'Rule', N'Rule', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.17 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (13, 11, 1, N'access-group', N'Group', N'Group', N'collapse', N'group', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.18 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (14, 12, 1, N'access-rule-list', N'Rule List', N'Rule List', N'item', N'list', N'/apps/security/security-rules/securityRuleList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.20 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (15, 12, 1, N'access-rule-creation', N'New Rule', N'New Rule', N'item', N'add_circle', N'/apps/security/security-rules/securityRule/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.19 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (16, 13, 1, N'access-group-creation', N'New Group', N'New Group', N'item', N'add_circle', N'/apps/security/security-groups/securityGroup/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.19 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (17, 13, 1, N'access-group-list', N'Group List', N'Group List', N'item', N'list', N'/apps/security/security-groups/securityGroupList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.19 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (18, 23, 1, N'person', N'Person', N'Person', N'collapse', N'person', NULL, NULL, NULL, 1, NULL, NULL, 0, CAST(1.20 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (19, 18, 1, N'add-person', N'Add New Person', N'Add New Person', N'item', N'person_add', N'/apps/security/persons/person/new', NULL, NULL, 1, NULL, NULL, 0, CAST(1.21 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (20, 18, 1, N'person-list', N'Person List', N'Person List', N'item', N'list', N'/apps/security/persons/personList', NULL, NULL, 1, NULL, NULL, 0, CAST(1.22 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (21, 24, 1, N'employee-attendance-dashboard', N'Employee''s Attendance', N'Employee''s Attendance', N'item', NULL, N'/apps/dashboards/employee-attendance-dashboard', NULL, NULL, 1, NULL, NULL, 1, CAST(1.34 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (22, 3, 1, N'license', N'License', N'License', N'item', N'card_membership', N'/pages/license', NULL, NULL, 1, NULL, NULL, 0, CAST(1.23 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (23, 3, 1, N'hrms', N'HRMS', N'HRMS', N'collapse', N'group', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.24 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (24, 23, 1, N'attendance', N'Attendance', N'Attendance', N'collapse', N'how_to_reg', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (25, 24, 1, N'remote-attendance', N'Remote Attendance', N'Remote Attendance', N'item', NULL, N'/apps/hrms/attendance/remoteAttendance', NULL, NULL, 1, NULL, NULL, 1, CAST(1.26 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (26, 142, 1, N'hrms-master-settings', N'HRMS', N'HRMS', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.26 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (27, 26, 1, N'department', N'Department', N'Department', N'item', NULL, N'/apps/hrms/masterdata/department', NULL, NULL, 1, NULL, NULL, 1, CAST(1.26 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (28, 26, 1, N'designation', N'Designation', N'Designation', N'item', NULL, N'/apps/hrms/masterdata/designation', NULL, NULL, 1, NULL, NULL, 1, CAST(1.28 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (29, 26, 1, N'division', N'Division', N'Division', N'item', NULL, N'/apps/hrms/masterdata/division', NULL, NULL, 1, NULL, NULL, 1, CAST(1.29 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (30, 23, 1, N'employee', N'Employee Info', N'Employee Info', N'collapse', N'person', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.30 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (31, 30, 1, N'add-employee', N'Add Employee', N'Add Employee', N'item', N'person_add', N'/apps/hrms/employee/addEmployeeWithPerson/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.31 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (32, 30, 1, N'employee-list', N'Employee List', N'Employee List', N'item', N'list', N'/apps/hrms/employee/employeeList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.32 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (33, 24, 1, N'approve-remote-attendance', N'Approve Attendance', N'Approve Attendance', N'item', NULL, N'/apps/hrms/attendance/approveRemoteAttendance', NULL, NULL, 1, NULL, NULL, 1, CAST(1.33 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (34, 23, 1, N'hrms-attenance', N'HR Attendance', N'HR Attendance', N'collapse', N'how_to_reg', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.30 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (35, 26, 1, N'cluster', N'Cluster', N'Cluster', N'item', NULL, N'/apps/hrms/masterdata/cluster', NULL, NULL, 1, NULL, NULL, 1, CAST(1.35 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (36, 26, 1, N'region', N'Region', N'Region', N'item', NULL, N'/apps/hrms/masterdata/region', NULL, NULL, 1, NULL, NULL, 1, CAST(1.36 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (37, 26, 1, N'branchinfo', N'Workstation', N'Workstation', N'item', NULL, N'/apps/hrms/masterdata/workstation', NULL, NULL, 1, NULL, NULL, 1, CAST(1.37 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (38, 30, 1, N'upcoming-employeelist', N'Upcoming Employee List', N'Upcoming Employee List', N'item', N'list', N'/apps/security/persons/unemploymentList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.38 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (39, 26, 1, N'financialyear', N'Financial Year', N'Financial Year', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.39 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (40, 39, 1, N'financialyear-list', N'Financial Year List', N'Financial Year List', N'item', NULL, N'/apps/security/masterdata/financialyear/financialYearList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.40 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (41, 39, 1, N'add-financialyear', N'Add Financial Year', N'Add Financial Year', N'item', NULL, N'/apps/security/masterdata/financialyear/addFinancialYear/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.41 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (42, 26, 1, N'shift', N'Shift', N'Shift', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.42 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (43, 42, 1, N'shift-list', N'Shift List', N'Shift List', N'item', NULL, N'/apps/hrms/masterdata/shiftList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.43 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (44, 42, 1, N'shift-new', N'Add Shift', N'Add Shift', N'item', NULL, N'/apps/hrms/masterdata/shift/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.44 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (45, 26, 1, N'leave', N'Leave', N'Leave', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.45 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (46, 45, 1, N'leave-policy', N'Leave Policy', N'Leave Policy', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.46 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (47, 46, 1, N'leavepolicy-list', N'Leave Policy List', N'Leave Policy List', N'item', NULL, N'/apps/hrms/masterdata/leave/leavepolicyList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.47 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (48, 46, 1, N'add-leavepolicy', N'Add Leave Policy', N'Add Leave Policy', N'item', NULL, N'/apps/hrms/masterdata/leave/addLeavepolicy/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.48 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (49, 26, 1, N'holiday-new', N'Holiday', N'Holiday', N'item', NULL, N'/apps/hrms/masterdata/holiday/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.49 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (50, 45, 1, N'employee-leave-account', N'Emp. Leave Account', N'Emp. Leave Account', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.50 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (51, 50, 1, N'employeeleaveaccount-list', N'Emp. Leave Account List', N'Emp. Leave Account List', N'item', NULL, N'/apps/hrms/masterdata/employeeleaveaccount/employeeleaveaccountList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.51 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (52, 50, 1, N'add-employeeleaveaccount', N'Add Emp. Leave Account', N'Add Emp. Leave Account', N'item', NULL, N'/apps/hrms/masterdata/employeeleaveaccount/addEmployeeleaveaccount/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.52 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (53, 23, 1, N'leave  Managment', N'Leave Managment', N'Leave Managment', N'collapse', N'how_to_reg', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.53 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (54, 202, 1, N'leave-application-list', N'Apply Leave Application', N'Apply Leave Application', N'item', NULL, N'/apps/hrms/leave-management/leave-application/employee/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.54 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (55, 67, 1, N'approval-process', N'Approval Process', N'Approval Process', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.55 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (56, 55, 1, N'approvaltype', N'Approval Type', N'Approval Type', N'item', NULL, N'/apps/approval/approvalType', NULL, NULL, 1, NULL, NULL, 1, CAST(1.56 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (57, 55, 1, N'approvalstaus', N'Approval Satus', N'Approval Satus', N'item', NULL, N'/apps/approval/approvalStatus', NULL, NULL, 1, NULL, NULL, 1, CAST(1.57 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (58, 55, 1, N'approvalpanel', N'Approval Panel', N'Approval Panel', N'item', NULL, N'/apps/approval/approvalPanel', NULL, NULL, 1, NULL, NULL, 1, CAST(1.58 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (59, 55, 1, N'employeepanelmap', N'Emp. Panel Map', N'Emp. Panel Map', N'item', NULL, N'/apps/approval/employeepanelMap', NULL, NULL, 1, NULL, NULL, 1, CAST(1.59 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (60, 55, 1, N'forwardemployeepanelmap', N'Fwd. Emp. Panel Map', N'Fwd. Emp. Panel Map', N'item', NULL, N'/apps/approval/forwardemployeepanelMap', NULL, NULL, 1, NULL, NULL, 1, CAST(1.60 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (61, 67, 1, N'approval-managment', N'Approval Managment', N'Approval Managment', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.61 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (62, 61, 1, N'pending-approval-list', N'Pending Approval List', N'Pending Approval List', N'item', NULL, N'/apps/approval/approvalRequests', NULL, NULL, 1, NULL, NULL, 1, CAST(1.62 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (63, 3, 1, N'notification-list', N'Notifications', N'Notifications', N'item', N'notifications', N'/apps/Notification', NULL, NULL, 1, NULL, NULL, 1, CAST(0.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (64, 3, 1, N'mail-setup', N'Mail Setup', N'Mail Setup', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.64 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (65, 64, 1, N'mailconfigure', N'Mail Configure', N'Mail Configure', N'item', NULL, N'/apps/mailconfigure/mailconfiguration', NULL, NULL, 1, NULL, NULL, 1, CAST(1.65 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (66, 64, 1, N'mailgroupsetup', N'Mail Group Setup', N'Mail Group Setup', N'item', NULL, N'/apps/mailconfigure/mailgroupsetup', NULL, NULL, 1, NULL, NULL, 1, CAST(1.66 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (67, 3, 1, N'approval', N'Approval', N'Approval', N'collapse', N'done', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.99 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (68, 23, 1, N'payroll-master', N'Payroll Master', N'Payroll Master', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.68 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (69, 68, 1, N'walletconfigure', N'Wallet Configure', N'Wallet Configure', N'item', NULL, N'/apps/hrms/payroll/walletconfigure', NULL, NULL, 1, NULL, NULL, 1, CAST(1.69 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (70, 68, 1, N'wagecodeconfigure', N'Wage Code Configure', N'Wage Code Configure', N'item', NULL, N'/apps/hrms/payroll/wagecodeconfigure', NULL, NULL, 1, NULL, NULL, 1, CAST(1.70 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (71, 68, 1, N'salarycalculationrule', N'Salary Calc. Rule', N'Salary Calc. Rule', N'item', NULL, N'/apps/hrms/payroll/salarycalculationrule', NULL, NULL, 1, NULL, NULL, 1, CAST(1.71 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (72, 68, 1, N'taxcalculationrule', N'Tax Calc. Rule', N'Tax Calc. Rule', N'item', NULL, N'/apps/hrms/payroll/taxcalculationrule', NULL, NULL, 1, NULL, NULL, 1, CAST(1.72 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (73, 23, 1, N'onboarding', N'On-Boarding', N'On-Boarding', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.73 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (74, 30, 1, N'onboarding', N'On-Boarding', N'On-Boarding', N'item', N'admin_panel_settings', N'/apps/hrms/onboarding/sendinvitation', NULL, NULL, 1, NULL, NULL, 1, CAST(1.37 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (75, 3, 1, N'reports', N'Reports', N'Reports', N'collapse', N'report', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.75 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (76, 75, 1, N'self-reportviwer', N'Self Reports', N'Self Reports', N'item', NULL, N'/apps/reportViewer/selfReportViewer', NULL, NULL, 1, NULL, NULL, 0, CAST(1.76 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (77, 75, 1, N'hr-reportviwer', N'HR Reports', N'HR Reports', N'item', NULL, N'/apps/reportViewer/hrReportViewer', NULL, NULL, 1, NULL, NULL, 0, CAST(1.77 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (78, 3, 1, N'NFA', N'NFA', N'NFA', N'item', N'assignment', N'/apps/nfa/nfaBoards', NULL, NULL, 1, NULL, NULL, 1, CAST(2.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (79, 5, 1, N'admindashboard', N'Admin Dashboard', N'Admin Dashboard', N'item', NULL, N'/apps/dashboards/admin-dashboard', NULL, NULL, 1, NULL, NULL, 1, CAST(1.79 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (80, 5, 1, N'approvaldashboard', N'NFA KPI ', N'NFA KPI', N'item', NULL, N'/apps/dashboards/approval-dashboard', NULL, NULL, 1, NULL, NULL, 0, CAST(1.80 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (81, 3, 1, N'Tutorial', N'Tutorial', N'Tutorial', N'collapse', N'list', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.02 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (82, 81, 1, N'Tutorial List', N'Tutorial List', N'Tutorial List', N'item', NULL, N'/apps/security/tutorial/tutorialList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.82 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (83, 81, 1, N'Add Tutorial', N'Add Tutorial', N'Add Tutorial', N'item', NULL, N'/apps/security/tutorial/addTutorial/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.83 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (84, 202, 1, N'Leave Application List', N'Leave Application List', N'Leave Application List', N'item', NULL, N'/apps/hrms/leave-management/leave-application-list', NULL, NULL, 1, NULL, NULL, 1, CAST(1.84 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (85, 86, 1, N'Custom Grid', N'Custom Grid', N'Custom Grid', N'item', N'admin_panel_settings', N'/apps/shared/CustomGrid', NULL, NULL, 1, NULL, NULL, 1, CAST(1.85 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (86, 142, 1, N'tools', N'Tools', N'Tools', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.82 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (87, 86, 1, N'manual-schedule-list', N'Schedule List', N'Schedule List', N'item', N'schedule', N'/apps/tools/schedule/manual-schedule-list', NULL, NULL, 1, NULL, NULL, 1, CAST(1.83 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (88, 3, 1, N'Finance & Accounts', N'Finance & Accounts', N'Finance & Accounts', N'collapse', N'credit_card', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.88 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (89, 88, 1, N'claims', N'Claims', N'Claims', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.89 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (90, 89, 1, N'expense-claims', N'Expense Claim', N'Expense Claim', N'item', NULL, N'/apps/accounts/payments/expense-payments/expensePaymentBoards', NULL, NULL, 1, NULL, NULL, 1, CAST(1.90 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (91, 88, 1, N'payments', N'Payments', N'Payments', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.91 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (92, 202, 1, N'all-employee-leave-applications', N'HR All Employee Leave', N'HR All Employee Leave', N'item', N'list', N'/apps/hrms/leave-management/all-employee-leave-applications', NULL, NULL, 1, NULL, NULL, 1, CAST(1.92 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (93, 91, 1, N'iou-or-expense-payment', N'Expense', N'Expense', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.93 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (94, 93, 1, N'iou-payment', N'IOU Payment', N'IOU Payment', N'item', NULL, N'/apps/accounts/payments/iou-or-expense-payment/createIOUPayment/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.94 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (95, 93, 1, N'iou-or-expense-payment-list', N'Payment List', N'Payment List', N'item', NULL, N'/apps/accounts/payments/iou-or-expense-payment/iouOrExpensePaymentList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.96 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (96, 93, 1, N'expense-payment', N'Expense Payment', N'Expense Payment', N'item', NULL, N'/apps/accounts/payments/iou-or-expense-payment/createExpensePayment/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.95 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (97, 88, 1, N'Budget', N'Budget', N'Budget', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.97 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (98, 97, 1, N'iou-expense-budget', N'Panel Wise Budget', N'Panel Wise Budget', N'item', NULL, N'/apps/accounts/budget/iouexpensebudget', NULL, NULL, 1, NULL, NULL, 1, CAST(1.98 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (99, 3, 1, N'scm', N'SCM', N'SCM', N'collapse', N'shopping_cart', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.99 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (100, 111, 1, N'supplier', N'Supplier', N'Supplier', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (101, 100, 1, N'basicinfo', N'Basic Info', N'Basic Info', N'item', NULL, N'/apps/scm/supplier/basicInfo', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (102, 99, 1, N'purchase-requisition', N'Purchase Requisition ', N'Purchase Requisition ', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (103, 112, 1, N'itemgroup', N'Item Group', N'Item Group', N'item', NULL, N'/apps/scm/itemgroup', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (104, 112, 1, N'itemsubgroup', N'Item Sub Group', N'Item Sub Group', N'item', NULL, N'/apps/scm/itemsubgroup', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (105, 112, 1, N'item', N'Item', N'Item', N'item', NULL, N'/apps/scm/item', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (106, 111, 1, N'unit', N'Unit', N'Unit', N'item', NULL, N'/apps/scm/unit', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (107, 113, 1, N'warehouse', N'Warehouse', N'Warehouse', N'item', NULL, N'/apps/scm/warehouse', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (108, 111, 1, N'vainfo', N'VatInfo', N'VatInfo', N'item', NULL, N'/apps/scm/vainfo', NULL, NULL, 1, NULL, NULL, 0, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (109, 102, 1, N'approved-prlist', N'Approved PR List', N'Approved PR List', N'item', NULL, N'/apps/scm/po/approvedPRList', NULL, NULL, 1, NULL, NULL, 1, CAST(112.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (110, 115, 1, N'polist', N'PO List', N'PO List', N'item', NULL, N'/apps/scm/po/poList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (111, 142, 1, N'scm-master-settings', N'SCM', N'SCM', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.17 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (112, 111, 1, N'item-settings', N'item Settings', N'item Settings', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (113, 111, 1, N'warehouse-settings', N'Warehouse Settings', N'Warehouse Settings', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (114, 102, 1, N'create-purchase-requisition-and-history', N'PR History', N'PR History', N'item', NULL, N'/apps/scm/purchaseRequisition/listPurchaseRequisition', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (115, 99, 1, N'purchase-order', N'Purchase Order ', N'Purchase Order ', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (117, 99, 1, N'material-receive', N'GRN', N'GRN', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (118, 117, 1, N'material-receive-list', N'GRN List', N'GRN List', N'item', NULL, N'/apps/scm/materialReceive/materialReceiveList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (119, 99, 1, N'lnvoice', N'Invoice', N'Invoice', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.20 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (120, 119, 1, N'create-lnvoice', N'Create Invoice', N'Create Invoice', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (121, 120, 1, N'Regular-Inoivce-list', N'Regular Inovice', N'Regular Inovice', N'item', NULL, N'/apps/scm/invoice/invoice-create/regularInvoiceList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (124, 120, 1, N'Advance-Inoivce-list', N'Advance Inovice', N'Advance Inovice', N'item', NULL, N'/apps/scm/invoice/invoice-create/advanceInvoiceList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (126, 91, 1, N'lnvoice-payment', N'lnvoice Payment', N'lnvoice Payment', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (127, 126, 1, N'Create-Invoice-Payment', N'Create Invoice Payment', N'Create Invoice Payment', N'item', NULL, N'/apps/invoicePayment/invoicePaymentBoards', NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (128, 126, 1, N'Invoice-Payment-List', N'Invoice Payment List', N'Invoice Payment List', N'item', NULL, N'/apps/invoice-payment/invoicePaymentList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (129, 120, 1, N'Created-Inoivce-list', N'Inovice List', N'Inovice List', N'item', NULL, N'/apps/scm/invoice/invoice-create/createdInvoiceList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (130, 99, 1, N'QC', N'QC', N'QC', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (131, 130, 1, N'approved-po-list', N'Create QC', N'Create QC', N'item', NULL, N'/apps/scm/po/approvedPOList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.31 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (132, 130, 1, N'qcList', N'QC List', N'QC List', N'item', NULL, N'/apps/scm/qc/qcList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.31 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (133, 138, 1, N'PO Reports', N'PO Reports', N'PO Reports', N'item', NULL, N'/apps/scm/po/poReportList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (134, 67, 1, N'Document Approval', N'Document Approval', N'Document Approval', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.34 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (135, 134, 1, N'Add Document Approval', N'Add Document Approval', N'Add Document Approval', N'item', NULL, N'/apps/approval/documentApproval/addDocumentApproval/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.35 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (136, 134, 1, N'Document Approval List', N'Document Approval List', N'Document Approval List', N'item', NULL, N'/apps/approval/documentApproval/documentApprovalList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.36 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (137, 45, 1, N'LeaveCategory', N'Leave Category', N'Leave Category', N'item', NULL, N'/apps/hrms/masterdata/leave/leaveCategory', NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (138, 75, 1, N'SCM Reports', N'SCM Reports', N'SCM Reports', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (139, 138, 1, N'QC OR GRN Reports', N'QC OR GRN Reports', N'QC OR GRN Reports', N'item', NULL, N'/apps/scm/allScmReports/allScmReportList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (140, 11, 1, N'access-control-users', N'Access Control Users', N'Access Control Users', N'item', N'account_box', N'/apps/security/access-control-users/all', NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (141, 111, 1, N'assessment-member', N'Assessment-Member', N'Assessment-Member', N'item', NULL, N'/apps/scm/assessmentMember', NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (142, 3, 1, N'master-settings', N'Master Settings', N'Master Settings', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (143, 26, 1, N'jobgrade', N'Job Grade', N'Job Grade', N'item', NULL, N'/apps/hrms/masterdata/jobGrade', NULL, NULL, 1, NULL, NULL, 1, CAST(1.50 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (144, 99, 1, N'material-requisiiton', N'Material Requisiiton', N'Material Requisiiton', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 0, CAST(1.09 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (145, 144, 1, N'approved-material-requisiiton', N'Approved MR List', N'Approved MR List', N'item', NULL, N'/apps/scm/materialRequisition/approvedMRList', NULL, NULL, 1, NULL, NULL, 0, CAST(145.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (146, 144, 1, N'create-material-requisition-and-history', N'Create MR And History', N'Create MR And History', N'item', NULL, N'/apps/scm/materialRequisition/materialRequisitionBoards', NULL, NULL, 1, NULL, NULL, 0, CAST(144.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (147, 130, 1, N'allQCList', N'All QC List', N'All QC List', N'item', NULL, N'/apps/scm/qc/allQCList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.31 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (148, 117, 1, N'allGRNList', N'All GRN List', N'All GRN List', N'item', NULL, N'/apps/scm/materialReceive/allGRNList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (149, 91, 1, N'Taxation Vetting', N'Taxation Vetting', N'Taxation Vetting', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.93 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (150, 149, 1, N'Approved-Invoice-list', N'Approved Invoice List', N'Approved Invoice List', N'item', NULL, N'/apps/taxation-vetting/approvedInvoiceList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.96 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (151, 142, 1, N'accounts-master-settings', N'Finance & Accounts', N'Finance & Accounts', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.27 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (152, 151, 1, N'vtDeduction-new', N'Deduction Source', N'Deduction Source', N'item', NULL, N'/apps/accounts/masterdata/deductionSource', NULL, NULL, 1, NULL, NULL, 1, CAST(1.49 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (153, 149, 1, N'Taxation Vetting List', N'Taxation Vetting List', N'Taxation Vetting List', N'item', NULL, N'/apps/accounts/payments/taxation-vetting/taxation-vetting-list', NULL, NULL, 1, NULL, NULL, 1, CAST(1.99 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (154, 3, 1, N'Micro-Site', N'Micro-Site', N'More Services', N'collapse', N'developer_board', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.01 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (155, 154, 1, N'Categories-And-Services', N'Categories & Services', N'Categories & Services', N'item', NULL, N'/apps/microSite/categoriesAndServices', NULL, NULL, 1, NULL, NULL, 1, CAST(2.02 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (156, 157, 1, N'create-taxation-payment', N'Create Taxation Payment', N'Create Taxation Payment', N'item', NULL, N'/apps/accounts/payments/taxation-payment/createTaxationPayment/new', NULL, NULL, 1, NULL, NULL, 1, CAST(2.03 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (157, 91, 1, N'taxation-payment', N'Taxation Payment', N'Taxation Payment', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.04 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (158, 157, 1, N'taxation-vetting-payment-list', N'Taxation Vetting Payment List', N'Taxation Vetting Payment List', N'item', NULL, N'/apps/accounts/payments/taxation-payment/taxation-vetting-payment-list', NULL, NULL, 1, NULL, NULL, 1, CAST(2.05 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (159, 154, 1, N'Microsite-Approval-List', N'Microsite Approval List', N'Microsite Approval List', N'item', NULL, N'/apps/microSite/microSiteList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.06 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (160, 3, 1, N'common', N'Common', N'Common', N'collapse', N'build_circle', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.07 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (161, 75, 1, N'Accounts and Finance', N'Accounts and Finance', N'Accounts and Finance', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.08 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (162, 161, 1, N'Claim and Payment', N'Claim and Payment', N'Claim and Payment', N'item', N'report', N'/apps/reportViewer/financeAndAccounts/claimAndPayment', NULL, NULL, 1, NULL, NULL, 1, CAST(2.09 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (163, 23, 1, N'employeeDirectory', N'Employee Directory', N'Employee Directory', N'item', N'list', N'/apps/hrms/common/employeeDirectoryList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.30 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (164, 75, 1, N'hrms', N'HRMS', N'HRMS', N'collapse', N'', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (165, 24, 1, N'Attendance-Report', N'Attendance Report', N'Attendance Report', N'item', N'', N'/apps/reportViewer/hrms/attendanceReport', NULL, NULL, 1, NULL, NULL, 1, CAST(1.35 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (166, 23, 1, N'Profile Update', N'Profile Update Status', N'Profile Update Status', N'item', N'list', N'/apps/security/profileUpdate/myProfileUpdateList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (167, 166, 1, N'Pending Profile', N'Pending Profile', N'Pending Profile', N'item', NULL, N'/apps/security/profileUpdate/myProfileUpdateList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (169, 168, 1, N'add-document-approval-template', N'Add Document Approval Template', N'Add Document Approval Template', N'item', NULL, N'/apps/approval/documentApprovalTemplate', NULL, NULL, 1, NULL, NULL, 1, CAST(2.16 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (170, 23, 1, N'Nagad-Payroll', N'Nagad Payroll', N'Nagad Payroll', N'collapse', N'list', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.17 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (171, 170, 1, N'Download PaySlip', N'Download PaySlip', N'Download PaySlip', N'item', NULL, N'/apps/hrms/payroll/payslip/downloadPayslip', NULL, NULL, 1, NULL, NULL, 1, CAST(2.18 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (172, 151, 1, N'cheque-book-entry', N'Cheque Book Entry', N'Cheque Book Entry', N'item', NULL, N'/apps/accounts/masterdata/chequeBookEntry', NULL, NULL, 1, NULL, NULL, 1, CAST(1.50 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (173, 142, 1, N'Document Management', N'Document Management', N'Document Management', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.34 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (174, 173, 1, N'add-document-approval-template', N'Add Template', N'Add Template', N'item', NULL, N'/apps/approval/documentApprovalTemplate', NULL, NULL, 1, NULL, NULL, 1, CAST(1.36 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (175, 23, 1, N'HrSupport', N'HR Support', N'HR Support', N'collapse', N'list', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.18 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (176, 187, 1, N'Exit Clearance', N'Exit Clearance', N'Exit Clearance', N'collapse', N'list', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.20 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (177, 187, 1, N'ExitInterview', N'Exit Interview', N'Exit Interview', N'collapse', N'list', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (178, 176, 1, N'Add Exit Clearance', N'Add Exit Clearance', N'Add Exit Clearance', N'item', N'add_circle', N'/apps/hrms/hrSupport/accessDeactivation/addAccessDeactivation/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (179, 176, 1, N'Exit Clearance List', N'Exit Clearance List', N'Exit Clearance List', N'item', N'list', N'/apps/hrms/hrSupport/accessDeactivation/accessDeactivationList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.19 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (180, 177, 1, N'AddExitInterview', N'Add Exit Interview', N'Add Exit Interview', N'item', N'add_circle', N'/apps/hrms/hrSupport/exitInterview/addExitInterview/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (181, 177, 1, N'ExitInterviewList', N'Exit Interview List', N'Exit Interview List', N'item', N'list', N'/apps/hrms/hrSupport/exitInterview/exitInterviewList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.20 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (182, 176, 1, N'AccessDeactivationDivisionClearenceList', N'DivisionClearence List', N'Division Clearence List', N'item', N'list', N'/apps/hrms/hrSupport/accessDeactivation/divisionClearenceList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.20 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (183, 175, 1, N'Letter & Certificate', N'Letter & Certificate', N'Letter & Certificate', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.08 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (184, 183, 1, N'Request Letter & Certificate', N'Request Letter & Certificate', N'Request Letter & Certificate', N'item', NULL, N'/apps/approval/hrDocumentApproval/addHRDocumentApproval/new', NULL, NULL, 1, NULL, NULL, 1, CAST(2.09 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (185, 183, 1, N'Letter & Certificate Approval', N'Letter & Certificate Approval', N'Letter & Certificate Approval', N'item', NULL, N'/apps/approval/hrDocumentApproval/hrDocumentApprovalList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (186, 26, 1, N'DivisionDepartmentSetupList', N'Division/Department Head', N'Division/Department Head', N'item', NULL, N'/apps/hrms/masterdata/divisionDepartmentSetup', NULL, NULL, 1, NULL, NULL, 1, CAST(1.51 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (187, 175, 1, N'EmployeeExit', N'Employee Exit', N'Employee Exit', N'collapse', N'list', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (188, 151, 1, N'bank-entry', N'Bank Entry', N'Bank Entry', N'item', NULL, N'/apps/accounts/masterdata/bankEntry', NULL, NULL, 1, NULL, NULL, 1, CAST(1.50 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (189, 99, 1, N'SCC', N'SCC', N'SCC', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.15 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (190, 189, 1, N'approved-po-list', N'Create SCC', N'Create SCC', N'item', NULL, N'/apps/scm/scc/approvedPOList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.21 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (191, 189, 1, N'sccList', N'SCC List', N'SCC List', N'item', NULL, N'/apps/scm/scc/sccList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.22 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (192, 161, 1, N'SOA', N'Statement of Affairs', N'Statement of Affairs', N'item', NULL, N'/apps/reportViewer/SOA', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (193, 26, 1, N'exit-interview-setup', N'Exit Interview Setup', N'Exit Interview Setup', N'item', NULL, N'/apps/hrms/masterdata/exitInterviewSetup', NULL, NULL, 1, NULL, NULL, 1, CAST(1.51 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (194, 120, 1, N'Regular-Inoivce-scc-list', N'Regular Inovice Scc', N'Regular Inovice Scc', N'item', NULL, N'/apps/scm/invoice/invoice-create/regularInvoiceSccList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.15 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (195, 161, 1, N'all-vendor-payment', N'All Vendor Payment', N'All Vendor Payment', N'item', N'report', N'/apps/reportViewer/financeAndAccounts/AllVendorPayment', NULL, NULL, 1, NULL, NULL, 1, CAST(2.09 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (196, 53, 1, N'Annual Leave Encashment', N'Annual Leave Encashment', N'Annual Leave Encashment', N'collapse', N'null', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.34 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (197, 196, 1, N'Create Encashment Window', N'Create Encashment Window', N'Create Encashment Window', N'item', NULL, N'/apps/hrms/leave-encashment/leave-encashment-window/createLeaveEncashmentWindow/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (198, 196, 1, N'ApplyLeaveEncashment', N'Apply Leave Encashment', N'Apply Leave Encashment', N'item', NULL, N'/apps/hrms/leave-encashment/leave-encashment-application/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.30 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (200, 196, 1, N'leave-encashment-history', N'Leave Encashment History', N'Leave Encashment History', N'item', NULL, N'/apps/hrms/leave-management/leave-encashment/annual-leave-encashment-history', NULL, NULL, 1, NULL, NULL, 1, CAST(1.20 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (201, 196, 1, N'leave-encashment-approval-list', N'Leave Encashment Approval List', N'Leave Encashment Approval List', N'item', NULL, N'/apps/hrms/leave-encashment/leave-encashment-approval/encashmentApprovalList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.40 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (202, 53, 1, N'leave-application', N'Leave Application', N'Leave Application', N'collapse', N'how_to_reg', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.53 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (203, 170, 1, N'Document-Upload-add', N'Add Document Upload', N'Add Document Upload', N'item', N'add_circle', N'/apps/hrms/payroll/documentUpload/addDocumentUpload/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (204, 170, 1, N'Document-Upload-list', N'Document Upload List', N'Document Upload List', N'item', N'list', N'/apps/hrms/payroll/documentUpload/documentUploadList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (205, 170, 1, N'hod-document-upload-list', N'HOD Document Upload List ', N'HOD Document Upload List', N'item', NULL, N'/apps/hrms/payroll/documentUpload/listDocumentUploadForHOD', NULL, NULL, 1, NULL, NULL, 1, CAST(3.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (206, 3099, 1, N'add-person', N'Add New Person with emp', N'Add New Person with emp', N'item', N'person_add', N'/apps/hrms/employee/addEmployeeWithPerson/new', NULL, NULL, 1, NULL, NULL, 0, CAST(1.21 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (207, 3, 1, N'business-support', N'Business Support', N'Business Support', N'collapse', N'shopping_cart', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.30 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (208, 207, 1, N'support-request', N'Support Request', N'Support Request', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.31 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (209, 222, 1, N'Create-Support-Request', N'Create Support Request', N'Create Support Request', N'item', N'add_circle', N'/apps/adminSupport/requestSupport/addSupportRequest/Self/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.32 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (210, 222, 1, N'Support-Request-List', N'Support Request List', N'Support Request List', N'item', N'list', N'/apps/adminSupport/requestSupport/supportRequestList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.33 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (211, 55, 1, N'employee-panel-map-config', N'Emp. Panel Map Config', N'Emp. Panel Map Config', N'item', NULL, N'/apps/approval/employeepanelmapConfig', NULL, NULL, 1, NULL, NULL, 1, CAST(2.01 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (212, 138, 1, N'invoice-reports', N'Invoice Reports', N'Invoice Reports', N'item', NULL, N'/apps/scm/invoice/invoice-create/invoiceReportList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.15 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (213, 102, 1, N'Create PR', N'Create PR', N'Create PR', N'item', NULL, N'/apps/scm/purchaseRequisition/createPurchaseRequisition/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (214, 67, 1, N'DOA', N'DOA', N'DOA', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (215, 214, 1, N'Create DOA', N'Create DOA', N'Create DOA', N'item', NULL, N'/apps/approval/doa/createDoa/emp/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (216, 214, 1, N'DOA List', N'DOA List', N'DOA List', N'item', NULL, N'/apps/approval/doa/doa-list/emp', NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (217, 214, 1, N'Create DOA(HR)', N'Create DOA(HR)', N'Create DOA(HR)', N'item', NULL, N'/apps/approval/doa/createDoa/hr/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (218, 214, 1, N'DOA List(HR)', N'DOA List(HR)', N'DOA List(HR)', N'item', NULL, N'/apps/approval/doa/doa-list/hr', NULL, NULL, 1, NULL, NULL, 1, CAST(1.14 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (219, 24, 1, N'Remote-Attendance-list', N'Remote Attendance List', N'Remote Attendance List', N'item', NULL, N'/apps/hrms/attendance/remoteAttendanceList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.34 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (220, 223, 1, N'Employee-Support-Request-List', N'Support Request List', N'Support Request List', N'item', N'list', N'/apps/adminSupport/requestSupport/employeeSupportRequestList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.35 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (221, 223, 1, N'Create-Support-Request-Employee', N'Create Support Request', N'Create Support Request', N'item', N'add_circle', N'/apps/adminSupport/requestSupport/addSupportRequest/Employee/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.32 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (222, 208, 1, N'Self', N'Self', N'Self', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.15 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (223, 208, 1, N'Employee', N'Employee', N'Employee', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.15 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (225, 86, 1, N'system-configuration', N'System Configuration', N'System Configuration', N'item', NULL, N'/apps/security/masterdata/systemConfiguration/createSystemConfiguration/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.50 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (226, 24, 1, N'Attendance-Adherence-Report', N'Attendance Adherence Report', N'Attendance Adherence Report', N'item', N'', N'/apps/reportViewer/hrms/attendanceAdherenceReport', NULL, NULL, 1, NULL, NULL, 1, CAST(1.36 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (227, 53, 1, N'notify-unauthorized-leaves', N'Notify Unauthorized Leaves', N'Notify Unauthorized Leaves', N'item', N'list', N'/apps/hrms/leave-management/notify-unauthorized-leaves', NULL, NULL, 1, NULL, NULL, 1, CAST(2.50 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (228, 189, 1, N'po-list-for-scc', N'PO List For SCC', N'PO List For SCC', N'item', NULL, N'/apps/scm/scc/poListForSCC', NULL, NULL, 1, NULL, NULL, 1, CAST(1.22 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (229, 202, 1, N'adjust-employees-leaves', N'Adjust Employee''s Leaves', N'Adjust Employee''s Leaves', N'item', NULL, N'/apps/hrms/leave-management/leave-application/hr/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.54 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (230, 67, 1, N'approval-Pannel-Window', N'Approval Pannel Window', N'Approval Pannel Window', N'collapse', N'admin_panel_settings', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.34 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (231, 230, 1, N'approval-pannel-window-history', N'Approval Pannel Window History', N'Approval Pannel Window History', N'item', NULL, N'/apps/approval/approvalPanelWindow/approval-pannel-window-history', NULL, NULL, 1, NULL, NULL, 1, CAST(1.03 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (232, 230, 1, N'create-Approval-Pannel-Window', N'Create Approval Pannel Window', N'Create Approval Pannel Window', N'item', NULL, N'/apps/approval/approvalPanelWindow/createApprovalPanelWindow/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.02 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (233, 91, 1, N'wallet', N'Wallet', N'Wallet', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.91 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (234, 233, 1, N'wallet-list', N'Wallet List', N'Wallet List', N'item', NULL, N'/apps/accounts/payments/wallet', NULL, NULL, 1, NULL, NULL, 1, CAST(2.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (235, 89, 1, N'disbursement-petty-cash-list', N'Disbursement Petty Cash List', N'Disbursement Petty Cash List', N'item', NULL, N'/apps/accounts/payments/expense-payments/disbursementPettyCashList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.90 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (236, 89, 1, N'resubmit-advance-petty-cash-list', N'Resubmit Advance Petty Cash List', N'Resubmit Advance Petty Cash List', N'item', NULL, N'/apps/accounts/payments/expense-payments/resubmitPettyCashList', NULL, NULL, 1, NULL, NULL, 1, CAST(1.90 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (237, 88, 1, N'reimbursement', N'Reimbursement', N'Reimbursement', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(1.90 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (238, 237, 1, N'create-PettyCash-Reimburse', N'Create Petty Cash Reimburse', N'Create Petty Cash Reimburse', N'item', NULL, N'/apps/accounts/payments/pettyCash-reimburse/createPettyCashReimburse/new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.12 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (239, 237, 1, N'pettyCash-Reimburse-List', N'Petty Cash Reimburse List', N'Petty Cash Reimburse List', N'item', NULL, N'/apps/accounts/payments/pettyCash-reimburse/pettyCash-reimburse-list', NULL, NULL, 1, NULL, NULL, 1, CAST(1.13 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (240, 91, 1, N'Petty Cash Payment', N'Petty Cash Payment', N'Petty Cash Payment', N'collapse', NULL, NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(3.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (241, 240, 1, N'approved-reimburse-list', N'Approved Reimburse List', N'Approved Reimburse List', N'item', NULL, N'/apps/accounts/payments/pettyCashApproved-reimburse/pettyCashApproved-reimburse-list', NULL, NULL, 1, NULL, NULL, 1, CAST(1.96 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (242, 240, 1, N'petty-cash-payment-list', N'Petty Cash Payment List', N'Petty Cash Payment List', N'item', NULL, N'/apps/accounts/payments/petty-cash/petty-cash-payment-list', NULL, NULL, 1, NULL, NULL, 1, CAST(1.99 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (243, 189, 1, N'all-scc-list', N'All SCC List', N'All SCC List', N'item', NULL, N'/apps/scm/scc/allSCCList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.23 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (245, 161, 1, N'expense-report', N'Expense Report', N'Expense Report', N'item', NULL, N'/apps/reportViewer/financeAndAccounts/expenseReport', NULL, NULL, 1, NULL, NULL, 1, CAST(2.09 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (279, 202, 1, N'Leave Application List New', N'Leave Application List New', N'Leave Application List New', N'item', NULL, N'/apps/hrms/leave-management/leave-application-list-new', NULL, NULL, 1, NULL, NULL, 1, CAST(1.85 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (500, 3, 1, N'common-interface', N'Common Interface', N'Common Interface', N'collapse', N'c', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(3.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (501, 500, 1, N'menu-interface', N'menu-interface', N'Menu', N'item', N'menu', N'/apps/comoninterface/interfaceType/listWithDialog/list/menu', NULL, NULL, 1, NULL, NULL, 1, CAST(3.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (504, 500, 1234, N'1234', N'Common', N'Common', N'item', N'test', N'/url/abce', NULL, NULL, 0, NULL, NULL, 1, CAST(6.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (505, 500, 1, N'demo', N'Demo', N'DEMO', N'item', N'list', N'/apps/demo/all', NULL, NULL, 0, NULL, NULL, 1, CAST(6.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (506, 500, 1, N'demo-item', N'Demo-item', N'DEMO-ITEM', N'item', N'list', N'/apps/demo-item/all', NULL, NULL, 0, NULL, NULL, 1, CAST(7.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (507, 500, 1, N'demo-item-creation', N'Demo Item Creation', N'DEMO-ITEM-CREATION', N'item', N'add_circle', N'/apps/demo-item-creation/new', NULL, NULL, 0, NULL, NULL, 1, CAST(7.00 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (508, 5, 1, N'organogram', N'Organogram', N'Organogram', N'item', NULL, N'/apps/dashboards/organogram', NULL, NULL, 1, NULL, NULL, 1, CAST(1.80 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1001, 3, 1, N'kpi-tools', N'KPI Tools', N'KPI Tools', N'collapse', N'chart_timelines', NULL, NULL, NULL, 1, NULL, NULL, 1, CAST(2.07 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1002, 1001, 1, N'kpi-settings', N'KPI Settings', N'Settings', N'collapse', NULL, N'', NULL, NULL, 1, NULL, NULL, 1, CAST(2.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1003, 1002, 1, N'kpi-period-types', N'KPI Period Types', N'Period Types', N'item', NULL, N'/apps/kpi-tools/periodTypes', NULL, NULL, 1, NULL, NULL, 1, CAST(2.07 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1006, 1001, 1, N'kpi-windows', N'KPI WIndows', N'KPI Windows', N'item', NULL, N'/apps/kpi-tools/windows/windowList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.08 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1007, 1001, 1, N'kpi-window-employees', N'KPI Evaluation Employees', N'KPI Evaluation Employees', N'item', NULL, N'/apps/kpi-tools/windows/windowEvaluationEmployeeList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.09 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1008, 1001, 1, N'kpi-window-employees', N'KPI Window Employees', N'KPI Window Employees', N'item', NULL, N'/apps/kpi-tools/windows/windowEmployeeList', NULL, NULL, 1, NULL, NULL, 1, CAST(2.09 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1009, 1001, 1, N'organization-hierarchy', N'Organization Hierarchy', N'Organization Hierarchy', N'item', NULL, N'/apps/kpi-tools/organizationHierarchy', NULL, NULL, 1, NULL, NULL, 1, CAST(2.10 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1011, 1002, 1, N'kpi-master-settings', N'KPI Master Settings', N'KPI Master Settings', N'item', NULL, N'/apps/kpi-tools/masterSettings', NULL, NULL, 1, NULL, NULL, 1, CAST(2.11 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1012, 1001, 1, N'kpi-statistics', N'KPI Statistics', N'KPI Statistics', N'item', NULL, N'/apps/kpi-tools/kpiStatics', NULL, NULL, 1, NULL, NULL, 1, CAST(2.07 AS Decimal(18, 2)))
GO
INSERT [dbo].[Menu] ([MenuID], [ParentID], [ApplicationID], [ID], [Title], [Translate], [Type], [Icon], [Url], [Badge], [Target], [Exact], [Auth], [Parameters], [IsVisible], [SequenceNo]) VALUES (1013, 1001, 1, N'kpi-profile', N'KPI Profile', N'KPI Profile', N'item', NULL, N'/apps/kpi-tools/profile', NULL, NULL, 1, NULL, NULL, 1, CAST(2.07 AS Decimal(18, 2)))
GO
