import pymysql
import warnings

from subscriber import Subscriber


class DatabaseManager:
    __connection = None


    def __new__(cls):
        if not hasattr(cls, 'instance'):
            cls.instance = super(DatabaseManager, cls).__new__(cls)
        return cls.instance
    

    def connect(self, server, database, user_id, password):
        self.__connection = pymysql.connect(host = server, db = database, user = user_id, password = password)
        self.__create_default_tables()


    def close(self):
        self.__connection.close()


    def select_all_subscribers(self):
        query = 'SELECT * FROM `subscriber`;'
        cursor = self.__connection.cursor()

        subscribers = list()

        cursor.execute(query)
        for id, email, subscribed_date in cursor.fetchall():
            subscriber = Subscriber(email, id, subscribed_date)
            subscribers.append(subscriber)
        
        return subscribers


    def add_subscriber(self, subscriber):
        query = 'INSERT INTO `subscriber` (`id`, `email`, `subscribed_date`) VALUES (%s, %s, %s);'
        cursor = self.__connection.cursor()

        cursor.execute(query, (subscriber.id, subscriber.email, subscriber.subscribed_date))
        self.__connection.commit()


    def remove_subscriber_by_id(self, id):
        query = 'DELETE FROM `subscriber` WHERE `id` = %s;'
        cursor = self.__connection.cursor()

        cursor.execute(query, (id,))
        self.__connection.commit()


    def select_all_stocks(self):
        query = 'SELECT * FROM `alphastock`.`Name`;'
        cursor = self.__connection.cursor()

        stocks = dict()

        cursor.execute(query)
        for id, name in cursor.fetchall():
            stocks[id] = name
        
        return stocks

    
    def select_favorite_stock_ids(self, subscriber):
        query = 'SELECT `stock_id` FROM `alphastock`.`favorite` WHERE `user_id` = %s;'
        cursor = self.__connection.cursor()

        stock_ids = list()

        cursor.execute(query, (subscriber.id,))
        for stock_id in cursor.fetchall():
            stock_ids.append(stock_id[0])
        
        return stock_ids

    
    def insert_favorites(self, subscriber, favorites):
        query = 'INSERT INTO `alphastock`.`favorite` (`user_id`, `stock_id`) VALUES (%s, %s);'
        cursor = self.__connection.cursor()

        for stock_id in favorites:
            cursor.execute(query, (subscriber.id, stock_id))
        self.__connection.commit()


    def delete_all_favorites(self, subscriber):
        query = 'DELETE FROM `alphastock`.`favorite` WHERE `user_id` = %s;'
        cursor = self.__connection.cursor()

        cursor.execute(query, (subscriber.id,))
        self.__connection.commit()


    def __create_default_tables(self):
        query = 'CREATE TABLE IF NOT EXISTS `alphastock`.`subscriber` (`id` INT NOT NULL, `email` VARCHAR(128) NOT NULL, `subscribed_date` VARCHAR(45) NOT NULL, PRIMARY KEY(`id`), UNIQUE INDEX `id_UNIQUE` (`id` ASC) VISIBLE, UNIQUE INDEX `email_UNIQUE` (`email` ASC) VISIBLE);'
        cursor = self.__connection.cursor()

        cursor.execute(query)
        self.__connection.commit()

    
warnings.filterwarnings('ignore')