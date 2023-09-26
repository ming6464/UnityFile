#if USE_APPSFLYER
using AppsFlyerSDK;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

public class AppsflyerHelper : MonoBehaviour
{
    public static AppsflyerHelper instance;

    private static string TAG;

    private void Awake()
    {
        TAG = "[" + name + "] ";
        instance = this;
    }

    private void Start()
    {
        try
        {
#if USE_APPSFLYER
            AppsFlyer.setIsDebug(false);
            LoadEvent();
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError(TAG + " Init Exception: " + ex.Message);
        }
    }


    public static void Log(eventId id)
    {
        if (instance == null && eventDic == null)
            return;

        try
        {
            Debug.Log("111111 " + id.ToString());
            int count = SetEvent(id);

            if (id == eventId.completed_level)
            {
#if USE_APPSFLYER
                AppsFlyer.sendEvent(id.ToString(), new Dictionary<string, string> { { id.ToString(), count.ToString() } });
#endif
                Debug.Log(TAG + "Log " + id.ToString() + " " + count);
                return;
            }

            //             if (id == eventId.level_up
            //                 || id == eventId.ad_banner_show
            //                 || id == eventId.ad_interstitial_show
            //                 || id == eventId.ad_videorewared_show)
            //             {
            // #if USE_APPSFLYER
            //                 AppsFlyer.sendEvent(id.ToString(), new Dictionary<string, string> { { id.ToString(), count.ToString() } });
            // #endif
            //                 Debug.Log(TAG + "Log " + id.ToString() + " " + count);
            //             }

#if USE_APPSFLYER
            AppsFlyer.sendEvent(id.ToString(), new Dictionary<string, string> { { id.ToString(), count.ToString() } });
#endif
            Debug.Log(TAG + "Log " + id.ToString() + " " + count);

            //             string counter = count.ToString();
            //             if (count > 10)
            //                 counter = "N";
            //             string eventName = id + "_" + counter;
            //
            //             if ((id == eventId.ad_interstitial_show) && count >= 5 && count % 5 != 0)
            //             {
            //                 Debug.Log(TAG + "Log " + eventName + " RETURN " + count);
            //                 return;
            //             }
            //             else if ((id == eventId.ad_videorewared_show) && count >= 5 && count % 5 != 0)
            //             {
            //                 Debug.Log(TAG + "Log " + eventName + " RETURN " + count);
            //                 return;
            //             }
            //             else if ((id == eventId.ad_banner_show) && count >= 10 && count % 10 != 0)
            //             {
            //                 Debug.Log(TAG + "Log " + eventName + " RETURN " + count);
            //                 return;
            //             }
            // #if USE_APPSFLYER
            //             AppsFlyer.sendEvent(eventName.ToLower(), new Dictionary<string, string> { { id.ToString(), count.ToString() } });
            // #endif
            //             Debug.Log(TAG + "Log " + eventName + " " + count);
        }
        catch (Exception ex)
        {
            Debug.LogError(TAG + "Log " + ex.Message);
        }
    }

    private static Dictionary<eventId, int> eventDic;
    private static void LoadEvent()
    {
        eventDic = new Dictionary<eventId, int>();
        foreach (eventId e in Enum.GetValues(typeof(eventId)))
        {
            int count = PlayerPrefs.GetInt(e.ToString(), 0);
            eventDic.Add(e, count);
        }
    }

    public static void SaveEvent()
    {
        if (eventDic != null)
        {
            foreach (var e in eventDic)
            {
                PlayerPrefs.SetInt(e.Key.ToString(), e.Value);
            }
            PlayerPrefs.Save();
        }
    }

    private static int SetEvent(eventId id, bool autoSave = true)
    {
        if (eventDic != null && eventDic.ContainsKey(id))
        {
            eventDic[id]++;
            int count = eventDic[id];
            if (autoSave)
            {
                PlayerPrefs.SetInt(id.ToString(), count);
                PlayerPrefs.Save();
            }
            return count;
        }
        return 0;
    }

    public enum eventId
    {
        completed_level,
        af_inters_ad_eligible,
        af_inters_api_called,
        af_inters_displayed,
        af_rewarded_ad_eligible,
        af_rewarded_api_called,
        af_rewarded_ad_displayed
    }

    public void OnAdRevenuePaidEvent(IronSourceImpressionData adInfo)
    {
        Dictionary<string, string> additionalParameters = new Dictionary<string, string>();
        additionalParameters.Add("ad_platform", "IronSource");
        additionalParameters.Add("ad_source", adInfo.adNetwork);
        additionalParameters.Add("ad_unit_name", adInfo.instanceName);
        additionalParameters.Add("ad_format", adInfo.adUnit);
        additionalParameters.Add("value", adInfo.revenue.ToString());
        additionalParameters.Add("currency", "USD");
        AppsFlyerAdRevenue.logAdRevenue(adInfo.adNetwork,
                                            AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeApplovinMax,
                                            (double)adInfo.revenue, "USD", additionalParameters);

    }
    public static void LogEvent(string eventName, Dictionary<string, string> dictionary = null)
    {
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

            Debug.Log("Log event af :" + eventName);
            if (dictionary != null)
            {
                AppsFlyer.sendEvent(eventName, dictionary);
            }
            else
            {
                dictionary = new Dictionary<string, string>();
                AppsFlyer.sendEvent(eventName, dictionary);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("[Firebase] LogEvent: " + ex.Message);
        }
    }
}
