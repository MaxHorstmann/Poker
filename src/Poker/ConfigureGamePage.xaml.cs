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
using PokerObjects;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace Poker
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ConfigureGamePage : Poker.Common.LayoutAwarePage
    {
        public ConfigureGamePage()
        {
            this.InitializeComponent();
        }

        private void CreateTable()
        {

            var limitType = (comboBoxGameType.SelectedIndex == 0) ? Enums.Limit.NoLimit : Enums.Limit.FixedLimit;
            long smallBlind = 50;
            long bigBlind = 100; // TODO from selection
            long ante = 0;
            var game = new Game(Game.GameTypeEnum.Holdem, limitType, smallBlind, bigBlind, ante);

            // Set up a new table
            Table table = new Table(game);

            // Add players ..
            var playerName = textBoxName.Text;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Poker King (you)";
            }

            var chipCount = 10000; // get from global chip count state

            table.AddPlayer(new HumanPlayer(playerName, chipCount));

            List<ComputerPlayer> computerPlayers = new List<ComputerPlayer>();
            computerPlayers.Add((new ComputerPlayer("John", 1000)));
            computerPlayers.Add((new ComputerPlayer("Phil", 2500)));
            computerPlayers.Add((new ComputerPlayer("Daniel", 3000)));
            computerPlayers.Add((new ComputerPlayer("Peter", 2000)));
            computerPlayers.Add((new ComputerPlayer("Andrew", 4000)));
            computerPlayers.Add((new ComputerPlayer("Michael", 7000)));
            computerPlayers.Add((new ComputerPlayer("Matt", 2000)));
            computerPlayers.Add((new ComputerPlayer("Luke", 6000)));

            int playersPerTable = 8; 
            foreach (Player p in computerPlayers)
            {
                table.AddPlayer(p);
                if (table.NumberOfPlayers >= playersPerTable)
                {
                    break;
                }
            }


            // Save table to global state
            App myApp = App.Current as App;
            myApp.CurrentTable = table;


        }

        private void Button_Start_Game_Click(object sender, RoutedEventArgs e)
        {
            CreateTable();
            Frame.Navigate(typeof(MainPage));
        }

        private void textBoxName_GotFocus(object sender, RoutedEventArgs e)
        {
            textBoxName.SelectAll();
        }
    }
}
