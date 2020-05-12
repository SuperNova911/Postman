using System;
using System.Collections.Generic;
using System.Text;

namespace Postman
{
    public class Subscriber : IEquatable<Subscriber>
    {
        public int Id { get; }
        public string Email { get; }
        public DateTime SubscribedDate { get; }
        public string Token => Id.ToString("X");

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

        public override bool Equals(object obj)
        {
            return Equals(obj as Subscriber);
        }

        public bool Equals(Subscriber other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(Subscriber left, Subscriber right)
        {
            return EqualityComparer<Subscriber>.Default.Equals(left, right);
        }

        public static bool operator !=(Subscriber left, Subscriber right)
        {
            return !(left == right);
        }
        public override string ToString()
        {
            return $"{nameof(Id)}: {Id}, {nameof(Email)}: {Email}, {nameof(SubscribedDate)}: {SubscribedDate}";
        }
    }
}
