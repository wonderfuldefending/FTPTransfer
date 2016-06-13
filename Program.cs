using IF.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Ftp;
using System.Text;

namespace FileMonitor
{
    class Program
    {
        static clsFTP fh;
        static SFTPOperation sftpOperation;
        static void Main(string[] args)
        {
            string uri = ConfigurationManager.AppSettings["FtpServerUri"];
            string userName = ConfigurationManager.AppSettings["user_name"] ?? "anonymous";
            string password = ConfigurationManager.AppSettings["password"] ?? "@anonymous";

            string sftpUri = ConfigurationManager.AppSettings["SftpServerUri"];
            string sftpUserName = ConfigurationManager.AppSettings["SftpServerUserName"];
            string sftpPassword = ConfigurationManager.AppSettings["SftpServerPassword"];
            string sftpPort = ConfigurationManager.AppSettings["SftpServerPort"] ?? "22";
            int pollingInterval = Convert.ToInt32(ConfigurationManager.AppSettings["PollingInterval"] ?? "10");

            fh = new clsFTP(new Uri(uri), userName, password);
            sftpOperation = new SFTPOperation(sftpUri, sftpPort, sftpUserName, sftpPassword);
            //Console.WriteLine("connect ftp : " + sftp.Connect());

            //fh.Uri = new Uri(uri);
            //fh.DirectoryPath += "in/";

            System.Timers.Timer aTimer = new System.Timers.Timer(1000 * pollingInterval);
            aTimer.Elapsed += aTimer_Elapsed;
            //aTimer.AutoReset = false; //run only one time
            aTimer.Enabled = true;

            //fh.DirectoryExist("todel");
            //fh.RemoveDirectory("todel");
            //FileStruct[] files = fh.ListFiles();
            Console.ReadLine();
        }

        static bool executing = false;

        static void aTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (executing)
                return;

            executing = true;

            FileStruct[] files = null;
            try 
            {
                files = fh.ListFiles();
                if (files.Length > 0)
                {
                    sftpOperation.Connect();
                }
                foreach (var fs in files)
                {
                    //fh.CopyFileToAnotherDirectory(fs.Name, "./Test/out");
                    Console.WriteLine(string.Format("{0} 执行文件: {1}", DateTime.Now.GetDateTimeFormats('s')[0], fs.Name));
                    byte[] bt = fh.GetUploadZipStream(fs.Name);
                    MemoryStream mem = new MemoryStream(bt);

                    sftpOperation.Put(mem, "OKQ8/" + Path.GetFileNameWithoutExtension(fs.Name) + ".zip");
                    mem.Close();
                    mem.Dispose();

                    //fh.ZipFileToAnotherDirectory(fs.Name, "./Test/out");
                    fh.DeleteFile(fs.Name);
                    Console.WriteLine(string.Format("{0} 执行成功: {1}\r\n", DateTime.Now.GetDateTimeFormats('s')[0], fs.Name));
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(string.Format("执行失败，原因：{0}", ex.Message));
            }
            finally
            {
                sftpOperation.Disconnect();
                executing = false;
            }
            
        }
    }
}
