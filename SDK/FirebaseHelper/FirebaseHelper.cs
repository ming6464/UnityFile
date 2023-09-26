#if USE_FIREBASE
using Firebase;
using Firebase.Analytics;
#endif

#if USE_FIREBASE_REMOTE
using Firebase.Messaging;
using Firebase.RemoteConfig;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

public class FirebaseHelper : MonoBehaviour
{
    [SerializeField]
    private bool dontDestroyOnLoad = true;
    [SerializeField]
    private static string topic = "default";

    public static WaitForSeconds waitTime = null;
    public static string token = "";
    public static Dictionary<string, object> defaultRemoteConfig = new Dictionary<string, object>();

    private DateTime startLoadTime = DateTime.Now;
    [SerializeField]
    private float waitTimeForLoadAd = 1;

#if USE_FIREBASE
    [SerializeField]
    private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    public static DependencyStatus DependencyStatus
    {
        get
        {
            if (instance)
                return instance.dependencyStatus;
            return DependencyStatus.UnavailableOther;
        }
        set
        {
            if (instance)
                instance.dependencyStatus = value;
        }
    }

    private FirebaseStatus status = FirebaseStatus.UnAvailable;

    public static FirebaseStatus Status
    {
        get
        {
            if (instance)
                return instance.status;
            return FirebaseStatus.Faulted;
        }
        private set
        {
            if (instance)
                instance.status = value;
        }
    }

#endif

    private static FirebaseHelper instance { get; set; }

    private void Awake()
    {
        try
        {
            if (instance == null)
                instance = this;

            if (dontDestroyOnLoad)
                DontDestroyOnLoad(this);
        }
        catch (Exception ex)
        {
            Debug.LogError("[FirebaseHelper] Exception: " + ex.Message);
        }
    }
    private void Start()
    {
        StartCoroutine(InitLoad());
    }
    #region Base
    IEnumerator InitLoad()
    {
#if USE_FIREBASE_REMOTE
        var remote = new GameConfig();
        FirebaseHelper.defaultRemoteConfig = new Dictionary<string, object>
        {
            { "timePlayToShowInter", GameConfig.TimePlayToShowInter},
        };
#endif

#if USE_FIREBASE
        yield return FirebaseHelper.DoCheckStatus(null, true);
#endif

#if USE_FIREBASE_REMOTE
        yield return FirebaseHelper.DoFetchRemoteData((status) =>
        {
            if (status == FirebaseStatus.Completed && GameConfig.Instacne != null)
            {
                GameConfig.TimePlayToShowInter = FirebaseHelper.RemoteGetValueFloat("timePlayToShowInter", GameConfig.TimePlayToShowInter);
            }
        });
#endif

        while ((int)(DateTime.Now - startLoadTime).TotalSeconds < waitTimeForLoadAd)
        {
            yield return null;
        }
        int loadGameIn = (int)(DateTime.Now - startLoadTime).TotalSeconds;
        Debug.Log("loadGameIn: " + loadGameIn + "s");
    }
    public static IEnumerator DoCheckStatus(Action<FirebaseStatus> status = null, bool initRemote = false, float timeOut = 5)
    {
        if (instance == null)
        {
            status(FirebaseStatus.Faulted);
            Debug.LogError("[Firebase] NULL");
            yield break;
        }

#if USE_FIREBASE
        if (DependencyStatus != DependencyStatus.Available && Status != FirebaseStatus.Checking)
        {
            bool isProcess = true;
            var elapsedTime = 0f;

            CheckAndFixDependencies((dependenciesStatus) =>
            {
                Status = dependenciesStatus;
            });

            while (Status == FirebaseStatus.Checking && elapsedTime < timeOut && isProcess)
            {
                elapsedTime += Time.deltaTime;
                yield return waitTime;
            }
        }
        else
        {
            Debug.Log("[Firebase] DoCheckStatus: " + Status);
        }

        InitializeFirebase(status, initRemote);
#endif
    }

