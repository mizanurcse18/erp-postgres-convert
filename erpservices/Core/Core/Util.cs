
using Core.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

namespace Core
{
    public static class Util
    {
        public static readonly string TokenHash = "00x567LKD";
        public static readonly List<string> ImageExtensionsIncluded = new List<string> {"jpg",
            "png", "jpeg", "pdf", "xls", "xlsx", "csv", "doc", "docx"};
        public static readonly List<string> AttachmentExtensionsIncluded = new List<string> {"jpg",
            "png", "jpeg", "pdf", "xls", "xlsx", "csv", "doc", "docx"};
        public static readonly List<string> AttachmentExtensionsForDocumentUpload = new List<string> {"jpg",
            "png", "jpeg", "pdf"};

        public const string NewIDString = "****<< NEW >>****";
        public const string DateFormat = "yyyy-MMM-dd";
        public const string TimeFormat = "hh:mm:ss tt";
        public const string TimeSpanFormat = @"hh\:mm\:ss\.fff";
        public const string EntryTimeFormat = "hh:mm tt";
        public const string DateTimeFormat = "yyyy-MM-dd hh:mm:ss.fff";
        public static string SysDateFormat = "MM/dd/yyyy";
        public static string SysDateTimeFormat = "MM/dd/yyyy hh:mm tt";
        public static string AutoLeaveAppDesc = "Auto Generated Leave Approval Process";
        public static string AutoLeaveEncashmentAppDesc = "Auto Generated Leave Encashment Approval Process";
        public static string AutoNFAAppDesc = "Auto Generated NFA Approval Process";
        public static string AutoEADAppDesc = "Auto Generated Employee Access Deactivation Approval Process";
        public static string AutoExAuditAppDesc = "Auto Generated External Audit Approval Process";

        public static string AutoMicroSiteAppDesc = "Auto Generated MicroSite Approval Process";
        public static string AutoExpenseAppDesc = "Auto Generated Expense Claim Approval Process";
        public static string AutoIOUAppDesc = "Auto Generated IOU Claim Approval Process";
        public static string AutoLeaveAppTitle = "Approval Process Type = LEAVE_APPLICATION,";
        public static string AutoLeaveEncashmentAppTitle = "Leave Encashment Approval,";
        public static string NFAApprovalTitle = "Note For Approval,";
        public static string EADApprovalTitle = "Access Deactivation Approval,";
        public static string EXAuditApprovalTitle = "External Audit Approval,";

        public static string EmployeeProfileApprovalTitle = "Employee Profile Approval,";
        public static string PRApprovalTitle = "Purchase Requisition";
        public static string MicroSiteApprovalTitle = "MicroSite";
        public static string TaxationVettingApprovalTitle = "Taxation Vetting";
        public static string TaxationVettingPaymentApprovalTitle = "Taxation Vetting Payment";
        public static string MRApprovalTitle = "Material Requisition";
        public static string AutoPRAppDesc = "Auto Generated Purchase Requisition Approval Process";
        public static string AutoTaxationVettingAppDesc = "Auto Generated Taxation Vetting Approval Process";
        public static string AutoTaxationVettingpaymentDesc = "Auto Generated Taxation Vetting Payment Approval Process";
        public static string AutoMRAppDesc = "Auto Generated Material Requisition Approval Process";

        public static string POApprovalTitle = "Purchase Order";
        public static string AutoPOAppDesc = "Auto Generated Purchase Order Approval Process";

        public static string GRNApprovalTitle = "GRN";
        public static string AutoGRNAppDesc = "Auto Generated GRN Approval Process";

        public static string QCApprovalTitle = "QC";
        public static string AutoQCAppDesc = "Auto Generated QC Approval Process";

        public static string SCCApprovalTitle = "SCC";
        public static string AutoSCCAppDesc = "Auto Generated SCC Approval Process";

        public static string DocumentApprovalTitle = "Document Approval";
        public static string AutoDocumentApprovalAppDesc = "Auto Generated Document Approval Process";

        public static string DivisionClearenceTitle = "Division Clearence Approval";
        public static string AutoDivisionClearenceAppDesc = "Auto Generated Division Clearence Approval Process";

        public static string ExitInterviewTitle = "Exit Interview Approval";
        public static string AutoExitInterviewAppDesc = "Auto Exit Interview Approval Process";

        public static string InvoiceApprovalTitle = "Invoice";
        public static string AutoInvoiceAppDesc = "Auto Generated Invoice Approval Process";

        public static string ExpenseApprovalTitle = "Expense Claim";
        public static string ExpenseSattlementApprovalTitle = "Expense Claim Settlement";
        public static string IOUSattlementApprovalTitle = "IOU Claim Settlement";
        public static string IOUApprovalTitle = "IOU Claim";
        public static string AutoExpenseSattlementDesc = "Auto Generated Expense Claim Settlement Approval Process";
        public static string AutoIOUSattlementDesc = "Auto Generated IOU Claim Settlement Approval Process";
        public static string InvoicePaymentApprovalTitle = "Invoice Payment";
        public static string AutoInvoicePaymentDesc = "Auto Generated Invoice Payment Approval Process";
        public static string AutoPersonApprovalDesc = "Auto Generated Employee Profile Approval Process";

