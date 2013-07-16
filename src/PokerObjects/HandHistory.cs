using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PokerObjects
{
    [DataContract]
    public class HandHistory
    {
        [DataMember]
        private List<string> _logMessages = new List<string>();

        [DataMember]
        public long HandNumber { get; protected set; }

        [DataMember]
        bool _unsaved = true;

        public void Log(string logMessage)
        {
            lock (_logMessages)
            {
                _unsaved = true;
                _logMessages.Add(logMessage);
            }
        }

        public HandHistory(long? startingHandNumber = null)
        {
            HandNumber = startingHandNumber.GetValueOrDefault(1);
        }

        public async Task SaveToDisk()
        {
            await SaveToDisk(HandNumber, _logMessages);
            _unsaved = false;
        }

        /// <summary>
        /// Override in subclass
        /// </summary>
        protected async virtual Task SaveToDisk(long handNumber, List<string> logMessages)
        {
            await Task.Delay(1);
        }


        /// <summary>
        /// Flush out the hand history and start a new hand
        /// </summary>
        /// <returns></returns>
        public async Task<long> StartNewHand()
        {
            if ((_logMessages.Count > 0) && (_unsaved))
            {
                await SaveToDisk();
            }
            _logMessages.Clear();
            HandNumber++;
            return HandNumber;
        }

        /// <summary>
        /// Returns the last n log messages
        /// </summary>
        /// <param name="numberOfRows"></param>
        /// <returns></returns>
        public string GetHistory(int numberOfRows = int.MaxValue)
        {
            StringBuilder sb = new StringBuilder();
            lock (_logMessages)
            {
                if (numberOfRows > _logMessages.Count)
                {
                    numberOfRows = _logMessages.Count;
                }

                if (numberOfRows > 0)
                {
                    int startPos = _logMessages.Count - numberOfRows;
                    for (int i = 0; i < numberOfRows; i++)
                    {
                        sb.AppendLine(_logMessages[i + startPos]);
                    }
                }
            }
            return sb.ToString();
        }

    }
}
