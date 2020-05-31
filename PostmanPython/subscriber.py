import time
from stringhash import sdbm_lower
from mytype import int32, uint32


class Subscriber:
    id = None
    email = None
    subscribed_date = None


    def __init__(self, email, id = None, subscribe_date = None):
        self.email = email
        self.id = int32(sdbm_lower(email)) if id is None else id
        self.subscribed_date = time.strftime('%m/%d/%Y %I:%M:%S %p').lstrip("0").replace(" 0", " ") if subscribe_date is None else subscribe_date


    @property
    def token(self):
        return format(uint32(self.id), 'X')

    
    def __eq__(self, other):
        if isinstance(other, Subscriber):
            return self.id == other.id
        return False

    
    def __ne__(self, other):
        return not self.__eq__(other)