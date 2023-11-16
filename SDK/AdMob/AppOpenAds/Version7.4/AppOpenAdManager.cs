using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using Firebase.Analytics;
#if USE_APPSFLYER
using AppsFlyerSDK;
#endif

public class AppOpenAdManager
{
    private const string test_Key = "ca-app-pub-3940256099942544/3419835294";
    private bool isEnableTest = false;
#if UNITY_ANDROID
    private const string ID_TIER_1 = "ca-app-pub-1919652342336147/7825856459";

#elif UNITY_IOS
    private const string ID_TIER_1 = "";
    private const string ID_TIER_2 = "";
    private const string ID_TIER_3 = "";
#else
    private const string ID_TIER_1 = "";
    private const string ID_TIER_2 = "";
    private const string ID_TIER_3 = "";
#endif

    private static AppOpenAdManager instance;

    private AppOpenAd _appOpenAd;

    private int tierIndex = 1;

    private bool isShowingAd = false;

    private bool isRequesting = false;

    public static bool ResumeFromAds = false;


    public static bool AdsActive = true;

    public static AppOpenAdManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new AppOpenAdManager();
            }

            return instance;
        }
    }

    public bool IsAdAvailable
    {
        get
        {
            // COMPLETE: Consider ad expiration
            return _appOpenAd != null;
        }
    }

    public void LoadAOA()
    {
        if (!AdsActive) return;
        // destroy old instance.
        DestroyAppOpenAd();
        isRequesting = true;
        string id = ID_TIER_1;
        if (isEnableTest)
        {
            id = test_Key;
        }
        Debug.Log($"Start request Open App Ads: Tier{tierIndex}- ID: {id}");

        // Create our request used to load the ad.
        var adRequest = new AdRequest();

        // send the request to load the ad.
        AppOpenAd.Load(ID_TIER_1, adRequest,
            (AppOpenAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("app open ad failed to load an ad " +
                                    "with error : " + error);
                    EventDispatcher.Instance.PostEvent(EventID.OnChangeSateLoadAOA, false);
                    return;
                }

                Debug.Log("App open ad loaded with response : "
                            + ad.GetResponseInfo());

                _appOpenAd = ad;
                RegisterEventHandlers(_appOpenAd);
                EventDispatcher.Instance.PostEvent(EventID.OnChangeSateLoadAOA, true);
            });
    }

    private void RegisterEventHandlers(AppOpenAd ad)
    {
        // Raised when the ad is estimated to have earned money.
        _appOpenAd.OnAdPaid += (AdValue adValue) =>
        {
            Debug.Log(String.Format("App open ad paid {0} {1}.",
                adValue.Value,
                adValue.CurrencyCode));
            double valueMicros = adValue.Value / 1000000f;
            string currencyCode = adValue.CurrencyCode;
            ResponseInfo responseInfo = _appOpenAd.GetResponseInfo();
            AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
            string adSourceId = loadedAdapterResponseInfo.AdSourceId;
            string adSourceName = loadedAdapterResponseInfo.AdSourceName;
#if USE_FIREBASE_LOG_EVENT
        var impressionParameters = new[] {
            new Parameter ("ad_platform", "Admob"),
            new Parameter ("value", valueMicros),
            new Parameter("ad_source", adSourceName),
            new Parameter("ad_unit_name", adSourceId),
            new Parameter("ad_format", "Open Ads"),
            new Parameter ("currency", currencyCode), // All AppLovin revenue is sent in USD
        };
        string text = $"ad_platform : admob\nvalue : {valueMicros}\nad_source : {adSourceName}\nad_unit_name : {adSourceId}\nad_format : Native Ads\ncurrency : {currencyCode}";
        Debug.Log("Track AOA Revenue AdFormat(AOA): " + text);
        FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
#endif

#if USE_APPSFLYER
        Dictionary<string, string> additionalParameters = new Dictionary<string, string>();
        additionalParameters.Add("ad_platform", "Admob");
        additionalParameters.Add("value", valueMicros.ToString());
        additionalParameters.Add("currency", currencyCode);
        AppsFlyerAdRevenue.logAdRevenue("Admob",
            AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
            valueMicros, currencyCode, additionalParameters);
#endif
        };
        // Raised when an impression is recorded for an ad.
        _appOpenAd.OnAdImpressionRecorded += () =>
        {
            Debug.Log("App open ad recorded an impression.");
        };
        // Raised when a click is recorded for an ad.
        _appOpenAd.OnAdClicked += () =>
        {
            Debug.Log("App open ad was clicked.");
        };
        // Raised when an ad opened full screen content.
        _appOpenAd.OnAdFullScreenContentOpened += () =>
        {
            Debug.Log("App open ad full screen content opened.");
            isShowingAd = true;
        };
        // Raised when the ad closed full screen content.
        _appOpenAd.OnAdFullScreenContentClosed += () =>
        {
            Debug.Log("App open ad full screen content closed.");
            EventDispatcher.Instance.PostEvent(EventID.OnShowHomeScene);
            Debug.Log("Closed app open ad");
            // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
            isShowingAd = false;
            EventDispatcher.Instance.PostEvent(EventID.OnCloseAOA);
            LoadAOA();
        };
        // Raised when the ad failed to open full screen content.
        _appOpenAd.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Debug.LogError("App open ad failed to open full screen content " +
                        "with error : " + error);
            LoadAOA();
        };
    }

    public void ShowAdIfAvailable()
    {
        Debug.Log(IsAdAvailable + " - " + isShowingAd);
        if (isShowingAd)
        {
            return;
        }
        if (!IsAdAvailable)
        {
            LoadAOA();
            return;
        }

        Debug.Log("Showing AOA");
        _appOpenAd.Show();
    }
    public void DestroyAppOpenAd()
    {
        if (_appOpenAd != null)
        {
            _appOpenAd.Destroy();
            _appOpenAd = null;
        }
    }
}
