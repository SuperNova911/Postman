using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Postman
{
    class Program
    {
        static void Main(string[] args)
        {
            ActionSet();
        }

        private static void ActionSet()
        {
            DatabaseManager.Instance.Connect("Postman.db");

            IEnumerable<Subscriber> subscribers = SubscribeManager.Instance.GetSubscribers();
            if (subscribers.Count() == 0)
            {
                //return;
            }

            IEnumerable<string> addresses = subscribers.Select(x => x.Email);

            string subject = $"[주가예측 알리미] {DateTime.Today.ToShortDateString()}";
            string body = "Body test";

            IMailSender mailSender = new GmailSender("", "", "Postman");
            mailSender.SendMailAsync(new List<string> { "", "" }, subject, body).Wait();

            DatabaseManager.Instance.Close();
        }
    }
}
