using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;


namespace PokerObjects
{
    public class TableEventArgs : EventArgs
    {
        public string StatusUpdate { get; private set; }
        public IList<Card> CommunityCards { get; private set; }
        public Pot Pot { get; private set; }
        public int ButtonPosition { get; private set; }
        public int ActionPosition { get; private set; }
        public string AdditionalLogText { get; private set; }

        public const int NO_POSITION = -1;

        public TableEventArgs(string statusUpdate, IList<Card> communityCards, Pot pot, 
            int buttonPosition = NO_POSITION, int actionPosition = NO_POSITION,
            string additionalLogText = "")
        {
            this.StatusUpdate = statusUpdate;
            this.CommunityCards = communityCards;
            this.Pot = pot;
            this.ButtonPosition = buttonPosition;
            this.ActionPosition = actionPosition;
            this.AdditionalLogText = additionalLogText;
        }
    }

    public class TablePlayerChangeEventArgs : EventArgs
    {
        public string StatusUpdate { get; private set; }
        public Player Player { get; private set; }
        public int Position { get; private set; }
        public bool IsLeaving { get; private set; } // joining or leaving table
        public TablePlayerChangeEventArgs(string statusUpdate, Player player, int position, bool isLeaving)
        {
            this.StatusUpdate = statusUpdate;
            this.Player = player;
            this.Position = position;
            this.IsLeaving = isLeaving;
        }
    }

    [DataContract]
    public class Table
    {
        #region constants
        const int NUMBER_OF_SEATS = 8; // not necessarily a player in every seat
        #endregion

        #region fields

        [DataMember]
        public Player[] Players { get; private set; }

        [DataMember]
        public List<Card> CommunityCards { get; private set; }

        [DataMember]
        public Pot Pot { get; private set; } // money in main and side pot(s)
        
        [DataMember]
        public int ButtonPosition { get; private set; }

        [DataMember]
        public int SmallBlindPosition { get; private set; }

        [DataMember]
        public int BigBlindPosition { get; private set; }

        [DataMember]
        public Game Game { get; private set; }

        [DataMember]
        public Deck Deck { get; private set; }

        [DataMember]
        public int ActionPosition { get; private set; }

        [DataMember]
        public HandHistory HandHistory { get; set; }

        [DataMember]
        public bool HandInProgress { get; private set; }

        [DataMember]
        public Enums.BettingRound CurrentBettingRound { get; private set; }

        [DataMember]
        public int PlayerAllInRaiseWithoutReopen { get; private set; }

        [DataMember]
        public bool BettingCompleted { get; private set; }

        [DataMember]
        public bool OptionForBigBlind { get; private set; }

        public bool PlayerHasLeft { get; set; }

        #endregion

        #region events

        public delegate Task TableUpdateEvent(object sender, TableEventArgs tableEventArgs);
        public delegate Task TablePlayerChangeEvent(object sender, TablePlayerChangeEventArgs tablePlayerChangeEventArgs);

        public event TablePlayerChangeEvent PlayerJoinOrLeave;
        public event TableUpdateEvent BeginNewHand;
        public event TableUpdateEvent PlayerHoleCardsDealt;
        public event TableUpdateEvent CommunityCardsDealt;
        public event TableUpdateEvent ActionOnPlayer;
        public event TableUpdateEvent PlayerBetHasChanged;
        public event TableUpdateEvent PotHasChanged;
        public event TableUpdateEvent PlayerShowsCards;
        public event TableUpdateEvent PlayerWins;
        public event TableUpdateEvent ButtonMoved;

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="SmallBlind"></param>
        /// <param name="BigBlind"></param>
        /// <param name="Ante"></param>
        /// <param name="buttonPosition"></param>
        public Table(Game game, int buttonPosition = Constants.NO_POSITION, HandHistory handHistory = null)
        {
            // Safeguard: if there is a hand in progress after resume from suspension,
            // make sure state has been fully restored
            if (HandInProgress)
            {
                if ((Game == null) || 
                    (Players == null) ||
                    (HandHistory == null) ||
                    (ButtonPosition == Constants.NO_POSITION))
                {
                    HandInProgress = false;
                }
            }

            if (!HandInProgress)
            {
                Game = game;
                Players = new Player[NUMBER_OF_SEATS];
                HandHistory = handHistory;
                ButtonPosition = buttonPosition;
            }

            if (HandHistory == null)
            {
                HandHistory = new HandHistory();
            }

        }

