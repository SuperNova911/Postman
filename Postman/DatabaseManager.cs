using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Postman
{
    public class DatabaseManager
    {
        private static DatabaseManager instance;
        private static readonly object instanceLock = new object();

        private SqliteConnection connection;

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

        public void Connect(string dataSource)
        {
            dataSource = dataSource.Trim();
            if (string.IsNullOrWhiteSpace(dataSource))
            {
                throw new ArgumentNullException(nameof(dataSource));
            }

            if (connection != null && connection.State == ConnectionState.Open)
            {
                Console.WriteLine("Reopen connection");
                connection.Close();
            }

            SqliteConnectionStringBuilder builder = new SqliteConnectionStringBuilder()
            {
                DataSource = dataSource
            };

            connection = new SqliteConnection(builder.ConnectionString);
            connection.Open();

            CreateDefaultTables();
        }

        public void Close()
        {
            if (connection == null || connection.State == ConnectionState.Closed)
            {
                Console.WriteLine("Connection is already closed");
                return;
            }

            connection.Close();
        }

        public List<Subscriber> SelectAllSubscribers()
        {
            string query = "SELECT * FROM `subscriber`";

            using var command = new SqliteCommand(query, connection);

            List<Subscriber> subscribers = new List<Subscriber>();

            try
            {
                using SqliteDataReader dataReader = command.ExecuteReader();
                while (dataReader.Read())
                {
                    int id = dataReader.GetInt32(0);
                    string email = dataReader.GetString(1);
                    string subscribedDate = dataReader.GetString(2);

                    subscribers.Add(new Subscriber(id, email, DateTime.Parse(subscribedDate)));
                }
            }
            catch (SqliteException e)
            {
                Console.WriteLine(e);
            }

            return subscribers;
        }

        public bool AddSubscriber(Subscriber subscriber)
        {
            string query = "INSERT INTO `subscriber` (`id`, `email`, `subscribed_date`) " +
                "VALUES (@id, @email, @subscribed_date);";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", subscriber.Id);
            command.Parameters.AddWithValue("@email", subscriber.Email);
            command.Parameters.AddWithValue("@subscribed_date", subscriber.SubscribedDate.ToString());

            try
            {
                command.ExecuteNonQuery();
                return true;
            }
            catch (SqliteException e)
            {
                Console.WriteLine(e);
                return false;
            }
        }

        public void RemoveSubscriber(Subscriber subscriber)
        {
            string query = "DELETE FROM `subscriber` WHERE `id` = @id;";

            using var command = new SqliteCommand(query, connection);
            command.Parameters.AddWithValue("@id", subscriber.Id);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (SqliteException e)
            {
                Console.WriteLine(e);
            }
        }

        private void CreateDefaultTables()
        {
            string query = "CREATE TABLE IF NOT EXISTS `subscriber` " +
                "(`id` INTEGER NOT NULL UNIQUE, `email` TEXT NOT NULL UNIQUE, `subscribed_date` TEXT NOT NULL, PRIMARY KEY(`id`));";

            using var command = new SqliteCommand(query, connection);
            command.ExecuteNonQuery();
        }
    }
}
