using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AlcoBearUpdater
{
    public class Updater
    {
        const string updateServerLogin = @"egais_check";
        const string updateServerPassword = @"Cktgnjy";
        const string updateConnectionStringPattern = @"ftp://{0}:{1}@ftp.sibatom.com/AlcoBear/";
        const string UpdateLogFile = "update.log";

        static string UpdateConnectionString
        {
            get
            {
                return String.Format(updateConnectionStringPattern, updateServerLogin, updateServerPassword);
            }
        }

        static readonly List<string> updateFiles = new List<string>() { @"LiteDB.dll", @"AlcoBear.exe.config", @"AlcoBear.exe" };
        static readonly List<string> filesToDelete = new List<string>();

        static void WriteLog(string message)
        {
            File.WriteAllText(UpdateLogFile, "[" + DateTime.Now.ToString() + "] " + message);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="args">Если запустить с ключем -a или --all, то удалятся база и логи</param>
        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;
            //Wait if AlcoBear kill himself
            Thread.Sleep(1000);
            //Kill AlcoBear's processes
            Process[] alcobearsProcs = Process.GetProcessesByName("AlcoBear");
            foreach (Process proc in alcobearsProcs)
            {
                proc.Kill();
            }
            //Парсинг настроек алкобира
            XElement config = XDocument.Load("AlcoBear.exe.config").Root;
            XElement appSettingsNode = config.Element("applicationSettings").Element("AlcoBear.Properties.Settings");
            foreach (XElement elem in appSettingsNode.Elements("setting"))
            {
                try
                {
                    if (elem.Attribute("name").Value.Equals("logFilePath") || elem.Attribute("name").Value.Equals("dbFilePath"))
                    {
                        filesToDelete.Add(elem.Element("value").Value);
                    }
                }
                catch 
                {
                    continue;
                }
            }
            //Download new files
            try
            {
                using (WebClient updateProcess = new WebClient())
                {
                    foreach (string new_file in updateFiles)
                    {
                        updateProcess.DownloadFile(UpdateConnectionString + new_file, "new_" + new_file);
                    }
                }
            }
            catch (Exception ex)
            {
                foreach (string del_file in updateFiles)
                {
                    File.Delete("new_" + del_file);
                }
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("Не удалось подключиться к серверу обновлений");
                WriteLog(ex.Message);
                Console.ForegroundColor = ConsoleColor.White;
                return;
            }

            //Delete old files
            foreach (string old_file in updateFiles)
            {
                File.Delete(old_file);
            }
            if (args[0].Equals("--all"))
              foreach(string files in filesToDelete)
              {
                  File.Delete(files);
              }

            //Rename new files
            foreach (string files in updateFiles)
            {
                File.Move("new_"+files, files);
            }
            Console.WriteLine("Обновление прошло успешно.");
        }
    }
}