        /// <summary>
        /// Returns a string describing a position
        /// internally, positions are 0-based, so on screen they're
        /// 1-based. i.e. pos=0 => "position 1" etc.
        /// Also this can be modified later to add more fancy 
        /// position descriptions such as "cutoff", "button".
        /// </summary>
        /// <param name="position">position</param>
        /// <returns>string description the position</returns>
        public string GetPositionDescription(int position, bool upperCase = false)
        {
            if (position == Constants.NO_POSITION)
            {
                return "no position";
            }

            if (upperCase)
            {
                return string.Format("Position {0}", (position + 1));
            }
            else
            {
                return string.Format("position {0}", (position + 1));
            }

        }

        /// <summary>
        /// Removes all players from the table with 0 chips left
        /// </summary>
        private void RemoveBrokePlayers()
        {
            for (int position = 0; position < NUMBER_OF_SEATS; position++)
            {
                Player p = Players[position];
                if ((p!=null) && (p.Chipcount == 0))
                {
                    if (PlayerJoinOrLeave != null)
                    {
                        string statusUpdate = string.Format("Player {0} is out of chips and is leaving the table.", p.Name);
                        TablePlayerChangeEventArgs args = new TablePlayerChangeEventArgs(statusUpdate, p, position, true);
                        PlayerJoinOrLeave(this, args);
                    }
                    Players[position] = null;
                }
            }
        }

        /// <summary>
        /// Adds a player to the table in the next available position
        /// </summary>
        /// <param name="p">the player</param>
        /// <returns>the posion of the player</returns>
        public int AddPlayer(Player p)
        {
            // find available seat and seat player
            for (int position = 0; position < NUMBER_OF_SEATS; position++)
            {
                if (Players[position] == null)
                {
                    Players[position] = p;
                    if (PlayerJoinOrLeave != null)
                    {
                        string statusUpdate = string.Format("Player {0} is now sitting on this table in {1}.", 
                            p.Name, 
                            GetPositionDescription(position));
                        TablePlayerChangeEventArgs args = new TablePlayerChangeEventArgs(statusUpdate, p, position, false);
                        PlayerJoinOrLeave(this, args);
                    }
                    return position;
                }
            }

            throw new InvalidOperationException("No available seats");

        }

        /// <summary>
        /// Returns the number of players currently seated at the table
        /// </summary>
        public int NumberOfPlayers
        {
            get
            {
                return Players.Where(p => p != null).Count();
            }
        }

        /// <summary>
        /// Finds the position with the next active player
        /// </summary>
        /// <param name="position">current position</param>
        /// <param name="stopPosition">optional: stop when this position is reached (even if player is not active)</param>
        /// <returns>next position</returns>
        private int GetNextActivePosition(int position, int stopPosition = -1)
        {
            int startPosition = position;
            do
            {
                position = (position + 1) % NUMBER_OF_SEATS;

                if ((stopPosition > -1) && (stopPosition == position))
                {
                    break;
                }

                if (position == startPosition)
                {
                    throw new InvalidOperationException("No active position found.");
                }
            }
            while ((Players[position] == null) || (!Pot.PlayerIsActive(position)));
            return position;
        }

