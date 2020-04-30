using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Postman
{
    interface IMailSender
    {
        Task SendMailAsync(IEnumerable<string> receivers, string subject, string body, SendCompletedEventHandler callback, bool isBodyHtml = false);
    }
}
