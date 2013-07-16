using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PokerObjects
{

    [DataContract]
    public class Deck
    {
        [DataMember]
        private LinkedList<Card> _cards = null;

        public Deck(bool shuffled = true)
        {
            // populate _cards
            _cards = new LinkedList<Card>();
            for (Card.CardValue v = Card.CardValue.deuce; v <= Card.CardValue.ace; v++)
            {
                for (Card.CardSuit s = (Card.CardSuit)1; s <= (Card.CardSuit)4; s++)
                {
                    _cards.AddLast(new Card(s, v));
                }
            }

            if (shuffled)
            {
                Shuffle();
            }

        }

        public int Count
        {
            get
            {
                return _cards.Count;
            }
        }

        public void Shuffle()
        {
            var _cardsShuffeled = _cards.OrderBy(a => Guid.NewGuid());
            _cards = new LinkedList<Card>();
            foreach (Card c in _cardsShuffeled)
            {
                _cards.AddLast(c);
            }
        } 

        public Card DrawCard()
        {
            if (_cards.Count == 0)
            {
                throw new InvalidOperationException("Out of cards.");
            }
            Card c = _cards.First.Value;
            _cards.RemoveFirst();
            return c;

        }
    }
}
