using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace PokerObjects
{
    [DataContract]
    public class Card
    {
        public enum CardSuit
        {
            unknown = 0,
            clubs = 1,
            spades = 2,
            hearts = 3,
            diamonds = 4
        }

        public enum CardValue
        {
            unknown = 0,
            deuce = 2,
            trey = 3,
            four = 4,
            five = 5,
            six = 6,
            seven = 7,
            eight = 8,
            nine = 9,
            ten = 10,
            jack = 11,
            queen = 12,
            king = 13,
            ace = 14
        }

        static char[] suitCharacters = new char[] { '?', '♣', '♠', '♥', '♦' };
        static char[] suitLetter = new char[] { '?', 'c', 's', 'h', 'd' };
        static char[] valueSymbols = new char[] { '?', 'A', '2', '3', '4', '5', '6', '7', '8', '9', 'T', 'J', 'Q', 'K', 'A' };

        [DataMember]
        public CardSuit Suit { get; set; }

        [DataMember]
        public CardValue Value { get; set; }

        /// <summary>
        /// returns a string describing the value of the card
        /// e.g. six/sixes, ace/aces
        /// </summary>
        /// <returns></returns>
        public string GetValueString(bool plural = false, bool withArticle = false)
        {
            return GetValueString(Value, plural, withArticle);
        }

        /// <summary>
        /// returns a string describing the value of a given cardValue
        /// e.g. six/sixes, ace/aces
        /// </summary>
        /// <returns></returns>
        public static string GetValueString(CardValue cardValue, bool plural = false, bool withArticle = false)
        {
            string article = "";
            string cardValueString = "";
            if (cardValue == CardValue.six)
            {
                if (!plural)
                {
                    cardValueString = "six";
                }
                else
                {
                    cardValueString = "sixes";
                }
            }
            else
            {
                if (!plural)
                {
                    cardValueString = cardValue.ToString();
                }
                else
                {
                    cardValueString = string.Format("{0}s", cardValue.ToString());
                }
            }

            if (withArticle)
            {
                if (IsVowel(cardValueString[0]))
                {
                    article = "an ";
                }
                else
                {
                    article = "a ";
                }
            }

            return article + cardValueString;
        }

        /// <summary>
        /// Small helper function: test if a character is a vowel
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private static bool IsVowel(char c)
        {
            return
               ((c == 'a') || (c == 'A') ||
               (c == 'e') || (c == 'E') ||
               (c == 'i') || (c == 'I') ||
               (c == 'o') || (c == 'O') ||
               (c == 'u') || (c == 'U'));
        }


        /// <summary>
        /// Returns name of file containing the card image
        /// </summary>
        public string ImageFileName
        {
            get
            {
                return string.Format("Assets/{0}{1}.png", 
                    valueSymbols[(int)Value],
                    suitLetter[(int)Suit]);
            }
        }

        /// <summary>
        /// Returns name of file containing only the upper half of the card image
        /// </summary>
        public string ImageFileNameHalf
        {
            get
            {
                return string.Format("Assets/{0}{1}_half.png",
                    valueSymbols[(int)Value],
                    suitLetter[(int)Suit]);
            }
        }


        public static string ImageCardBack
        {
            get
            {
                return "Assets/cardBack.png";
            }
        }

        public static string ImageCardBackHalf
        {
            get
            {
                return "Assets/cardBack_half.png";
            }
        }

 
        public Card(CardSuit suit, CardValue value)
        {
            this.Suit = suit;
            this.Value = value;
        }

        public override string ToString()
        {
            return string.Format("{0}{1}", valueSymbols[(int)Value], suitCharacters[(int)Suit]);
        }





    }
}
