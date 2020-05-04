using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace Postman
{
    public class Logger
    {
        private static Logger instance;
        private static readonly object instanceLock = new object();

        private static readonly CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");

        public Level LogLevel { get; set; }

        private Logger()
        {
            LogLevel = Level.Trace;
        }

        public static Logger Instance
        {
            get
            {
                lock (instanceLock)
                {
                    if (instance == null)
                    {
                        instance = new Logger();
                    }
                }
                return instance;
            }
        }

        public enum Level
        {
            Trace, Debug, Info, Warn, Error, Fatal 
        }

        public void Log(Level level, string message, [CallerMemberName] string memberName = "")
        {
            if (LogLevel <= level)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH.mm.ss.fff}] {level} [{memberName}]: {message}");
            }
        }

        public void Log(Level level, string message, Exception exception, [CallerMemberName] string memberName = "")
        {
            if (LogLevel <= level)
            {
                Console.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH.mm.ss.fff}] {level} [{memberName}]: {message}\n{exception}");
            }
        }
    }
}
