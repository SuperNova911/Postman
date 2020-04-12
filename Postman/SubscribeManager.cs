using EmailValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Postman
{
    public class SubscribeManager
    {
        private static SubscribeManager instance;
        private static readonly object instanceLock = new object();

        private Dictionary<int, Subscriber> subscriberTable;

        private SubscribeManager()
        {
            subscriberTable = new Dictionary<int, Subscriber>();

            LoadSubscribers();
        }

        public static SubscribeManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if(instance == null)
                    {
                        instance = new SubscribeManager();
                    }
                }
                return instance;
            }
        }

        public bool Subscribe(string email)
        {
            email = email.Trim();
            if (string.IsNullOrWhiteSpace(email) || EmailValidator.Validate(email) == false)
            {
                Console.WriteLine("유효하지 않은 이메일 주소");
                return false;
            }

            Subscriber subscriber = new Subscriber(email);
            if (subscriberTable.ContainsKey(subscriber.Id))
            {
                Console.WriteLine("이미 등록된 이메일 주소");
                return false;
            }

            if (DatabaseManager.Instance.AddSubscriber(subscriber) == false)
            {
                Console.WriteLine("데이터베이스에 구독자 등록 실패");
                return false;
            }

            subscriberTable.Add(subscriber.Id, subscriber);
            return true;
        }

        public bool Unsubscribe(string email)
        {
            email = email.Trim();
            if (string.IsNullOrWhiteSpace(email) || EmailValidator.Validate(email) == false)
            {
                Console.WriteLine("유효하지 않은 이메일 주소");
                return false;
            }

            Subscriber subscriber = new Subscriber(email);
            if (subscriberTable.ContainsKey(subscriber.Id) == false)
            {
                Console.WriteLine("구독중이 아닌 이메일 주소");
                return false;
            }

            DatabaseManager.Instance.RemoveSubscriber(subscriber);
            subscriberTable.Remove(subscriber.Id);

            return true;
        }

        private void LoadSubscribers()
        {
            List<Subscriber> subscribers = DatabaseManager.Instance.SelectAllSubscribers();

            if (subscribers.Count > 0)
            {
                subscriberTable.Clear();
                foreach (var subscriber in subscribers)
                {
                    subscriberTable.Add(subscriber.Id, subscriber);
                }
            }
        }
    }
}
