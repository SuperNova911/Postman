import json
import os.path


class DBSettings:
    mysql_server = "example.com"
    database = "master"
    user_id = "id"
    password = "password"

    
class PostmanSettings:
    gmail_account = "id"
    gmail_password = "password"
    db_settings = DBSettings()

    
    def create_settings(self, path):
        db_data = dict()
        db_data['mysql_server'] = self.db_settings.mysql_server
        db_data['database'] = self.db_settings.database
        db_data['user_id'] = self.db_settings.user_id
        db_data['password'] = self.db_settings.password

        data = dict()
        data['gmail_account'] = self.gmail_account
        data['gmail_password'] = self.gmail_password
        data['db_settings'] = db_data

        with open(path, 'w') as file:
            json.dump(data, file, indent=4)


    def load_settings(self, path):
        if not os.path.exists(path):
            print(f"설정 파일이 경로에 없음, '{path}'")
            return
        
        with open(path, 'r') as file:
            data = json.load(file)
            self.gmail_account = data['gmail_account']
            self.gmail_password = data['gmail_password']
            db_data = data['db_settings']
            self.db_settings.mysql_server = db_data['mysql_server']
            self.db_settings.database = db_data['database']
            self.db_settings.user_id = db_data['user_id']
            self.db_settings.password = db_data['password']