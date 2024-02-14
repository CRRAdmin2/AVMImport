using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace AvmImport
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var dbr = new ZipRoutines();
            //dbr.SplitFile("COUNTYRECORDSRESEARCH_5.0_AVM_07652.txt", 30000000);
            try
            {
                var timeStart = DateTime.Now;
                Console.WriteLine("AVM Import!!!!!");

                XElement xInfo = XElement.Load(@"DynamicInfo.xml");
                XName xLastFile = XName.Get("LastFile");
                var sLastFile = xInfo.Element(xLastFile).Value;
                Console.WriteLine("Last file processed: " + sLastFile);

                //Task.Run(async () => await DownloadFile());
                //DownloadFile();
                var ftp = new sFTPRoutines();
                ftp.DownloadFile(sLastFile);

                if (sLastFile != ftp.DownloadedFile)
                {
                    var zip = new ZipRoutines();
                    zip.UnZipFile(ftp.DownloadedFile);
                    zip.SplitFile(ftp.DownloadedFile.Replace(".zip", ".txt"));

                    var db = new DBRoutines();
                    db.InsertData(ftp.DownloadedFile.Replace(".zip",".txt"));

                    xInfo.Element(xLastFile).Value = ftp.DownloadedFile;
                    xInfo.Save(@"DynamicInfo.xml");
                }
                var timeEnd = DateTime.Now;

                Console.WriteLine("Total time: " + timeEnd.Subtract(timeStart).TotalMinutes + " mins.");
                //Console.ReadKey();
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); }

        }

    }
}
