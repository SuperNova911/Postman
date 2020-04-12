using System;
using System.Collections.Generic;
using System.Text;

namespace Postman
{
    public class Subscriber
    {
        public int Id { get; }
        public string Email { get; }
        public DateTime SubscribedDate { get; }

        public Subscriber(string email)
        {
            Id = StringHash.SDBMLower(email);
            Email = email;
            SubscribedDate = DateTime.Now;
        }

        public Subscriber(int id, string email, DateTime subscribedDdate)
        {
            Id = id;
            Email = email;
            SubscribedDate = subscribedDdate;
        }
    }
}