        /// <summary>
        /// Performs one betting round (can be pre-flop, flop, turn, or river)
        /// </summary>
        /// <param name="resume"></param>
        /// <returns>returns true if game is to be suspended</returns>
        private async Task<bool> BettingRound(bool resume)
        {
            if (!resume)
            {
                ActionPosition = GetNextActivePosition(ButtonPosition);
                PlayerAllInRaiseWithoutReopen = Constants.NO_POSITION;
                if (CurrentBettingRound == Enums.BettingRound.Preflop)
                {
                    await PostBlindsAndAntes();
                }
                FindInitialActionPosition();            
                BettingCompleted = false;
                OptionForBigBlind = false;
            }

            while ((!BettingCompleted) || (Pot.CheckIfPlayersStillNeedToCall()))
            {
                Player p = Players[ActionPosition];

                if (BettingCompleted && (ActionPosition == PlayerAllInRaiseWithoutReopen))
                {
                    break;
                }

                if (Pot.PlayerIsActive(ActionPosition))
                {
                    if (ActionOnPlayer!= null)
                    {
                        string currentBetInfo = "";
                        if (Pot.CurrentBet > 0)
                        {
                            currentBetInfo = string.Format("The bet is {0}.", Pot.CurrentBet);
                        }
                        string actionStatusUpdate = string.Format("Action on {0}. {1}", 
                            p.Name, 
                            currentBetInfo);
                        TableEventArgs args = new TableEventArgs(actionStatusUpdate, CommunityCards, Pot, ButtonPosition, ActionPosition);
                        await ActionOnPlayer(this, args);
                    }

                    // in case action is already completed and there are only some players who
                    // still need to call (after one or more all-ins), check if THIS player
                    // needs to call. Otherwise, skip.
                    if (BettingCompleted)
                    {
                        if (Pot.GetChipsPlayerInvested(ActionPosition) == Pot.CurrentBet)
                        {
                            continue;
                        }
                    }

                    // check if raise allowed
                    bool raiseAllowed = true;
                    if ((Pot.NumberOfRaises >= Game.MaxNumberOfRaises) || (BettingCompleted))
                    {
                        raiseAllowed = false;
                    }

                    long amount = await p.GetAction(Pot, ActionPosition, CurrentBettingRound, raiseAllowed);

                    if ((amount == Constants.ACTION_SUSPEND_GAME) || (PlayerHasLeft))
                    {
                        return true; 
                    }


                    long maxRaiseTotal = Pot.GetMaxRaise(ActionPosition, CurrentBettingRound);
                    long maxRaiseDiff = maxRaiseTotal - Pot.GetChipsPlayerInvested(ActionPosition);
                    if (amount > maxRaiseDiff)
                    {
                        amount = maxRaiseDiff;
                    }

                    long previousBet = Pot.CurrentBet;

                    string playerBetStatusUpdate = GetActionUpdateString(ActionPosition, amount);
                    Pot.Bet(ActionPosition, amount);
                    await RaisePlayerBetsAndPotUpdateEvents(ActionPosition, playerBetStatusUpdate);

                    if ((CurrentBettingRound == Enums.BettingRound.Preflop) && (OptionForBigBlind) && (amount == 0))
                    {
                        BettingCompleted = true;
                    }

                }

                if (Pot.NumberOfActivePlayers < 1) 
                {
                    BettingCompleted = true;
                }
                else
                {
                    ActionPosition = GetNextActivePosition(ActionPosition, Pot.OpeningPlayerPosition); 

                    // check if betting round completed
                    if (ActionPosition == Pot.OpeningPlayerPosition)
                    {
                        // special case: pre-flop, if no raise and back to BB, action is not completed yet (BB option)
                        if ((CurrentBettingRound != Enums.BettingRound.Preflop)
                            || ((ActionPosition != BigBlindPosition)
                            || (Pot.CurrentBet != Game.BigBlind)
                            || (Pot.NumberOfActivePlayers < 2)))
                        {
                            BettingCompleted = true;
                        }
                        else
                        {
                            OptionForBigBlind = true;
                        }
                    }
                }


            }


            // Rake money into the pot(s)...
            Pot.FinishBetting();
            string finishBettingStatusUpdate = string.Format("Betting completed.");
            await RaisePlayerBetsAndPotUpdateEvents(ActionPosition, finishBettingStatusUpdate);

            return false; // do not suspend game
            

        }

