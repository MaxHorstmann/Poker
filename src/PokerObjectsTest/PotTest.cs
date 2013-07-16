using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PokerObjects;

namespace PokerObjectsTest
{
    [TestClass]
    public class PotTest
    {
        [TestMethod]
        public void TestMinRaise()
        {
            long Ante = 1;
            long SB = 3;
            long BB = 5;
            Game game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, SB, BB, Ante, 4);
            Table table = new Table(game);
            table.AddPlayer(new TestPlayer("P1", 100));
            table.AddPlayer(new TestPlayer("P2", 100));
            table.AddPlayer(new TestPlayer("P3", 100));
            table.AddPlayer(new TestPlayer("P4", 100));
            table.AddPlayer(new TestPlayer("P5", 100));
            Pot pot = new Pot(table);

            Assert.AreEqual(pot.CurrentBet, 0);
            Assert.AreEqual(pot.PreviousRaise, 0);
            Assert.AreEqual(pot.NumberOfRaises, 0);


            // antes
            for (int i=0;i<table.NumberOfPlayers;i++)
            {
                pot.Bet(i,Ante, Enums.BetType.Ante);
            }
            pot.FinishBetting();
            Assert.AreEqual(pot.CurrentBet, 0);
            Assert.AreEqual(pot.PreviousRaise, 0);
            Assert.AreEqual(pot.NumberOfRaises, 0);

            int pos = 0;

            // small blind
            pot.Bet(pos, SB, Enums.BetType.SmallBlind);
            Assert.AreEqual(pot.CurrentBet, SB);
            Assert.AreEqual(pot.PreviousRaise, 0); // SB doesn't count as "raise"

            // big blind
            pos++;
            pot.Bet(pos, BB, Enums.BetType.BigBlind);
            Assert.AreEqual(pot.CurrentBet, BB);
            Assert.AreEqual(pot.PreviousRaise, BB);
            Assert.AreEqual(pot.OpeningPlayerPosition, pos);
            Assert.AreEqual(pot.GetMinRaise(Enums.BettingRound.Preflop), 2 * BB);
            Assert.AreEqual(pot.NumberOfRaises, 1);

            // next player calls
            pos++;
            pot.Bet(pos, pot.CurrentBet);
            Assert.AreEqual(pot.CurrentBet, BB);
            Assert.AreEqual(pot.PreviousRaise, BB);
            Assert.AreEqual(pot.OpeningPlayerPosition, pos-1); 
            Assert.AreEqual(pot.GetMinRaise(Enums.BettingRound.Preflop), 2 * BB);
            Assert.AreEqual(pot.NumberOfRaises, 1);

            // next player raises - now there should be new opening player, bet, and previous raise
            pos++;
            pot.Bet(pos, 3 * pot.CurrentBet);
            Assert.AreEqual(pot.CurrentBet, 3 * BB);
            Assert.AreEqual(pot.PreviousRaise, 2 * BB);
            Assert.AreEqual(pot.OpeningPlayerPosition, pos);
            Assert.AreEqual(pot.GetMinRaise(Enums.BettingRound.Preflop), 5 * BB);
            Assert.AreEqual(pot.NumberOfRaises, 2);

            // and let's do one more re-raise
            pos++;
            pot.Bet(pos, 2 * pot.CurrentBet);
            Assert.AreEqual(pot.CurrentBet, 6 * BB);
            Assert.AreEqual(pot.PreviousRaise, 3 * BB);
            Assert.AreEqual(pot.OpeningPlayerPosition, pos);
            Assert.AreEqual(pot.GetMinRaise(Enums.BettingRound.Preflop), 9 * BB);
            Assert.AreEqual(pot.NumberOfRaises, 3);

            // Finish betting and make sure everything's back to zero
            pot.Call(0);
            pot.Call(1);
            pot.Call(2);
            pot.Call(3);
            pot.FinishBetting();
            Assert.AreEqual(pot.CurrentBet, 0);
            Assert.AreEqual(pot.PreviousRaise, 0);
            Assert.AreEqual(pot.NumberOfRaises, 0);




        }

        [TestMethod]
        public void TestSidePotAfterAllinCall()
        {
            long SB = 50;
            long BB = 100;
            Game game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, SB, BB, 0, 4);
            Table table = new Table(game);
            table.AddPlayer(new TestPlayer("P0", 1000));
            table.AddPlayer(new TestPlayer("P1", 1000));
            table.AddPlayer(new TestPlayer("P2", 220));
            table.AddPlayer(new TestPlayer("P3", 600));

            Pot pot = new Pot(table);

