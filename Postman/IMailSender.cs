using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Postman
{
    interface IMailSender
    {
        Task SendMailAsync(IEnumerable<string> receivers, string subject, string body, bool isBodyHtml = false);
    }
}
