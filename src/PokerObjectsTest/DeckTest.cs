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
    public class DeckTest
    {
        [TestMethod]
        public void TestCardCount()
        {
            Deck deck = new Deck(false);
            Assert.AreEqual(deck.Count, 52); // make sure the deck says it has 52 cards

            // now count the actual cards
            int cardCount = 0;
            try
            {
                while (true)
                {
                    Card c = deck.DrawCard();
                    cardCount++;
                }
            }
            catch (InvalidOperationException)
            {
                // as expected .. deck should now be empty
            }

            Assert.AreEqual(cardCount, 52);

        }

        [TestMethod]
        public void TestShuffle()
        {
            Deck deck = new Deck();
            deck.Shuffle();

            Assert.AreEqual(deck.Count, 52);


        }


    }

}