    public static void CheckAndFixDependencies(Action<FirebaseStatus> status = null)
    {
#if USE_FIREBASE
        if (instance == null)
        {
            status?.Invoke(FirebaseStatus.Faulted);
            Debug.LogError("[Firebase] NULL");
            return;
        }

        if (DependencyStatus != DependencyStatus.Available && Status != FirebaseStatus.Checking)
        {
            try
            {
                Status = FirebaseStatus.Checking;
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
                {
                    if (task != null)
                    {
                        DependencyStatus = task.Result;
                        if (DependencyStatus == DependencyStatus.Available)
                        {
                            Debug.Log("[Firebase] CheckDependencies: " + DependencyStatus);
                            status?.Invoke(FirebaseStatus.Available);
                        }
                        else
                        {
                            Debug.LogError("[Firebase] CheckDependencies: " + DependencyStatus);
                            status?.Invoke(FirebaseStatus.Faulted);
                        }
                    }
                    else
                    {
                        Debug.LogError("[Firebase] CheckDependencies: " + DependencyStatus);
                        status?.Invoke(FirebaseStatus.Faulted);
                    }
                });
            }
            catch (FirebaseException ex)
            {
                Debug.LogError("[Firebase] CheckDependencies: " + ex.Message);
                status?.Invoke(FirebaseStatus.Faulted);
            }
            catch (Exception ex)
            {
                Debug.LogError("[Firebase] CheckDependencies: " + ex.Message);
                status?.Invoke(FirebaseStatus.Faulted);
            }
        }
        else
        {
            status?.Invoke(Status);
            Debug.Log("[Firebase] CheckDependencies again: " + DependencyStatus);
        }
#endif
    }

    public static void InitializeFirebase(Action<FirebaseStatus> status = null, bool initRemote = false)
    {
#if USE_FIREBASE
        if (instance == null)
        {
            status?.Invoke(FirebaseStatus.Faulted);
            Debug.LogError("[Firebase] NULL");
            return;
        }

        if (DependencyStatus == DependencyStatus.Available &&
            Status != FirebaseStatus.Initialized
            && Status != FirebaseStatus.Initialing)
        {
            try
            {
                Status = FirebaseStatus.Initialing;
                Debug.Log("[Firebase] Initialize " + Status);

                try
                {
                    FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
                    Debug.Log("[Firebase] Analytics " + "Initialized");
                }
                catch (FirebaseException ex)
                {
                    Debug.LogError("[Firebase] Analytics " + "Initialized " + ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Firebase] Analytics " + "Initialized " + ex.Message);
                }
#if USE_FIREBASE_REMOTE
                try
                {
                    FirebaseMessaging.TokenReceived += OnTokenReceived;
                    FirebaseMessaging.MessageReceived += OnMessageReceived;

                    ////FirebaseMessaging.SubscribeAsync(topic).ContinueWith(task =>
                    ////{
                    ////    LogTaskCompletion(task, "[Firebase] SubscribeAsync");
                    ////});

                    //// This will display the prompt to request permission to receive
                    //// notifications if the prompt has not already been displayed before. (If
                    //// the user already responded to the prompt, thier decision is cached by
                    //// the OS and can be changed in the OS settings).
                    FirebaseMessaging.RequestPermissionAsync().ContinueWith(task =>
                    {
                        LogTaskCompletion(task, "[Firebase] RequestPermissionAsync");
                    });
                    Debug.Log("[Firebase] Messaging " + "Initialized");
                }
                catch (FirebaseException ex)
                {
                    Debug.LogError("[Firebase] Messaging " + "Initialized " + ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Firebase] Messaging " + "Initialized " + ex.Message);
                }

                try
                {
                    if (initRemote)
                    {
                        if (defaultRemoteConfig != null && defaultRemoteConfig.Count > 0)
                        {
                            FirebaseRemoteConfig.SetDefaults(defaultRemoteConfig);
                            ConfigSettings remoteSettings = FirebaseRemoteConfig.Settings;
                            remoteSettings.IsDeveloperMode = isDebugMode;
                            Debug.Log("[Firebase] RemoteConfig " + "Initialized");
                        }
                        else
                        {
                            Debug.LogError("defaultRemoteConfig NULL or EMPTY! Please set defaultRemoteConfig befor call Init");
                        }
                    }
                }
                catch (FirebaseException ex)
                {
                    Debug.LogError("[Firebase] RemoteConfig " + "Initialized " + ex.Message);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Firebase] RemoteConfig " + "Initialized " + ex.Message);
                }
#endif

                Status = FirebaseStatus.Initialized;
                Debug.Log("[Firebase] Status " + Status);
                status?.Invoke(Status);
            }
            catch (FirebaseException ex)
            {
                Status = FirebaseStatus.UnkownError;
                Debug.LogError("[Firebase] Initialize " + Status + "\n" + ex.Message);
                status?.Invoke(Status);
            }
            catch (Exception ex)
            {
                Status = FirebaseStatus.UnkownError;
                Debug.LogError("[Firebase] Initialize " + Status + "\n" + ex.Message);
                status?.Invoke(Status);
            }
        }
        else
        {
            Debug.Log("[Firebase] Initialize again " + Status);
            status?.Invoke(Status);
        }
