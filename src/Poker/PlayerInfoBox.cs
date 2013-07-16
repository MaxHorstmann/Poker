using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.Text;
using PokerObjects;


namespace Poker
{
    public class PlayerInfoBox
    {
        public Canvas CanvasContainer { get; private set; }
        public string PlayerName { get; private set; }
        public int NumberOfHoleCards { get; private set; }

        private TextBlock textBlockPlayerName = null;
        private Image[] imageCards = null;

        public PlayerInfoBox(Canvas canvas, int numberOfHoleCards, string playerName)
        {
            this.CanvasContainer = canvas;
            this.PlayerName = playerName;
            this.NumberOfHoleCards = numberOfHoleCards;

            CreateElements();
        }

        public void CreateElements()
        {
            CanvasContainer.Children.Clear();

            textBlockPlayerName = new TextBlock();
            textBlockPlayerName.FontSize = 24;
            textBlockPlayerName.Text = PlayerName;

            Canvas.SetLeft(textBlockPlayerName, 20);
            Canvas.SetTop(textBlockPlayerName, 20);

            CanvasContainer.Children.Add(textBlockPlayerName);

        }

        /// <summary>
        /// Clear all the card images
        /// </summary>
        private void ClearCardsImages()
        {
            if (imageCards != null)
            {
                foreach (Image img in imageCards)
                {
                    CanvasContainer.Children.Remove(img);
                }
                imageCards = null;
            }
        }

        /// <summary>
        /// Sets the images of the player's cards
        /// </summary>
        /// <param name="cards"></param>
        public void SetCards(IList<Card> cards)
        {
            ClearCardsImages();

            if (cards != null)
            {

                imageCards = new Image[NumberOfHoleCards];
                for (int i = 0; i < NumberOfHoleCards; i++)
                {
                    Image img = new Image();
                    Canvas.SetLeft(img, i * 100); // TODO

                    imageCards[i] = img;
                    CanvasContainer.Children.Add(img);
                }

            }

        }

        
    }
}
