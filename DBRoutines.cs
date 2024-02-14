using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Threading;

namespace AvmImport
{
    internal class DBRoutines
    {
        //string extractFolder = ConfigurationManager.AppSettings["ExtractFolder"];
        string splitFolder = ConfigurationManager.AppSettings["SplitFolder"];
        //int bulkCount = int.Parse(ConfigurationManager.AppSettings["BulkCount"].ToString());

        public void InsertData(string avmFile)
        {
            DateTime timeStartTotal = DateTime.Now;
            DateTime timeEndTotal;
            DateTime timeStart = DateTime.Now;
            DateTime timeEnd;
            //Console.Write("Loading AVM file into memory: " + avmFile);
            //var lines = File.ReadAllLines(@"C:\CRR\AVM\extract\COUNTYRECORDSRESEARCH_5.0_AVM_0583.txt");
            //var lines = File.ReadAllLines(extractFolder + avmFile);
            //var lines = File.ReadLines(extractFolder + avmFile).Take(bulkCount).ToList();
            var header = File.ReadLines(splitFolder + avmFile.Replace(".txt","_0.txt")).Take(1).ToList();
            

            if (header.Count() == 0) return;
            var columns = header[0].Split('	');
            var table = new DataTable();
            foreach (var c in columns)
                table.Columns.Add(c);
            timeEnd = DateTime.Now;
            //Console.WriteLine(" ... " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");
            //Console.WriteLine("Deleting extracted files.");
            //DirectoryInfo directory = new DirectoryInfo(extractFolder);
            //foreach (FileInfo file in directory.GetFiles()) file.Delete();

            //lines[0].Remove(0);
            //lines = File.ReadLines(extractFolder + avmFile).Take()

            //Console.WriteLine("Total records to insert: " + lines.Count());
            SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["CRRConnectionString"].ConnectionString);
            try
            {
                conn.Open();
                var sqlBulk = new SqlBulkCopy(conn);
                sqlBulk.BulkCopyTimeout = 0;
                sqlBulk.DestinationTableName = ConfigurationManager.AppSettings["AVMTable"];
                
                timeStart = DateTime.Now;
                Console.WriteLine("Truncating table: " + ConfigurationManager.AppSettings["AVMTable"]);
                SqlCommand command = new SqlCommand("TRUNCATE TABLE " + ConfigurationManager.AppSettings["AVMTable"], conn);
                command.ExecuteNonQuery();
                timeEnd = DateTime.Now;
                Console.WriteLine(" ... " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");

                //int bulkCount = 500000;
                //int loops = (lines.Count() / bulkCount) + 1;
                var dirFiles = new DirectoryInfo(splitFolder).GetFiles();
                int loops = dirFiles.Count();
                int i;
                List<string> lines;
                for (int j = 0; j < loops; j++)
                {
                    timeStart = DateTime.Now;
                    lines = File.ReadLines(splitFolder + avmFile.Replace(".txt", "_" + j + ".txt")).ToList();
                    //if (j % 2 == 0)
                    //{
                        //is even
                        //timeStart = DateTime.Now;
                        //Console.Write("Bulking from: " + ((bulkCount * j) + 1).ToString());
                        Console.Write("Bulking " + lines.Count());
                    //}
                    //for (i = (bulkCount * j) + 1; i < bulkCount * (j + 1) && i < lines.Count(); i++)
                    for (i = 0; i < lines.Count(); i++)
                        if (j>0 && i>0) table.Rows.Add(lines[i].Split('	'));
                    //Console.Write(" to: " + i.ToString());
                    Console.Write(" .");

                    //Console.WriteLine(table.Rows.Count);
                    //if (j % 2 == 0 && j < lines.Count() - 1)
                    //{
                    //    //is even
                    //    Console.Write(".");
                    //}
                    //else
                    //{
                        //is odd
                        sqlBulk.WriteToServer(table);
                        table.Rows.Clear();
                        timeEnd = DateTime.Now;
                        Console.WriteLine(". " + timeEnd.Subtract(timeStart).TotalSeconds + " secs.");
                    //}
                }
                timeEndTotal = DateTime.Now;
                Console.WriteLine("Total time bulking: " + timeEndTotal.Subtract(timeStartTotal).TotalMinutes + " mins.");

                DirectoryInfo directory = new DirectoryInfo(splitFolder);
                foreach (FileInfo file in directory.GetFiles()) file.Delete();
            }
            catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
            finally { conn.Close(); }

        }

    }
}