#endif
    }

    public static void Subscribe()
    {
        if (instance == null || !IsConnected)
        {
            Debug.LogError("[Firebase] NULL");
            return;
        }

#if USE_FIREBASE_REMOTE
        FirebaseMessaging.SubscribeAsync(topic).ContinueWith(task =>
        {
            LogTaskCompletion(task, "SubscribeAsync");
        });
        Debug.Log("Subscribed to " + topic);
#endif
    }

    public static void Unsubscribe()
    {
        if (instance == null || !IsConnected)
        {
            Debug.LogError("[Firebase] NULL");
            return;
        }

#if USE_FIREBASE_REMOTE
        FirebaseMessaging.UnsubscribeAsync(topic).ContinueWith(task =>
        {
            LogTaskCompletion(task, "UnsubscribeAsync");
        });
#endif
        Debug.Log("UnsubscribeAsync to " + topic);
    }

    #endregion

    #region FirebaseRemoteConfigs
    public static void FetchRemoteData(Action<FirebaseStatus> status, int cacheExpirationHours = 12, float timeOut = 5)
    {
        Debug.Log("[Firebase] FetchRemoteData");
        if (instance == null || !IsConnected)
        {
            status?.Invoke(FirebaseStatus.Faulted);
            Debug.LogError("[Firebase] NULL");
            return;
        }
        instance.StartCoroutine(DoFetchRemoteData(status, cacheExpirationHours, timeOut));
    }

    public static IEnumerator DoFetchRemoteData(Action<FirebaseStatus> status, int cacheExpirationHours = 12, float timeOut = 5)
    {
        Debug.Log("[Firebase] DoFetchRemoteData");
        if (instance == null || !IsConnected)
        {
            status?.Invoke(FirebaseStatus.Faulted);
            Debug.LogError("[Firebase] NULL");
            yield break;
        }

        bool isProcess = true;
        var elapsedTime = 0f;

        FetchAsync((s) =>
        {
            if (isProcess)
                status?.Invoke(s);
            isProcess = false;
        }, cacheExpirationHours);

        while (elapsedTime < timeOut && isProcess)
        {
            elapsedTime += Time.deltaTime;
            yield return waitTime;
        }

        if (isProcess)
        {
            isProcess = false;
            status?.Invoke(FirebaseStatus.TimeOut);
            Debug.LogError("[Firebase] DoFetchRemoteData TimeOut " + elapsedTime.ToString("0.0"));
            yield break;
        }
    }

    /// <summary>
    /// FetchAsync only fetches new data if the current data is older than the provided
    /// timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
    /// By default the timespan is 12 hours, and for production apps, this is a good
    /// number.  For this example though, it's set to a timespan of zero, so that
    /// changes in the console will always show up immediately.
    /// </summary>
    /// <param name="status"></param>
    /// <param name="cacheExpirationHours">default = 12</param>
    public static void FetchAsync(Action<FirebaseStatus> status, int cacheExpirationHours = 12)
    {
#if USE_FIREBASE_REMOTE
        if (DependencyStatus == DependencyStatus.Available && Status == FirebaseStatus.Initialized)
        {
            if (isDebugMode)
                cacheExpirationHours = 0;

            FirebaseRemoteConfig.FetchAsync(TimeSpan.FromHours(cacheExpirationHours)).ContinueWith((fetchTask) =>
            {
                try
                {
                    if (fetchTask.IsCanceled)
                    {
                        Debug.LogError("Fetch canceled.");
                        status?.Invoke(FirebaseStatus.Canceled);
                        return;
                    }
                    else if (fetchTask.IsFaulted)
                    {
                        Debug.LogError("Fetch encountered an error.");
                        status?.Invoke(FirebaseStatus.Faulted);
                        return;
                    }
                    else if (fetchTask.IsCompleted)
                    {
                        Debug.Log("Fetch completed successfully!");
                    }

                    var info = FirebaseRemoteConfig.Info;
                    switch (info.LastFetchStatus)
                    {
                        case LastFetchStatus.Success:
                            FirebaseRemoteConfig.ActivateFetched();
                            Debug.Log(string.Format("[Firebase] DoFetchRemoteData Remote data loaded and ready (last fetch time {0}).", info.FetchTime));

                            string remoteData = "[Firebase] DoFetchRemoteData";
                            foreach (var i in FirebaseRemoteConfig.Keys)
                            {
                                string key = i;
                                remoteData += "\n" + key + ": " + FirebaseRemoteConfig.GetValue(key).StringValue;
                            }
                            Debug.Log(remoteData);
                            status?.Invoke(FirebaseStatus.Completed);
                            break;
                        case LastFetchStatus.Failure:
                            switch (info.LastFetchFailureReason)
                            {
                                case FetchFailureReason.Error:
                                    Debug.LogError("[Firebase] DoFetchRemoteData Fetch failed for unknown reason");
                                    status?.Invoke(FirebaseStatus.UnkownError);
                                    break;
                                case FetchFailureReason.Throttled:
                                    Debug.LogError("[Firebase] DoFetchRemoteData Fetch throttled until " + info.ThrottledEndTime);
                                    status?.Invoke(FirebaseStatus.TimeOut);
                                    break;
                            }
                            break;
                        case LastFetchStatus.Pending:
                            Debug.LogError("[Firebase] DoFetchRemoteData Latest Fetch call still pending.");
                            status?.Invoke(FirebaseStatus.Getting);
                            break;
                    }
                }
                catch (FirebaseException ex)
                {
                    Debug.LogError("[Firebase] DoFetchRemoteData FirebaseException: " + ex.Message);
                    status?.Invoke(FirebaseStatus.UnkownError);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[Firebase] DoFetchRemoteData Exception: " + ex.Message);
                    status?.Invoke(FirebaseStatus.UnkownError);
                }
            });
        }
        else
        {
            status?.Invoke(FirebaseStatus.UnAvailable);
        }
#endif
    }

    public static string RemoteGetValueString(string title, string defaultValue)
    {
#if USE_FIREBASE_REMOTE
        if (FirebaseRemoteConfig.Keys != null && FirebaseRemoteConfig.Keys.Contains(title))
        {
            var data = FirebaseRemoteConfig.GetValue(title).StringValue;
            //Debug.Log(title + " " + data);
            return data;
        }
#endif
        return defaultValue;
    }

    public static int RemoteGetValueInt(string title, int defaultValue)
    {
#if USE_FIREBASE_REMOTE
        if (FirebaseRemoteConfig.Keys != null && FirebaseRemoteConfig.Keys.Contains(title))
        {
            var data = (int)FirebaseRemoteConfig.GetValue(title).LongValue;
            //Debug.Log(title + " " + data);
            return data;
        }
#endif
        return defaultValue;
    }

    public static float RemoteGetValueFloat(string title, float defaultValue)
    {
#if USE_FIREBASE_REMOTE
        if (FirebaseRemoteConfig.Keys != null && FirebaseRemoteConfig.Keys.Contains(title))
        {
            var style = NumberStyles.AllowDecimalPoint;
            var culture = CultureInfo.CreateSpecificCulture("en-US");
            float data = -9999f;

            if (float.TryParse(FirebaseRemoteConfig.GetValue(title).StringValue, style, culture, out data))
            {
                //Debug.Log(title + " " + data);
                return data;
            }
        }
#endif
        return defaultValue;
    }

    public static byte[] RemoteGetValueArray(string title, byte[] defaultValue)
    {
#if USE_FIREBASE_REMOTE
        if (FirebaseRemoteConfig.Keys != null && FirebaseRemoteConfig.Keys.Contains(title))
        {
            return (byte[])FirebaseRemoteConfig.GetValue(title).ByteArrayValue;
        }
#endif
        return defaultValue;
    }
    #endregion

    #region FirebaseAnalytic
    public static void SetUser(string title, object property)
    {
#if USE_FIREBASE
        try
        {
            if (instance == null || DependencyStatus != DependencyStatus.Available || property == null)
                return;
            FirebaseAnalytics.SetUserProperty(title, property.ToString());
        }
        catch (Exception ex)
        {
            Debug.LogError("[Firebase] SetUser: " + ex.Message);
        }
#endif
    }
    public static string ConvertLogStartLevel()
    {
        string event_log = "";
        return event_log;
    }
    public static string ConvertLogWinLevel()
    {
        string event_log = "";
        return event_log;
    }
    public static void LogEvent(string eventName, Dictionary<string, object> dictionary = null)
    {
#if USE_FIREBASE
        if (instance == null || DependencyStatus != DependencyStatus.Available)
            return;
        try
        {
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("eventName IsNullOrEmpty");
                return;
            }

            if (eventName.Length >= 32)
                eventName = eventName.Substring(0, 32);
            eventName = eventName.ToLower();

            if (dictionary != null)
            {
                var param = dictionary.Select(x =>
                {
                    if (x.Value is float)
                        return new Parameter(x.Key.ToLower(), (float)x.Value);
                    else if (x.Value is long)
                        return new Parameter(x.Key.ToLower(), (long)x.Value);
                    else if (x.Value is int)
                        return new Parameter(x.Key.ToLower(), (int)x.Value);
                    else if (x.Value is string)
                        return new Parameter(x.Key.ToLower(), !x.Value.ToString().Contains("_") ? Regex.Replace(x.Value.ToString(), @"\B[A-Z]", m => "_" + m.ToString()).ToLower() : x.Value.ToString());
                    else
                        return new Parameter(x.Key.ToLower(), "");
                }).ToArray();

                FirebaseAnalytics.LogEvent(eventName, param);
            }
            else
            {
                FirebaseAnalytics.LogEvent(eventName);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[Firebase] LogEvent: " + ex.Message);
        }
#endif
    }

    public static void LogEarnVirtualCurrency(int value, string name)
    {
#if USE_FIREBASE
        if (instance == null || DependencyStatus != DependencyStatus.Available)
            return;

        FirebaseAnalytics.LogEvent(
          FirebaseAnalytics.EventEarnVirtualCurrency,
          new Parameter[]
          {
            new Parameter(FirebaseAnalytics.ParameterValue, value),
            new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name)
          });
