using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Client.Utils.LogHelper
{
    public class Logger
    {
        private static string logFileName = "/a.txt";
        private static string logpath;
        private static Logger instance = new Logger();

        //多线程写锁
        static ReaderWriterLockSlim writeLock = new ReaderWriterLockSlim();

        private Logger()
        {
            string basepath = System.IO.Directory.GetCurrentDirectory()+"/log";
            if (!Directory.Exists(basepath))
            {
                Directory.CreateDirectory(basepath);
            }
            logpath = basepath + logFileName;
            //重新写
            //if (File.Exists(logpath))
            //    File.Delete(logpath);

            if(File.Exists(logpath))
            {
                logpath = basepath + "/b.txt";
            }

        }

        public static Logger GetLogger()
        {
           return instance;
        }
        
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="type">日志等级</param>
        /// <param name="Log">内容</param>
        public void WriteLog(LogType type,String Log)
        {
            string log = string.Format("Time={0} LogType={1} Log={2}\n",DateTime.Now, type.ToString(), Log);

            //写日志
            writeLock.EnterWriteLock();
            
            //控制台输出
            //Console.WriteLine(log);
            //写文件
            File.AppendAllText(logpath, log);

            writeLock.ExitWriteLock();
        }

        /// <summary>
        /// Info等级日志
        /// </summary>
        /// <param name="Log"></param>
        public void Info(String Log)
        {
            WriteLog(LogType.Info, Log);
        }
        /// <summary>
        /// Warn等级日志
        /// </summary>
        /// <param name="Log"></param>
        public void Warn(String Log)
        {
            WriteLog(LogType.Warn, Log);
        }
        /// <summary>
        /// Debug等级日志
        /// </summary>
        /// <param name="Log"></param>
        public void Debug(String Log)
        {
            WriteLog(LogType.Debug, Log);
        }
        /// <summary>
        /// Error等级日志
        /// </summary>
        /// <param name="Log"></param>
        public void Error(String Log)
        {
            WriteLog(LogType.Error, Log);
        }
        /// <summary>
        /// Fatal等级日志
        /// </summary>
        /// <param name="Log"></param>
        public void Fatal(String Log)
        {
            WriteLog(LogType.Fatal, Log);
        }

    }
    public enum LogType
    {
        /// <summary>
        /// 信息
        /// </summary>
        Info = 0,
        /// <summary>
        /// 警告
        /// </summary>
        Warn = 1,
        /// <summary>
        /// 调试
        /// </summary>
        Debug = 2,
        /// <summary>
        /// 异常
        /// </summary>
        Error = 3,
        /// <summary>
        /// 错误
        /// </summary>
        Fatal = 4
    }
}
