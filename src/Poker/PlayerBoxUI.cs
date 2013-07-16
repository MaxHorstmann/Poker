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
    /// <summary>
    /// Creates the child elements to display  a player
    /// (name, chipcount, cards .. )
    /// </summary>
    public class PlayerBoxUI
    {
        public Page Page { get; private set; }
        public Canvas Canvas { get; private set; }
        
        public double CardHeight { get; private set; }
        public double CardWidth
        {
            get
            {
                return CardHeight / Constants.CARD_RATIO_HEIGHT_TO_WIDTH;
            }
        }


        public double CardSpaceBetween { get; private set; }
        public double CardSpaceBelow { get; private set; }

        private bool _displayHalfCards;

        //public string SpeechBubble { get; set; }

        /// <summary>
        /// Pass the Canvas element which this
        /// PlayerBoxUI belongs to
        /// </summary>
        public PlayerBoxUI(Page page, Canvas canvas, bool displayHalfCards) 
        {
            Canvas = canvas;
            Page = page;
            _displayHalfCards = displayHalfCards;

            // compute card width and space between card relative to canvas size
            // (which is relative to screen resolution)
            //CardWidth =
            //    (canvas.ActualWidth - (Constants.MAX_HOLECARDS_ALL_GAMES-1) * CardSpaceBetween) 
            //    / Constants.MAX_HOLECARDS_ALL_GAMES;

            CardHeight = canvas.ActualHeight * 0.5;

            CardSpaceBetween = CardHeight * 0.5;
            CardSpaceBelow = CardHeight * 0.2;
            //CardSpaceBetween = canvas.ActualWidth / (Constants.MAX_HOLECARDS_ALL_GAMES * 3);

        }

        /// <summary>
        /// Draws the UI elements based on the property values
        /// </summary>
        public void DrawElements(Player p, int position, Table table)
        {
            // Defined order of child elements

            Pot pot = table.Pot;
            int countChildElements = 0;


            // (0) Player Name
            TextBlock tbPlayerInfo = null;
            if (Canvas.Children.Count > 0)
            {
                tbPlayerInfo = Canvas.Children[0] as TextBlock;
            }
            else
            {
                tbPlayerInfo = new TextBlock();
                tbPlayerInfo.FontSize = 20;
                tbPlayerInfo.HorizontalAlignment = HorizontalAlignment.Center;
                tbPlayerInfo.TextAlignment = TextAlignment.Center;
               tbPlayerInfo.Margin = new Thickness(0, CardHeight + CardSpaceBelow, 0, 0);
                Canvas.Children.Add(tbPlayerInfo);
            }

            long bet = 0;
            if (pot != null)
            {
                bet = pot.GetChipsPlayerInvested(position);
            }
            if (bet > 0)
            {
                tbPlayerInfo.Text = string.Format("{0}      {1} chips     bets {2}",p.Name,  p.Chipcount, bet);
            }
            else
            {
                tbPlayerInfo.Text = string.Format("{0}      {1} chips",p.Name,  p.Chipcount);
            }
            countChildElements++;



            // (1) Chipcount image
            Image imgChips = null;
            if (Canvas.Children.Count > 1)
            {
                imgChips = Canvas.Children[1] as Image;
            }
            else
            {
                imgChips = new Image();
                // TODO!
                imgChips.Visibility = Visibility.Collapsed;
                Canvas.Children.Add(imgChips);
            }
            countChildElements++;


            // (2) Dealer button
            Image imgDealerButton = null;
            if (Canvas.Children.Count > 2)
            {
                imgDealerButton = Canvas.Children[2] as Image;
            }
            else
            {
                imgDealerButton = new Image();
                imgDealerButton.Visibility = Visibility.Collapsed;
                imgDealerButton.Source = new BitmapImage(new Uri(Page.BaseUri, Constants.BUTTON_FILENAME));
                imgDealerButton.HorizontalAlignment = HorizontalAlignment.Center;
                imgDealerButton.Width = Canvas.Width * 0.1;
                Canvas.Children.Add(imgDealerButton);
            }
            if (position == table.ButtonPosition)
            {
                imgDealerButton.Visibility = Visibility.Visible;
            }
            else
            {
                imgDealerButton.Visibility = Visibility.Collapsed;
            }
            countChildElements++;

            // (3+) Hole cards  
            int countUIElements = countChildElements;
            if ((p.HoleCards != null))
            {
                int holeCardNumber = 0;
                double cardOffsetLeft = (Canvas.ActualWidth / (double)p.HoleCards.Count) - CardWidth - CardSpaceBetween;
                double cardOffsetTop = 0; //  (Canvas.ActualHeight - CardHeight) / 2;
                foreach (Card c in p.HoleCards)
                {
                    Image img = null;
                    if (Canvas.Children.Count > holeCardNumber + countUIElements)
                    {
                        img = Canvas.Children[holeCardNumber + countUIElements] as Image;
                    }
                    else
                    {
                        img = new Image();
                        Canvas.Children.Add(img);
                    }

                    if ((pot != null) && (pot.PlayerHasFolded(position)))
                    {
                        img.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        img.Visibility = Visibility.Visible;

                        if ((p.ShowsHoleCards) || (p is HumanPlayer))
                        {
                            BitmapImage bmp;
                            if (_displayHalfCards)
                            {
                                bmp = new BitmapImage(new Uri(Page.BaseUri, c.ImageFileNameHalf));
                            }
                            else
                            {
                                bmp = new BitmapImage(new Uri(Page.BaseUri, c.ImageFileName));
                            }
                            img.Source = bmp;
                        }
                        else
                        {
                            if (_displayHalfCards)
                            {
                                img.Source = new BitmapImage(new Uri(Page.BaseUri, Card.ImageCardBackHalf));
                            }
                            else
                            {
                                img.Source = new BitmapImage(new Uri(Page.BaseUri, Card.ImageCardBack));
                            }
                        }
                        img.Margin = new Thickness(
                            cardOffsetLeft + (CardSpaceBetween) * holeCardNumber,
                            cardOffsetTop,
                            0, 0);
                        img.Height = CardHeight;
                    }


                    //img.Width = CardWidth;
                    holeCardNumber++;

                }

            }



        }

        /// <summary>
        /// Clears all the UI elements from the player box
        /// </summary>
        public void Clear()
        {
            Canvas.Children.Clear();
        }


               




    }
}
