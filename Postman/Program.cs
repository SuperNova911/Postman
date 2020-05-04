using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Postman
{
    class Program
    {
        private static readonly string subscriberDBPath = "PostmanDB.db";
        private static readonly string gmailCredentialPath = "GmailCredential.txt";
        private static readonly string projectNickname = "주가예측 알리미";

        static void Main(string[] args)
        {
            Logger.Instance.Log(Logger.Level.Info, "프로그램 시작");

            // 구독자 데이터베이스 연결
            Logger.Instance.Log(Logger.Level.Info, $"구독자 데이터베이스 연결, '{subscriberDBPath}'");
            DatabaseManager.Instance.Connect(subscriberDBPath);

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
            NetworkCredential gmailCredential = LoadGmailCredential(gmailCredentialPath);
            if (gmailCredential == null)
            {
                Logger.Instance.Log(Logger.Level.Warn, "Gmail credential이 없음");
                return;
            }
            Logger.Instance.Log(Logger.Level.Info, $"Gmail credential을 불러옴, '{gmailCredential.UserName}'");
            IMailSender mailSender = new GmailSender(gmailCredential.UserName, gmailCredential.Password, projectNickname);

            // 메일 내용 빌드
            string subject = $"[{projectNickname}] {DateTime.Today.ToShortDateString()}";
            string body = DateTime.Now.Second % 2 == 1 ? "📈 떡상 가즈아~~!" : "📉 내려간다 꽉잡아!!!";

            // 메일 전송
            Logger.Instance.Log(Logger.Level.Info, "메일 전송 시작");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            mailSender.SendMail(subscriberEmails, subject, body);

            stopwatch.Stop();
            Logger.Instance.Log(Logger.Level.Info, $"메일 전송 완료, {TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds}secs");

            // 구독자 데이터베이스 연결 종료
            Logger.Instance.Log(Logger.Level.Info, "구독자 데이터베이스 연결 종료");
            DatabaseManager.Instance.Close();
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
    }
}
