from subscriber import Subscriber
from dbmanager import DatabaseManager
from mytype import int32, uint32
from validate_email import validate_email


class SubscribeManager:
    __subscriber_table = None
    __auth_table = None


    def __new__(cls):
        if not hasattr(cls, 'instance'):
            cls.instance = super(SubscribeManager, cls).__new__(cls)
        return cls.instance
    

    def __init__(self):
        self.__subscriber_table = dict()
        self.__auth_table = dict()
        self.__update_subscriber_table()


    def add_to_auth(self, subscriber):
        self.__auth_table[subscriber.email] = subscriber.token


    def auth(self, email, token):
        if email in self.__auth_table and token == self.__auth_table[email]:
            del self.__auth_table[email]
            return True
        else:
            return False


    def subscribe(self, email):
        if email is None or email.isspace():
            return False
        
        email = email.strip()
        if not validate_email(email):
            print('유효하지 않은 이메일 주소', email)
            return False

        subscriber = Subscriber(email)
        if subscriber.id in self.__subscriber_table:
            print('이미 등록된 이메일 주소', email)
            return False
        
        DatabaseManager().add_subscriber(subscriber)
        self.__subscriber_table[subscriber.id] = subscriber
        return True


    def unsubscribe(self, email, token):
        if email is None or email.isspace():
            return False

        email = email.strip()
        if not validate_email(email):
            print('유효하지 않은 이메일 주소', email)
            return False

        if token is None or token.isspace():
            return False

        if len(token) != 8:
            print('유효하지 않은 토큰', token)
            return False

        id = int32(int(token, 16))
        if id not in self.__subscriber_table:
            print('구독중이 아닌 이메일 주소', email)
            return False
        
        DatabaseManager().remove_subscriber_by_id(id)
        del self.__subscriber_table[id]

        return True


    def get_subscribers(self):
        self.__update_subscriber_table()
        return self.__subscriber_table.values()


    def __update_subscriber_table(self):
        subscribers = DatabaseManager().select_all_subscribers()
        if len(subscribers) > 0:
            self.__subscriber_table.clear()
            for subscriber in subscribers:
                self.__subscriber_table[subscriber.id] = subscriber