        private void FindInitialActionPosition()
        {
            if (NumberOfPlayers == 2)
            {
                // heads-up is a special case
                if (CurrentBettingRound == Enums.BettingRound.Preflop)
                {
                    ActionPosition = SmallBlindPosition;
                }
                else
                {
                    ActionPosition = BigBlindPosition;
                }
            }
            else
            {
                if (CurrentBettingRound == Enums.BettingRound.Preflop)
                {
                    ActionPosition = GetNextActivePosition(BigBlindPosition);
                }
            }

            if (CurrentBettingRound != Enums.BettingRound.Preflop)
            {
                Pot.OpeningPlayerPosition = ActionPosition;
            }
        }

        private async Task PostBlindsAndAntes()
        {
            // post antes
            if (Game.Ante > 0)
            {
                for (int i = 0; i < NUMBER_OF_SEATS; i++)
                {
                    if (Players[i] != null)
                    {
                        Pot.Bet(i, Game.Ante, Enums.BetType.Ante);
                    }
                }
                Pot.FinishBetting();
                if (PotHasChanged != null)
                {
                    string actionStatusUpdate = string.Format("Posting Antes ({0}).", Game.Ante);
                    TableEventArgs args = new TableEventArgs(actionStatusUpdate, CommunityCards, Pot, ButtonPosition, ActionPosition);
                    await PotHasChanged(this, args);
                }

            }

            // post blinds
            Pot.Bet(SmallBlindPosition, Game.SmallBlind, Enums.BetType.SmallBlind);
            Pot.Bet(BigBlindPosition, Game.BigBlind, Enums.BetType.BigBlind);

            if (PotHasChanged != null)
            {
                string actionStatusUpdate = string.Format("Posting Small Blind ({0}) and Big Blind ({1}).",
                    Game.SmallBlind, Game.BigBlind);
                TableEventArgs args = new TableEventArgs(actionStatusUpdate, CommunityCards, Pot, ButtonPosition, ActionPosition);
                await PotHasChanged(this, args);
            }
        }

        private async Task RaisePlayerBetsAndPotUpdateEvents(int actionPosition, string playerBetStatusUpdate)
        {
            TableEventArgs argsPlayerBet = new TableEventArgs(playerBetStatusUpdate, CommunityCards, Pot, ButtonPosition, actionPosition);

            if (PlayerBetHasChanged != null)
            {
                await PlayerBetHasChanged(this, argsPlayerBet);
            }

            if (PotHasChanged != null)
            {
                await PotHasChanged(this, argsPlayerBet);
            }
        }

        /// <summary>
        /// Returns the string describing the action of a player (check, call, bet, fold)
        /// </summary>
        /// <param name="actionPosition"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        private string GetActionUpdateString(int actionPosition, long amount)
        {
            Player p = Players[actionPosition];
            string actionStatusUpdate = string.Empty;
            long chipsPlayerInvested = Pot.GetChipsPlayerInvested(actionPosition);

            if (amount <= 0) // check or fold
            {
                if (amount < (Pot.CurrentBet - chipsPlayerInvested))
                {
                    actionStatusUpdate = string.Format("{0} folds.", p.Name);
                }
                else
                {
                    actionStatusUpdate = string.Format("{0} checks.", p.Name);
                }
            }
            else
            {
                if (Pot.CurrentBet == 0)
                {
                    actionStatusUpdate = string.Format("{0} bets {1}.", p.Name, amount);
                }
                else
                {
                    if (amount <= (Pot.CurrentBet - chipsPlayerInvested))
                    {
                        actionStatusUpdate = string.Format("{0} calls {1}.", p.Name, amount + chipsPlayerInvested);
                    }
                    else
                    {
                        actionStatusUpdate = string.Format("{0} raises to {1}.", p.Name, amount + chipsPlayerInvested);
                    }
                }
            }
            return actionStatusUpdate;
        }

        /// <summary>
        /// Returns the next betting round. e.g. for "flop", returns "turn"
        /// </summary>
        /// <param name="bettingRound"></param>
        /// <returns></returns>
        private Enums.BettingRound GetNextBettingRound(Enums.BettingRound bettingRound)
        {
            switch (bettingRound)
            {
                case Enums.BettingRound.Preflop:
                    return Enums.BettingRound.Flop;
                case Enums.BettingRound.Flop:
                    return Enums.BettingRound.Turn;
                case Enums.BettingRound.Turn:
                    return Enums.BettingRound.River;
                case Enums.BettingRound.River:
                    return Enums.BettingRound.Showdown;
            }

            throw new ArgumentException("bettingRound");

        }

