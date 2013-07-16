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

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Poker
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class TitlePage : Poker.Common.LayoutAwarePage
    {
        public TitlePage()
        {
            this.InitializeComponent();

            // check if there is a game in progress by trying to
            // restore the last table
            App myApp = App.Current as App;
            if (myApp.CurrentTable == null)
            {
                _buttonResumeGame.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            else
            {
                _buttonResumeGame.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }

        }

        private void _buttonResumeGame_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void _buttonStartNewGame_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ConfigureGamePage));
        }


    }
}