        public static string DocumentUploadApprovalTitle = "Document Upload";
        public static string AutoDocumentUploadAppDesc = "Auto Generated Document Upload Approval Process";

        public static string AdminSupportRequestApprovalTitle = "Admin Support Request";
        public static string AutoAdminSupportRequestAppDesc = "Auto Generated Admin Support Request Approval Process";

        //Petty Cash
        public static string PettyCashAdvanceApprovalTitle = "Petty Cash Advance Claim";
        public static string AutoPettyCashAdvanceAppDesc = "Auto Generated Petty Cash Advance Approval Process";
        public static string PettyCashExpenseApprovalTitle = "Petty Cash Expense Claim";
        public static string AutoPettyCashExpenseAppDesc = "Auto Generated Petty Cash Approval Process";

        public static string PettyCashReimburseApprovalTitle = "Petty Cash Reimburse";
        public static string AutoPettyCashReimburseAppDesc = "Auto Generated Petty Cash Reimburse Approval Process";


        public static string PettyCashPaymentApprovalTitle = "Petty Cash Payment";
        public static string AutoPettyCashPaymentAppDesc = "Auto Generated Petty Cash Payment Approval Process";

        public static string ITSupportRequisitionApprovalTitle = "IT Support Requisition";
        public static string ITSupportRequisitionAppDesc = "Auto Generated IT Support Requisition Approval Process";

        //End Petty Cash

        //Encryption Credential
        public static string passPhrase = "n@G@D 3Rp PasSpHraS3";
        public static string saltValue = "n@G@D  3Rp sa1TvA1uE";
        public static string initVector = "6e40674064335270";

        // Leave Day Status 
        public static string Fullday = "Full day";
        public static string Halfday = "Half day";
        public static string FirstHalfday = "First Half day";
        public static string SecondHalfday = "Second Half day";
        public static string Conflict = "Overlaps with existing leave";
        public static string OnlyHalfday = "Only Half day";
        public static string NonWorkingDay = "Non working day";
        public static string SecretPassword = "NagadNotBaki";
        //public static string[] PermittedEmployees = { "TW200413", "TW190182", "TW200376", "TW200442", "TW200439", "TU001" };
        public static string[] TokenIDs = { "IU001" };
        public static string integrationHashToken = "superadmin@007";
        public static string FaildToMapData = "Failed to map data!";
        public static string OTPKey = "NZAGOQDEJ5KFAR3FJZSVEYKUEFHW4==="; //n@g@dOTPGeNeRaT!On
        public static string SaveSuccessfullyMessage = "Information Submitted Successfully!";
        public enum LoginPolicyReason
        {
            Success = 0,
            InvalidPassword = 216,
            OverSystemDays = 217,
            ManuallyLocked = 218

        }

        public enum Integrations
        {
            superadmin
        }

        //public static string[] PermittedEmployees = { "Md.Nazmul Hasan Nazim", "Kayser Ahmed", "Md.Mizanur Rahman Munna", " Md.Asadullah Sarker"
        //, "Ziaul Kabir Fahad" };


        public enum UserRole
        {
            MC = 20,
            BP,
            Uddoktas,
            TMRs,
            DSS,
            DM,
            BDO,
            DSO,
            DH,
            TO,
            AM,
            RSM,
            CH,
            HQ,
            TM,
            PRO
        }

        public enum Gender
        {
            Male = 1,
            Female = 2,
        }

        public enum Status
        {
            success,
            error,
            upgrade_app
        }

        public enum MessageType
        {
            success,
            error,
            loginPasswordFailed,
            upgrade_app,
            changePassword,
            missMatchPassword,
            missMatchOldPassword,
            reuseOldPassword,
            successChangedPassword,
            savedSuccesMsg,
            errorMsgForAttendanceBoth,
            errorMsgForAttendanceStart,
            errorMsgForAttendanceEnd,
            errorGlobal,
            errorMsgForAttendanceNoStart,
            errorMsgForAttendanceNoEnd,
            errorMsgUddoktaEmpty,
            errorMsgNotYourDH,
            errorMeetingStart,
            errorMeetingEnd,
            successUddoktaNumber
        }