        /// <summary>
        /// Plays an entire hand: deal cards, betting rounds, showdown
        /// (resume suspended hand, if applicable)
        /// </summary>
        /// <param name="deck"></param>
        /// <returns>true if the game should be suspended</returns>
        public async Task<bool> PlayHand(Deck deck = null)
        {
            bool resume = false;

            // Safeguard: if HandInProgress flag tells us that this is a hand
            // in progress which we should resume, however the hand's state hasn't been
            // fully restored, start a new hand instead.
            if (HandInProgress)
            {
                if ((Pot == null) || (CommunityCards == null))
                {
                    HandInProgress = false;
                }
            }

            if (HandInProgress)
            {
                resume = true;
                if (PotHasChanged != null)
                {
                    var args = new TableEventArgs(string.Empty, CommunityCards, Pot, ButtonPosition, ActionPosition);
                    await PotHasChanged(this, args);
                }
            }
            else
            {
                await StartNewHand(deck);

                // Ok, now we've reached a state where the hand is "in progress"
                // if we resume from suspension, we'll not start a new hand but
                // resume the interrupted one
                CurrentBettingRound = Enums.BettingRound.Preflop;
                HandInProgress = true;
            }


            while (HandInProgress)
            {
                while (CurrentBettingRound != Enums.BettingRound.Showdown)
                {
                    if (Pot.NumberOfPlayersNotFolded > 1)
                    {
                        await DealCommunityCards();
                        if (Pot.NumberOfActivePlayers > 1)
                        {
                            bool suspend = await BettingRound(resume);
                            if (suspend)
                            {
                                return true;
                            }
                            resume = false;

                        }
                    }
                    CurrentBettingRound = GetNextBettingRound(CurrentBettingRound); 
                }

                // All betting rounds completed
                await Showdown();
                RemoveBrokePlayers();
                HandInProgress = false;
                Pot = null;
                CommunityCards = null;

            }

            return false; // signal that the game is not to be suspended => play another hand!

        }

        /// <summary>
        /// Deal community cards on flop, turn, or river
        /// </summary>
        /// <returns></returns>
        private async Task DealCommunityCards()
        {
            string statusUpdate = string.Empty;

            if (CurrentBettingRound == Enums.BettingRound.Flop)
            {
                if (CommunityCards.Count == 0)
                {
                    Deck.DrawCard(); // burn card
                    Card flop1 = Deck.DrawCard(); CommunityCards.Add(flop1);
                    Card flop2 = Deck.DrawCard(); CommunityCards.Add(flop2);
                    Card flop3 = Deck.DrawCard(); CommunityCards.Add(flop3);
                    statusUpdate = string.Format("Flop: {0} {1} {2}", flop1, flop2, flop3);
                }
            }

            if (CurrentBettingRound == Enums.BettingRound.Turn)
            {
                if (CommunityCards.Count == 3)
                {
                    Deck.DrawCard(); // burn card
                    Card turn = Deck.DrawCard(); CommunityCards.Add(turn);
                    statusUpdate = string.Format("Turn: {0}", turn);
                }
            }

            if (CurrentBettingRound == Enums.BettingRound.River)
            {
                if (CommunityCards.Count == 4)
                {
                    Deck.DrawCard(); // burn card
                    Card river = Deck.DrawCard(); CommunityCards.Add(river);
                    statusUpdate = string.Format("River: {0}", river);
                }
            }
            
            // Raise event
            if (CommunityCardsDealt != null)
            {
                TableEventArgs args = new TableEventArgs(statusUpdate,CommunityCards,Pot);
                await CommunityCardsDealt(this, args);
            }
        }

