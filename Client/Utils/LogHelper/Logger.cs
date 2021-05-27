using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Utils.LogHelper
{
    public class Logger
    {
        public static Logger GetLogger()
        {
           return new Logger();
        }
        
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="type">日志等级</param>
        /// <param name="Log">内容</param>
        public void WriteLog(LogType type,String Log)
        {
            //TODO
            Console.WriteLine("LogType={0} Log={1}\n", type.ToString(), Log);
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