        public static string GetMessage(Enum messageType)
        {
            string msg = "";
            switch (messageType)
            {
                case MessageType.success:
                    msg = "লগইন সফলভাবে সম্পন্ন হয়েছে";
                    break;
                case MessageType.loginPasswordFailed:
                    msg = "পাসওয়ার্ড ভুল হয়েছে, সঠিক পাসওয়ার্ড ব্যবহার করে পুনরায় চেষ্টা করুন";
                    break;
                case MessageType.upgrade_app:
                    msg = "এ্যাপ স্টোর থেকে আপনার এ্যাপটি আপডেট করে নিন";
                    break;
                case MessageType.missMatchPassword:
                    msg = "নতুন পাসওয়ার্ড এবং কনফার্ম পাসওয়ার্ড মেলেনি";
                    break;
                case MessageType.missMatchOldPassword:
                    msg = "পুরানো পাসওয়ার্ড মেলেনি";
                    break;
                case MessageType.reuseOldPassword:
                    msg = "আপনি পুরানো পাসওয়ার্ড পুনরায় ব্যবহার করতে পারবেন না";
                    break;
                case MessageType.successChangedPassword:
                    msg = "পাসওয়ার্ড সফলভাবে পরিবর্তিত হয়েছে";
                    break;
                case MessageType.savedSuccesMsg:
                    msg = "ধন্যবাদ। আপনার প্রদত্ত তথ্য গুলো সফলভাবে সংরক্ষিত হয়েছে";
                    break;
                case MessageType.errorMsgForAttendanceBoth:
                    msg = "আপনি এই দিনের জন্য শুরু এবং শেষের সময় উভয়ই প্রবেশ করেছেন";
                    break;
                case MessageType.errorMsgForAttendanceStart:
                    msg = "আপনি এই দিনের জন্য ইতিমধ্যে শুরুর সময় প্রবেশ করেছেন";
                    break;
                case MessageType.errorMsgForAttendanceEnd:
                    msg = "আপনি এই দিনটির জন্য ইতিমধ্যে শেষের সময়টি প্রবেশ করেছেন";
                    break;
                case MessageType.errorGlobal:
                    msg = "দুঃখিত, তথ্য গুলো সংরক্ষণ করা যায়নি, পুনরায় চেষ্টা করুন।";
                    break;
                case MessageType.errorMsgForAttendanceNoStart:
                    msg = "আপনি এই দিনের জন্য শুরুর সময় প্রবেশ করেননি৷";
                    break;
                case MessageType.errorMsgForAttendanceNoEnd:
                    msg = "আপনি এই দিনের জন্য শেষের সময় প্রবেশ করেননি৷";
                    break;
                case MessageType.successUddoktaNumber:
                    msg = "উদ্যোক্তা নাম্বারটি সঠিক";
                    break;
                case MessageType.errorMsgUddoktaEmpty:
                    msg = "উদ্যোক্তা নাম্বারটি সঠিক নয়";
                    break;
                case MessageType.errorMsgNotYourDH:
                    msg = "এই উদ্যোক্তা আপনার ডিস্ট্রিবিউশন হাউজের নয়";
                    break;
                case MessageType.errorMeetingStart:
                    msg = "আপনি ইতিমধ্যেই মিটিং শুরু করেছেন, অনুগ্রহ করে প্রথমে মিটিং শেষ করুন এবং তারপর আবার মিটিং শুরু করার চেষ্টা করুন৷";
                    break;
                case MessageType.errorMeetingEnd:
                    msg = "আপনি আজ কোনো মিটিং শুরু করেননি";
                    break;

            }

            return msg;
        }

        public enum AuditType
        {
            Uddokta,
            DH,
            UddoktaDso,
            UddoktaDss,
            UddoktaDm,
            UddoktaHq,
            UddoktaCh,
            UddoktaRsm,
            UddoktaBdoSales,
            UddoktaBdoRevisit,
            UddoktaBp,
            DsoMc,
            UddoktaStrAudit,
            LocalMediaAudit
        }

        public enum LocationTrackingType
        {
            tracking_data,
            visit_data,
            tmo_activity_data,
            daily_activity_data,
            day_start_data,
            day_end_data
        }

        public static int GetEnumValue(Type enumType, string value)
        {
            switch (value)
            {
                case "Integer":
                    value = "Int32";
                    break;
                case "Float":
                    value = "double";
                    break;
            }

            var i = (int)Enum.Parse(enumType, value);
            return i;
        }
        [Flags]
        public enum ExportType
        {
            PDF = 1,
            Excel = 2,
            Word = 3,
            CSV = 4,
            EXCELOPENXML = 5
        }
        public static List<KeyValuePair<int, string>> GetAllMonths()
        {
            var monthList = new List<KeyValuePair<int, string>>();

            for (var i = 1; i <= 12; i++)
            {
                if (DateTimeFormatInfo.CurrentInfo != null)
                    monthList.Add(new KeyValuePair<int, string>(i, DateTimeFormatInfo.CurrentInfo.GetMonthName(i)));
            }

            return monthList;
        }

        public static string BuildXmlString(string xmlRootName, IEnumerable<string> values)
        {
            var xmlString = new StringBuilder();

            xmlString.AppendFormat("<{0}>", xmlRootName);
            foreach (var str in values)
            {
                xmlString.AppendFormat("<value>{0}</value>", str);
            }

            xmlString.AppendFormat("</{0}>", xmlRootName);
            return xmlString.ToString();
        }

