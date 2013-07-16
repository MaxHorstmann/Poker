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
    /// <summary>
    /// The main game page, displaying the player's current poker table 
    /// </summary>
    /// 
    public sealed partial class MainPage : LayoutAwarePage
    {
        #region Fields

        private HumanPlayer _humanPlayer = null;
        private TaskCompletionSource<long> _humanPlayerAction= null;
        private long _currentBet = 0;  // number of the current bet player needs to call
        private long _chipsPlayerInvested = 0; // nuber of chips human player has currently invested in pot
        private int _actionPosition = Constants.NO_POSITION;
        private App _myApp = App.Current as App;

        private double _cardHeight = 0;
        private double _cardWidth = 0;
        private bool _displayHalfCardsForPlayers = true;
        private int _historyRowsInTooltip = 20; // less for release build

        public bool _playerHasLeft = false;        


        private Table CurrentTable
        {
            get
            {
                return _myApp.CurrentTable;
            }
        }

        private Canvas[] _canvasPositions = new Canvas[Constants.MAX_POSITIONS];
        private PlayerInfoBox[] _playerInfoBoxes = new PlayerInfoBox[Constants.MAX_POSITIONS];
        
        Brush _neutralBackground;
        Brush _playerBackground = null; //new SolidColorBrush(Windows.UI.Colors.Black);
        Brush _actionBackground = new SolidColorBrush(Windows.UI.Colors.Orange);

        #endregion

        #region Initialization

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += OnPageLoaded;
        }

        /// <summary>
        /// Initializes canvases and kicks off game loop
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPageLoaded(object sender, RoutedEventArgs e)
        {
            if (CurrentTable == null)
            {
                Leave();
                return;
            }

            CurrentTable.PlayerHasLeft = false;

            // Once done restoring state, wire up event handlers
            WireUpEventHandlers();

            // restore relationship Pot --> Table
            if (CurrentTable.Pot != null)
            {
                CurrentTable.Pot.Table = CurrentTable;
            }
            InitCanvasPositions(); // now that page is fully loaded, init canvas positions 
            HideBetButtons();
            ClearAllCardsAndChips();
            await StartGame();  // Kick off the game loop
        }

  

        private async Task OnBeginNewHand(object sender, TableEventArgs tableEventArgs)
        {
            ClearAllCardsAndChips();
            DrawAllPlayers();
            await OutputStatusUpdate(tableEventArgs.StatusUpdate);
            CurrentTable.HandHistory.Log(tableEventArgs.AdditionalLogText);

        }


        /// <summary>
        /// The main game loop
        /// </summary>
        async Task StartGame()
        {
            bool suspend = false;
            while ((!_playerHasLeft)&&(!suspend))
            {
                ClearAllCardsAndChips();
                DrawGameLabel();
                suspend = await CurrentTable.PlayHand();
                if (!_playerHasLeft)
                {
                    await CurrentTable.HandHistory.SaveToDisk();
                }
            }

            if (!_playerHasLeft)
            {
                Leave();
            }
        }

        /// <summary>
        /// Leave the table
        /// </summary>
        private void Leave()
        {
            if (CurrentTable != null)
            {
                CurrentTable.PlayerHasLeft = true;
            }
            _playerHasLeft = true;
            GoHome(this, null); // back to main screen
        }

        #endregion

   


    }
}