using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ExcelConvertLib
{
    public enum ELOGType
    {
        Log,
        Warning,
        Error,
    }

    public class LogSystem : Singleton<LogSystem>
    {     
        public struct LogData
        {
            public ELOGType type;
            public string msg;

            public LogData(ELOGType _type,string _msg)
            {
                type = _type;
                msg = _msg;
            }
        };

        List<LogData> logList = new List<LogData>();
        StringBuilder sb = new StringBuilder();

        public List<LogData> LogList { get => logList; }

        public string GetLog(ELOGType type = ELOGType.Log)
        {
            sb.Clear();
            foreach (var log in logList)
            {
                sb.Append(log.msg + "\r\n");
            }
            return sb.ToString();
        }

        public void Clear()
        {
            logList.Clear();
        }

        public void Add( string msg, ELOGType type = ELOGType.Log )
        {
            if (type == ELOGType.Error)
            {
                MainModule.Inst.ErrorCount++;
                Console.WriteLine( "Error:"+ msg);
            }
            else
            {
                Console.WriteLine(msg);
            }
            logList.Add( new LogData(type, msg));
        }

        public void Save()
        {
            string output = AppDomain.CurrentDomain.BaseDirectory + "/LogFile.txt";
            StreamWriter writer = new StreamWriter(@output, true );

            writer.WriteLine("================");
            writer.WriteLine(System.DateTime.Now.ToString());
            foreach ( LogData log in logList )
            {
                writer.WriteLine(log.msg);
            }

            writer.Close();
        }
    }
}
