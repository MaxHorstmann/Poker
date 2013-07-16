using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PokerObjects
{
    [DataContract]
    public class Enums
    {

        [DataContract]
        public enum Limit
        {
            [EnumMember] 
            Undefined = 0,
            
            [EnumMember]
            NoLimit = 1,

            [EnumMember]
            FixedLimit = 2,
            
            [EnumMember]
            PotLimit = 3
        }

        [DataContract]
        public enum BettingRound
        {
            [EnumMember]
            Undefined = 0,

            [EnumMember]
            Preflop = 1,

            [EnumMember]
            Flop = 2,

            [EnumMember]
            Turn = 3,

            [EnumMember]
            River = 4,

            [EnumMember]
            Showdown = 5
        }

        [DataContract]
        public enum BetType
        {
            [EnumMember]
            Undefined = 0,

            [EnumMember]
            Ante = 1,
            
            [EnumMember]
            SmallBlind = 2,
            
            [EnumMember]
            BigBlind = 3,
            
            [EnumMember]
            Voluntary = 4
        }
            
    }
}
