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
            // 구독자 데이터베이스 연결
            DatabaseManager.Instance.Connect(subscriberDBPath);

            // 구독자 정보 불러오기
            IEnumerable<Subscriber> subscribers = SubscribeManager.Instance.GetSubscribers();
            if (subscribers.Count() == 0)
            {
                Console.WriteLine("메일을 보낼 구독자가 없음");
                return;
            }

            // 이메일 주소 선택
            IEnumerable<string> addresses = subscribers.Select(x => x.Email);
            Console.WriteLine($"'{addresses.Count()}'명의 구독자 이메일 주소를 불러옴");

            // MailSender 초기화
            NetworkCredential gmailCredential = LoadGmailCredential(gmailCredentialPath);
            if (gmailCredential == null)
            {
                Console.WriteLine("Gmail credential이 없음");
                return;
            }
            Console.WriteLine($"Gmail credential을 불러옴, '{gmailCredential.UserName}'");
            IMailSender mailSender = new GmailSender(gmailCredential.UserName, gmailCredential.Password, projectNickname);

            // 메일 내용 빌드
            string subject = $"[{projectNickname}] {DateTime.Today.ToShortDateString()}";
            string body = DateTime.Now.Second % 2 == 1 ? "📈 떡상 가즈아~~!" : "📉 내려간다 꽉잡아!!!";

            // 메일 전송
            Console.WriteLine("메일 전송 시작");
            mailSender.SendMailAsync(addresses, subject, body, SendMailCallback).Wait();

            // 구독자 데이터베이스 연결 종료
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
                        Console.WriteLine($"잘못된 형식의 Credential, '{credential}'");
                        continue;
                    }

                    string id = credential[0];
                    string password = credential[1];
                    if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(password))
                    {
                        Console.WriteLine("아이디 또는 비밀번호가 빈 문자열");
                        continue;
                    }

                    return new NetworkCredential(id, password);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Gmail credential 파일이 '{path}' 경로에 생성됨");
                File.WriteAllText(path, "id/password");
            }
            catch (IOException e)
            {
                Console.WriteLine($"Gmail credential을 불러오는 중 문제 발생, '{path}'");
                Console.WriteLine(e);
            }

            return null;
        }

        private static void SendMailCallback(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Console.WriteLine($"메일 전송이 취소됨");
            }
            
            if (e.Error != null)
            {
                Console.WriteLine($"메일 전송 에러, {e.Error}");
            }
            else
            {
                Console.WriteLine($"메일이 전송됨");
            }
        }
    }
}