        //Call Util.ListFrom<TypeOfEnum>()
        public static IList<dynamic> ListFrom<T>()
        {
            var list = new List<dynamic>();
            var enumType = typeof(T);

            foreach (var o in Enum.GetValues(enumType))
            {
                list.Add(new
                {
                    Name = Enum.GetName(enumType, o),
                    Value = o
                });
            }

            return list;
        }

        public static string ToEnumString(Enum code)
        {
            var a = Enum.GetName(code.GetType(), code);
            return a;
        }

        public enum ApprovalStatus
        {
            Pending = 22,
            Approved = 23,
            Rejected = 24,
            Initiated = 25,
        }
        public enum PRISM_APPROVAL_STATUS
        {
            Submitted = 1,
            Requested,
            Approved,
            Reject,
            Return,
            Initiated,
            NotYetRequested
        }
        public enum APPROVAL_TYPES
        {
            DHFF = 1,
            DSO_Wallet_Tagging,
            DH_Expenditure,
            Route,
            DSO_TARGET_UPDATE
        }
        public enum LeaveCategory
        {
            Annual = 33,
            Pilgrim = 68,
            Compensatory = 69
        }
        public enum PaymentType
        {
            EMoney = 2,
            Bank = 1
        }
        public enum Desination
        {
            HeadOfSourcing = 685
        }
        public enum PaymentTerms
        {
            Advance = 114
        }
        public enum PersonAddressType
        {
            Present = 26,
            Permanent = 27
        }
        public enum PersonType
        {
            Employee = 9,
            Applicant = 10,
            Onboarding = 47
        }
        public enum DayOfWeek
        {
            Sunday = 0,
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6,
        }

        public enum LeaveEncashmentStatus
        {
            Initiated = 177,
            Ongoing = 178,
            Expired = 179,
            Closed = 180,
        }
        public enum InvoiceDocumentType
        {
            VendorInvoice = 185,
            MusokChalan = 186,
            Other = 187
        }
        public enum DHFF_Upload_File_Category
        {
            Photo = 1,
            CV = 2,
            NID = 3,
            AppoinmentLetter = 4,
        }
        public enum ApprovalType
        {
            LeaveApplication = 1,
            NFA,
            ExpenseClaim,
            IOUClaim,
            IOUPayment,
            ExpensePayment,
            PR,
            PO,
            GRN,
            Invoice,
            InvoicePayment,
            QC,
            DocumentApproval,
            MR,
            TaxationVetting,
            MicroSite,
            TaxationVettingPayment,
            EmployeeProfileApproval,
            DivisionClearance,
            AccessDeactivation,
            ExitInterview,
            HRSupportDocApproval,
            SCC,
            LeaveEncashmentApplication,
            EmployeeeDocumentUpload,
            AdminSupportRequest,
            EmailNotification,
            PettyCashExpenseClaim,
            PettyCashAdvanceClaim,
            PettyCashAdvanceResubmitClaim,
            PettyCashReimburseClaim,
            PettyCashPaymentClaim,
            AdminSupportSettlement,
            PettyCashDisbursement,
            ResubmitFromNotification,
            CustodianThresold,
            SupportRequisition,
            ExternalAudit
        }

        public enum ExternalAuditConfig
        {
            AuditThresold = 30
        }
        public enum HRSupportCategoryType
        {
            PaySlip = 162,
            TaxCard = 163,
            IncentivePayslip = 206,
            FestivalBonusPayslip = 207
        }

        public enum SCCPaymentType
        {
            FullPayment = 172,
            PartialPayment = 173,
            DeductionOrPenalty = 174
        }
        public enum DOAType
        {
            Replace = 208,
            Proxy = 209
        }
        public enum DOAStatus
        {
            Active = 210,
            InActive = 211,
            Expired = 212
        }
        public enum SCCLifeCycle
        {
            Continue = 1,
            Discontinue = 2,
            FinalBill = 3
        }
        public enum BudgetRemarksCategory
        {
            ApprovedBudget = 182,
            StrategicBudget = 183,
            ManagementSpecialBudget = 184
        }

