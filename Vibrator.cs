using UnityEngine;

public static class Vibrator
{
#if UNITY_ANDROID && !UNITY_EDITOR    
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject curActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = curActivity.Call<AndroidJavaObject>("getSystemService", "vibrator"); 
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject curActivity;
    public static AndroidJavaObject vibrator;
#endif

    private static bool IsAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        return true;
#else
        return false;
#endif
    }
    
    //works well on android devices
    public static void Vibrate(long milliseconds = 250)
    {
        if (IsAndroid())
        {
            //vibration should be with vibration time of milliseconds
            vibrator.Call("vibrate", milliseconds);
        }
        else
        {
            Handheld.Vibrate();
        }
    }

    public static void Cancel()
    {
        //stop vibration if the device is vibrating
        if(IsAndroid()) vibrator.Call("cancel");
    }
    
}
