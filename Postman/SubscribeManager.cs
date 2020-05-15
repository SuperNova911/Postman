using EmailValidation;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public bool Unsubscribe(string email, string token)
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

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (token.Length != 8)
            {
                Logger.Instance.Log(Logger.Level.Warn, $"유효하지 않은 토큰, '{token}'");
                return false;
            }

            int id = int.Parse(token, NumberStyles.HexNumber);
            if (subscriberTable.ContainsKey(id) == false)
            {
                Logger.Instance.Log(Logger.Level.Warn, $"구독중이 아닌 이메일 주소, '{email}'");
                return false;
            }

            DatabaseManager.Instance.RemoveSubscriberById(id);
            subscriberTable.Remove(id);

            return true;
        }

        public IEnumerable<Subscriber> GetSubscribers()
        {
            UpdateSubscriberTable();

            return subscriberTable.Values.AsEnumerable();
        }

        public bool CheckSubscribe(string email)
        {
            var subscriber = new Subscriber(email);
            return subscriberTable.ContainsKey(subscriber.Id);
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