#endif
    }

    public static void LogSpendVirtualCurrency(int value, string name)
    {
#if USE_FIREBASE
        if (instance == null || DependencyStatus != DependencyStatus.Available)
            return;

        FirebaseAnalytics.LogEvent(
          FirebaseAnalytics.EventSpendVirtualCurrency,
          new Parameter[]
          {
            new Parameter(FirebaseAnalytics.ParameterValue, value),
            new Parameter(FirebaseAnalytics.ParameterVirtualCurrencyName, name)
          });
#endif
    }

    public static void LogScreen(string screentitle, string screenPopup = "None")
    {
#if USE_FIREBASE
        if (instance == null || DependencyStatus != DependencyStatus.Available)
            return;

        if (string.IsNullOrEmpty(screentitle) || string.IsNullOrEmpty(screenPopup))
            return;

        // FirebaseAnalytics.SetCurrentScreen(screentitle, screenPopup);
#endif
    }

    public static void LogLevelUp(string character, string level)
    {
#if USE_FIREBASE
        if (instance == null || DependencyStatus != DependencyStatus.Available)
            return;

        if (string.IsNullOrEmpty(character) || string.IsNullOrEmpty(level))
            return;

        SetUser("Level", level);

        FirebaseAnalytics.LogEvent(
            FirebaseAnalytics.EventLevelUp,
            new Parameter[] {
            new Parameter( FirebaseAnalytics.ParameterCharacter, character),
            new Parameter(  FirebaseAnalytics.ParameterLevel, level),
            }
        );
#endif
    }
    #endregion

    #region FirebaseMessage
