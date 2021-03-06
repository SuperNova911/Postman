﻿using CommandLine;
using EmailValidation;
using Google.Protobuf.WellKnownTypes;
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
using System.Text;
using System.Text.Json;
using System.Threading;
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
            var dbSettings = settings.DBSettings;

            // 구독자 데이터베이스 연결
            Logger.Instance.Log(Logger.Level.Info, $"구독자 데이터베이스 연결, '{dbSettings.Server}'");
            DatabaseManager.Instance.Connect(dbSettings.Server, dbSettings.Database, dbSettings.UserId, dbSettings.Password);

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
                    var loadedSettings = JsonSerializer.Deserialize<Settings>(json);

                    json = JsonSerializer.Serialize(loadedSettings, new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                    });
                    File.WriteAllText(settingFilePath, json);

                    return loadedSettings;
                }
                else
                {
                    Logger.Instance.Log(Logger.Level.Info, $"새로운 설정 파일 생성, '{path}'");
                    string json = JsonSerializer.Serialize(Settings.Defaults, new JsonSerializerOptions()
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All)
                    });
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

            return Settings.Defaults;
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
                IEnumerable<string> emails = options.SubscribeEmails.Split(',').Where(x => x.Length > 0);
                Logger.Instance.Log(Logger.Level.Info, $"구독자 '{emails.Count()}'명 추가");
                foreach (string email in emails)
                {
                    SubscribeManager.Instance.Subscribe(email.Trim());
                }
            }

            // 구독 해지
            if (string.IsNullOrWhiteSpace(options.UnsubscribeEmails) == false)
            {
                IEnumerable<string> emails = options.UnsubscribeEmails.Split(',').Where(x => x.Length > 0);
                Logger.Instance.Log(Logger.Level.Info, $"구독자 '{emails.Count()}'명 제거");
                foreach (string email in emails)
                {
                    var subscriber = new Subscriber(email.Trim());
                    SubscribeManager.Instance.Unsubscribe(subscriber.Email, subscriber.Token);
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
            // MailSender 초기화
            NetworkCredential mailCredential = settings.MailSettings.GetCrediential();
            if (mailCredential == null)
            {
                Logger.Instance.Log(Logger.Level.Warn, "Mail credential이 없음");
                return;
            }

            // 구독자 정보 불러오기
            IEnumerable<Subscriber> subscribers = SubscribeManager.Instance.GetSubscribers();
            if (subscribers.Count() == 0)
            {
                Logger.Instance.Log(Logger.Level.Warn, "메일을 보낼 구독자가 없음");
                return;
            }
            Logger.Instance.Log(Logger.Level.Info, $"'{subscribers.Count()}'명의 구독자를 불러옴");

            string template;
            try
            {
                template = File.ReadAllText(settings.DailyMailTemplatePath);
            }
            catch (IOException e)
            {
                Logger.Instance.Log(Logger.Level.Error, $"파일 읽기 오류, path: {settings.DailyMailTemplatePath}", e);
                return;
            }
            catch (Exception e)
            {
                Logger.Instance.Log(Logger.Level.Error, $"파일 읽기 오류, path: {settings.DailyMailTemplatePath}", e);
                return;
            }

            IMailSender mailSender = new GmailSender(mailCredential.UserName, mailCredential.Password, settings.ProjectNickname);

            Logger.Instance.Log(Logger.Level.Info, "메일 전송 시작");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            var sendMailTasks = new List<Task>();

            // 메일 내용 빌드
            string subject = $"[{settings.ProjectNickname}] {DateTime.Today:yyyy-MM-dd}";
            DateTime from = DateTime.Today - TimeSpan.FromDays(14);
            DateTime to = DateTime.Today + TimeSpan.FromDays(7);
            foreach (Subscriber subscriber in subscribers)
            {
                DailyMailBuilder dailyMailBuilder = new DailyMailBuilder(template)
                {
                    Receiver = subscriber
                };

                List<string> favoriteStockIds = DatabaseManager.Instance.SelectFavoriteStockIds(subscriber);
                foreach (string stockId in favoriteStockIds)
                {
                    int todayClosingPrice = DatabaseManager.Instance.SelectClosingPrice(stockId, DateTime.Today);
                    Dictionary<DateTime, int> predictClosingPrices = DatabaseManager.Instance.SelectPredictPrices(stockId, 
                        DateTime.Today + TimeSpan.FromDays(1), 
                        DateTime.Today + TimeSpan.FromDays(11));

                    var chartData = new DailyMailBuilder.ChartData(stockId, todayClosingPrice, predictClosingPrices.Values);
                    dailyMailBuilder.ChartDatas.Add(chartData);
                }

                string body = dailyMailBuilder.Build();

                // 메일 전송
                sendMailTasks.Add(Task.Run(() =>
                {
                    mailSender.SendMail(subscriber.Email, subject, body, true);
                }));
            }

            Logger.Instance.Log(Logger.Level.Info, "메일 전송 완료 대기 중");
            Task.WaitAll(sendMailTasks.ToArray());

            stopwatch.Stop();
            Logger.Instance.Log(Logger.Level.Info, $"메일 전송 완료, {TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds}secs");
        }

        private class Options
        {
            [Option('s', HelpText = "구독 목록에 추가할 이메일 입력 ','로 구분")]
            public string SubscribeEmails { get; set; }

            [Option('u', HelpText = "구독 목록에서 제거할 이메일 입력 ','로 구분")]
            public string UnsubscribeEmails { get; set; }

            [Option("daily", HelpText = "데일리 메일을 구독자들에게 전송")]
            public bool SendDailyMail { get; set; }
        }

        private class Settings
        {
            public string ProjectNickname { get; set; }
            public string DailyMailTemplatePath { get; set; }
            public MailSenderSettings MailSettings { get; set; }
            public DatabaseSettings DBSettings { get; set; }

            public static Settings Defaults => new Settings()
            {
                ProjectNickname = "주가예측 알리미",
                DailyMailTemplatePath = "./dailyMail.html",
                MailSettings = MailSenderSettings.Defaults,
                DBSettings = DatabaseSettings.Defaults
            };

            public class MailSenderSettings
            {
                public string Account { get; set; }
                public string Password { get; set; }

                public static MailSenderSettings Defaults => new MailSenderSettings
                {
                    Account = "example@email.com",
                    Password = "password"
                };

                public NetworkCredential GetCrediential()
                {
                    return new NetworkCredential(Account, Password);
                }
            }

            public class DatabaseSettings
            {
                public string Server { get; set; }
                public string Database { get; set; }
                public string UserId { get; set; }
                public string Password { get; set; }

                public static DatabaseSettings Defaults => new DatabaseSettings
                {
                    Server = "example.com",
                    Database = "master",
                    UserId = "id",
                    Password = "password"
                };
            }
        }
    }
}
