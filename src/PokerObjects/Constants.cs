using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokerObjects
{
    public class Constants
    {
        public const long PLAYERACTION_FOLD = -1;
        public const long PLAYERACTION_CHECK = 0;

        public const int NO_POSITION = -1;
        public const double CARD_RATIO_HEIGHT_TO_WIDTH = 1.4;

        public const int MAX_HOLECARDS_ALL_GAMES = 4; // no games with more than 4 hole cards 

        public const string TEXT_FOLD = "Fold";
        public const string TEXT_CHECK = "Check";
        public const string TEXT_CALL = "Call";
        public const string TEXT_RAISE = "Raise to";
        public const string TEXT_BET = "Bet";
        public const string TEXT_ALLIN = "All in";

        public const int FONT_SIZE_SMALL = 14;
        public const int FONT_SIZE_MEDIUM = 20;
        public const int FONT_SIZE_LARGE = 30;



        public const string BUTTON_FILENAME = "Assets/button.png";

        //public static TimeSpan DELAY_BEFORE_HAND_START = TimeSpan.FromSeconds(.5);
        public static TimeSpan DELAY_AFTER_STATUS_UPDATE = TimeSpan.FromSeconds(.5);
        public static TimeSpan DELAY_AFTER_HAND_SHOW = TimeSpan.FromSeconds(2);
        public static TimeSpan DELAY_AFTER_HAND_WINNER = TimeSpan.FromSeconds(3);
        public static TimeSpan DELAY_MICRO = TimeSpan.FromMilliseconds(10);

        public const long ACTION_SUSPEND_GAME = long.MinValue;
        public const long MAX_POSITIONS = 8;

        public const string SETTINGS_LAST_HAND_NUMBER = "LastHandNumber";
        public const string SETTINGS_TABLE = "Table";
        

    }


}
