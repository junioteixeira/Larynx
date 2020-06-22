using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LarynxModule
{
    public static class LogHelper
    {
        private static string Divider = "\r\n-------------------------------\r\n";
        public static event EventHandler ErrorEngine;

        public static void WriteLog(object sender ,string message)
        {
            try
            {
                string NameLogFile = $"larynx_{DateTime.Now.ToString("yyyy-MM-dd")}.log";
                if (!Directory.Exists("logs"))
                    Directory.CreateDirectory("logs");
                using (FileStream fs = new FileStream(Path.Combine("logs", NameLogFile), FileMode.OpenOrCreate))
                {
                    fs.Seek(0, SeekOrigin.End);
                    string logMessage = $"{Divider}{DateTime.Now.ToString("hh:mm:ss")} - {message}{Divider}";
                    byte[] bufRaw = Encoding.Default.GetBytes(logMessage);
                    fs.Write(bufRaw, 0, bufRaw.Length);
                }

                ErrorEngine?.Invoke(sender, new EventArgs());
            }
            catch
            {
                
            }
        }
    }
}
