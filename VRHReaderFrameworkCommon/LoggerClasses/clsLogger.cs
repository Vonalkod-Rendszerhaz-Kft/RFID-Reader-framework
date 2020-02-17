using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRHReaderFrameworkCommon
{
    public sealed class clsLogger
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger("VRHReaderFramework");

        private clsLogger() { }

        private static string MethodAndFunction()
       {
            string message = "";
            try
            {
                message = "[" + new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().Module + "] " + new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().ReflectedType.Name + "." + new System.Diagnostics.StackTrace().GetFrame(2).GetMethod().Name + "() ";
            }
            catch { };

            return message;
        }

        public static void Error(object message, Exception exception = null)
        {
            log.Error(MethodAndFunction() + message, exception);
        }

        public static void Fatal(object message, Exception exception = null)
        {
            log.Fatal(MethodAndFunction() + message, exception);
        }

        public static void Info(object message)
        {
            if (log.IsInfoEnabled)
                log.Info(MethodAndFunction() + message);
        }

        public static void Warn(object message)
        {
            if (log.IsWarnEnabled)
                log.Warn(MethodAndFunction() + message);
        }

        public static void Debug(object message)
        {
            if (log.IsDebugEnabled)
                log.Debug(MethodAndFunction() + message);
        }

        public static void LoadConfig(string s)
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo(s));
        }

        public static void cleanup(DateTime date)
        {
            string directory = string.Empty;
            string filePrefix = string.Empty;
            
            var repo = log4net.LogManager.GetAllRepositories().FirstOrDefault(); ;
            if (repo == null)
                throw new NotSupportedException("Log4Net has not been configured yet.");

            var app = repo.GetAppenders().Where(x => x.GetType() == typeof(log4net.Appender.RollingFileAppender)).FirstOrDefault();
            if (app != null)
            {
                var appender = app as log4net.Appender.RollingFileAppender;

                directory = System.IO.Path.GetDirectoryName(appender.File);
                filePrefix = System.IO.Path.GetFileName(appender.File);

                cleanup(directory, filePrefix, date);
            }
        }

        private static void cleanup(string logDirectory, string logPrefix, DateTime date)
        {
            if (string.IsNullOrEmpty(logDirectory))
                throw new ArgumentException("logDirectory is missing");

            if (string.IsNullOrEmpty(logPrefix))
                throw new ArgumentException("logPrefix is missing");

            var dirInfo = new System.IO.DirectoryInfo(logDirectory);
            if (!dirInfo.Exists)
                return;

            var fileInfos = dirInfo.GetFiles(logPrefix + "*.*");

            if (fileInfos.Length == 0)
                return;

            foreach (var info in fileInfos)
            {
                if (info.LastWriteTime < date)
                {
                    try
                    {
                        info.Delete();
                    }
                    catch { }
                }
            }

        }
    }
}
