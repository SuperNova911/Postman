from abc import *


class MailSender(metaclass = ABCMeta):
    @abstractmethod
    def send_mail(self, receivers, subject, body, is_body_html = False):
        pass


class GmailSender(MailSender):
    def send_mail(self, receivers, subject, body, is_body_html = False):
        
        pass