        public enum AdminSupportCategory
        {
            ConsumbleGoods = 195,
            Vehicle = 196,
            Facilities = 197,
            RenovationOrMaintenance = 219

        }
        public enum SupportRequisitionCategory
        {
            AccessRequest = 232,
            AccessoriesRequisition = 233,
            AssetRequisition = 234
        }
        public enum MailGroupSetup
        {
            UserCreationEmailConfiguration = 1,
            LeaveApprovalMail,
            LeaveApplicationMailForBackupEmployee,
            OnboardingMail,
            NFAInitiatedMail,
            FinalNFAAPprovalStatusToInitiator,
            NFAForwardedOrRejectionMail,
            NFAForwardFeedbackReceieve,
            ForgotPasswordRequest,
            NFARemoveMail,
            RemoteAttendacneCheckInOutRequest,
            RemoteAttendacneAcceptOrRejectMail,
            IOUClaimInitiatedMail,
            FinalIOUCliamApprovalStatusToInitiator,
            IOUClaimForwardedOrRejectionMail,
            IOUClaimForwardFeedbackReceieve,
            ExpenseClaimInitiatedMail,
            FinalExpenseCliamApprovalStatusToInitiator,
            ExpenseClaimForwardedOrRejectionMail,
            ExpenseClaimForwardFeedbackReceieve,
            ExpensePaymentInitiatedMail,
            FinalExpensePaymentApprovalStatusToInitiator,
            ExpensePaymentForwardedOrRejectionMail,
            ExpensePaymentForwardFeedbackReceieve,
            IOUPaymentInitiatedMail,
            FinalIOUPaymentApprovalStatusToInitiator,
            IOUPaymentForwardedOrRejectionMail,
            IOUPaymentForwardFeedbackReceieve,
            PRInitiatedMail,
            FinalPRApprovalStatusToInitiator,
            PRForwardedOrRejectionMail,
            PRForwardFeedbackReceieve,
            POInitiatedMail,
            FinalPOApprovalStatusToInitiator,
            POForwardedOrRejectionMail,
            POForwardFeedbackReceieve,
            SendMailToSCMGroupAfterPRApproved,

            GRNInitiatedMail,
            FinalGRNApprovalStatusToInitiator,
            GRNForwardedOrRejectionMail,
            GRNForwardFeedbackReceieve,

            InvoiceInitiatedMail,
            FinalInvoiceApprovalStatusToInitiator,
            InvoiceForwardedOrRejectionMail,
            InvoiceForwardFeedbackReceieve,

            InvoicePaymentInitiatedMail,
            FinalInvoicePaymentApprovalStatusToInitiator,
            InvoicePaymentForwardedOrRejectionMail,
            InvoicePaymentForwardFeedbackReceieve,

            QCInitiatedMail,
            FinalQCApprovalStatusToInitiator,
            QCForwardedOrRejectionMail,
            QCForwardFeedbackReceieve,

            DocumentApprovalInitiatedMail,
            FinalDocumentApprovalApprovalStatusToInitiator,
            DocumentApprovalForwardedOrRejectionMail,
            DocumentApprovalForwardFeedbackReceieve,

            MRInitiatedMail,
            FinalMRApprovalStatusToInitiator,
            MRForwardedOrRejectionMail,
            MRForwardFeedbackReceieve,

            TaxationVettingInitiatedMail,
            FinalTaxationVettingApprovalStatusToInitiator,
            TaxationVettingForwardedOrRejectionMail,
            TaxationVettingForwardFeedbackReceieve,

            MicroSiteInitiatedMail,
            FinalMicroSiteApprovalStatusToInitiator,
            MicroSiteForwardedOrRejectionMail,
            MicroSiteForwardFeedbackReceieve,

            TaxationPaymentInitiatedMail,
            FinalTaxationPaymentApprovalStatusToInitiator,
            TaxationPaymentForwardedOrRejectionMail,
            TaxationPaymentForwardFeedbackReceieve,

            EmployeeProfileInitiatedMail,
            FinalEmployeeProfileApprovalStatusToInitiator,
            EmployeeProfileForwardedOrRejectionMail,
            EmployeeProfileForwardFeedbackReceieve,

            OTPMessageBody,
            OTPMessageBodyTax,

            ExitInterviewInitiatedMail,
            FinalExitInterviewApprovalStatusToInitiator,
            ExitInterviewForwardedOrRejectionMail,
            ExitInterviewForwardFeedbackReceieve,

            EmployeeAccessDeactivationInitiatedMail,
            FinalEmployeeAccessDeactivationApprovalStatusToInitiator,
            EmployeeAccessDeactivationForwardedOrRejectionMail,
            EmployeeAccessDeactivationForwardFeedbackReceieve,

            DivisionClearenceInitiatedMail,
            FinalDivisionClearenceApprovalStatusToInitiator,
            DivisionClearenceForwardedOrRejectionMail,
            DivisionClearenceForwardFeedbackReceieve,

            SCCInitiatedMail,
            FinalSCCApprovalStatusToInitiator,
            SCCForwardedOrRejectionMail,
            SCCForwardFeedbackReceieve,

            LeaveEncashmentInitiatedMail,
            FinalLeaveEncashmentApprovalStatusToInitiator,
            LeaveEncashmentForwardedOrRejectionMail,
            LeaveEncashmentForwardFeedbackReceieve,

            LeaveEncashmentWindowMail,

            EmployeeDocumentUploadInitiatedMail,
            FinalEmployeeDocumentUploadApprovalStatusToInitiator,
            EmployeeDocumentUploadForwardedOrRejectionMail,
            EmployeeDocumentUploadForwardFeedbackReceieve,

