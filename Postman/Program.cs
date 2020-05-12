using CommandLine;
using EmailValidation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace Postman
{
    class Program
    {
        private static readonly string settingFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}Settings.json";
        private static Settings settings;

        static void Main(string[] args)
        {
            Logger.Instance.Log(Logger.Level.Info, "--------------------------------------------------");
            Logger.Instance.Log(Logger.Level.Info, "프로그램 시작");
            
            // 설정 파일 불러오기
            settings = LoadSettings(settingFilePath);

            // 구독자 데이터베이스 연결
            Logger.Instance.Log(Logger.Level.Info, $"구독자 데이터베이스 연결, '{settings.DatabasePath}'");
            DatabaseManager.Instance.Connect(settings.DatabasePath);

            // 명령줄 인수 파싱
            Options options = ParseOptions(args);

            // 옵션 처리
            if (options != null)
            {
                HandleOptions(options);
            }

            // 구독자 데이터베이스 연결 종료
            Logger.Instance.Log(Logger.Level.Info, "구독자 데이터베이스 연결 종료");
            DatabaseManager.Instance.Close();

            Logger.Instance.Log(Logger.Level.Info, "프로그램 종료");
        }

        private static Settings LoadSettings(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            try
            {
                if (File.Exists(path))
                {
                    Logger.Instance.Log(Logger.Level.Info, "설정 파일 로드");
                    string json = File.ReadAllText(settingFilePath);
                    return JsonSerializer.Deserialize<Settings>(json);
                }
                else
                {
                    Logger.Instance.Log(Logger.Level.Info, $"새로운 설정 파일 생성, '{path}'");
                    string json = JsonSerializer.Serialize(Settings.DefaultSettings);
                    File.WriteAllText(settingFilePath, json);
                }
            }
            catch (JsonException e)
            {
                Logger.Instance.Log(Logger.Level.Error, "설정 파일 처리중 문제 발생", e);
            }
            catch (IOException e)
            {
                Logger.Instance.Log(Logger.Level.Error, "설정 파일 처리중 문제 발생", e);
            }

            return Settings.DefaultSettings;
        }

        private static Options ParseOptions(string[] args)
        {
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }

            ParserResult<Options> result = Parser.Default.ParseArguments<Options>(args);
            if (result is Parsed<Options> parsedOptions)
            {
                Logger.Instance.Log(Logger.Level.Info, "명령줄 인수 파싱");
                return parsedOptions.Value;
            }
            else
            {
                Logger.Instance.Log(Logger.Level.Warn, "명령줄 인수 파싱 실패");
                return null;
            }
        }

        private static void HandleOptions(Options options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            // 구독
            if (string.IsNullOrWhiteSpace(options.SubscribeEmails) == false)
            {
                IEnumerable<string> emails = options.SubscribeEmails.Split(';').Where(x => x.Length > 0);
                Logger.Instance.Log(Logger.Level.Info, $"구독자 '{emails.Count()}'명 추가");
                foreach (string email in emails)
                {
                    SubscribeManager.Instance.Subscribe(email);
                }
            }

            // 구독 해지
            if (string.IsNullOrWhiteSpace(options.UnsubscribeEmails) == false)
            {
                IEnumerable<string> emails = options.UnsubscribeEmails.Split(';').Where(x => x.Length > 0);
                Logger.Instance.Log(Logger.Level.Info, $"구독자 '{emails.Count()}'명 제거");
                foreach (string email in emails)
                {
                    var subscriber = new Subscriber(email);
                    SubscribeManager.Instance.Unsubscribe(email, subscriber.Token);
                }
            }

            // 데일리 메일 전송
            if (options.SendDailyMail)
            {
                SendDailyMail();
            }
        }

        private static void SendDailyMail()
        {
            // 구독자 정보 불러오기
            IEnumerable<Subscriber> subscribers = SubscribeManager.Instance.GetSubscribers();
            if (subscribers.Count() == 0)
            {
                Logger.Instance.Log(Logger.Level.Warn, "메일을 보낼 구독자가 없음");
                return;
            }

            // 이메일 주소 선택
            IEnumerable<string> subscriberEmails = subscribers.Select(x => x.Email);
            Logger.Instance.Log(Logger.Level.Info, $"'{subscriberEmails.Count()}'명의 구독자 이메일 주소를 불러옴");

            // MailSender 초기화
            NetworkCredential gmailCredential = LoadGmailCredential(settings.GmailCredentialPath);
            if (gmailCredential == null)
            {
                Logger.Instance.Log(Logger.Level.Warn, "Gmail credential이 없음");
                return;
            }
            Logger.Instance.Log(Logger.Level.Info, $"Gmail credential을 불러옴, '{gmailCredential.UserName}'");
            IMailSender mailSender = new GmailSender(gmailCredential.UserName, gmailCredential.Password, settings.ProjectNickname);

            // 메일 내용 빌드
            string subject = $"[{settings.ProjectNickname}] {DateTime.Today.ToShortDateString()}";
            string body = DateTime.Now.Second % 2 == 1 ? "📈 떡상 가즈아~~!" : "📉 내려간다 꽉잡아!!!";

            // 메일 전송
            Logger.Instance.Log(Logger.Level.Info, "메일 전송 시작");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            mailSender.SendMail(subscriberEmails, subject, body);

            stopwatch.Stop();
            Logger.Instance.Log(Logger.Level.Info, $"메일 전송 완료, {TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds}secs");
        }

        private static NetworkCredential LoadGmailCredential(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            try
            {
                foreach (string accountString in File.ReadAllLines(path))
                {
                    string[] credential = accountString.Split('/');
                    if (credential.Length != 2)
                    {
                        Logger.Instance.Log(Logger.Level.Warn, $"잘못된 형식의 Credential, '{credential}'");
                        continue;
                    }

                    string id = credential[0];
                    string password = credential[1];
                    if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
                    {
                        Logger.Instance.Log(Logger.Level.Warn, "아이디 또는 비밀번호가 빈 문자열");
                        continue;
                    }

                    if (EmailValidator.Validate(id) == false)
                    {
                        Logger.Instance.Log(Logger.Level.Warn, $"올바른 이메일의 형식이 아님, '{id}'");
                        continue;
                    }

                    return new NetworkCredential(id, password);
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Instance.Log(Logger.Level.Info, $"Gmail credential 파일이 '{path}' 경로에 생성됨");
                File.WriteAllText(path, "id/password");
            }
            catch (IOException e)
            {
                Logger.Instance.Log(Logger.Level.Error, $"Gmail credential을 불러오는 중 문제 발생, '{path}'", e);
            }

            return null;
        }

        private class Options
        {
            [Option('s', HelpText = "구독 목록에 추가할 이메일 입력 ';'로 구분")]
            public string SubscribeEmails { get; set; }

            [Option('u', HelpText = "구독 목록에서 제거할 이메일 입력 ';'로 구분")]
            public string UnsubscribeEmails { get; set; }

            [Option("daily", HelpText = "데일리 메일을 구독자들에게 전송")]
            public bool SendDailyMail { get; set; }
        }

        private class Settings
        {
            public string ProjectNickname { get; set; }
            public string DatabasePath { get; set; }
            public string GmailCredentialPath { get; set; }

            public static Settings DefaultSettings => new Settings()
            {
                ProjectNickname = "주가예측 알리미",
                DatabasePath = $"{AppDomain.CurrentDomain.BaseDirectory}PostmanDB.db",
                GmailCredentialPath = $"{AppDomain.CurrentDomain.BaseDirectory}GmailCredential.txt"
            };
        }
    }
}
