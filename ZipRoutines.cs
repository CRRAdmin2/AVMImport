using System.IO.Compression;
using System.Configuration;
using System;
using System.IO;
using System.Collections.Generic;


namespace AvmImport
{
    internal class ZipRoutines
    {
        string zipFolder = ConfigurationManager.AppSettings["ZipFolder"];
        string extractFolder = ConfigurationManager.AppSettings["ExtractFolder"];
        string splitFolder = ConfigurationManager.AppSettings["SplitFolder"];
        int chunkSize = int.Parse(ConfigurationManager.AppSettings["ChunkSize"].ToString());
        public void UnZipFile(string zipFile) 
        {
            DirectoryInfo directory = new DirectoryInfo(extractFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            DateTime timeStart = DateTime.Now;
            DateTime timeEnd;
            //ZipFile.CreateFromDirectory(localPath + "start", localPath + zipFile);
            Console.Write("Unzipping file");
            ZipFile.ExtractToDirectory(zipFolder + zipFile, extractFolder);
            timeEnd = DateTime.Now;
            Console.WriteLine(" ... " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");
            Console.WriteLine("Deleting zip file: " + zipFolder + zipFile);
            File.Delete(zipFolder + zipFile);
        }

        public void SplitFile(string inputFile)
        {
            DirectoryInfo directory = new DirectoryInfo(splitFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();
            DateTime timeStart = DateTime.Now;
            DateTime timeEnd;
            Console.Write("Splitting files");

            byte[] buffer = new byte[chunkSize];
            List<byte> extraBuffer = new List<byte>();

            using (Stream input = File.OpenRead(extractFolder + inputFile))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    using (Stream output = File.Create(splitFolder + inputFile.Replace(".txt", "") + "_" + index + ".txt"))
                    {
                        int chunkBytesRead = 0;
                        while (chunkBytesRead < chunkSize)
                        {
                            int bytesRead = input.Read(buffer,
                                                       chunkBytesRead,
                                                       chunkSize - chunkBytesRead);

                            if (bytesRead == 0)
                            {
                                break;
                            }

                            chunkBytesRead += bytesRead;
                        }

                        byte extraByte = buffer[chunkSize - 1];
                        while (extraByte != '\n')
                        {
                            int flag = input.ReadByte();
                            if (flag == -1)
                                break;
                            extraByte = (byte)flag;
                            extraBuffer.Add(extraByte);
                        }

                        output.Write(buffer, 0, chunkBytesRead);
                        if (extraBuffer.Count > 0)
                            output.Write(extraBuffer.ToArray(), 0, extraBuffer.Count);

                        extraBuffer.Clear();
                    }
                    index++;
                }
            }

            timeEnd = DateTime.Now;
            Console.WriteLine(" ... " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");
            Console.WriteLine("Deleting big&splitted file: " + zipFolder + inputFile);
            directory = new DirectoryInfo(extractFolder);
            foreach (FileInfo file in directory.GetFiles()) file.Delete();

        }

    }
}
