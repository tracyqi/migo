using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using ProductData;
using System.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace StatisticsJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            //var host = new JobHost();
            //// The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();

            CollectStatistics();
        }

        private static void CollectStatistics()
        {
            string conn = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;
            IProductStorage productStorage = new ProductStorage(conn, "productsen");

            Console.WriteLine(productStorage.Count());            //insert
 

            Console.ReadLine();
        }

        private static void InsertDailyMetrics(int cnt)
        {
           //insert
            using (var dm = new ygmEntities())
            {
                dm.DailyMetrics.Add (new DailyMetric()
                {
                    NumOfRecords = cnt,
                    NumOfNewRecords = cnt

                });
                dm.SaveChanges();
 
            }
}

    }
}