        private async Task StartNewHand(Deck deck)
        {
            // Start a new hand
            if (NumberOfPlayers < 2)
            {
                throw new InvalidOperationException("Not enough players on this table.");
            }

            Pot = new Pot(this);
            CommunityCards = new List<Card>();

            long handNumber = 1;
            if (HandHistory != null)
            {
                await HandHistory.StartNewHand();
                handNumber = HandHistory.HandNumber;
            }

            // Take hole cards away from players
            TableEventArgs argsInit = new TableEventArgs(string.Format("Hand #{0}", handNumber),
                CommunityCards, Pot, Constants.NO_POSITION, Constants.NO_POSITION, this.ToString());

            foreach (Player p in Players)
            {
                if (p != null)
                {
                    p.DealHoleCards(null);
                }
            }

            if (BeginNewHand != null)
            {
                await BeginNewHand(this, argsInit);
            }

            // Move button 
            if (ButtonPosition == Constants.NO_POSITION)
            {
                ButtonPosition = DrawRandomButtonPosition();
            }
            else
            {
                ButtonPosition = GetNextActivePosition(ButtonPosition);
            }
            if (ButtonMoved != null)
            {
                TableEventArgs args = new TableEventArgs(
                    string.Format("Dealer button on {0}", Players[ButtonPosition].Name),
                    CommunityCards,
                    Pot,
                    ButtonPosition);
                await ButtonMoved(this, args);
            }

            // Set blind positions
            SmallBlindPosition = GetNextActivePosition(ButtonPosition);
            BigBlindPosition = GetNextActivePosition(SmallBlindPosition);
            if (NumberOfPlayers == 2) // heads-up SB/BB are reversed
            {
                int smallBlindPositionTmp = SmallBlindPosition;
                SmallBlindPosition = BigBlindPosition;
                BigBlindPosition = smallBlindPositionTmp;
            }


            // Deal
            Deck = (deck == null) ? new Deck() : deck;
            await DealCards();
        }

        /// <summary>
        /// Assigns the button randomly to one of the players on the table
        /// </summary>
        /// <returns></returns>
        private int DrawRandomButtonPosition()
        {
            Random rand = new Random();
            int buttonMoves = rand.Next(NumberOfPlayers);
            int pos = 0;
            if (buttonMoves > 0)
            {
                for (int i = 0; i < buttonMoves; i++)
                {
                    pos = GetNextActivePosition(pos);
                }
            }
            return pos;
        }

        /// <summary>
        /// Deals hole cards
        /// </summary>
        /// <returns></returns>
        private async Task DealCards()
        {
            for (int position = 0; position < NUMBER_OF_SEATS; position++)
            {
                Player p = Players[position];
                if (p != null)
                {
                    Card[] holeCards = new Card[Game.NumberOfHoleCards];
                    for (int i = 0; i < Game.NumberOfHoleCards; i++)
                    {
                        holeCards[i] = Deck.DrawCard();
                    }
                    p.DealHoleCards(holeCards);
                    if (PlayerHoleCardsDealt != null)
                    {
                        string statusUpdate = string.Format("Hole cards dealt to {0} in {1}", 
                            p.Name, 
                            GetPositionDescription(position));
                        TableEventArgs args = new TableEventArgs(statusUpdate, CommunityCards, Pot, ButtonPosition, position);
                        await PlayerHoleCardsDealt(this, args);
                    }
                }
            }

       }

