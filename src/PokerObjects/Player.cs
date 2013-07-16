using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PokerObjects
{
    [DataContract]
    public abstract class Player
    {
        #region fields

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public long Chipcount { get; set; }

        [DataMember]
        public IList<Card> HoleCards { get; private set; }

        [DataMember]
        public bool ShowsHoleCards { get; set; }

        #endregion


        public Player(string name, long chipcount)
        {
            Name = name;
            Chipcount = chipcount;
        }

        public void ReceiveChips(long count)
        {
           Chipcount += count;
        }

        public long GetBetChips(long count)
        {
            Chipcount -= count;
            return count;
        }

        public void DealHoleCards(IList<Card> cards)
        {
            ShowsHoleCards = false;
            if (cards == null)
            {
                cards = new List<Card>();
            }
            HoleCards = cards;
        }

        public abstract Task<long> GetAction(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed);



    }
}