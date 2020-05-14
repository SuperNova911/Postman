import smtplib
from email.mime.text import MIMEText
from email.mime.multipart import MIMEMultipart
from abc import ABCMeta, abstractmethod
from email.message import EmailMessage


class MailSender(metaclass = ABCMeta):
    @abstractmethod
    def send_mail(self, receivers, subject, body, is_body_html = False):
        pass


class GmailSender(MailSender):
    __account = None
    __password = None
    __display_name = None
    
    def __init__(self, account, password, display_name):
        self.__account = account
        self.__password = password
        self.__display_name = display_name


    def send_mail(self, receivers, subject, body, is_body_html = False):
        receivers = list(filter(lambda x: len(x) > 0, receivers))
        if len(receivers) == 0:
            print('수신자 주소가 비어있음')
            return

        message = EmailMessage()
        message['Subject'] = subject
        message['From'] = f'{self.__display_name} <{self.__account}>'
        message['Bcc'] = ', '.join(receivers)

        if is_body_html:
            message.add_alternative(body, subtype = 'html')
        else:
            message.set_content(body)

        smtp = smtplib.SMTP(host = 'smtp.gmail.com', port = 587)
        smtp.ehlo()
        smtp.starttls()
        smtp.login(self.__account, self.__password)
        smtp.send_message(message)
        smtp.quit()


gmail = GmailSender('bucephalussw@gmail.com', 'qifbhowhamhdohfn', 'Python Postman')
gmail.send_mail(['suwhan77@naver.com', 'joduska001@gmail.com', 'chbang22@gmail.com'], 'Subject', 'Plain text<h1>html text</h1>', True)