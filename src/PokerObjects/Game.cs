using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace PokerObjects
{
    /// <summary>
    /// Contains rules for a game of poker
    /// e.g. which type (Holdem, Omaha), limits, blinds
    /// </summary>
    [DataContract]
    public class Game
    {
        public enum GameTypeEnum
        {
            undefined = 0,
            Holdem = 1,
            Omaha = 2
        }

        #region fields

        [DataMember]
        public GameTypeEnum GameType { get; private set; }

        [DataMember]
        public Enums.Limit Limit { get; private set; }

        [DataMember]
        public long SmallBlind { get; private set; }

        [DataMember]
        public long BigBlind { get; private set; }

        [DataMember]
        public long Ante { get; private set; }

        [DataMember]
        public int MaxNumberOfRaises { get; private set; }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="gameType"></param>
        public Game(GameTypeEnum gameType, Enums.Limit limit, long smallBlind, long bigBlind, long ante, int maxNumberOfRaises = 3)
        {
            GameType = gameType;
            Limit = limit;
            SmallBlind = smallBlind;
            BigBlind = bigBlind;
            Ante = ante;
            MaxNumberOfRaises = maxNumberOfRaises;

        }


        /// <summary>
        /// Returns the number of hole cards dealt in this game
        /// </summary>
        public int NumberOfHoleCards
        {
            get
            {
                switch (GameType)
                {
                    case (GameTypeEnum.Holdem):
                        return 2;
                    case (GameTypeEnum.Omaha):
                        return 4;
                    default:
                        throw new InvalidOperationException("Unknown game");
                }
            }
        }

        /// <summary>
        /// Minimum number of cards the player needs to use to make a hand
        /// </summary>
        public int NumberOfHoleCardsMinPlay
        {
            get
            {
                switch (GameType)
                {
                    case (GameTypeEnum.Holdem):
                        return 0;
                    case (GameTypeEnum.Omaha):
                        return 2;
                    default:
                        throw new InvalidOperationException("Unknown game");
                }
            }
        }

        /// <summary>
        /// Maximum number of cards the player needs to use to make a hand
        /// </summary>
        public int NumberOfHoleCardsMaxPlay
        {
            get
            {
                switch (GameType)
                {
                    case (GameTypeEnum.Holdem):
                        return 2;
                    case (GameTypeEnum.Omaha):
                        return 2;
                    default:
                        throw new InvalidOperationException("Unknown game");
                }
            }
        }

        public override string ToString()
        {
            string anteString = Ante==0?"":string.Format("{0}/", Ante);
            string limitString = "";
            string gameString = "";
            
            switch (Limit)
            {
                case Enums.Limit.FixedLimit: limitString = "Limit"; break;
                case Enums.Limit.PotLimit: limitString = "Pot Limit"; break;
                case Enums.Limit.NoLimit: limitString = "No Limit"; break;
            }

            switch(GameType)
            {
                case GameTypeEnum.Holdem: gameString = "Texas Hold'em";break;
                case GameTypeEnum.Omaha: gameString = "Omaha"; break;
            }



            return string.Format("{0}{1}/{2} {3} {4}",
                anteString,
                SmallBlind,
                BigBlind,
                limitString,
                gameString);
                
        }


    }
}
