using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using System.Threading.Tasks;
using System.Text;
using System.Runtime.Serialization;
using Poker.Common;
using PokerObjects;


namespace Poker
{
    public sealed partial class MainPage : LayoutAwarePage
    {

        /// <summary>
        /// Once a table has been set up, wire up all the table's event handlers
        /// </summary>
        private void WireUpEventHandlers()
        {
            CurrentTable.PlayerJoinOrLeave += OnPlayerJoinOrLeaveTable;
            CurrentTable.ActionOnPlayer += OnActionOnPlayer;
            CurrentTable.CommunityCardsDealt += OnCommunityCardsDealt;
            CurrentTable.PlayerHoleCardsDealt += OnPlayerHoleCardsDealt;
            CurrentTable.PotHasChanged += OnPotHasChanged;
            CurrentTable.PlayerBetHasChanged += OnPlayerBetHasChanged;
            CurrentTable.PlayerShowsCards += OnPlayerShowsCards;
            CurrentTable.PlayerWins += OnPlayerWins;
            CurrentTable.ButtonMoved += OnButtonMoved;
            CurrentTable.BeginNewHand += OnBeginNewHand;

            foreach (Player p in CurrentTable.Players)
            {
                HumanPlayer hp = p as HumanPlayer;
                if (hp != null)
                {
                    hp.GetActionFromUIDelegate = GetActionFromUI;
                }
            }
        }

        /// <summary>
        /// Display hole cards of a player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tablePlayerHoleCardsEventArgs"></param>
        private async Task OnPlayerHoleCardsDealt(object sender, TableEventArgs tablePlayerHoleCardsEventArgs)
        {
            DrawAllPlayers();
            await Task.Delay(Constants.DELAY_MICRO);
        }



        /// <summary>
        /// Highlight the avatar of a player who has the action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tableEventArgs"></param>
        private async Task OnActionOnPlayer(object sender, TableEventArgs tableEventArgs)
        {
            _actionPosition = tableEventArgs.ActionPosition;
            DrawAllPlayers();
            await OutputStatusUpdate(tableEventArgs.StatusUpdate, false);
        }

        /// <summary>
        /// Updates the display for the current bet of a player
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tableEventArgs"></param>
        /// <returns></returns>
        private async Task OnPlayerBetHasChanged(object sender, TableEventArgs tableEventArgs)
        {
            DrawAllPlayers();
            await Task.Delay(Constants.DELAY_MICRO);
        }


        /// <summary>
        /// Add or remove player to canvas
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tablePlayerChangeEventArgs"></param>
        private async Task OnPlayerJoinOrLeaveTable(object sender, TablePlayerChangeEventArgs tablePlayerChangeEventArgs)
        {
            DrawAllPlayers();
            await OutputStatusUpdate(tablePlayerChangeEventArgs.StatusUpdate);

            if (tablePlayerChangeEventArgs.IsLeaving)
            {
                // TODO Remove player icon
            }
            else
            {
                // TODO show player icon


            }
        }

        /// <summary>
        /// Displays the new set of community cards
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tableEventArgs"></param>
        private async Task OnCommunityCardsDealt(object sender, TableEventArgs tableEventArgs)
        {
            _actionPosition = Constants.NO_POSITION;
            DrawAllPlayers();

            BoardUI board = _canvasBoard.Tag as BoardUI;
            board.CardHeight = _cardHeight;
            board.DrawElements(CurrentTable, tableEventArgs.CommunityCards);

            await OutputStatusUpdate(tableEventArgs.StatusUpdate);
        }


        /// <summary>
        /// Displays pot(s) size(s)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tableEventArgs"></param>
        private async Task OnPotHasChanged(object sender, TableEventArgs tableEventArgs)
        {
            DrawPot(tableEventArgs.Pot);
            await OutputStatusUpdate(tableEventArgs.StatusUpdate);
        }

        /// <summary>
        /// Display a player's hole cards on showdown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tableEventArgs"></param>
        private async Task OnPlayerShowsCards(object sender, TableEventArgs tableEventArgs)
        {
            _actionPosition = tableEventArgs.ActionPosition;
            DrawAllPlayers();
            await OutputStatusUpdate(tableEventArgs.StatusUpdate);
            await Task.Delay(Constants.DELAY_AFTER_HAND_SHOW);
        }

        /// <summary>
        /// Player wins money
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="tableEventArgs"></param>
        /// <returns></returns>
        private async Task OnPlayerWins(object sender, TableEventArgs tableEventArgs)
        {
            _actionPosition = tableEventArgs.ActionPosition;
            DrawAllPlayers();
            await OutputStatusUpdate(tableEventArgs.StatusUpdate);
            await Task.Delay(Constants.DELAY_AFTER_HAND_WINNER);
        }


        /// <summary>
        /// Get the bet amount from the slider and set the amount as player action
        /// </summary>
        private void OnButtonRaiseClick(object sender, RoutedEventArgs e)
        {
            long bet = Convert.ToInt64(sliderAmount.Value);
            bet -= _chipsPlayerInvested;
            _humanPlayerAction.SetResult(bet);
        }


        /// <summary>
        /// Set the player action as "fold"
        /// </summary>
        private void OnButtonFoldClick(object sender, RoutedEventArgs e)
        {
            _humanPlayerAction.SetResult(Constants.PLAYERACTION_FOLD);
        }

        /// <summary>
        /// Set the player action as "call"
        /// </summary>
        private void OnButtonCallClick(object sender, RoutedEventArgs e)
        {
            _humanPlayerAction.SetResult(_currentBet - _chipsPlayerInvested);
        }

        /// <summary>
        /// Make sure the label of the bet/raise button gets updated with the latest slider amount
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSliderAmountValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            UpdateRaiseButtonLabel();
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            if (_humanPlayerAction == null)
            {
                Leave();
            }
            else
            {
                _humanPlayerAction.SetResult(Constants.ACTION_SUSPEND_GAME);
            }

        }

        private async Task OnButtonMoved(object sender, TableEventArgs tableEventArgs)
        {
            DrawAllPlayers();
            await OutputStatusUpdate(tableEventArgs.StatusUpdate);
        }


    }
}