            int pos = 0;
            pot.Bet(pos++, 100);
            pot.Bet(pos++, 300);
            pot.Bet(pos++, 300); // really just 220 chips... player "calls all-in"

            Assert.AreEqual(pot.NumberOfPots, 2);
            Assert.AreEqual(pot.PlayerIsInPot(0, 0), true);
            Assert.AreEqual(pot.PlayerIsInPot(0, 1), true);
            Assert.AreEqual(pot.PlayerIsInPot(0, 2), true);
            Assert.AreEqual(pot.PlayerIsInPot(0, 3), true);

            Assert.AreEqual(pot.PlayerIsInPot(1, 0), true);
            Assert.AreEqual(pot.PlayerIsInPot(1, 1), true);
            Assert.AreEqual(pot.PlayerIsInPot(1, 2), false);
            Assert.AreEqual(pot.PlayerIsInPot(1, 3), true);

            Assert.AreEqual(pot.GetChipsPlayerInvested(0), 100);
            Assert.AreEqual(pot.GetChipsPlayerInvested(1), 300);
            Assert.AreEqual(pot.GetChipsPlayerInvested(2), 220);
            Assert.AreEqual(pot.GetChipsPlayerInvested(3), 0);

            // try to finish betting .. not possible yet since P3 and P0 still needs to call
            try
            {
                pot.FinishBetting();
                throw new AssertFailedException("pot.FinishBetting didn't throw exception.");
            }
            catch (InvalidOperationException)
            {
            }

            // P3 and P0 call. NOW betting is completed
            pot.Call(3);
            pot.Call(0);
            pot.FinishBetting();

            // check pot sizes
            Assert.AreEqual(pot.GetChipsInPot(0), 4 * 220);
            Assert.AreEqual(pot.GetChipsInPot(1), 3 * 80);

            // Time for another betting round, creating another side pot
            pos = 0;
            pot.Bet(pos++, 400);
            pot.Call(pos++);

            // try to act on behalf of the all-in player P2
            try
            {
                pot.Call(pos++);
                throw new AssertFailedException("All-in player shouldn't be allowed to call.");
            }
            catch (InvalidOperationException)
            {
            }

            // now P3 makes an all-in call, creating another side-pot
            pot.Call(pos);
            Assert.AreEqual(pot.NumberOfPots, 3);

            Assert.AreEqual(pot.GetChipsPlayerInvested(0), 400);
            Assert.AreEqual(pot.GetChipsPlayerInvested(1), 400);
            Assert.AreEqual(pot.GetChipsPlayerInvested(2), 0);
            Assert.AreEqual(pot.GetChipsPlayerInvested(3), 300); 

            // Main pot
            Assert.AreEqual(pot.PlayerIsInPot(0, 0), true);
            Assert.AreEqual(pot.PlayerIsInPot(0, 1), true);
            Assert.AreEqual(pot.PlayerIsInPot(0, 2), true);
            Assert.AreEqual(pot.PlayerIsInPot(0, 3), true);

            // First side pot (P0, P1, P3)
            Assert.AreEqual(pot.PlayerIsInPot(1, 0), true);
            Assert.AreEqual(pot.PlayerIsInPot(1, 1), true);
            Assert.AreEqual(pot.PlayerIsInPot(1, 2), false);
            Assert.AreEqual(pot.PlayerIsInPot(1, 3), true);

