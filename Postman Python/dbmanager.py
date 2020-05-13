import sqlite3

from subscriber import Subscriber


class DatabaseManager:
    __connection = None


    def __new__(cls):
        if not hasattr(cls, 'instance'):
            cls.instance = super(DatabaseManager, cls).__new__(cls)
        return cls.instance
    

    def connect(self, db_path):
        self.__connection = sqlite3.connect(db_path)
        self.__create_default_tables()


    def close(self):
        self.__connection.close()


    def select_all_subscribers(self):
        query = 'SELECT * FROM `subscriber`;'
        cursor = self.__connection.cursor()

        subscribers = list()

        cursor.execute(query)
        cursor.row_factory = sqlite3.Row
        for row in cursor.fetchall():
            id = row['id']
            email = row['email']
            subscribed_date = row['subscribed_date']
            subscriber = Subscriber(email, id, subscribed_date)
            subscribers.append(subscriber)
        
        return subscribers


    def add_subscriber(self, subscriber):
        query = 'INSERT INTO `subscriber` (`id`, `email`, `subscribed_date`) VALUES (?, ?, ?);'
        cursor = self.__connection.cursor()

        cursor.execute(query, (subscriber.id, subscriber.email, subscriber.subscribed_date))
        self.__connection.commit()


    def remove_subscriber_by_id(self, id):
        query = 'DELETE FROM `subscriber` WHERE `id` = ?;'
        cursor = self.__connection.cursor()

        cursor.execute(query, (id,))
        self.__connection.commit()


    def __create_default_tables(self):
        query = 'CREATE TABLE IF NOT EXISTS `subscriber` (`id` INTEGER NOT NULL UNIQUE, `email` TEXT NOT NULL UNIQUE, `subscribed_date` TEXT NOT NULL, PRIMARY KEY(`id`));'
        cursor = self.__connection.cursor()

        cursor.execute(query)
        self.__connection.commit()