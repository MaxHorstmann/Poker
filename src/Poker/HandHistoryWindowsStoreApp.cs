using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using PokerObjects;

namespace Poker
{
    [DataContract]
    class HandHistoryWindowsStoreApp : HandHistory
    {
        public HandHistoryWindowsStoreApp(long? startingHandNumber = null)
            : base(startingHandNumber)
        {
        }

        protected async override Task SaveToDisk(long handNumber, List<string> logMessages)
        {
            string fileName = string.Format("Hand_{0}.txt", handNumber);

            StorageFolder folder = ApplicationData.Current.LocalFolder;
            StorageFile sf = await folder.CreateFileAsync(
                fileName, CreationCollisionOption.ReplaceExisting);
            using (Stream st = await sf.OpenStreamForWriteAsync())
            {
                using (StreamWriter sw = new StreamWriter(st))
                {
                    foreach (string msg in logMessages)
                    {
                        sw.WriteLine(msg);
                    }
                    sw.Flush();
                }
            }

        }
    }
}
