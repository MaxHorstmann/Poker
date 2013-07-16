using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using PokerObjects;

namespace PokerObjectsTest
{
    [DataContract]
    public class TestPlayer : Player
    {
        public delegate long GetActionDelegate(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed);

        public event GetActionDelegate OnGetAction = null;

        public TestPlayer(string name, long chipcount)
            : base(name, chipcount)
        {
        }

        public override Task<long> GetAction(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed)
        {
            long action = 0;
            if (OnGetAction != null)
            {
                action = OnGetAction(pot, playerIndex, bettingRound, raiseAllowed);
            }
            return Task.FromResult<long>(action);

        }
    }
}