            AdminRequestSupportInitiatedMail,
            FinalAdminRequestSupportApprovalStatusToInitiator,
            AdminRequestSupportForwardedOrRejectionMail,
            AdminRequestSupportForwardFeedbackReceieve,
            AdminRequestSupportSettlementFeedbackReceieve,
            AdminRequestSupportWithoutVehicleSettlementFeedbackReceieve,
            AdminRequestSupportVehicleSupportRejectMail,
            AdminRequestSupportConsumbleGoodsFeedbackReceieve,
            AdminRequestSupportEmployeeInitiatedMail,
            AdminRequestSupportSettlementEmployeeFeedbackReceieve,
            AdminRequestSupportEmployeeConsumbleGoodsFeedbackReceieve,
            AdminRequestSupportEmployeeWithoutVehicleSettlementFeedbackReceieve,
            FinalAdminRequestSupportEmployeeApprovalStatusToInitiator,
            AdminRequestSupportEmployeeVehicleSupportRejectMail,
            EmailNotificationMail,
            AdminRequestSupportRenovationOrMaintenanceFeedbackReceieve,
            AdminRequestSupportEmployeeRenovationOrMaintenanceFeedbackReceieve,
            //Petty cash Expense
            PettyCashExpenseClaimInitiatedMail,
            FinalPettyCashExpenseCliamApprovalStatusToInitiator,
            PettyCashExpenseClaimForwardedOrRejectionMail,
            PettyCashExpenseClaimForwardFeedbackReceieve,

            //Petty cash Advance
            PettyCashAdvanceClaimInitiatedMail,
            FinalPettyCashAdvanceClaimApprovalStatusToInitiator,
            PettyCashAdvanceClaimForwardedOrRejectionMail,
            PettyCashAdvanceClaimForwardFeedbackReceieve,
            //Petty cash Resubmit
            PettyCashAdvanceResubmitClaimInitiatedMail,
            FinalPettyCashAdvanceResubmitClaimApprovalStatusToInitiator,
            PettyCashAdvanceResubmitClaimForwardedOrRejectionMail,
            PettyCashAdvanceResubmitClaimForwardFeedbackReceieve,
            //Petty cash Reimburse
            PettyCashReimburseClaimInitiatedMail,
            FinalPettyCashReimburseClaimApprovalStatusToInitiator,
            PettyCashReimburseClaimForwardedOrRejectionMail,
            PettyCashReimburseClaimForwardFeedbackReceieve,
            //Petty cash Payment
            PettyCashPaymentInitiatedMail,
            FinalPettyCashPaymentApprovalStatusToInitiator,
            PettyCashPaymentForwardedOrRejectionMail,
            PettyCashPaymentForwardFeedbackReceieve,

            //RequisitionSupport
            RequisitionSupportInitiatedMail,
            FinalRequisitionSupportApprovalStatusToInitiator,
            RequisitionSupportForwardedOrRejectionMail,
            RequisitionSupportForwardFeedbackReceieve,

            //RequisitionSupport
            ExternalAuditInitiatedMail,
            FinalExternalAuditApprovalStatusToInitiator,
            ExternalAuditForwardedOrRejectionMail,
            ExternalAuditForwardFeedbackReceieve,
            ExternalAuditInitiatedMailDepartmentWise

        }
        public enum ApprovalFeedback
        {
            Draft = 0,
            NotYetRequested = 1,
            FeedbackRequested,
            WorkingOnFeedback,
            OnHoldByApprovalPanelMember,
            Approved,
            Rejected,
            Canceled,
            Forwarded,
            ForwardResponseReceived,
            Reset,
            Returned,
            ForwardResponded,
            MemberReplaced,
            ProxyAdded
        }
        public enum ApprovalPanel
        {
            HRLeaveApprovalPanel = 1,
            NFAApprovalPanel = 2,
            ExpenseClaimBelowTheLimit,
            ExpenseClaimAboveTheLimit,
            IOUClaimBelowTheLimit,
            IOUClaimAboveTheLimit,
            GRNBelowTheLimit = 15,
            GRNAboveTheLimit = 16,
            QCBelowTheLimit = 17,
            QCAboveTheLimit = 18,
            InvoicePaymentBelowTheLimit = 19,
            InvoicePaymentAboveTheLimit = 20,
            MRBelowTheLimit = 21,
            MRAboveTheLimit = 22,
            TaxationVetting = 23,
            MicroSite = 24,
            TaxationVettingPayment = 25,
            EmployeeProfileApproval = 26,
            DocumentApproval = 27,
            DivisionClearance = 28,
            AccessDeactivation = 29,
            ExitInterview = 30,
            HRSupportDocApproval = 31,
            SCC = 32,
            LeaveEncashmentApplication = 33,
            EmployeeDocumentUploadApproval = 34,
            AdminSupportRequest = 35,
            PettyCashAdvanceClaim = 39,
            PettyCashAdvanceClaimAvove = 40,
            PettyCashReimburse = 41,
            SupportRequisition = 47,
            SupportRequisition_Accessories = 48,
            SupportRequisition_Asset = 49,
            ExternalAudit = 50,
        }
        public enum SystemVariableEntityType
        {
            LeaveCategory = 10
        }
        public enum SystemVariableEntityTypeOracle
        {
            tm_other_activity_category = 8
        }
        public enum SupervisorType
        {
            Regular = 50,
            Dotted = 51,
            Delegated = 52
        }
        public enum TravelType
        {
            Local = 71,
            Overseas = 72,
        }
        public enum AttendanceStatus
        {
            Present = 73,
            Late,
            Absent,
            Leave,
            Holiday,
            OffDay,
            Weekend,
            HolidayPresent,
            WeekendPresent,
            OffDayPresent,
            LeavePresent,
            HolidayLate,
            WeekendLate,
            OffDayLate,
            LeaveLate,
            Invalid,
            LeavePending = 214,
            RemotePending = 215
        }

