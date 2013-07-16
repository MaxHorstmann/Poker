using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using PokerObjects;

namespace Poker
{
    [DataContract]
    public class HumanPlayer : Player
    {
        public delegate Task<long> GetActionFromUI(HumanPlayer p, long currentBet, long chipsPlayerInvested, long minRaise, long maxRaise, 
            long defaultRaise, long stepFrequency, bool raiseAllowed);

        
        /// <summary>
        /// Delegate to be invoked when it's the player's turn
        /// </summary>
        public GetActionFromUI GetActionFromUIDelegate { get; set; }


        public HumanPlayer(string name, long chipcount)
            : base(name, chipcount)
        {
        }



        public override async Task<long> GetAction(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed)
        {

            long minRaise = pot.GetMinRaise(bettingRound);
            long maxRaise = pot.GetMaxRaise(playerIndex, bettingRound);
            long defaultRaise = 0;
            long stepFrequency = pot.Table.Game.BigBlind;

            if (pot.Table.Game.Limit == Enums.Limit.NoLimit)
            {
                defaultRaise = pot.CurrentBet > 0 ? 3 * pot.CurrentBet : 3 * pot.Table.Game.BigBlind;
                stepFrequency = pot.Table.Game.BigBlind;
            }

            if (pot.Table.Game.Limit == Enums.Limit.FixedLimit)
            {
                defaultRaise = maxRaise;
            }

            if (pot.Table.Game.Limit == Enums.Limit.PotLimit)
            {
                throw new NotImplementedException("todo implement pot limit");
            }

            if (minRaise > maxRaise)
            {
                minRaise = maxRaise;
            }

            if (defaultRaise > maxRaise)
            {
                defaultRaise = maxRaise;
            }

            long bet = await GetActionFromUIDelegate(this, pot.CurrentBet,pot.GetChipsPlayerInvested(playerIndex),  
                minRaise, maxRaise, defaultRaise, stepFrequency, raiseAllowed);
            
            return bet;

        }
    }
}