#if USE_FIREBASE_REMOTE
    public static FirebaseMessage firebaseMessage { get; private set; }

    public static FirebaseNotification notification { get; private set; }

    private static void OnTokenReceived(object sender, TokenReceivedEventArgs e)
    {
        token = e.Token;
        //instance?.StartCoroutine(WaitServerOnlineToUpdate(token));
        Debug.Log("[Firebase] On Token Received:" + "\n" + token);
    }

    public static void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("On Message Received");

        firebaseMessage = e.Message;
        notification = firebaseMessage?.Notification;

        Debug.Log("Received a new message");
        if (notification != null)
        {
            Debug.Log("Title: " + notification.Title);
            Debug.Log("Body: " + notification.Body);
            var android = notification.Android;
            if (android != null)
            {
                Debug.Log("ChannelId: " + android.ChannelId);
            }
        }
        if (e.Message.From.Length > 0)
            Debug.Log("From: " + e.Message.From);
        if (e.Message.Link != null)
        {
            Debug.Log("Link: " + e.Message.Link.ToString());
        }
        if (e.Message.Data.Count > 0)
        {
            Debug.Log("Data:");
            foreach (KeyValuePair<string, string> i in e.Message.Data)
            {
                Debug.Log("\n" + i.Key + ": " + i.Value);
            }
        }
    }
#endif
    #endregion

    public static bool IsConnected
    {
        get
        {
            switch (Application.internetReachability)
            {
                case NetworkReachability.ReachableViaLocalAreaNetwork:
                    return true;
                case NetworkReachability.ReachableViaCarrierDataNetwork:
                    return true;
                default:
                    return false;
            }
        }
    }

}

public enum FirebaseStatus
{
    UnAvailable,
    Checking,
    Available,
    Initialing,
    Initialized,
    Getting,
    Completed,
    Faulted,
    Canceled,
    TimeOut,
    NoInternet,
    UnkownError
}