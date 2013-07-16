using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;


namespace PokerObjects
{
    [DataContract]
    public class ComputerPlayer : Player
    {

        Random rand = null;

        public ComputerPlayer(string name, long chipcount)
            : base(name, chipcount)
        {
        }

        public override async Task<long> GetAction(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed)
        {
            rand = new Random();  // .net bug?? shouldn't be necessary

            // artificial delay
            await Task.Delay(TimeSpan.FromMilliseconds(rand.Next(500,1000)));

            long chipsAlreadyInvested = pot.GetChipsPlayerInvested(playerIndex);

            // AI : random action
            int actionType = rand.Next(100);

            if (!raiseAllowed)
            {
                return 0; // TODO....
            }

            if (pot.CurrentBet == 0)
            {
                // unopened pot
                if (actionType < 50)
                {
                    return 0; // check 
                }
                else
                {
                    return 2 * pot.Table.Game.BigBlind; 
                }
            }
            else
            {
                // opened pot
                if (actionType < 20)
                    return 0; // fold
                //return Chipcount;
                if (actionType <= 100)
                {
                    return pot.CurrentBet - chipsAlreadyInvested; // call
                }
                return Math.Min(Chipcount, 3 * pot.CurrentBet); // raise

            }
           
        }
    }
}
