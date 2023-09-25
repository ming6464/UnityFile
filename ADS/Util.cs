using System;
using UnityEngine;

public abstract class Util
{
    private const string FIRST_TIME_OPEN = "FIRST_TIME_OPEN";
    public static string ConvertNum(double input)
    {
        if (input < 1000.0)
        {
            return Math.Round(input).ToString();
        }
        double num = 0.0;
        for (int i = 0; i < format.Length; i++)
        {
            num = input / Math.Pow(1000.0, (double)(i + 1));
            if (num < 1000.0)
            {
                return Math.Round(num, (num >= 100.0) ? 0 : 1).ToString() + format[i];
            }
        }
        return num.ToString();
    }
    private static string[] format = new string[]
        {
            "K",
            "M",
            "B",
            "T",
            "aa",
            "ab",
            "ac",
            "ad",
            "ae",
            "af",
            "ag",
            "ah",
            "ai",
            "aj",
            "ak",
            "al",
            "am",
            "an",
            "ao",
            "ap",
            "aq",
            "ar",
            "as",
            "at",
            "au",
            "av",
            "aw",
            "ax",
            "ay",
            "az"
        };

    #region save day
    public static int Dayreward
    {
        get { return PlayerPrefs.GetInt("Dayreward"); }
        set { PlayerPrefs.SetInt("Dayreward", value); }
    }
    public static int DayGame
    {
        get { return PlayerPrefs.GetInt("DayGame"); }
        set { PlayerPrefs.SetInt("DayGame", value); }
    }
    public static int MonthGame
    {
        get { return PlayerPrefs.GetInt("MonthGame"); }
        set { PlayerPrefs.SetInt("MonthGame", value); }
    }
    public static int YearGame
    {
        get { return PlayerPrefs.GetInt("YearGame"); }
        set { PlayerPrefs.SetInt("YearGame", value); }
    }
    public static bool IsNewDay
    {
        get
        {
            DateTime dateTime = DateTime.Now;
            if (dateTime.Year >= YearGame)
            {
                if (dateTime.Month > MonthGame)
                {
                    return true;
                }
                if (dateTime.Month == MonthGame)
                {
                    if (dateTime.Day > DayGame)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        set
        {
            DateTime dateTime = DateTime.Now;
            DayGame = dateTime.Day;
            MonthGame = dateTime.Month;
            YearGame = dateTime.Year;
        }
    }
    public static int CurrentStarSong { get; internal set; }
    #endregion

    public static int timeNow
    {
        get
        {
            DateTime datetime = DateTime.Now;
            int month = datetime.Month;
            int day = datetime.Day;
            int hour = datetime.Hour;
            int minute = datetime.Minute;
            int second = datetime.Second;
            int TimeNow = month * 30 * 24 * 60 * 60 + (day * 24 * 60 * 60) + (hour * 60 * 60) + (minute * 60) + second;
            return TimeNow;
        }
    }
    public static int GetTime (DateTime datetime)
    {
        int month = datetime.Month;
        int day = datetime.Day;
        int hour = datetime.Hour;
        int minute = datetime.Minute;
        int second = datetime.Second;
        int TimeNow = month * 30 * 24 * 60 * 60 + (day * 24 * 60 * 60) + (hour * 60 * 60) + (minute * 60) + second;
        return TimeNow;
    }
    
    public static int timeOut
    {
        set { PlayerPrefs.SetInt("TimeOutGame", value); }
        get { return PlayerPrefs.GetInt("TimeOutGame", 0); }
    }
    public static string ConverTime(int time)
    {
        int hour = time % 86400 / 3600;
        int min = time % 3600 / 60;
        int sec = time % 60;
        return hour > 0 ? GetNumSec(hour) + ":" : "" + GetNumSec(min) + ":" + GetNumSec(sec);
    }
    public static string ConverTimeMinSec(int time)
    {
        int min = time / 60;
        int sec = time % 60;
        return GetNumSec(min) + ":" + GetNumSec(sec);
    }
    static string GetNumSec(int num)
    {
        string temp = "";
        if (num > 0)
        {
            temp += (num < 10 ? "0" + num : num + "");
        }
        else
            temp += "00";
        return temp;
    }

    public static bool isInternetConection
    {
        get => Application.internetReachability != NetworkReachability.NotReachable;
    }
    public static bool IsShowBanner = false;
    public static bool IsShowRewards = false;

    // check app open first time
    public static bool IsAppFirstOpen()
    {
        bool firstTimeOpen = true;
        PlayerPrefs.DeleteKey(FIRST_TIME_OPEN);
        if (!PlayerPrefs.HasKey(FIRST_TIME_OPEN))
        {
            PlayerPrefs.SetInt(FIRST_TIME_OPEN, 1);
            Debug.Log("First Time Open: true");
        }
        else
        {
            Debug.Log("First Time Open: false");
            firstTimeOpen = false;
        }
        return firstTimeOpen;
    }

    //public static string TextRanged = "_ranged_";
    //public static string TextMelee = "_melee_";

    public static bool _isStartFight = false;
    public static bool IsBuyChar = false;
    public static bool IsBuyCharTutorial = false;
    public static bool IsBuyCharAds = false;
    public static bool FirstOpen = true;
    public static int timeLastShowAds = 0;
    public static int CountShowInter = 0;
    public static int CountShowReward = 0;
    public static int CountShowBanner = 0;
    public static int CoinWinLose = 0;
    public static int TimeStarGame = 0;
    public static int time_level_start = 0;
    public static DateTime LastTimeShowAds;
}
