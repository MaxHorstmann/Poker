using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PokerObjects
{
    [DataContract]
    public class Pot
    {
        const long NOT_IN_THIS_POT = -1;

        [DataMember]
        private IList<long[]> Chips { get; set; }


        public Table Table { get; set; }

        public int NumberOfPots
        {
            get
            {
                if (Chips == null)
                {
                    return 0;
                }
                return Chips.Count;
            }
        }

        
        /// <summary>
        /// Tracks whether the current bet is exactly the big blind.
        /// This is necessary because in some corner cases, the amount to call is
        /// equal to the BB although the player sitting in the BB position didn't actually
        /// post a full BB (because he's all-in).
        /// </summary>
        [DataMember]
        public bool CurrentBetIsBigBlind { get; private set; }

        [DataMember]
        public long PreviousRaise { get; private set; }

        [DataMember]
        public int NumberOfRaises { get; private set; }

        [DataMember]
        public int OpeningPlayerPosition { get; set; }

        /// <summary>
        /// Gets the minimum raise currently allowd for any player
        /// </summary>
        /// <returns></returns>
        public long GetMinRaise(Enums.BettingRound bettingRound)
        {
            if (Table.Game.Limit == Enums.Limit.NoLimit)
            {
                return CurrentBet > 0 ? (CurrentBet+PreviousRaise): Table.Game.BigBlind; 
            }

            if (Table.Game.Limit == Enums.Limit.FixedLimit)
            {
                if ((bettingRound == Enums.BettingRound.Preflop)
                    || (bettingRound == Enums.BettingRound.Flop))
                {
                    return CurrentBet + Table.Game.BigBlind;
                }
                else
                {
                    return CurrentBet + 2 * Table.Game.BigBlind;
                }
            }

            throw new NotImplementedException("only no limit and fixed limit supported");

        }

        /// <summary>
        /// Gets the maximum raise currently allowed for a given player
        /// </summary>
        /// <param name="playerIndex"></param>
        /// <returns></returns>
        public long GetMaxRaise(int playerIndex, Enums.BettingRound bettingRound)
        {
            Player p = Table.Players[playerIndex];

            if (Table.Game.Limit == Enums.Limit.NoLimit)
            {
                return p.Chipcount + GetChipsPlayerInvested(playerIndex);
            }

            if (Table.Game.Limit == Enums.Limit.FixedLimit)
            {
                if ((bettingRound == Enums.BettingRound.Preflop)
                    || (bettingRound == Enums.BettingRound.Flop))
                {
                    return CurrentBet + Table.Game.BigBlind;
                }
                else
                {
                    return CurrentBet + 2 * Table.Game.BigBlind;
                }
            }

            throw new NotImplementedException("only no limit and fixed limit supported");


        }

        /// <summary>
        /// returns the number of chips in the DEAD MONEY of a given pot
        /// </summary>
        /// <param name="potIndex"></param>
        /// <returns></returns>
        public long GetChipsInPot(int potIndex)
        {
            return Chips[potIndex][Constants.MAX_POSITIONS];
        }

        /// <summary>
        /// Checks whether a player is involved in a given pot
        /// (i.e. can potentially win it)
        /// </summary>
        public bool PlayerIsInPot(int potIndex, int playerIndex)
        {
            return (Chips[potIndex][playerIndex] != NOT_IN_THIS_POT);
        }

        /// <summary>
        /// Returns a strin containing a description of the pot:
        /// main or side pot, players involved
        /// </summary>
        /// <param name="potIndex"></param>
        /// <returns></returns>
        public string GetPotDescription(int potIndex, bool includePlayerNames = false)
        {
            StringBuilder sb = new StringBuilder();

            if (potIndex == 0)
            {
                sb.Append("Main ");
            }
            else
            {
                sb.Append("Side ");
            }

            if (includePlayerNames)
            {
                sb.Append(" (");
                bool playerNamesAdded = false;
                for (int playerIndex = 0; playerIndex < Constants.MAX_POSITIONS; playerIndex++)
                {
                    if (PlayerIsInPot(potIndex, playerIndex))
                    {
                        if (playerNamesAdded)
                        {
                            sb.Append(",");
                        }
                        sb.Append(Table.Players[playerIndex].Name);
                        playerNamesAdded = true;
                    }
                }

                sb.Append(")");
            }


            return sb.ToString();
        }

        //public int NumberOfActivePlayers
        //{
        //    get
        //    {
        //        int count = 0;
        //        for (int i = 0; i < NumberOfPlayers; i++)
        //        {
        //            if (PlayerIsActive(i))
        //            {
        //                count++;
        //            }
        //        }
        //        return count;
        //    }
        //}

        /// <summary>
        /// Returns the number of players which haven't folded yet,
        /// i.e. can still win the hand.
        /// </summary>
        public int NumberOfPlayersNotFolded
        {
            get
            {
                int count = 0;
                for (int position=0;position<Constants.MAX_POSITIONS;position++)
                {
                    if ((Table.Players[position] != null) && (!PlayerHasFolded(position)))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Returns the number of players which are active, i.e.  
        /// which haven't folded and are not all-in
        /// </summary>
        public int NumberOfActivePlayers
        {
            get
            {
                int count = 0;
                for (int position = 0; position < Constants.MAX_POSITIONS; position++)
                {
                    if ((!PlayerIsAllIn(position) && (!PlayerHasFolded(position))))
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public long TotalSize
        {
            get
            {
                if (Chips == null)
                {
                    return 0;
                }
                long totalPotSize = 0;
                foreach (long[] chips in Chips)
                {
                    foreach (long c in chips)
                    {
                        if (c != NOT_IN_THIS_POT)
                        {
                            totalPotSize += c;
                        }
                    }
                }
                return totalPotSize;
            }
        }

        /// <summary>
        /// Returns the current total bet amount to be called
        /// </summary>
        public long CurrentBet
        {
            get
            {
                if (Chips == null)
                {
                    return 0;
                }

                long currentBet = 0;
                foreach (long[] chips in Chips)
                {
                    long max = 0;
                    for (int i=0; i<Constants.MAX_POSITIONS; i++)
                    {
                        if (chips[i] != NOT_IN_THIS_POT)
                        {
                            max = Math.Max(max, chips[i]);
                        }
                    }
                    currentBet += max;
                }
                if ((CurrentBetIsBigBlind) && (currentBet < Table.Game.BigBlind))
                {
                    return Table.Game.BigBlind;
                }
                return currentBet;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public Pot(Table table)
        {
            if (table == null)
            {
                throw new ArgumentNullException("table");
            }
            this.Table = table;
            Chips = new List<long[]>();
            PreviousRaise = 0;
            NumberOfRaises = 0;
            Chips.Add(CreateNewPot()); // the main pot
        }



        /// <summary>
        /// Creates a new pot (main pot first time, otherwise new side pot) 
        /// (doesn't add it to the list of pots yet though)
        /// </summary>
        private long[] CreateNewPot()
        {
            long[] chips = new long[Constants.MAX_POSITIONS + 1]; // one extra entry for dead money
            for (int i=0; i<Constants.MAX_POSITIONS; i++)
            {
                if (Table.Players[i] == null)
                {
                    chips[i] = NOT_IN_THIS_POT;
                }
            }
            return chips;
        }

        /// <summary>
        /// Checks weather a player has folded, i.e. is not involved anymore
        /// in any of the pots
        /// </summary>
        /// <param name="position">position of the player</param>
        /// <returns>true if the player has folded</returns>
        public bool PlayerHasFolded(int position)
        {
            if (Chips == null)
            {
                return false;
            }
            foreach (long[] chips in Chips)
            {
                if (chips[position] != NOT_IN_THIS_POT)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if a player is all-in, i.e. has no more chips.
        /// otherwise, returns false.
        /// </summary>
        /// <param name="position">position of the player</param>
        /// <returns>true if player is all in</returns>
        public bool PlayerIsAllIn(int position)
        {
            return ((Table.Players[position] != null) && (Table.Players[position].Chipcount == 0));
        }

        /// <summary>
        /// checks if player is still active, i.e. didn't fold or 
        /// made an all-in call
        /// </summary>
        /// <param name="position">index of the player</param>
        /// <returns>true if the player is still active, otherwise false</returns>
        public bool PlayerIsActive(int position)
        {
            return !(PlayerHasFolded(position) || PlayerIsAllIn(position));
        }

        /// <summary>
        /// Helper function: find the current bet in one of the pots
        /// </summary>
        /// <param name="chips"></param>
        /// <returns></returns>
        private long GetCurrentPotBet(long[] chips)
        {
            if (chips == null)
            {
                throw new ArgumentNullException();
            }
            if (chips.Length != Constants.MAX_POSITIONS + 1)
            {
                throw new ArgumentOutOfRangeException();
            }

            long max = 0;
            for (int i = 0; i < Constants.MAX_POSITIONS; i++)
            {
                if (chips[i] != NOT_IN_THIS_POT)
                {
                    max = Math.Max(chips[i], max);
                }
            }
            return max;
        }

        public long GetChipsPlayerInvested(int playerIndex)
        {
            long playerInvestedTotal = 0;
            foreach (long[] chips in Chips)
            {
                if (chips[playerIndex] != NOT_IN_THIS_POT)
                {
                    playerInvestedTotal += chips[playerIndex];
                }
            }
            return playerInvestedTotal;
        }


        /// <summary>
        /// Bets additional chips for a player, creating side pots as necessary
        /// </summary>
        /// <param name="playerPosition"></param>
        /// <param name="amount"></param>
        /// <param name="blindBet">indicates whether this is a small blind bet (which is 
        /// special because it doesn't change the current bet / raise)</param>
        public void Bet(int playerPosition, long amount, Enums.BetType betType = Enums.BetType.Voluntary)
        {
            if (betType == Enums.BetType.Undefined)
            {
                throw new InvalidOperationException("Undefined betType");
            }

            if (PlayerIsAllIn(playerPosition) && (betType == Enums.BetType.Voluntary))
            {
                throw new InvalidOperationException("Player is all-in and can't make a voluntary bet.");
            }
            if (PlayerHasFolded(playerPosition))
            {
                throw new InvalidOperationException("Player has folded and can't act.");
            }

            Player player = Table.Players[playerPosition];

            bool reopen = false;

            // Make sure player bets only chips he has
            amount = Math.Min(amount, player.Chipcount); 

            // check how much player has invested yet across all pots
            long playerInvestedTotal = GetChipsPlayerInvested(playerPosition);

            // check amount player needs to put in for at least a call
            long diffTotal = CurrentBet - playerInvestedTotal;

            // Is player putting in enough chips for at least a call?
            if (amount < diffTotal)
            {
                // Player didn't put in enough chips
                // So, did he go all in?
                if (amount < player.Chipcount)
                {
                    // no ? then it's an automatic fold
                    Fold(playerPosition);
                }
                else
                {
                    // yes? then it's an all-in call by the player!                    
                    // go over pots and check which one to split for a new side-pot
                    long[] newSidePot = null;
                    foreach (long[] chips in Chips)
                    {
                        if (amount == 0)
                        {
                            chips[playerPosition] = NOT_IN_THIS_POT;
                            continue;
                        }

                        long potBet = GetCurrentPotBet(chips);
                        long playerInvestedPot = chips[playerPosition];
                        if (playerInvestedPot == NOT_IN_THIS_POT)
                        {
                            continue;
                        }
                        long diffPot = potBet - playerInvestedPot;
                        bool createSidePot = (amount < diffPot);
                        
                        if (!createSidePot)
                        {
                            // player has enough chips to call THIS pot
                            // so don't create a side-pot yet
                            amount -= diffPot;
                            player.GetBetChips(diffPot);
                            chips[playerPosition] += diffPot;
                        }
                        else
                        {
                            // now THIS is the pot we need to split and create
                            // a new side-pot
                            chips[playerPosition] += amount;
                            player.GetBetChips(amount);


                            newSidePot = CreateNewPot();

                            // players which didn't have a stake in the pot we're splitting
                            // won't have a stake in the new side pot either
                            for (int j = 0; j < Constants.MAX_POSITIONS; j++)
                            {
                                if (chips[j] == NOT_IN_THIS_POT)
                                {
                                    newSidePot[j] = NOT_IN_THIS_POT;
                                }
                            }
                            
                            newSidePot[playerPosition] = NOT_IN_THIS_POT;
                            long newBet = playerInvestedPot + amount;
                            amount = 0;

                            // Move other players' bets from current pot into new side pot
                            for (int j = 0; j < Constants.MAX_POSITIONS; j++)
                            {
                                if ((chips[j] != NOT_IN_THIS_POT) && (chips[j] > newBet))
                                {
                                    long moveChips = (chips[j] - newBet);
                                    chips[j] -= moveChips;
                                    newSidePot[j] += moveChips;
                                }

                            }

                        }    
                    }

                    if (newSidePot != null)
                    {
                        Chips.Add(newSidePot);
                    }

                }               
            }
            else
            {
                // player did put in enough chips for at least a call, maybe even a raise
                // go over all the pots and distribute the chips
                long previousBet = CurrentBet;

                for (int potIndex = 0; potIndex < Chips.Count; potIndex++)
                {
                    long[] chips = Chips[potIndex];
                    bool potOpen = (potIndex == Chips.Count - 1); // can only bet into the last pot (and call others)
                    long potBet = GetCurrentPotBet(chips);
                    
                    long potCapDueToAllInPlayer = 0;
                    for (int allinPlayerPosition = 0; allinPlayerPosition < Constants.MAX_POSITIONS; allinPlayerPosition++)
                    {
                        if (allinPlayerPosition == playerPosition)
                        {
                            continue;
                        }
                        if (PlayerIsAllIn(allinPlayerPosition) && PlayerIsInPot(potIndex, allinPlayerPosition))
                        {
                            Player p = Table.Players[allinPlayerPosition];
                            potCapDueToAllInPlayer = chips[allinPlayerPosition];
                        }
                    }
                    

                    // check how much player has invested yet
                    long playerInvestedPot = chips[playerPosition];
                    if (playerInvestedPot == NOT_IN_THIS_POT)
                    {
                        continue;
                    }

                    // how much does player still have to pay for at least a call
                    long diffPot = potBet - playerInvestedPot;

                    if (!potOpen)
                    {
                        // this pot is already closed; see if players still needs to call
                        if (diffPot > 0)
                        {
                            if (amount >= diffPot)
                            {
                                amount -= diffPot;
                                chips[playerPosition] += diffPot;
                                player.GetBetChips(diffPot);
                            }
                            else
                            {
                                // this shouldn't occur... we checked in the beginning if player has enough chips
                                // to at least call all the pots
                                throw new InvalidOperationException();
                            }
                        }
                    }
                    else
                    {
                        // this is the active pot - bet all the remaining chips 
                        chips[playerPosition] += amount;
                        player.GetBetChips(amount);

                        // check for special case: player raised or called another all-in player
                        if ((potCapDueToAllInPlayer > 0) && (playerInvestedPot + amount >= potCapDueToAllInPlayer))
                        {
                            // need to split the pot: we raised over a player who just went all-in
                            long[] newSidePot = CreateNewPot();
                            Chips.Add(newSidePot);

                            // Move other players' bets exceeding the pot cap from current pot into new side pot
                            for (int j = 0; j < Constants.MAX_POSITIONS; j++)
                            {
                                if (chips[j] == NOT_IN_THIS_POT)
                                {
                                    newSidePot[j] = NOT_IN_THIS_POT;
                                    continue;
                                }
                                if (PlayerIsAllIn(j) && (j!=playerPosition))
                                {
                                    newSidePot[j] = NOT_IN_THIS_POT;
                                    continue;
                                }
                                if (chips[j] > potCapDueToAllInPlayer)
                                {
                                    long moveChips = (chips[j] - potCapDueToAllInPlayer);
                                    chips[j] -= moveChips;
                                    newSidePot[j] += moveChips;
                                }
                            }
                        }

                        // check if this reopened the pot
                        amount = 0;
                        long newRaise = (CurrentBet - previousBet);
                        if (betType == Enums.BetType.BigBlind)
                        {
                            reopen = true;
                            PreviousRaise = CurrentBet;
                            NumberOfRaises++;
                        }
                        if (betType == Enums.BetType.Voluntary)
                        {
                            reopen = (CurrentBet > 0) && (newRaise >= PreviousRaise);
                            if (reopen)
                            {
                                PreviousRaise = newRaise;
                                NumberOfRaises++;
                            }
                        }


                    }

                    
                }


            }

            if (reopen)
            {
                OpeningPlayerPosition = playerPosition;
            }

            CurrentBetIsBigBlind = (betType == Enums.BetType.BigBlind);

        }

        
        /// <summary>
        /// Folds a player and moves all his previous bet into dead money
        /// </summary>
        /// <param name="playerIndex">index of the player</param>
        public void Fold(int playerIndex)
        {
            foreach (long[] chips in Chips)
            {
                if (chips[playerIndex] != NOT_IN_THIS_POT)
                {
                    chips[Constants.MAX_POSITIONS] += chips[playerIndex];
                    chips[playerIndex] = NOT_IN_THIS_POT;

                }
            }

        }

        /// <summary>
        /// Makes a player call the current bet
        /// </summary>
        /// <param name="playerIndex"></param>
        public void Call(int playerIndex)
        {
            long totalChips = 0;
            if (CurrentBet > 0)
            {
                long chipsInPots = 0;
                foreach (long[] chips in Chips)
                {
                    if (chips[playerIndex] != NOT_IN_THIS_POT)
                    {
                        chipsInPots += chips[playerIndex];
                    }
                }
                totalChips = Math.Min(CurrentBet - chipsInPots, Table.Players[playerIndex].Chipcount);
                Bet(playerIndex, totalChips);
            }
        }

        /// <summary>
        /// Checks whether there is at least one player who still needs to call the
        /// current bet (and therefore the action is not complete yet)
        /// </summary>
        /// <returns></returns>
        public bool CheckIfPlayersStillNeedToCall()
        {
            foreach (long[] chips in Chips)
            {
                // test if all players have called 
                long callAmount = long.MinValue;
                for (int i = 0; i < Constants.MAX_POSITIONS; i++)
                {
                    if (chips[i] != NOT_IN_THIS_POT)
                    {
                        if (callAmount == long.MinValue)
                        {
                            callAmount = chips[i];
                        }
                        else
                        {
                            if (callAmount != chips[i])
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Move all chips to dead money
        /// </summary>
        public void FinishBetting()
        {
            if (CheckIfPlayersStillNeedToCall())
            {
                throw new InvalidOperationException("Cannot finish betting, at least one player still needs to call.");
            }

            foreach (long[] chips in Chips)
            {
                for (int i = 0; i < Constants.MAX_POSITIONS; i++)
                {
                    if (chips[i] != NOT_IN_THIS_POT)
                    {
                        chips[Constants.MAX_POSITIONS] += chips[i];
                        chips[i] = 0;
                    }
                }
            }

            PreviousRaise = 0;
            NumberOfRaises = 0;

            ResolveUncontestedSidePots();
        }

        /// <summary>
        /// Takes chips out of pot (to hand them to a player)
        /// </summary>
        /// <param name="potIndex"></param>
        /// <param name="chipCount"></param>
        /// <returns></returns>
        public long RemoveDeadMoney(int potIndex, long chipCount)
        {
            long chips = Chips[potIndex][Constants.MAX_POSITIONS];
            if (chips < chipCount)
            {
                throw new ArgumentOutOfRangeException("Invalid chipCount");
            }
            Chips[potIndex][Constants.MAX_POSITIONS] -= chipCount;
            return chipCount;
        }

        /// <summary>
        /// If there are any side pots with only one player in it,
        /// remove them and return money to the player
        /// </summary>
        private void ResolveUncontestedSidePots()
        {
            if (Chips.Count < 2)
            {
                return; // no side pots
            }

            List<long[]> potsToRemove = new List<long[]>();

            for (int potIndex = 1; potIndex < Chips.Count; potIndex++)
            {
                long[] chips = Chips[potIndex];
                int playerInPotFound = -1;
                bool potIsUncontested = true;
                for (int position = 0; position < Constants.MAX_POSITIONS; position++)
                {
                    if (!potIsUncontested)
                        continue;

                    if (chips[position] != NOT_IN_THIS_POT)
                    {
                        if (playerInPotFound == -1)
                        {
                            playerInPotFound = position;
                        }
                        else 
                        {
                            potIsUncontested = false;
                        }
                    }
                }
                if (potIsUncontested)
                {
                    long chipCount = chips[Constants.MAX_POSITIONS];
                    Table.Players[playerInPotFound].ReceiveChips(chipCount);
                    chips[Constants.MAX_POSITIONS] = 0;
                    potsToRemove.Add(chips);
                }
            }

            foreach (long[] potToRemove in potsToRemove)
            {
                Chips.Remove(potToRemove);
            }
            


        }

    //    /// <summary>
    //    /// Provides a string representation of current pots
    //    /// </summary>
    //    /// <returns></returns>
    //    public override string ToString()
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        foreach (Player p in Table.Players)
    //        {
    //            if (p != null)
    //            {
    //                sb.AppendFormat("{0}\t", p.Name);
    //            }
    //        }
    //        sb.AppendLine();
    //        foreach (Player p in Table.Players)
    //        {
    //            if (p != null)
    //            {
    //                sb.AppendFormat("{0}\t", p.Chipcount);
    //            }
    //        }
    //        sb.AppendLine();

    //        sb.AppendLine("-----------------------------------------------");
                    
    //        foreach (long[] chips in _chips)
    //        {
    //            for (int i = 0; i < Table.NumberOfPlayers; i++)
    //            {
    //                if (chips[i] != NOT_IN_THIS_POT)
    //                {
    //                    sb.AppendFormat("{0}\t", chips[i]);
    //                }
    //                else
    //                {
    //                    sb.AppendFormat("out\t");
    //                }
    //            }
    //            sb.AppendFormat("\t({0})\n", chips[Table.NumberOfPlayers]);
    //        }
    //        sb.AppendLine("-----------------------------------------------");

    //        sb.AppendFormat("Current bet: {0}\n", CurrentBet);
    //        return sb.ToString();
    //    }

    }

    
}
