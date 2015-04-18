using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using ProductData;

namespace ProductCleanup
{
    public class Cleanup
    {
        // The number of days a shuffle is kept in the system before we delete it
        private const int RetentionPolicyDays = 2;
        private readonly IProductStorage storage;

        public Cleanup(IProductStorage storage)
        {
            if (storage == null)
            {
                throw new ArgumentNullException("storage");
            }

            this.storage = storage;
        }

        // This job will only triggered on demand
        [NoAutomaticTrigger()]
        public static void CleanupFunction(CloudStorageAccount storageAccount, TextWriter log)
        {
            Cleanup cleanupClass = new Cleanup(new ProductStorage(storageAccount, "producten"));
            cleanupClass.DoCleanup(log);
        }

        public void DoCleanup(TextWriter log)
        {
            IEnumerable<Product> expiredProducts = this.storage.GetProductsExpired();
            this.storage.Delete(expiredProducts);
        }
    }
}
