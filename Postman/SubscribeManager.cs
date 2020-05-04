using EmailValidation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Postman
{
    public class SubscribeManager
    {
        private static SubscribeManager instance;
        private static readonly object instanceLock = new object();

        private readonly Dictionary<int, Subscriber> subscriberTable;

        private SubscribeManager()
        {
            subscriberTable = new Dictionary<int, Subscriber>();

            UpdateSubscriberTable();
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
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            email = email.Trim();
            if (EmailValidator.Validate(email) == false)
            {
                Logger.Instance.Log(Logger.Level.Warn, $"유효하지 않은 이메일 주소, '{email}'");
                return false;
            }

            Subscriber subscriber = new Subscriber(email);
            if (subscriberTable.ContainsKey(subscriber.Id))
            {
                Logger.Instance.Log(Logger.Level.Warn, $"이미 등록된 이메일 주소, '{email}'");
                return false;
            }

            if (DatabaseManager.Instance.AddSubscriber(subscriber) == false)
            {
                Logger.Instance.Log(Logger.Level.Error, $"데이터베이스에 구독자 등록 실패, '{subscriber}'");
                return false;
            }

            subscriberTable.Add(subscriber.Id, subscriber);
            return true;
        }

        public bool Unsubscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentNullException(nameof(email));
            }

            email = email.Trim();
            if (EmailValidator.Validate(email) == false)
            {
                Logger.Instance.Log(Logger.Level.Warn, $"유효하지 않은 이메일 주소, '{email}'");
                return false;
            }

            Subscriber subscriber = new Subscriber(email);
            if (subscriberTable.ContainsKey(subscriber.Id) == false)
            {
                Logger.Instance.Log(Logger.Level.Warn, $"구독중이 아닌 이메일 주소, '{email}'");
                return false;
            }

            DatabaseManager.Instance.RemoveSubscriber(subscriber);
            subscriberTable.Remove(subscriber.Id);

            return true;
        }

        public IEnumerable<Subscriber> GetSubscribers()
        {
            UpdateSubscriberTable();

            return subscriberTable.Values.AsEnumerable();
        }

        private void UpdateSubscriberTable()
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
