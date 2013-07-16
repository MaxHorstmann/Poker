using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;
using Windows.Graphics;
using Windows.Graphics.Imaging;
using PokerObjects;


namespace Poker
{
    public class BoardUI
    {
        public Page Page { get; private set; }
        public Canvas Canvas { get; private set; }

        public double CardHeight { get; set; }
        public double CardWidth
        {
            get
            {
                return CardHeight / Constants.CARD_RATIO_HEIGHT_TO_WIDTH;
            }
        }

        public double CardSpacing
        {
            get
            {
                return 0.2 * CardWidth;
            }
        }

        private bool _displayHalfCards;

        public BoardUI(Page page, Canvas canvas, bool displayHalfCards = false) 
        {
            Canvas = canvas;
            Page = page;
            _displayHalfCards = displayHalfCards;
        }


        /// <summary>
        /// Draws the board cards and pot information
        /// </summary>
        /// <param name="table"></param>
        /// <param name="communityCards"></param>
        public void DrawElements(Table table, IEnumerable<Card> communityCards)
        {
            int boardCardNumber = 0;
            int MAX_BOARD_CARDS = 5;
            double marginLeft = (Canvas.ActualWidth / 2) - ((MAX_BOARD_CARDS * (CardWidth + CardSpacing))/2); 
            
            foreach (Card c in communityCards)
            {
                Image img;
                if (Canvas.Children.Count > boardCardNumber)
                {
                    img = Canvas.Children[boardCardNumber] as Image;
                }
                else
                {
                    img = new Image();
                    Canvas.Children.Add(img);
                    img.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Left;
                }

                img.Height = CardHeight;

                img.Source = new BitmapImage(new Uri(Page.BaseUri, c.ImageFileName));
                img.Margin = new Thickness(marginLeft + boardCardNumber * (CardWidth + CardSpacing), 0, 0, 0);

                boardCardNumber++;
            }


        }


    }
}
