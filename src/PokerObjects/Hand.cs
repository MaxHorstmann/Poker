using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PokerObjects
{
    public class Hand : IComparable<Hand>
    {
        public class HandValue : IComparable<HandValue>
        {
            public HandCategory HandCategory { get; set; }
            public int[] Secondary { get; set; }


            public override string ToString()
            {
                switch (HandCategory)
                {
                    case (HandCategory.RoyalFlush):
                        return "a royal flush";
                    case (HandCategory.StraightFlush):
                        return string.Format("a straight flush ({0} high)", (Card.GetValueString((Card.CardValue)(Secondary[0]))));
                    case (HandCategory.FourOfAKind):
                        return string.Format("four of a kind (four {0} with {1})",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[1]), false, true)));
                    case (HandCategory.FullHouse):
                        return string.Format("a full boat ({0} full of {1})",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[1]), true)));
                    case (HandCategory.Flush):
                        return string.Format("a flush ({0} high)",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), false)));
                    case (HandCategory.Straight):
                        return string.Format("a straight ({0} high)",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), false)));
                    case Hand.HandCategory.ThreeOfAKind:
                        return string.Format("three of a kind ({0} with {1} and {2})",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[1]), false, true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[2]), false, true)));
                    case Hand.HandCategory.TwoPair:
                        return string.Format("two pair ({0} and {1} with {2})",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[1]), true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[2]), false, true)));
                    case Hand.HandCategory.Pair:
                        return string.Format("one pair ({0} with {1}, {2}, and {3})",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[1]), false, true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[2]), false, true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[3]), false, true)));
                    case Hand.HandCategory.HighCard:
                        return string.Format("{0} high (with {1}, {2}, {3}, and {4})",
                            (Card.GetValueString((Card.CardValue)(Secondary[0]), false)),
                            (Card.GetValueString((Card.CardValue)(Secondary[1]), false, true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[2]), false, true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[3]), false, true)),
                            (Card.GetValueString((Card.CardValue)(Secondary[4]), false, true)));

                }
                return string.Empty;
            }


            public int CompareTo(HandValue other)
            {
                if (HandCategory != other.HandCategory)
                    return HandCategory.CompareTo(other.HandCategory);

                if (Secondary.Length != other.Secondary.Length)
                    throw new ArgumentOutOfRangeException();

                for (int i = 0; i < Secondary.Length; i++)
                {
                    if (Secondary[i] != other.Secondary[i])
                        return Secondary[i].CompareTo(other.Secondary[i]);
                }

                return 0; // equal
            }
        }

        public const int CARDS_IN_A_HAND = 5;


        public enum HandCategory
        {
            Unknown = 0,
            HighCard = 10,
            Pair = 20,
            TwoPair = 30,
            ThreeOfAKind = 40,
            Straight = 50,
            Flush = 60,
            FullHouse = 70,
            FourOfAKind = 80,
            StraightFlush = 90,
            RoyalFlush = 100
        }

        public Card[] Cards { get; set; }

        public static List<List<Card>> GetCardSubsets(List<Card> cards, int count, List<Card> alreadyPicked = null)
        {
            // TODO: current implementation returns many duplicates
            // TODO: optimize for performance
            List<List<Card>> res = new List<List<Card>>();
            if (alreadyPicked == null)
            {
                alreadyPicked = new List<Card>();
            }

            if (count == 0)
            {
                res.Add(new List<Card>(alreadyPicked));
            }
            else
            {
                foreach (Card c in cards)
                {
                    List<Card> cards2 = new List<Card>(cards);

                    alreadyPicked.Add(c);
                    cards2.Remove(c);
                    List<List<Card>> foo = GetCardSubsets(cards2, count - 1, alreadyPicked);
                    res.AddRange(foo);
                    alreadyPicked.Remove(c);
                }
            }
            return res;
                
        }

        public static HandValue EvaluateHandWithBoard(IList<Card> HoleCards, IList<Card> CommunityCards,
            int minHoleCards = 0, int maxHoleCards = 2)
        {
            if (HoleCards.Count + CommunityCards.Count < CARDS_IN_A_HAND)
                throw new ArgumentOutOfRangeException();

            HandValue bestHandValue = null;

            for (int numberOfHoleCards = minHoleCards; numberOfHoleCards <= maxHoleCards; numberOfHoleCards++)
            {
                int numberOfCommunityCards = CARDS_IN_A_HAND - numberOfHoleCards;

                // go through all combinations...
                List<List<Card>> holeCardCombinations = GetCardSubsets(new List<Card>(HoleCards), numberOfHoleCards);
                List<List<Card>> commonityCardCombinations = GetCardSubsets(new List<Card>(CommunityCards), numberOfCommunityCards);

                foreach (List<Card> holeCardsPick in holeCardCombinations)
                    foreach (List<Card> communityCardsPick in commonityCardCombinations)
                    {
                        List<Card> fiveCards = new List<Card>(holeCardsPick);
                        fiveCards.AddRange(communityCardsPick);

                        HandValue newHandValue = EvaluateFiveCards(fiveCards.ToArray());
                        if ((bestHandValue == null) || (newHandValue.CompareTo(bestHandValue) > 0))
                        {
                            bestHandValue = newHandValue;
                        }
                    }

            }

            return bestHandValue;

        }

        public HandValue Evaluate()
        {
            return EvaluateFiveCards(Cards);
        }

        private static HandValue EvaluateFiveCards(Card[] cards)
        {
            if (cards.Length != 5)
                throw new ArgumentOutOfRangeException();

            cards = cards.OrderBy(x => x.Value).ToArray();


            bool flush =
                  ((cards[0].Suit == cards[1].Suit)
                && (cards[1].Suit == cards[2].Suit)
                && (cards[2].Suit == cards[3].Suit)
                && (cards[3].Suit == cards[4].Suit));

            bool straight =
                ((cards[0].Value + 1 == cards[1].Value)
                && (cards[1].Value + 1 == cards[2].Value)
                && (cards[2].Value + 1 == cards[3].Value)
                && (cards[3].Value + 1 == cards[4].Value));

            int straightHighCard = (int)cards[4].Value;

            if ((cards[0].Value == Card.CardValue.deuce)
                && (cards[1].Value == Card.CardValue.trey)
                && (cards[2].Value == Card.CardValue.four)
                && (cards[3].Value == Card.CardValue.five)
                && (cards[4].Value == Card.CardValue.ace))
            {
                straight = true; // wheel (A-2-3-4-5)
                straightHighCard = (int)cards[3].Value; 
            }

            // STRAIGHT FLUSH / ROYAL FLUSH
            if (straight && flush)
            {
                if (cards[4].Value == Card.CardValue.ace)
                {
                    return new HandValue() { HandCategory = HandCategory.RoyalFlush };
                }
                else
                {
                    return new HandValue() { 
                        HandCategory = HandCategory.StraightFlush, 
                        Secondary = new int[] { straightHighCard }
                    };
                }
            }

            // FOUR OF A KIND
            if ((cards[0].Value == cards[1].Value)
                && (cards[1].Value == cards[2].Value)
                && (cards[2].Value == cards[3].Value))
            {
                return new HandValue() 
                { 
                    HandCategory = HandCategory.FourOfAKind, 
                    Secondary = new int[] { (int)cards[0].Value, (int)cards[4].Value } 
                };
            }

            if ((cards[1].Value == cards[2].Value)
                && (cards[2].Value == cards[3].Value)
                && (cards[3].Value == cards[4].Value))
            {
                return new HandValue() 
                { 
                    HandCategory = HandCategory.FourOfAKind, 
                    Secondary = new int[] { (int)cards[4].Value , (int)cards[0].Value }
                };
            }

            // FULL HOUSE
            if ((cards[0].Value == cards[1].Value)
                &&(cards[1].Value == cards[2].Value)
                &&(cards[3].Value == cards[4].Value))
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.FullHouse,
                    Secondary = new int[] { (int)cards[0].Value, (int)cards[3].Value }
                };
            }

            if ((cards[0].Value == cards[1].Value)
                && (cards[2].Value == cards[3].Value)
                && (cards[3].Value == cards[4].Value))
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.FullHouse,
                    Secondary = new int[] { (int)cards[3].Value, (int)cards[0].Value }
                };
            }
            
                
            // FLUSH
            if (flush)
            {
                return new HandValue() 
                { 
                    HandCategory = HandCategory.Flush, 
                    Secondary = new int[] { 
                        (int)cards[4].Value, 
                        (int)cards[3].Value, 
                        (int)cards[2].Value,
                        (int)cards[1].Value,
                        (int)cards[0].Value 
                    }
                };
            }

            // STRAIGHT
            if (straight)
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.Straight,
                    Secondary = new int[] { straightHighCard }
                };
            }

            // THREE OF A KIND
            if ((cards[0].Value == cards[1].Value)
                && (cards[1].Value == cards[2].Value))
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.ThreeOfAKind,
                    Secondary = new int[] { (int)cards[0].Value, (int)cards[4].Value, (int)cards[3].Value }
                };
            }

            if ((cards[1].Value == cards[2].Value)
                && (cards[2].Value == cards[3].Value))
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.ThreeOfAKind,
                    Secondary = new int[] { (int)cards[1].Value, (int)cards[4].Value, (int)cards[0].Value }
                };
            }

            if ((cards[2].Value == cards[3].Value)
                && (cards[3].Value == cards[4].Value))
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.ThreeOfAKind,
                    Secondary = new int[] { (int)cards[2].Value, (int)cards[1].Value, (int)cards[0].Value }
                };
            }

            // TWO PAIR / ONE PAIR
            if ((cards[0].Value == cards[1].Value))
            {
                if ((cards[2].Value == cards[3].Value))
                {
                    return new HandValue()
                    {
                        HandCategory = HandCategory.TwoPair,
                        Secondary = new int[] { (int)cards[2].Value, (int)cards[0].Value, (int)cards[4].Value }
                    };
                }
                if ((cards[3].Value == cards[4].Value))
                {
                    return new HandValue()
                    {
                        HandCategory = HandCategory.TwoPair,
                        Secondary = new int[] { (int)cards[3].Value, (int)cards[0].Value, (int)cards[2].Value }
                    };
                }

                return new HandValue()
                {
                    HandCategory = HandCategory.Pair,
                    Secondary = new int[] { (int)cards[0].Value, (int)cards[4].Value, (int)cards[3].Value, (int)cards[2].Value }
                };
            }


            if ((cards[1].Value == cards[2].Value))
            {
                if ((cards[3].Value == cards[4].Value))
                {
                    return new HandValue()
                    {
                        HandCategory = HandCategory.TwoPair,
                        Secondary = new int[] { (int)cards[3].Value, (int)cards[1].Value, (int)cards[0].Value }
                    };
                }

                return new HandValue()
                {
                    HandCategory = HandCategory.Pair,
                    Secondary = new int[] { (int)cards[1].Value, (int)cards[4].Value, (int)cards[3].Value, (int)cards[0].Value }
                };
            }


            if ((cards[2].Value == cards[3].Value))
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.Pair,
                    Secondary = new int[] { (int)cards[2].Value, (int)cards[4].Value, (int)cards[1].Value, (int)cards[0].Value }
                };
            }

            if ((cards[3].Value == cards[4].Value))
            {
                return new HandValue()
                {
                    HandCategory = HandCategory.Pair,
                    Secondary = new int[] { (int)cards[3].Value, (int)cards[2].Value, (int)cards[1].Value, (int)cards[0].Value }
                };
            }



            // HIGH CARD
            return new HandValue()
            {
                HandCategory = HandCategory.HighCard,
                Secondary = new int[] { 
                        (int)cards[4].Value, 
                        (int)cards[3].Value, 
                        (int)cards[2].Value,
                        (int)cards[1].Value,
                        (int)cards[0].Value 
                    }
            };

        }


        public int CompareTo(Hand other)
        {
            return Evaluate().CompareTo(other.Evaluate());
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Card c in Cards)
            {
                sb.Append(c.ToString());
                sb.Append(" ");
            }
            return sb.ToString();
        }
    }
}