        public enum PaymentStatus
        {
            Payable = 96,
            Receivable = 97,
        }
        public enum ClaimSattlementType
        {
            Expense = 1,
            IOU = 2,
        }
        public enum InventoryType
        {
            Inventory = 107,
            NonInventory,
            Services,
            Asset
        }
        public enum EmployeeType
        {
            Permanent = 19,
            OnProcess = 20,
            Probation = 21,
            Discontinued = 48,
            Terminated = 49,
            Contractual = 70
        }
        public enum CancellationStatus
        {
            NotCancelled = 0,
            PartiallyCancelled,
            FullyCancelled
        }
        public enum DocApprovalCategory
        {
            HR = 165,
            Security = 166,
            Accounts = 167,
            SCM = 168
        }
        public enum Month
        {
            January = 1,
            February,
            March,
            April,
            May,
            June,
            July,
            August,
            September,
            October,
            November,
            December
        }

        public enum COAAccountCategory
        {
            Layer_1,
            Primary_ledger,
            Control_ledger,
            Sub_Control_Ledger,
            Ledger
        }
        public enum NFAType
        {
            CreateStrategicNFA = 56,
            CreateNFA = 57
        }
        public enum SystemVariables
        {
            ConsumableGoods = 195,
            VehicleOrTransport,
            FacilitiesOrSupport,
            RenovationOrMaintenance = 219
        }

        public enum ExternalAuditWalletType
        {
            UDDOKTA = 235,
            MERCHANT = 236
        }