        /// <summary>
        /// Determines the winner of a hand
        /// </summary>
        private async Task Showdown()
        {
            if (Pot.NumberOfPlayersNotFolded == 0)
            {
                throw new InvalidOperationException("Something's wrong- EVERYONE folded. Cannot determine a winner.");
            }

            int winningPlayerPosition = Constants.NO_POSITION;
            if (Pot.NumberOfPlayersNotFolded == 1)
            {
                // no need for an actual "showdown" if everyone except one player folded -
                // the remaining player just wins the entire pot
                for (int position = 0; position < Constants.MAX_POSITIONS; position++)
                {
                    if ((Players[position] != null) && (!Pot.PlayerHasFolded(position)))
                    {
                        winningPlayerPosition = position;
                        break;
                    }
                }
                if (winningPlayerPosition == Constants.NO_POSITION)
                {
                    // should not occur
                    throw new InvalidOperationException("Could not identify winning player.");
                }

            }

            for (int potIndex = Pot.NumberOfPots - 1; potIndex >= 0; potIndex--)
            {
                Hand.HandValue bestHand = null;
                List<int> playersWithBestHand = new List<int>();

                if (winningPlayerPosition != Constants.NO_POSITION)
                {
                    // everyone folded except one player - so this player now wins this pot
                    playersWithBestHand.Add(winningPlayerPosition);
                }
                else
                {
                    // actual showdown: compare hands
                    // TODO start at button + 1
                    for (int i = 0; i < NUMBER_OF_SEATS; i++)
                    {
                        if (Players[i] == null)
                            continue;

                        int posPlayerToShow = i;
                        Player p = Players[posPlayerToShow];
                        if (Pot.PlayerIsInPot(potIndex, posPlayerToShow))
                        {
                            Hand.HandValue hv =
                                Hand.EvaluateHandWithBoard(p.HoleCards, CommunityCards);

                            if ((bestHand != null) && (hv.CompareTo(bestHand) == 0))
                            {
                                playersWithBestHand.Add(posPlayerToShow);
                            }

                            if ((bestHand == null) || (hv.CompareTo(bestHand) > 0))
                            {
                                bestHand = hv;
                                playersWithBestHand.Clear();
                                playersWithBestHand.Add(posPlayerToShow);
                            }

                            p.ShowsHoleCards = true;
                            if (PlayerShowsCards != null)
                            {
                                string statusUpdate = string.Format("{0} shows {1} {2} for {3}", p.Name, p.HoleCards[0], p.HoleCards[1], hv);
                                TableEventArgs args = new TableEventArgs(statusUpdate, CommunityCards, Pot, ButtonPosition, posPlayerToShow);
                                await PlayerShowsCards(this, args);
                            }
                        }
                    }
                }

                // and the winner is...
                if (playersWithBestHand.Count == 1)
                {
                    long chips = Pot.GetChipsInPot(potIndex);
                    Player p = Players[playersWithBestHand[0]];
                    string statusUpdate = string.Format("{0} wins a pot of {1} chips!", p.Name, chips);
                    if (PlayerWins != null)
                    {
                        TableEventArgs args = new TableEventArgs(statusUpdate, CommunityCards, Pot);
                        await PlayerWins(this, args);
                    }
                    p.ReceiveChips(Pot.RemoveDeadMoney(potIndex, chips));
                    await RaisePlayerBetsAndPotUpdateEvents(Constants.NO_POSITION, statusUpdate); 
                }
                else
                {
                    // chop the pot
                    long chips = Pot.GetChipsInPot(potIndex);
                    long chopChips = chips / playersWithBestHand.Count;
                    bool extraChip = (chips * playersWithBestHand.Count < chips);
                    for (int i = 0; i < playersWithBestHand.Count; i++)
                    {
                        Player p = Players[playersWithBestHand[i]];
                        long winChips = chopChips;
                        if (extraChip && (i == 0))
                        {
                            winChips++;
                        }
                        p.ReceiveChips(Pot.RemoveDeadMoney(potIndex, winChips));
                        string statusUpdate = string.Format("{0} splits the pot and wins {1} chips!", p.Name, winChips);
                        if (PlayerWins != null)
                        {
                            TableEventArgs args = new TableEventArgs(statusUpdate, CommunityCards, Pot);
                            await PlayerWins(this, args);
                        }
                        await RaisePlayerBetsAndPotUpdateEvents(Constants.NO_POSITION, statusUpdate);

                    }

                }


            }
        }

        /// <summary>
        /// returns a description of the game and all the players
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Game.ToString());
            for (int pos = 0; pos < NUMBER_OF_SEATS; pos++)
            {
                Player p = Players[pos];
                if (p != null)
                {
                    string positionString = GetPositionDescription(pos, true);
                    sb.AppendFormat("{0}: {1} ({2} chips)",
                        positionString,
                        p.Name,
                        p.Chipcount);
                    sb.AppendLine();
                }

            }
            return sb.ToString();
        }

        

    }
}
