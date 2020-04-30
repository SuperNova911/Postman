using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Postman
{
    public sealed class GmailSender : IMailSender
    {
        private readonly NetworkCredential credential;
        private readonly MailAddress sender;

        public GmailSender(string gmailAccount, string password, string displayName)
        {
            if (string.IsNullOrWhiteSpace(gmailAccount))
            {
                throw new ArgumentException($"유효하지 않은 {nameof(gmailAccount)}");
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException($"유효하지 않은 {nameof(password)}");
            }

            if (string.IsNullOrWhiteSpace(displayName))
            {
                throw new ArgumentException($"유효하지 않은 {nameof(displayName)}");
            }

            credential = new NetworkCredential(gmailAccount, password);
            sender = new MailAddress(gmailAccount, displayName);
        }

        public async Task SendMailAsync(IEnumerable<string> receivers, string subject, string body, SendCompletedEventHandler callback, bool isBodyHtml = false)
        {
            if (receivers == null)
            {
                throw new ArgumentNullException(nameof(receivers));
            }

            if (string.IsNullOrWhiteSpace(subject))
            {
                throw new ArgumentNullException(nameof(subject));
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                throw new ArgumentNullException(nameof(body));
            }

            receivers = receivers.Where(x => string.IsNullOrWhiteSpace(x) == false);
            if (receivers.Count() == 0)
            {
                throw new ArgumentException("수신자 주소가 비어있음", nameof(receivers));
            }

            using MailMessage message = new MailMessage()
            {
                From = sender,
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            };

            foreach (var receiver in receivers)
            {
                message.Bcc.Add(receiver);
            }

            using var smtpClient = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = credential
            };
            smtpClient.SendCompleted += callback;

            await smtpClient.SendMailAsync(message);
        }
    }
}
