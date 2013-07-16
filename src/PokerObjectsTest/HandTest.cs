using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PokerObjects;

namespace PokerObjectsTest
{
    [TestClass]
    public class HandTest
    {
        [TestMethod]
        public void TestHandEvaluationFlush()
        {
            Card[] cards = new Card[]
            {
                new Card(Card.CardSuit.clubs, Card.CardValue.eight),
                new Card(Card.CardSuit.clubs, Card.CardValue.nine),
                new Card(Card.CardSuit.clubs, Card.CardValue.king),
                new Card(Card.CardSuit.clubs, Card.CardValue.deuce),
                new Card(Card.CardSuit.clubs, Card.CardValue.four)
            };

            Hand hand = new Hand() { Cards = cards };
            Hand.HandValue hv = hand.Evaluate();
            Assert.AreEqual(hv.HandCategory, Hand.HandCategory.Flush);
        }

        [TestMethod]
        public void TestHandEvaluationStraight()
        {
            Card[] cards = new Card[]
            {
                new Card(Card.CardSuit.clubs, Card.CardValue.eight),
                new Card(Card.CardSuit.hearts, Card.CardValue.nine),
                new Card(Card.CardSuit.diamonds, Card.CardValue.queen),
                new Card(Card.CardSuit.diamonds, Card.CardValue.jack),
                new Card(Card.CardSuit.spades, Card.CardValue.ten)
            };

            Hand hand = new Hand() { Cards = cards };
            Hand.HandValue hv = hand.Evaluate();
            Assert.AreEqual(hv.HandCategory, Hand.HandCategory.Straight);
        }

        [TestMethod]
        public void TestHandFullHouseWithJustOneHoleCard()
        {
            Card[] holeCards = new Card[]   //9♣ T♦
            {
                new Card(Card.CardSuit.clubs, Card.CardValue.nine), 
                new Card(Card.CardSuit.diamonds, Card.CardValue.ten)
            };

            Card[] communityCards = new Card[]
            {
                new Card(Card.CardSuit.diamonds, Card.CardValue.nine),
                new Card(Card.CardSuit.hearts, Card.CardValue.nine),
                new Card(Card.CardSuit.spades, Card.CardValue.trey),
                new Card(Card.CardSuit.diamonds, Card.CardValue.trey),
                new Card(Card.CardSuit.clubs, Card.CardValue.seven)
            };

            Hand.HandValue hv = Hand.EvaluateHandWithBoard(holeCards, communityCards);
            Assert.AreEqual(Hand.HandCategory.FullHouse, hv.HandCategory);

        }

        [TestMethod]
        public void TestHandFullHouse()
        {

            Card[] cards = new Card[]
            {
                new Card(Card.CardSuit.diamonds, Card.CardValue.nine),
                new Card(Card.CardSuit.hearts, Card.CardValue.nine),
                new Card(Card.CardSuit.spades, Card.CardValue.trey),
                new Card(Card.CardSuit.diamonds, Card.CardValue.trey),
                new Card(Card.CardSuit.spades, Card.CardValue.nine)
            };

            Hand hand = new Hand();
            hand.Cards = cards;

            Hand.HandValue hv = hand.Evaluate(); 
            Assert.AreEqual(Hand.HandCategory.FullHouse, hv.HandCategory);
            string hvString = hv.ToString();
            Assert.AreEqual("a full boat (nines full of treys)", hvString);

        }


    }
}
