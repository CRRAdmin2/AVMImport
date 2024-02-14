using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Renci.SshNet;
using Renci.SshNet.Async;
using System.IO;
using Renci.SshNet.Sftp;
using System.Configuration;

namespace AvmImport
{
    internal class sFTPRoutines
    {
        string host = ConfigurationManager.AppSettings["FtpHost"];
        string username = ConfigurationManager.AppSettings["FtpUser"];
        string password = ConfigurationManager.AppSettings["FtpPassword"];
        string ftpFolder = ConfigurationManager.AppSettings["FtpFolder"];
        string zipFolder = ConfigurationManager.AppSettings["ZipFolder"];
        //string localPath = @"C:\CRR\AVM";
        //string lastFile = "COUNTYRECORDSRESEARCH_5.0_AVM_0583.zip";
        SftpClient client = null;

        public string DownloadedFile {  get; set; }

        public void DownloadFile(string lastFile)
        {
            DirectoryInfo directory = new DirectoryInfo(zipFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            //using (client = new SftpClient(host, username, password))
            //{
            //    client.Connect();
            //    var files = client.ListDirectory("/Outgoing/V2/").OrderByDescending(x => x.LastWriteTime).ToList();

            //    var tasks = new List<Task>();

            //    if (files.Count > 0 && files[0].Name != lastFile)
            //    {
            //        DownloadedFile = files[0].Name;
            //        tasks.Add(DownloadFileAsync(files[0].FullName, localPath + "\\" + files[0].Name));
            //    }

            //    //foreach (var file in files.OrderByDescending(x => x.LastWriteTime).ToList())
            //    //{
            //    //    if ()
            //    //    //tasks.Add(DownloadFileAsync(file.FullName, localPath + "\\" + file.Name));
            //    //}

            //    await Task.WhenAll(tasks);
            //    client.Disconnect();

            //}
            DateTime timeStart = DateTime.Now;
            DateTime timeEnd;

            Console.Write("Connecting to FTP");
            client = new SftpClient(host, username, password);
            client.Connect();
            timeEnd = DateTime.Now;
            Console.WriteLine(" ... " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");

            timeStart = DateTime.Now;
            Console.Write("Checking AVM Files");
            var listing =  client.ListDirectory(ftpFolder)
                .Where(p => p.Name.Contains("_AVM_"))
                .OrderByDescending(x => x.LastWriteTime)
                .ToList();
            timeEnd = DateTime.Now;
            Console.WriteLine(" ... " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");
            //var listing =  client.ListDirectory("/Outgoing/").ToList();
            //var listing = await client.ListDirectoryAsync("/Outgoing/V2/");

            //var tasks = new List<Task>();

            //listing.OrderByDescending(x => x.LastWriteTime).ToList(); 

            if (listing.Count > 0 && listing[0].Name != lastFile)
            {
                DownloadedFile = listing[0].Name;
                Console.WriteLine("New file to process: " + DownloadedFile);
                if (!File.Exists(listing[0].FullName))
                {
                    using (var saveFile = File.OpenWrite(zipFolder + listing[0].Name))
                    {
                        timeStart = DateTime.Now;
                        Console.WriteLine("Downloading file from FTP");
                        client.DownloadFile(listing[0].FullName, saveFile);
                        Console.Write("File downloaded");
                        timeEnd = DateTime.Now;
                        Console.WriteLine(" ... " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");
                        //var task = Task.Factory.FromAsync(client.BeginDownloadFile(source, saveFile), client.EndDownloadFile);
                    }
                }

                //SaveFile(listing[0].FullName, @"extract\" + listing[0].Name);
                //tasks.Add(DownloadFileAsync(files[0].FullName, localPath + "\\" + files[0].Name));
            }
            else 
            {
                DownloadedFile = lastFile;
                Console.WriteLine("No new file to process!"); 
            }


            //foreach (var file in listing.OrderByDescending(x => x.LastWriteTime).ToList())
            //{
            //    Console.WriteLine(file.FullName);
            //}

            Console.WriteLine("Disconnecting from FTP");
            client.Disconnect();

        }

        //private void SaveFile(string source, string destination)
        //{
        //    if (!File.Exists(source))
        //    {
        //        using (var saveFile = File.OpenWrite(destination))
        //        {
        //            client.DownloadFile(source, saveFile);
        //            //var task = Task.Factory.FromAsync(client.BeginDownloadFile(source, saveFile), client.EndDownloadFile);
        //        }
        //    }
        //}
        //async Task DownloadFileAsync(string source, string destination)
        //{
        //    if (!File.Exists(source))
        //    {
        //        using (var saveFile = File.OpenWrite(destination))
        //        {
        //            var task = Task.Factory.FromAsync(client.BeginDownloadFile(source, saveFile), client.EndDownloadFile);
        //            await task;
        //        }
        //    }
        //}

        //private void Download()
        //{
        //    try
        //    {
        //        string RemotePath = "/Outgoing/";
        //        string SourcePath = @"C:\CRR\AVM\";
        //        //string FileName = "download.txt";

        //        string SourceFilePath = SourcePath + FileName;
        //        using (var stream = new FileStream(SourceFilePath, FileMode.Create))
        //        using (var client = new SftpClient(host, username, password))
        //        {
        //            client.Connect();
        //            string RemoteFilePath = RemotePath + FileName;
        //            SftpFileAttributes attrs = client.GetAttributes(RemoteFilePath);
        //            // Set progress bar maximum on foreground thread
        //            int max = (int)attrs.Size;
        //           // progressBar1.Invoke(
        //            //    (MethodInvoker)delegate { progressBar1.Maximum = max; });
        //            // Download with progress callback
        //            client.DownloadFile(RemoteFilePath, stream, DownloadProgresBar);
        //            Console.WriteLine("Download complete");
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine(e.Message);
        //    }
        //}

        //private void DownloadProgresBar(ulong uploaded)
        //{
        //    Console.Write(".");
        //}


    }
}
