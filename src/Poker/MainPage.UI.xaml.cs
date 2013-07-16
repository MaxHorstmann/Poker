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
        /// Create the canvas elements for the players
        /// </summary>
        private void InitCanvasPositions()
        {
            _canvasPositions[0] = _canvasPosition0;
            _canvasPositions[1] = _canvasPosition1;
            _canvasPositions[2] = _canvasPosition2;
            _canvasPositions[3] = _canvasPosition3;
            _canvasPositions[4] = _canvasPosition4;
            _canvasPositions[5] = _canvasPosition5;
            _canvasPositions[6] = _canvasPosition6;
            _canvasPositions[7] = _canvasPosition7;

            // Attach a new PlayerBoxUI to every canvas
            foreach (Canvas canvas in _canvasPositions)
            {
                canvas.Tag = new PlayerBoxUI(this, canvas, _displayHalfCardsForPlayers);
            }

            _canvasBoard.Tag = new BoardUI(this, _canvasBoard);

            _neutralBackground = _canvasPot.Background;
            if (_playerBackground == null)
            {
                _playerBackground = _neutralBackground;
            }
        }

        /// <summary>
        /// Display a status update on screen
        /// </summary>
        /// <param name="s"></param>
        private async Task OutputStatusUpdate(string s, bool withDelay = true)
        {
            ToolTipService.SetToolTip(textBlockStatus, CurrentTable.HandHistory.GetHistory(_historyRowsInTooltip));
            textBlockStatus.Text = s;
            CurrentTable.HandHistory.Log(s);
            if ((withDelay) && (!string.IsNullOrEmpty(s)))
            {
                await Task.Delay(Constants.DELAY_AFTER_STATUS_UPDATE);
            }
        }



        /// <summary>
        /// Show the UI buttons for human player action and await action
        /// </summary>
        /// <param name="currentBet"></param>
        /// <returns></returns>
        public async Task<long> GetActionFromUI(HumanPlayer p, long currentBet, long chipsPlayerInvested, long minRaise, long maxRaise, long defaultRaise, long stepFrequency, bool raiseAllowed)
        {
            if (_playerHasLeft)
            {
                return Constants.ACTION_SUSPEND_GAME;
            }

            _currentBet = currentBet;
            _chipsPlayerInvested = chipsPlayerInvested;

            _humanPlayerAction = new TaskCompletionSource<long>();
            ShowBetButtons(p, minRaise, maxRaise, defaultRaise, stepFrequency, raiseAllowed);
            await _humanPlayerAction.Task;
            long action = _humanPlayerAction.Task.Result;
            _humanPlayerAction = null;
            HideBetButtons();

            return action;
        }

        /// <summary>
        /// Updates label of bet/raise button based on slider value
        /// </summary>
        private void UpdateRaiseButtonLabel()
        {
            if (sliderAmount.Value < _humanPlayer.Chipcount)
            {
                buttonRaise.Content = string.Format("{0} {1}",
                    _currentBet > 0 ? Constants.TEXT_RAISE : Constants.TEXT_BET,
                    Convert.ToInt64(sliderAmount.Value));
            }
            else
            {
                buttonRaise.Content = Constants.TEXT_ALLIN;
            }
        }

        /// <summary>
        /// Show the fold/call/bet buttons to get player's action
        /// </summary>
        private void ShowBetButtons(HumanPlayer p, long minRaise, long maxRaise, long defaultRaise, long stepFrequency, bool raiseAllowed)
        {
            _humanPlayer = p;

            // Make sure max > min and defaultRaise within boundaries
            if (minRaise > maxRaise)
            {
                throw new ArgumentOutOfRangeException("maxRaise > minRaise");
            }

            if ((defaultRaise < minRaise) || (defaultRaise > maxRaise))
            {
                throw new ArgumentOutOfRangeException("defaultRaise invalid");
            }

            // Set bet/raise slider range
            sliderAmount.Minimum = minRaise;
            sliderAmount.Maximum = maxRaise;
            sliderAmount.Value = defaultRaise;
            sliderAmount.StepFrequency = stepFrequency;

            // Label buttons
            buttonFold.Content = Constants.TEXT_FOLD;
            if (_currentBet > _chipsPlayerInvested)
            {
                long callAmount = _currentBet;
                long callDiff = _currentBet - _chipsPlayerInvested;
                if (callDiff < _humanPlayer.Chipcount)
                {
                    buttonCall.Content = string.Format("{0} {1}", Constants.TEXT_CALL, callAmount);
                }
                else
                {
                    // all-in call
                    buttonCall.Content = Constants.TEXT_ALLIN;
                    raiseAllowed = false;
                }

            }
            else
            {
                buttonCall.Content = Constants.TEXT_CHECK;
            }
            UpdateRaiseButtonLabel();

            // Make the buttons visible
            buttonFold.Visibility = Visibility.Visible;
            buttonCall.Visibility = Visibility.Visible;
            if (raiseAllowed)
            {
                buttonRaise.Visibility = Visibility.Visible;
            }

            // Even if a raise is allowed,
            // show slider only if there is a range of possible raises 
            // (never the case in limit games, for instance)
            if ((minRaise != maxRaise) && (raiseAllowed))
            {
                sliderAmount.Visibility = Visibility.Visible;
            }
        }

        /// <summary>
        /// Hide fold/call/bet buttons when it's not the player's turn
        /// </summary>
        private void HideBetButtons()
        {
            buttonRaise.Visibility = Visibility.Collapsed;
            buttonFold.Visibility = Visibility.Collapsed;
            buttonCall.Visibility = Visibility.Collapsed;
            sliderAmount.Visibility = Visibility.Collapsed;
        }




        /// <summary>
        /// Removes all the card images from the table
        /// </summary>
        private void ClearAllCardsAndChips()
        {
            foreach (Canvas canvas in _canvasPositions)
            {
                canvas.Children.Clear();
            }
            _canvasBoard.Children.Clear();
            DrawPot();


        }

        /// <summary>
        /// Draw the box with all the player information
        /// </summary>
        /// <param name="p"></param>
        /// <param name="position"></param>
        private void DrawPlayer(Pot pot, Player p, int position)
        {
            Canvas canvas = _canvasPositions[position];
            PlayerBoxUI playerBoxUI = canvas.Tag as PlayerBoxUI;
            if (p == null)
            {
                playerBoxUI.Clear();
                canvas.Background = _neutralBackground;
            }
            else
            {
                playerBoxUI.DrawElements(p, position, CurrentTable);
                if ((position != Constants.NO_POSITION) && (position == _actionPosition))
                {
                    canvas.Background = _actionBackground;
                }
                else
                {
                    canvas.Background = _playerBackground;
                }

                // Store card dimensions 
                if (_displayHalfCardsForPlayers)
                {
                    _cardHeight = playerBoxUI.CardHeight * 2; // since players only see half cards
                }
                else
                {
                    _cardHeight = playerBoxUI.CardHeight;
                }
                _cardWidth = _cardHeight / Constants.CARD_RATIO_HEIGHT_TO_WIDTH;
            }
        }

        /// <summary>
        /// Draws the chips in the pot (main pot and, potentially, side pots)
        /// </summary>
        private void DrawPot(Pot pot = null)
        {
            _textBlockPotSize.FontSize = Constants.FONT_SIZE_MEDIUM;

            string potText = string.Empty;
            if (pot != null)
            {
                if (pot.NumberOfPots <= 1)
                {
                    long chips = pot.GetChipsInPot(0);
                    if (chips == 0)
                    {
                        potText = "";
                    }
                    else
                    {
                        potText = pot.GetChipsInPot(0).ToString();
                    }
                }
                else
                {
                    StringBuilder sbPotText = new StringBuilder();
                    for (int potIndex = 0; potIndex < pot.NumberOfPots; potIndex++)
                    {
                        long chips = pot.GetChipsInPot(potIndex);
                        if (chips > 0)
                        {
                            sbPotText.AppendFormat("{0}  {1}\n",
                                chips,
                                pot.GetPotDescription(potIndex));
                        }
                    }
                    potText = sbPotText.ToString();
                }
            }

            _textBlockPotSize.Text = potText;
        }

        /// <summary>
        /// Draws the boxes for all the players
        /// </summary>
        private void DrawAllPlayers()
        {
            for (int position = 0; position < Constants.MAX_POSITIONS; position++)
            {
                DrawPlayer(CurrentTable.Pot, CurrentTable.Players[position], position);
            }
        }

        /// <summary>
        /// Draws the label which displays the current type of game
        /// </summary>
        private void DrawGameLabel()
        {
            if ((CurrentTable == null) || (CurrentTable.Game == null))
            {
                _textBlockGame.Text = "";
            }
            else
            {
                _textBlockGame.Text = CurrentTable.Game.ToString();
            }
        }

    }
}
