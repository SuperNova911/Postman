using System.Collections.Generic;
using System.Net.Mail;
using System.Threading.Tasks;

namespace Postman
{
    interface IMailSender
    {
        bool SendMail(string receiver, string subject, string body, bool isBodyHtml = false);
        bool SendMail(IEnumerable<string> receivers, string subject, string body, bool isBodyHtml = false);
    }
}