        public enum ExternalAuditPOSM
        {
            UDDOKTA_POSM = 56,
            MERCHANT_POSM = 57
        }
        public static string Encrypt(string objText)
        {
            int keySize = 256;
            int passwordIterations = 03;
            string hashAlgorithm = "MD5";
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(objText);
            PasswordDeriveBytes password = new PasswordDeriveBytes
            (
            passPhrase,
            saltValueBytes,
            hashAlgorithm,
            passwordIterations
            );
            byte[] keyBytes = password.GetBytes(keySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor
            (
            keyBytes,
            initVectorBytes
            );
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream
            (
            memoryStream,
            encryptor,
            CryptoStreamMode.Write
            );
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherTextBytes);
            cipherText = HttpUtility.UrlEncode(cipherText);
            return cipherText;
        }
        public static string Decrypt(string cipherText)
        {
            string plainText = "";
            int keySize = 256;
            int passwordIterations = 03;
            string hashAlgorithm = "MD5";
            try
            {
                cipherText = HttpUtility.UrlDecode(cipherText);
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes(saltValue);
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                PasswordDeriveBytes password = new PasswordDeriveBytes
                (
                passPhrase,
                saltValueBytes,
                hashAlgorithm,
                passwordIterations
                );
                byte[] keyBytes = password.GetBytes(keySize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor
                (
                keyBytes,
                initVectorBytes
                );
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream
                (
                memoryStream,
                decryptor,
                CryptoStreamMode.Read
                );
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read
                (
                plainTextBytes,
                0,
                plainTextBytes.Length
                );
                memoryStream.Close();
                cryptoStream.Close();
                plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
            catch (Exception ex)
            {
                plainText = "";
            }
            return plainText;
        }
        public static string PayrollEncrypt(string objText, string secretKey)
        {
            int keySize = 256;
            int passwordIterations = 03;
            string hashAlgorithm = "MD5";
            byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
            byte[] saltValueBytes = Encoding.ASCII.GetBytes((saltValue + secretKey));
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(objText);
            PasswordDeriveBytes password = new PasswordDeriveBytes
            (
            passPhrase,
            saltValueBytes,
            hashAlgorithm,
            passwordIterations
            );
            byte[] keyBytes = password.GetBytes(keySize / 8);
            RijndaelManaged symmetricKey = new RijndaelManaged();
            symmetricKey.Mode = CipherMode.CBC;
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor
            (
            keyBytes,
            initVectorBytes
            );
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream
            (
            memoryStream,
            encryptor,
            CryptoStreamMode.Write
            );
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            byte[] cipherTextBytes = memoryStream.ToArray();
            memoryStream.Close();
            cryptoStream.Close();
            string cipherText = Convert.ToBase64String(cipherTextBytes);
            cipherText = HttpUtility.UrlEncode(cipherText);
            return cipherText;
        }
        public static string PayrollDecrypt(string cipherText, string secretKey)
        {
            string plainText = "";
            int keySize = 256;
            int passwordIterations = 03;
            string hashAlgorithm = "MD5";
            try
            {
                cipherText = HttpUtility.UrlDecode(cipherText);
                byte[] initVectorBytes = Encoding.ASCII.GetBytes(initVector);
                byte[] saltValueBytes = Encoding.ASCII.GetBytes((saltValue + secretKey));
                byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
                PasswordDeriveBytes password = new PasswordDeriveBytes
                (
                passPhrase,
                saltValueBytes,
                hashAlgorithm,
                passwordIterations
                );
                byte[] keyBytes = password.GetBytes(keySize / 8);
                RijndaelManaged symmetricKey = new RijndaelManaged();
                symmetricKey.Mode = CipherMode.CBC;
                ICryptoTransform decryptor = symmetricKey.CreateDecryptor
                (
                keyBytes,
                initVectorBytes
                );
                MemoryStream memoryStream = new MemoryStream(cipherTextBytes);
                CryptoStream cryptoStream = new CryptoStream
                (
                memoryStream,
                decryptor,
                CryptoStreamMode.Read
                );
                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                int decryptedByteCount = cryptoStream.Read
                (
                plainTextBytes,
                0,
                plainTextBytes.Length
                );
                memoryStream.Close();
                cryptoStream.Close();
                plainText = Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            }
            catch (Exception ex)
            {
                plainText = "";
            }
            return plainText;
        }
        public static bool ValidateWithSecretPassword(string password)
        {
            if (SecretPassword == password) return true;
            return false;
        }
        public static int GetTotalMonthsFrom(this DateTime dt1, DateTime dt2)
        {
            DateTime earlyDate = (dt1 > dt2) ? dt2.Date : dt1.Date;
            DateTime lateDate = (dt1 > dt2) ? dt1.Date : dt2.Date;

            // Start with 1 month's difference and keep incrementing
            // until we overshoot the late date
            int monthsDiff = 1;
            while (earlyDate.AddMonths(monthsDiff) <= lateDate)
            {
                monthsDiff++;
            }

            return monthsDiff - 1;
        }

        public static string ConvertListOfDicToString(List<Dictionary<string, object>> jsonData)
        {
            return JsonSerializer.Serialize(jsonData);
        }
        public static string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string key, string cipherText)
        {
            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static string CheckValidFileExtensionsForImage(string ext)
        {
            string error = "";
            if (!ImageExtensionsIncluded.Contains(ext))
            {
                error = "Uploaded file extension is not allowed.";
            }
            return error;
        }

        public static string CheckValidFileExtensionsForAttachment(string ext, string fileName)
        {
            string error = "";
            string fileExt = fileName.IsNullOrEmpty() ? "" : System.IO.Path.GetExtension(fileName).Remove(0, 1);
            if (!AttachmentExtensionsIncluded.Contains(ext) && (fileExt.IsNotNullOrEmpty() && !AttachmentExtensionsIncluded.Contains(fileExt)))
            {
                error = "Uploaded file extension is not allowed.";
            }
            return error;
        }

        public static Tuple<string, string> CheckMissingFileExtensionTypeAndGetExtension(string attachedFile)
        {
            if (attachedFile.Split(';').Length > 0)
            {
                if (attachedFile.Split(';')[0].Length > 0)
                {
                    if (attachedFile.Split(';')[0].Split('/').Length > 1)
                    {
                        return new Tuple<string, string>(attachedFile.Split(';')[0].Split('/')[1], "");
                    }
                    else
                    {
                        return new Tuple<string, string>("", "Uploaded file extension is not allowed.");
                    }
                }
                else
                {
                    return new Tuple<string, string>("", "Uploaded file extension is not allowed.");
                }
            }
            else
            {
                return new Tuple<string, string>("", "Uploaded file extension is not allowed.");
            }
        }
        public static (String, String) UploadPayslipUserCredential()
        {
            return ("5104", "tofail.ahmed@nagad.com.bd");
            //return ("11342,11344,5104", "azmanul.abedin@nagad.com.bd,farhan.tanjim@nagad.com.bd, tofail.ahmed@nagad.com.bd");
            //return (11344, "farhan.tanjim@nagad.com.bd");        
        }

        public enum UploadPayslipCategory
        {
            Payslip = 162,
            RegularIncentive = 247,
            MonthlyIncentive = 246,
            FestivalBonus = 207
        }
    }
}
