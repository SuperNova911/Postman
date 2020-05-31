using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Text;

namespace Postman
{
    public class DatabaseManager
    {
        private static DatabaseManager instance;
        private static readonly object instanceLock = new object();
        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

        private MySqlConnection connection;

        private DatabaseManager()
        {
            
        }

        public static DatabaseManager Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new DatabaseManager();
                    }
                }
                return instance;
            }
        }

        public void Connect(string server, string database, string userId, string password)
        {
            if (connection != null && connection.State == ConnectionState.Open)
            {
                Logger.Instance.Log(Logger.Level.Warn, "이미 연결된 데이터베이스 연결 종료");
                connection.Close();
            }

            MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder
            {
                Server = server,
                Database = database,
                UserID = userId,
                Password = password,
            };

            connection = new MySqlConnection(builder.ConnectionString);
            connection.Open();

            CreateDefaultTables();
        }

        public void Close()
        {
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                Logger.Instance.Log(Logger.Level.Warn, "데이터베이스와 연결 중이 아닙니다");
                return;
            }

            connection.Close();
        }

        public List<Subscriber> SelectAllSubscribers()
        {
            string query = "SELECT * FROM `subscriber`";

            using var command = new MySqlCommand(query, connection);

            List<Subscriber> subscribers = new List<Subscriber>();

            try
            {
                using MySqlDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    int id = dataReader.GetInt32("id");
                    string email = dataReader.GetString("email");
                    string subscribedDate = dataReader.GetString("subscribed_date");

                    subscribers.Add(new Subscriber(id, email, DateTime.Parse(subscribedDate, culture)));
                }
            }
            catch (MySqlException e)
            {
                Logger.Instance.Log(Logger.Level.Error, "데이터베이스 오류", e);
            }

            return subscribers;
        }

        public bool AddSubscriber(Subscriber subscriber)
        {
            string query = "INSERT INTO `subscriber` (`id`, `email`, `subscribed_date`) " +
                "VALUES (@id, @email, @subscribed_date);";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", subscriber.Id);
            command.Parameters.AddWithValue("@email", subscriber.Email);
            command.Parameters.AddWithValue("@subscribed_date", subscriber.SubscribedDate.ToString(culture));

            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (MySqlException e)
            {
                Logger.Instance.Log(Logger.Level.Error, "데이터베이스 오류", e);
                return false;
            }
        }

        public void RemoveSubscriberById(int id)
        {
            string query = "DELETE FROM `subscriber` WHERE `id` = @id;";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (MySqlException e)
            {
                Logger.Instance.Log(Logger.Level.Error, "데이터베이스 오류", e);
            }
        }

        private void CreateDefaultTables()
        {
            string query = "CREATE TABLE IF NOT EXISTS `alphastock`.`subscriber` " +
                "(`id` INT NOT NULL, `email` VARCHAR(128) NOT NULL, `subscribed_date` VARCHAR(45) NOT NULL, PRIMARY KEY(`id`), " +
                "UNIQUE INDEX `id_UNIQUE` (`id` ASC) VISIBLE, UNIQUE INDEX `email_UNIQUE` (`email` ASC) VISIBLE);";

            using var command = new MySqlCommand(query, connection);
            command.ExecuteNonQuery();
        }
    }
}
