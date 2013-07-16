using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PokerObjects;


namespace PokerObjectsTest
{
    [TestClass]
    public class ResumeTest
    {
        Table table_TestPreflopResume = null;
        string state_TestPreflopResume = "";

        private DataContractSerializer GetSerializer()
        {
            var knownTypes = new List<Type>();
            knownTypes.Add(typeof(Table));
            knownTypes.Add(typeof(Table));
            knownTypes.Add(typeof(Table));
            knownTypes.Add(typeof(HandHistory));
            knownTypes.Add(typeof(TestPlayer));
            knownTypes.Add(typeof(ComputerPlayer));
            knownTypes.Add(typeof(Pot));
            knownTypes.Add(typeof(Deck));

            var serializer = new DataContractSerializer(typeof(Table), knownTypes);
            return serializer;

        }

        [TestMethod]
        public void TestPreflopResume()
        {
            var game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, 1, 2, 0, 3);
            table_TestPreflopResume = new Table(game, 0);
            var p0 = new TestPlayer("player 0", 100);
            var p1 = new TestPlayer("player 1", 100);
            var p2 = new TestPlayer("player 2", 100);

            p0.OnGetAction += Bet5;
            p1.OnGetAction += Bet5;
            p2.OnGetAction += SaveState;

            table_TestPreflopResume.AddPlayer(p0);
            table_TestPreflopResume.AddPlayer(p1);
            table_TestPreflopResume.AddPlayer(p2);

            var deck = new Deck(false);
            Task task = table_TestPreflopResume.PlayHand(deck);

            // now player 0 and 1 have acted, game has been suspended
            // with action on player 2

            Assert.AreEqual(2, table_TestPreflopResume.ActionPosition);
            Assert.AreEqual(5, table_TestPreflopResume.Pot.CurrentBet);
            Assert.AreEqual(1, table_TestPreflopResume.Pot.OpeningPlayerPosition);


            // now restore the table and check if everything is as expected
            var serializer = GetSerializer();
            StringReader sr = new StringReader(state_TestPreflopResume);
            XmlReader xmlReader = XmlReader.Create(sr);
            Table restoredTable = serializer.ReadObject(xmlReader) as Table;

            Assert.AreEqual(3, restoredTable.NumberOfPlayers);
            Assert.AreEqual(2, restoredTable.ActionPosition);
            Assert.AreEqual(5, restoredTable.Pot.CurrentBet);
            Assert.AreEqual(1, restoredTable.Pot.OpeningPlayerPosition);

        }

        [TestMethod]
        public void TestFlopResume()
        {
            var game = new Game(Game.GameTypeEnum.Holdem, Enums.Limit.NoLimit, 1, 2, 0, 3);
            table_TestPreflopResume = new Table(game, 0);
            var p0 = new TestPlayer("player 0", 100);
            var p1 = new TestPlayer("player 1", 100);
            var p2 = new TestPlayer("player 2", 100);

            p0.OnGetAction += Bet5;
            p1.OnGetAction += Bet5;
            p2.OnGetAction += Bet5PreFlopSuspendOnFlop;

            table_TestPreflopResume.AddPlayer(p0);
            table_TestPreflopResume.AddPlayer(p1);
            table_TestPreflopResume.AddPlayer(p2);

            var deck = new Deck(false);
            Task task = table_TestPreflopResume.PlayHand(deck);

            // now we should have played all the way to the flop

            Assert.AreEqual(Enums.BettingRound.Flop, table_TestPreflopResume.CurrentBettingRound);
            Assert.AreEqual(3, table_TestPreflopResume.CommunityCards.Count);

              
            // now restore the table and check if everything is as expected
            var serializer = GetSerializer();
            StringReader sr = new StringReader(state_TestPreflopResume);
            XmlReader xmlReader = XmlReader.Create(sr);
            Table restoredTable = serializer.ReadObject(xmlReader) as Table;

            Assert.AreEqual(Enums.BettingRound.Flop, restoredTable.CurrentBettingRound);
            Assert.AreEqual(3, restoredTable.CommunityCards.Count);
  
        }

        private long Bet5PreFlopSuspendOnFlop(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed)
        {
            if (bettingRound == Enums.BettingRound.Preflop)
            {
                return 5 - pot.GetChipsPlayerInvested(playerIndex);
            }
            return SaveState(pot, playerIndex, bettingRound, raiseAllowed);
        }

        private long SaveState(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed)
        {
            var serializer = GetSerializer();
            var sb = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(sb))
            {
                serializer.WriteObject(xmlWriter, table_TestPreflopResume);
                xmlWriter.Flush();
            }
            state_TestPreflopResume = sb.ToString();

            return Constants.ACTION_SUSPEND_GAME; 
        }

        private long Bet5(Pot pot, int playerIndex, Enums.BettingRound bettingRound, bool raiseAllowed)
        {
            return 5 - pot.GetChipsPlayerInvested(playerIndex);
        }
    }
}
