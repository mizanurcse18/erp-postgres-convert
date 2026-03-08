using DAL.Core;
using Manager.Core;
using Security.Manager.Dto;
using Security.Manager.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Threading.Tasks;

namespace Security.Manager.Implementations
{
    public class EmailManager : ManagerBase, IEmailManager
    {
        private readonly SmtpClient client = new SmtpClient();
        public EmailManager()
        {
            client.Host = "smtp.gmail.com";
            client.Port = 587;
            client.EnableSsl = true;
            client.UseDefaultCredentials = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.PickupDirectoryLocation=
            //var securePassword = new SecureString();
            //"%NGD#$nagaderp@020".ToCharArray().ToList().ForEach(securePassword.AppendChar);
            //securePassword.MakeReadOnly();
            var securePassword= "hnqspdswaegdtgyu";
            client.Credentials = new NetworkCredential("nagad.erp.test@gmail.com", securePassword);
        }

        public async Task SendEmail(EmailDto emailData)
        {
            /*using (var unitOfWork=new UnitOfWork())
            {
                MailMessage mail = new MailMessage();

            }*/
            MailMessage mail = new MailMessage();
            //mail.From = new MailAddress("admin@naga.com.bd");
            mail.From = new MailAddress(emailData.FromEmailAddress,emailData.FromEmailAddressDisplayName);
            if (emailData.ToEmailAddress.Count > 0 && emailData.ToEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).Count > 0)
            {
                mail.To.Add(string.Join(',', emailData.ToEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).ToArray()));
            }
            if (emailData.CCEmailAddress.Count > 0 && emailData.CCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).Count > 0)
            {
                mail.CC.Add(string.Join(',', emailData.CCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).ToArray()));
            }
            if (emailData.BCCEmailAddress.Count > 0 && emailData.BCCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).Count > 0)
            {
                mail.Bcc.Add(string.Join(',', emailData.BCCEmailAddress.FindAll(x => (!string.IsNullOrEmpty(x)) && (new EmailAddressAttribute().IsValid(x))).ToArray()));
            }
            mail.Subject = emailData.Subject;//GetEmailSubject(emailData);
            mail.Body = emailData.EmailBody;//GetEmailBody(emailData);
            mail.IsBodyHtml = true;        // temporary
            await client.SendMailAsync(mail);
        }

        private string GetEmailSubject(EmailDto emailData)
        {
            string subject = "";
            if (emailData is ResetPasswordEmailDto)
            {
                subject = GetResetPasswordEmailSubject((ResetPasswordEmailDto)emailData);
            }
            return subject;
        }

        private string GetEmailBody(EmailDto emailData)
        {
            string body = "";
            if (emailData is ResetPasswordEmailDto)
            {
                body = GetResetPasswordEmailBody((ResetPasswordEmailDto)emailData);
            }
            return body;
        }

        private string GetResetPasswordEmailSubject(ResetPasswordEmailDto emailData)
        {
            string subject = "";

            // temporary plaintext subject - can be modified later using data from model

            subject = "Reset Password";

            return subject;
        }

        private string GetResetPasswordEmailBody(ResetPasswordEmailDto emailData)
        {
            string body = "";

            // temporary plaintext body - can be modified later using data from model

            body = @$"Your account password is reset to <b>{emailData.GeneratedPassword}</b><br/>.Please visit to Nagad ERP to login using this password.";

            return body;
        }
    }
}
