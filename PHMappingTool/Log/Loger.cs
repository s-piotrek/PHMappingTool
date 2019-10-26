using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PHMappingTool.Log
{
    class Loger// : ILoger
    {
        public static string Path { get; set; }
        private static Object createLogLock = new Object();

        public static void log(string message)
        {
            lock (createLogLock)
            {
                try
                {
                    if (File.Exists(Path))
                    {
                        string logFileName = $"LogPHMappingError + {DateTime.Now.ToString("yyyy/mm/dd H:mm")}";
                        using (StreamWriter w = File.CreateText(Path + "\\" + logFileName + ".txt"))
                        {
                            // foreach (var line in _messages)
                            w.WriteLine(message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.ToString());
                }
            }
        }
        public static void log(IEnumerable<string> messages, string fileName)
        {
            if (!messages.Any())
                return;

            lock (createLogLock)
            {
                try
                {
                    if (Directory.Exists(Path))
                    {
                        string logFileName = $"{fileName} {DateTime.Now.ToString("yyyy_mm_dd_H_mm_ss_ms")}";
                        string pathsss = Path + "\\" + logFileName + ".txt";
                        using (StreamWriter w = File.CreateText(Path + "\\" + logFileName + ".txt"))
                        {
                            foreach (var line in messages)
                                w.WriteLine(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.ToString());
                }
            }
        }
    }
}
