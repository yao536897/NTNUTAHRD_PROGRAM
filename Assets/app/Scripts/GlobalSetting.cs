using System.Collections.Generic;
using UnityEngine;

namespace GlobalSetting
{
    public class Language
    {
        public enum languageType
        {
            English,
            Chinese
        }
        public static languageType currentLanguage = languageType.Chinese;
    }

    public class GameController
    {
        public static string[] playerName = new string[4];
        public static Dictionary<string, string> playerNameMap = new Dictionary<string, string>();
        public static Dictionary<string, Sprite> player = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> playerCard = new Dictionary<string, Sprite>();
        public static string[] characterNameCh = new string[] { "", "約翰", "杰奇", "愛瑞絲", "泰瑞莎", "校長", "四人" };
        public static string[] characterNameEn = new string[] { "", "John", "Jacky", "Aries", "Teresa", "Master", "All" };
        public static bool useOriginalName = true;

        public static int selectedIndex = 0;
        public static Game_Status gameStatus = Game_Status.None;
        public static string currentCharacter = "";
    }
    public enum Character
    {
        None,
        John,
        Jacky,
        Aries,
        Teresa,
        Master,
        All
    }

    public enum Game_Status {
        None,
        SelectCharacter
    }
}