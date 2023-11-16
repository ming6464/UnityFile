using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
#if USE_APPSFLYER
using AppsFlyerSDK;
#endif

public class AppOpenAdManager
{
    private const string test_Key = "ca-app-pub-3940256099942544/3419835294";
    private bool isEnableTest = false;
#if UNITY_ANDROID
    private const string ID_TIER_1 = "ca-app-pub-9778753195567892/9551688472";
    private const string ID_TIER_2 = "ca-app-pub-9778753195567892/2527911286";
    private const string ID_TIER_3 = "ca-app-pub-9778753195567892/2742452134";

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

    private AppOpenAd ad;

    private int tierIndex = 1;

    private bool isShowingAd = false;

    private bool isRequesting = false;

    public static bool ResumeFromAds = false;

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
            return ad != null;
        }
    }


    public void LoadAOA()
    {
        // destroy old instance.
        DestroyAppOpenAd();
        isRequesting = true;
        string id = ID_TIER_1;
        if (tierIndex == 2)
            id = ID_TIER_2;
        else if (tierIndex == 3)
            id = ID_TIER_3;
        if (isEnableTest)
        {
            id = test_Key;
        }
        Debug.Log($"Start request Open App Ads: Tier{tierIndex}- ID: {id}");

        AdRequest request = new AdRequest.Builder().Build();

        AppOpenAd.LoadAd(id, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
        {
            if (error != null)
            {
                // Handle the error.
                Debug.LogFormat($"Failed to load AOA tier {tierIndex} - id: {id}. Reason: {error.LoadAdError.GetMessage()}");
                tierIndex++;
                if (tierIndex <= 3)
                    LoadAOA();
                else
                {
                    tierIndex = 1;
                    isRequesting = false;
                }
                return;
            }

            // App open ad is loaded.
            ad = appOpenAd;
            tierIndex = 1;
            isRequesting = false;
        }));
    }

    public void ShowAdIfAvailable()
    {
        //if (ResumeFromAds)
        //{
        //    return;
        //}
        if (isShowingAd)
        {
            return;
        }
        if (!IsAdAvailable && !isRequesting)
        {
            LoadAOA();
            return;
        }
        ad.OnAdDidDismissFullScreenContent += HandleAdDidDismissFullScreenContent;
        ad.OnAdFailedToPresentFullScreenContent += HandleAdFailedToPresentFullScreenContent;
        ad.OnAdDidPresentFullScreenContent += HandleAdDidPresentFullScreenContent;
        ad.OnAdDidRecordImpression += HandleAdDidRecordImpression;
        ad.OnPaidEvent += HandlePaidEvent;

        ad.Show();
    }
    public void DestroyAppOpenAd()
    {
        if (ad != null)
        {
            ad.Destroy();
            ad = null;
        }
    }
    private void HandleAdDidDismissFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Closed app open ad");
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        isShowingAd = false;
        LoadAOA();
    }

    private void HandleAdFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
    {
        Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        LoadAOA();
    }
    private void HandleAdDidPresentFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Displayed app open ad");
        isShowingAd = true;
    }

    private void HandleAdDidRecordImpression(object sender, EventArgs args)
    {
        Debug.Log("Recorded ad impression");
    }

    private void HandlePaidEvent(object sender, AdValueEventArgs args)
    {
        Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
            args.AdValue.CurrencyCode, args.AdValue.Value);
        AdValue adValue = args.AdValue;
        double valueMicros = adValue.Value / 1000f;
        string currencyCode = adValue.CurrencyCode;
#if USE_FIREBASE_LOG_EVENT
        var impressionParameters = new[] {
            new Parameter ("ad_platform", "Admob"),
            new Parameter ("value", valueMicros),
            new Parameter ("currency", currencyCode), // All AppLovin revenue is sent in USD
        };
        Debug.Log("Track AOA Revenue AdFormat: " + valueMicros);
        FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
#endif


#if USE_APPSFLYER
        Dictionary<string, string> additionalParameters = new Dictionary<string, string>();
        additionalParameters.Add("ad_platform", "Admob");
        additionalParameters.Add("value", valueMicros.ToString());
        additionalParameters.Add("currency", currencyCode);
        AppsFlyerAdRevenue.logAdRevenue("Admob",
            AppsFlyerAdRevenueMediationNetworkType.AppsFlyerAdRevenueMediationNetworkTypeGoogleAdMob,
            args.AdValue.Value, args.AdValue.CurrencyCode, additionalParameters);
#endif
        
    }
}
