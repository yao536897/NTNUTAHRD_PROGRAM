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
        public static Dictionary<string, Sprite> player = new Dictionary<string, Sprite>();
        public static Dictionary<string, Sprite> playerCard = new Dictionary<string, Sprite>();
        public static string[] characterNameCh = new string[] { "約翰", "杰奇", "愛瑞絲", "泰瑞莎", "校長" };
        public static string[] characterNameEn = new string[] { "John", "Jacky", "Aries", "Teresa", "Master" };
    }
    public enum Character
    {
        John,
        Jacky,
        Aries,
        Teresa,
        Master,
        Custom
    }
}