            // Second side pot (P0, P1)
            Assert.AreEqual(pot.PlayerIsInPot(2, 0), true);
            Assert.AreEqual(pot.PlayerIsInPot(2, 1), true);
            Assert.AreEqual(pot.PlayerIsInPot(2, 2), false);
            Assert.AreEqual(pot.PlayerIsInPot(2, 3), false);
           
        }


        [TestMethod]
        public void TestSidePotAfterAllinRaise()
        {
            long SB = 50;
            long BB = 100;
            Game game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, SB, BB, 0, 4);
            Table table = new Table(game);
            table.AddPlayer(new TestPlayer("P0", 3000));
            table.AddPlayer(new TestPlayer("P1", 300));
            table.AddPlayer(new TestPlayer("P2", 3000));
            table.AddPlayer(new TestPlayer("P3", 10000));


            Pot pot = new Pot(table);

            pot.Bet(0, 100);
            pot.Bet(1, 300); // P1 raises and is all-in
            
            Assert.AreEqual(pot.NumberOfPots, 1); // this does NOT create a new side-pot yet..
            Assert.AreEqual(pot.OpeningPlayerPosition, 1); // also, this was a full raise

            pot.Bet(2, 1200); // P2 re-raises. 
            Assert.AreEqual(pot.NumberOfPots, 2); // NOW we have a side-pot
            pot.Bet(3, 10000); // P3 raises all-in
            Assert.AreEqual(pot.NumberOfPots, 2); // still just two pots
            pot.Call(0); // this is an all-in call 
            Assert.AreEqual(pot.NumberOfPots, 3); // now third pot
            pot.Call(2);
            Assert.AreEqual(pot.NumberOfPots, 3); // still three pots

            pot.FinishBetting();
            Assert.AreEqual(pot.NumberOfPots, 2); // third pot was only one player, money back to player
            Assert.AreEqual(pot.GetChipsInPot(0), 1200);
            Assert.AreEqual(pot.GetChipsInPot(1), 3*2700);

            Assert.AreEqual(pot.NumberOfActivePlayers, 1);

        }


        [TestMethod]
        public void TestAnteAllIn()
        {
            long SB = 50;
            long BB = 100;
            long Ante = 10;
            Game game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, SB, BB, Ante, 4);
            Table table = new Table(game);
            table.AddPlayer(new TestPlayer("P0", 1000));
            table.AddPlayer(new TestPlayer("P1", 1000));
            table.AddPlayer(new TestPlayer("P2", 8));

            Pot pot = new Pot(table);


            // antes
            for (int i = 0; i < table.NumberOfPlayers; i++)
            {
                pot.Bet(i, Ante, Enums.BetType.Ante);
            }
            pot.FinishBetting();
            Assert.AreEqual(pot.CurrentBet, 0);
            Assert.AreEqual(pot.PreviousRaise, 0);
            Assert.AreEqual(pot.NumberOfRaises, 0);
            Assert.AreEqual(pot.NumberOfPots, 2);

            // Now request a BB from the player who is all-in...
            pot.Bet(2, BB, Enums.BetType.BigBlind);

            // http://poker.stackexchange.com/questions/966/player-very-low-on-chips-cant-pay-antebb-what-happens
            // .. this should STILL set the current bet to the BB amount!
            Assert.AreEqual(pot.CurrentBet, BB);
 
        }


        [TestMethod]
        public void TestHeadsUp()
        {
            long SB = 50;
            long BB = 100;
            long Ante = 10;
            Game game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, SB, BB, Ante, 4);
            Table table = new Table(game, 0);
            table.AddPlayer(new TestPlayer("P0", 1000));
            table.AddPlayer(new TestPlayer("P1", 1000));

            Pot pot = new Pot(table);
            pot.Bet(0, SB, Enums.BetType.SmallBlind);
            pot.Bet(1, BB, Enums.BetType.BigBlind);

            Assert.AreEqual(pot.OpeningPlayerPosition, 1);
            Assert.AreEqual(pot.CurrentBet, BB);

            pot.Bet(0, 3*BB);
            Assert.AreEqual(pot.OpeningPlayerPosition, 0);


        }

        [TestMethod]
        public void TestTwoSidePots()
        {
            long SB = 50;
            long BB = 100;
            Game game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, SB, BB, 0, 4);
            Table table = new Table(game);
            table.AddPlayer(new TestPlayer("P0", 1000));
            table.AddPlayer(new TestPlayer("P1", 2000));
            table.AddPlayer(new TestPlayer("P2", 3000));
            Pot pot = new Pot(table);

            pot.Bet(0, 500);
            Assert.AreEqual(1, pot.NumberOfPots);

            pot.Bet(1, 2000);
            Assert.AreEqual(1, pot.NumberOfPots);

            pot.Bet(2, 3000);
            Assert.AreEqual(2, pot.NumberOfPots);

            Assert.AreEqual(true, pot.PlayerIsInPot(1, 0));  // P0 could *potentially* still get involved...

            pot.Call(0);
            Assert.AreEqual(3, pot.NumberOfPots);
            Assert.AreEqual(true, pot.PlayerIsInPot(0, 0));
            Assert.AreEqual(false, pot.PlayerIsInPot(1, 0));  // critical! P0 called but it wasn't enough for side pot 1
            Assert.AreEqual(false, pot.PlayerIsInPot(2, 0));

            Assert.AreEqual(true, pot.PlayerIsInPot(0, 1));
            Assert.AreEqual(false, pot.PlayerIsInPot(1, 1));
            Assert.AreEqual(true, pot.PlayerIsInPot(2, 1));

            Assert.AreEqual(true, pot.PlayerIsInPot(0, 2));
            Assert.AreEqual(true, pot.PlayerIsInPot(1, 2));
            Assert.AreEqual(true, pot.PlayerIsInPot(2, 2));




        }

    }
}
