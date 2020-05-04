using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Postman
{
    interface IMailSender
    {
        Task<bool> SendMailAsync(IEnumerable<string> receivers, string subject, string body, bool isBodyHtml = false);
    }
}
