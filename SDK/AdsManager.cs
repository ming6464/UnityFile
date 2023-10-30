using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Firebase.Analytics;
using UnityEngine.SceneManagement;

#if USE_FIREBASE
using Firebase.Analytics;
#endif

#if USE_APPLOVIN

#endif

// Cài đặt các thiết lập quảng cáo cho nền tảng IOS khi sử dụng AppLovin (MaxSDK)
#if UNITY_IOS && USE_APPLOVIN
namespace AudienceNetwork
{
    public static class AdSettings
    {
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
        }
    }
}
#endif

public class AdsManager : MonoBehaviour
{
    public static AdsManager instance;
    public bool IsShowedInter;
    private WaitForSecondsRealtime waitRewarded;
    private DateTime timePauseGane;
    private int timePausedGane;

    Action actionRewarded;
    Action actionFullAds;

#if USE_APPLOVIN
    //static string forceAdsPosition;//ad start
    //static string rewardAdsPosition;
    [SerializeField] string MaxSdkKey = "Key AppLovin (MaxSDK)"; //
    [SerializeField] string InterstitialAdUnitId = "";
    [SerializeField] string RewardedAdUnitId = "";
    [SerializeField] string BannerAdUnitId = "";
    [SerializeField] string MrecAdUnitId = "";
    [SerializeField] string AppOpenAdUnitId = "YOUR_IOS_AD_UNIT_ID";
    [SerializeField] MaxSdkBase.BannerPosition bannerPosition;
    [SerializeField] MaxSdkBase.AdViewPosition mrecPosition;

#endif
#if USE_IRON_SOURCE
    [SerializeField] private string IronSourceAppKey;
    [SerializeField] private IronSourceBannerPosition bannerPosition;
    [SerializeField] private IronSourceBannerPosition mRecPosition;
#endif
    [SerializeField] bool showUIWaitAds;
    [SerializeField] bool isShowReviewAdsUnityEditor;
    private bool isBannerShowing;
    private bool isMRecShowing;
    private bool isAdsShowing;

    private int interstitialRetryAttempt;
    private int bannerRetryAttempt;
    private int rewardedRetryAttempt;
    private int rewardedInterstitialRetryAttempt;
    private int mrecRetryAttempt;

    private int tracking_timeLoadAds, tracking_timeLoadAds2;
    [SerializeField] float timeWaitShow = 0;
    public static bool IsAdsShowing
    {
        get { return instance.isAdsShowing; }
        set { instance.isAdsShowing = value; }
    }

    void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else instance = this;
        
        //enable if have ironsource ad quality
        //IronSourceAdQuality.Initialize(IronSourceAppKey);
        
    }


    private void Start()
    {
        waitRewarded = new WaitForSecondsRealtime(0.2f);
        Util.timeLastShowAds = Util.timeNow;
#if USE_APPLOVIN
        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            MaxSdkCallbacks.AppOpen.OnAdHiddenEvent += OnAppOpenDismissedEvent;
            InitializeInterstitialAds();
            InitializeRewardedAds();
#if USE_APPLOVIN_BANNER
            InitializeBannerAds();
#endif
#if USE_APPLOVIN_MREC
            InitializeMRecAds();
#endif
        };
        MaxSdk.SetSdkKey(MaxSdkKey);
        MaxSdk.InitializeSdk();
#endif
#if USE_IRON_SOURCE
        IronSource.Agent.setConsent(true);
        IronSource.Agent.setMetaData("do_not_sell", "false");
        IronSource.Agent.setMetaData("is_child_directed", "false");
        IronSource.Agent.validateIntegration();
        IronSource.Agent.init(IronSourceAppKey);
        IronSourceBannerSize.BANNER.SetAdaptive(true);
        InitializeInterstitialAds();
        InitializeRewardedAds();
#if USE_IRON_SOURCE_BANNER
        InitializeBannerAds();
#endif
#if USE_IRON_SOURCE_MREC
        InitializeMRecAds();
#endif
#endif
    }



#if USE_IRON_SOURCE
    void OnApplicationPause(bool isPaused)
    {
        Debug.Log("Ironsource: OnApplicationPause = " + isPaused);
        IronSource.Agent.onApplicationPause(isPaused);
    }
#endif

    // -------------------------------------------------------------------------------------------------------------------------------------------------------
    #region Retry
    private void ReinitAdSdk()
    {
#if USE_APPLOVIN
        if (Util.isInternetConection && !MaxSdk.IsInitialized())
        {
            // Set MaxSdkKey
            MaxSdk.SetSdkKey(MaxSdkKey);
            MaxSdk.InitializeSdk();
        }
#endif
    }
    private void OnEnable()
    {
        IronSourceEvents.onImpressionDataReadyEvent += OnImpressionDataReady;
        this.RegisterListener(EventID.OnRetryCheckInternet, OnTryInitAndRequestAd);
    }

    private void OnTryInitAndRequestAd(object obj)
    {
        Debug.Log("OnTryInitAndRequestAd");
        TryInitAndRequestAd();
    }

    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener(EventID.OnRetryCheckInternet, OnTryInitAndRequestAd);
        IronSourceEvents.onImpressionDataReadyEvent -= OnImpressionDataReady;
    }
    private void OnImpressionDataReady(IronSourceImpressionData impressionData)
    {
        Debug.Log("OnImpressionDataReadyEvent :" + impressionData);
        TrackAdRevenue(impressionData);
#if USE_APPSFLYER
    AppsflyerHelper.instance.OnAdRevenuePaidEvent(impressionData);
#endif
    }

    public void TryInitAndRequestAd()
    {
#if USE_APPLOVIN
        ReinitAdSdk();
        LoadRewardedAd();
        LoadInterstitial();
        InitializeBannerAds();
        InitializeMRecAds();
#endif
#if USE_IRON_SOURCE
        IronSource.Agent.validateIntegration();
        IronSource.Agent.init(IronSourceAppKey);
        InitializeInterstitialAds();
        InitializeRewardedAds();
#if USE_IRON_SOURCE_BANNER
        InitializeBannerAds();
#endif
#endif
    }

    #endregion


    // -------------------------------------------------------------------------------------------------------------------------------------------------------
    #region Interstitial Ad Methods
    private void InitializeInterstitialAds()
    {
#if USE_APPLOVIN
        // Attach callbacks
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += InterstitialOnAdLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += InterstitialOnAdLoadFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += InterstitialOnAdDisplayedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += InterstitialOnAdDisplayFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdClickedEvent += InterstitialOnAdClickedEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += InterstitialOnAdHiddenEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += InterstitialOnAdRevenuePaidEvent;

        LoadInterstitial();
#endif
#if USE_IRON_SOURCE
        //Add AdInfo Interstitial Events
        IronSourceInterstitialEvents.onAdReadyEvent += InterstitialOnAdReadyEvent; // InterstitialOnAdLoadedEvent
        IronSourceInterstitialEvents.onAdLoadFailedEvent += InterstitialOnAdLoadFailed; // InterstitialOnAdLoadFailedEvent
        IronSourceInterstitialEvents.onAdOpenedEvent += InterstitialOnAdOpenedEvent; //InterstitialOnAdDisplayedEvent
        IronSourceInterstitialEvents.onAdClickedEvent += InterstitialOnAdClickedEvent; // InterstitialOnAdClickedEvent
        IronSourceInterstitialEvents.onAdShowSucceededEvent += InterstitialOnAdShowSucceededEvent; //InterstitialOnAdRevenuePaidEvent
        IronSourceInterstitialEvents.onAdShowFailedEvent += InterstitialOnAdShowFailedEvent; //InterstitialOnAdDisplayFailedEvent
        IronSourceInterstitialEvents.onAdClosedEvent += InterstitialOnAdClosedEvent; // InterstitialOnAdHiddenEvent

        LoadInterstitial();
#endif
    }

    public void LoadInterstitial()
    {
        ShowMessage("Load full Ads");
#if USE_IRON_SOURCE
        if (!IronSource.Agent.isInterstitialReady())
        {
            ShowMessage("Load full Ads - Max ready");
            IronSource.Agent.loadInterstitial();
#if USE_FIREBASE_LOG_EVENT
            //FirebaseHelper.LogEvent("ad_inter_load");
#endif
#if USE_APPSFLYER
            AppsflyerHelper.Log(AppsflyerHelper.eventId.af_inters_api_called);
#endif
        }
        else
        {
            ShowMessage("Load full Ads - AdsIsReady - Not Load");
        }
#endif


#if USE_APPLOVIN
        
        if (MaxSdk.IsInitialized())
        {
            ShowMessage("Load full Ads - Max ready");
            if (!MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
            {
                MaxSdk.LoadInterstitial(InterstitialAdUnitId);
                tracking_timeLoadAds = (int)Time.time;
#if USE_FIREBASE_LOG_EVENT
                FirebaseHelper.LogEvent("ad_inter_load");
#endif
            }
            else
            {
                ShowMessage("Load full Ads - AdsIsReady - Not Load");
            }
        }
        else
        {
            interstitialRetryAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));

            ShowMessage("Reload full Ads - Max not ready");

            Invoke("LoadInterstitial", (float)retryDelay);
        }
#endif
    }

    public void CheckShowInterstitial(Action callBack = null,string placementName = null)
    {
        Debug.Log("af_inters_ad_eligible");
        if (callBack == null) callBack = delegate { };
        if (!isShowReviewAdsUnityEditor)
        {
#if UNITY_EDITOR
            callBack?.Invoke();
            return;
#endif
        }
        LogEventAppsflyerHelper("af_inters_ad_eligible");
        int adTimer = 0;
#if USE_FIREBASE_REMOTE_CONFIG && USE_FIREBASE
        adTimer = RemoteConfigControl.instance.ads_interval;
#endif
        Debug.Log($"CheckShowInterstitial_1_{timePausedGane}");
        Debug.Log($"CheckShowInterstitial_2_{Util.timeNow - (Util.timeLastShowAds + timePausedGane)} >= {adTimer}");
        if (Util.timeNow - (Util.timeLastShowAds + timePausedGane) >= adTimer)
        {
            //Util.timeLastShowAds = Util.timeNow;
            StartCoroutine(DelayShowAdUnit(callBack,placementName));
        }
        else
        {
            Debug.Log($"CheckShowInterstitial false time {Util.timeNow - Util.timeLastShowAds}>={adTimer}");
            callBack?.Invoke();
        }
    }
    public void ShowInterstitialResume(Action callBack = null,string placementName = null)
    {
        if (!isShowReviewAdsUnityEditor)
        {
#if UNITY_EDITOR
            callBack?.Invoke();
            return;
#endif
        }
        StartCoroutine(DelayShowAdUnit(callBack,placementName));
    }
    
    IEnumerator DelayShowAdUnit(Action actionDone,string placementName = null)
    {
        if (showUIWaitAds)
        {
            this.PostEvent(EventID.OnShowUIWaitAds, true);
        }
        yield return new WaitForSeconds(timeWaitShow);
        this.actionFullAds = actionDone;
#if UNITY_EDITOR
        if (!isShowReviewAdsUnityEditor)
        {
            actionFullAds?.Invoke();
            if (showUIWaitAds)
            {
                this.PostEvent(EventID.OnShowUIWaitAds, false);
            }
        }
#elif !UNITY_EDITOR
        ShowAdUnit(actionDone,placementName);
        if (showUIWaitAds)
        {
            this.PostEvent(EventID.OnShowUIWaitAds, false);
        }
#endif
    }

    public void ShowAdUnit(Action actionDone,string placementName = null)
    {
#if UNITY_EDITOR
        if (!isShowReviewAdsUnityEditor)
        {
            ShowMessage($"Show Full Ads:");
            actionDone?.Invoke();
            return;
        }
#endif
        bool is_ads = true;
#if USE_FIREBASE_REMOTE_CONFIG
        is_ads = RemoteConfigControl.instance.is_ads;
#endif
        if (is_ads)
        {
            if (!isShowReviewAdsUnityEditor)
            {
#if UNITY_EDITOR
                ShowMessage($"Show Full Ads:");
                actionDone?.Invoke();
                return;
#endif
            }
#if USE_APPLOVIN
        if (MaxSdk.IsInterstitialReady(InterstitialAdUnitId))
        {
            if (actionDone != null) actionFullAds = actionDone;
            isAdsShowing = true;
            Util.LastTimeShowAds = DateTime.Now;
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);
#if USE_FIREBASE_LOG_EVENT
            FirebaseHelper.LogEvent("ad_inter_show");
#endif
        }
        else
        {
#if USE_FIREBASE_LOG_EVENT
            FirebaseHelper.LogEvent("ad_inter_fail");
#endif
            actionDone.Invoke();
        }
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
#endif

#if USE_IRON_SOURCE
            if (IronSource.Agent.isInterstitialReady())
            {
                if (actionDone != null) actionFullAds = actionDone;
                isAdsShowing = true;

                if (string.IsNullOrEmpty(placementName))
                {
                    IronSource.Agent.showInterstitial();
                }
                else
                {
                    IronSource.Agent.showInterstitial(placementName);
                }
                ShowMessage("Show Full Ads Complite");
            }
            else
            {
#if USE_FIREBASE_LOG_EVENT
                //FirebaseHelper.LogEvent("ad_inter_fail");
#endif
                ShowMessage($"Show Full Ads Fail {IronSource.Agent.isInterstitialReady()}");
                actionDone?.Invoke();
            }
            IronSource.Agent.loadInterstitial();
#endif
        }
    }

#if USE_APPLOVIN
    private void InterstitialOnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
        interstitialRetryAttempt = 0;
        int time = (int)Time.time - tracking_timeLoadAds;
    }

    private void InterstitialOnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        interstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void InterstitialOnAdDisplayedEvent(string adUnitID, MaxSdkBase.AdInfo adInfo)
    {
        ShowMessage("InterstitialOnAdDisplayedEvent AdInfo :" + adInfo);
        ShowMessage("InterstitialOnAdDisplayedEvent AdInfo revenue :" + adInfo.Revenue);

        Util.CountShowInter++;
        var count = Util.CountShowInter;
        var log = new Dictionary<string, object>();
        log.Add("Interstitial_Ad_DisplayedEvent", count);
#if USE_FIREBASE_LOG_EVENT
        FirebaseHelper.LogEvent("inter_ad");
#endif
    }

    private void InterstitialOnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        ShowMessage("Interstitial failed to display with error code: " + errorInfo);

        if (actionFullAds != null)
        {
            actionFullAds.Invoke();
            actionFullAds = null;
        }

        //fix anr
        ShowMessage("Show AdInter Fail:" + adUnitId);
        Invoke("LoadInterstitial", 0.5f);
    }

    private void InterstitialOnAdClickedEvent(string adUnitID, MaxSdkBase.AdInfo adInfo)
    {
    }

    private void InterstitialOnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad
        ShowMessage("Interstitial dismissed - end pause game");

        if (actionFullAds != null)
        {
            actionFullAds.Invoke();
            actionFullAds = null;
        }

        //fix anr
        ShowMessage("Show AdInter Success:" + adUnitId);
        ShowMessage("Show AdInter Success AdInfo :" + adInfo);
        Invoke("LoadInterstitial", 0.5f);
    }

#endif
#if USE_IRON_SOURCE
    /************* Interstitial AdInfo Delegates *************/
    // Invoked when the interstitial ad was loaded succesfully.
    void InterstitialOnAdReadyEvent(IronSourceAdInfo adInfo)
    {
        LogEventAppsflyerHelper("af_inters_api_called");
        interstitialRetryAttempt = 0;
        int time = (int)Time.time - tracking_timeLoadAds;
    }
    // Invoked when the initialization process has failed.
    void InterstitialOnAdLoadFailed(IronSourceError ironSourceError)
    {
        interstitialRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, interstitialRetryAttempt));
        Invoke("LoadInterstitial", (float)retryDelay);
    }
    // Invoked when the Interstitial Ad Unit has opened. This is the impression indication. 
    void InterstitialOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log("af_inters_displayed");
        LogEventAppsflyerHelper("af_inters_displayed");
        ShowMessage("InterstitialOnAdDisplayedEvent AdInfo :" + adInfo);
        ShowMessage("InterstitialOnAdDisplayedEvent AdInfo revenue :" + adInfo.revenue);
    }
    // Invoked when end user clicked on the interstitial ad
    void InterstitialOnAdClickedEvent(IronSourceAdInfo adInfo)
    {
    }
    // Invoked when the ad failed to show.
    void InterstitialOnAdShowFailedEvent(IronSourceError ironSourceError, IronSourceAdInfo adInfo)
    {
        ResetState();
        // Interstitial ad failed to display. We recommend loading the next ad
        ShowMessage("Interstitial failed to display with error code: " + ironSourceError);

        if (actionFullAds != null)
        {
            actionFullAds.Invoke();
            actionFullAds = null;
        }

        //fix anr
        ShowMessage("Show AdInter Fail:" + adInfo.auctionId);
        Invoke("LoadInterstitial", 0.5f);
    }
    // Invoked when the interstitial ad closed and the user went back to the application screen.
    void InterstitialOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad

        IsShowedInter = true;
        
        ShowMessage("Interstitial dismissed - end pause game");

        if (actionFullAds != null)
        {
            actionFullAds.Invoke();
            actionFullAds = null;
        }
        timePausedGane = 0;
        Util.timeLastShowAds = Util.timeNow;
        ResetState();
        //fix anr
        ShowMessage("Show AdInter Success:" + adInfo.auctionId);
        ShowMessage("Show AdInter Success AdInfo :" + adInfo);
        Invoke("LoadInterstitial", 0.5f);
        PlayerPrefs.SetInt("CheckShowedInter",1);
    }
    // Invoked before the interstitial ad was opened, and before the InterstitialOnAdOpenedEvent is reported.
    // This callback is not supported by all networks, and we recommend using it only if  
    // it's supported by all networks you included in your build. 
    void InterstitialOnAdShowSucceededEvent(IronSourceAdInfo adInfo)
    {
        Debug.Log("Sending Interstitial ad revenue: " + adInfo.adNetwork + " - " + adInfo.revenue);
        ShowMessage("Sending Interstitial ad revenue: " + adInfo.adNetwork + " - " + adInfo.revenue);
//        TrackAdRevenue(adInfo);
//#if USE_APPSFLYER
//        AppsflyerHelper.instance.OnAdRevenuePaidEvent(adInfo);
//#endif
    }
#endif
    #endregion


    // -------------------------------------------------------------------------------------------------------------------------------------------------------
    #region Rewarded Ad Methods
    public void ShowRewardVideo(Action actionDone, Action actionFail, string placementName = null)
    {
        
#if UNITY_EDITOR
        actionDone?.Invoke();
        return;
#endif
        LogEventAppsflyerHelper("af_rewarded_ad_eligible");
        bool is_ads = true;
#if USE_FIREBASE_REMOTE_CONFIG
        is_ads = RemoteConfigControl.instance.is_ads;
#endif
        if (is_ads)
        {
            ShowMessage($"Show Reward Ads: {placementName}");
            if (!isShowReviewAdsUnityEditor)
            {
            }

#if USE_IRON_SOURCE
            if (IronSource.Agent.isRewardedVideoAvailable())
            {
                if (actionDone != null) actionRewarded = actionDone;
                isAdsShowing = true;
                timePauseGane = DateTime.Now;
                if (string.IsNullOrEmpty(placementName))
                {
                    IronSource.Agent.showRewardedVideo();
                }
                else
                {
                    IronSource.Agent.showRewardedVideo(placementName);
                }
                
                Util.CountShowReward++;
            }
            else
            {
                actionFail?.Invoke();
                ShowMessageNOTCONFIG("Ads not ready yet!");
                
                if (Util.isInternetConection)
                {
                }
                else
                {
                    ShowMessageNOTCONFIG("No internet connection!");
                }
            }
#endif


#if USE_APPLOVIN
        if (MaxSdk.IsRewardedAdReady(RewardedAdUnitId))
        {
            if (actionDone != null) actionRewarded = actionDone;
            isAdsShowing = true;
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
            Util.CountShowReward++;
#if USE_FIREBASE_LOG_EVENT
            FirebaseHelper.LogEvent("show_reward", new System.Collections.Generic.Dictionary<string, object> { { "count_show", Util.CountShowReward } });
#endif
        }
        else
        {
            actionFail?.Invoke();
                if (MessageSpawner.Instance != null)
                {
                    MessageSpawner.Instance.SpawnMessage("Ads not ready yet!");
                }
                else
                {
                    ShowMessage("Ads not ready yet!");
                }
            
            if (Util.isInternetConection)
            {
            }
            else
            {
                ShowMessage("No internet connection!");
            }
        }
#endif
        }

    }

    private void InitializeRewardedAds()
    {
#if USE_APPLOVIN
        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += RewardedOnAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += RewardedOnAdLoadFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += RewardedOnAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += RewardedOnAdDisplayFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += RewardedOnAdDisplayFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += RewardedOnAdHiddenEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += RewardedOnAdRevenuePaidEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += RewardedOnAdReceivedRewardEvent;

        // Load the first RewardedAd
        //fix anr
        Invoke("LoadRewardedAd", 0.3f);
#endif
#if USE_IRON_SOURCE
        //Add AdInfo Rewarded Video Events
        IronSourceRewardedVideoEvents.onAdOpenedEvent += RewardedVideoOnAdOpenedEvent; // tương đương RewardedOnAdDisplayedEvent
        IronSourceRewardedVideoEvents.onAdClosedEvent += RewardedVideoOnAdClosedEvent; // tương đương RewardedOnAdHiddenEvent
        IronSourceRewardedVideoEvents.onAdAvailableEvent += RewardedVideoOnAdAvailable; // tương tự RewardedOnAdLoadedEvent
        IronSourceRewardedVideoEvents.onAdUnavailableEvent += RewardedVideoOnAdUnavailable;// tương tự RewardedOnAdLoadFailedEvent
        IronSourceRewardedVideoEvents.onAdShowFailedEvent += RewardedVideoOnAdShowFailedEvent; // tương đương RewardedOnAdDisplayFailedEvent
        IronSourceRewardedVideoEvents.onAdRewardedEvent += RewardedVideoOnAdRewardedEvent; // tương đương RewardedOnAdReceivedRewardEvent
        IronSourceRewardedVideoEvents.onAdClickedEvent += RewardedVideoOnAdClickedEvent; // tương đương RewardedOnAdDisplayFailedEvent

        Invoke("LoadRewardedAd", 0.3f);
#endif
    }

    private void LoadRewardedAd()
    {
        ShowMessage("Load Rewarded Ads");
#if USE_APPLOVIN
        if (MaxSdk.IsInitialized())
        {
            ShowMessage("Load Rewarded Ads - Max ready");
            MaxSdk.LoadRewardedAd(RewardedAdUnitId);
#if USE_FIREBASE_LOG_EVENT
            FirebaseHelper.LogEvent("ad_rewarded_load");
#endif
        }
#endif
#if USE_IRON_SOURCE
        if (!IronSource.Agent.isRewardedVideoAvailable())
        {
            ShowMessage("Load Rewarded Ads - Iron source");
            IronSource.Agent.loadRewardedVideo();
        }
#endif
    }

#if USE_APPLOVIN
    private void RewardedOnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(rewardedAdUnitId) will now return 'true'
        ShowMessage("Rewarded ad loaded");

        // Reset retry attempt
        rewardedRetryAttempt = 0;
    }

    private void RewardedOnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        ShowMessage("Rewarded ad failed to load with error code: " + errorInfo);

        rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));
        Invoke("LoadRewardedAd", (float)retryDelay);
    }

    private void RewardedOnAdDisplayFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        ShowMessage("Rewarded ad failed to display with error code: " + errorInfo);
        ShowMessage("Rewarded ad failed to display with adinfo : " + adInfo);

        Invoke("LoadRewardedAd", 0.5f);
    }

    private void RewardedOnAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        ShowMessage("Rewarded ad displayed - pause game show ads reward");
        ShowMessage("RewardedOnAdDisplayedEvent AdInfo :" + adInfo);
        ShowMessage("RewardedOnAdDisplayedEvent AdInfo revenue :" + adInfo.Revenue);

        //fix anr
        Invoke("LoadRewardedAd", 0.5f);
        Util.CountShowReward++;
        var count = Util.CountShowReward;
        var log = new Dictionary<string, object>();
        log.Add("CountShowReward", count);
#if USE_FIREBASE_LOG_EVENT
        FirebaseHelper.LogEvent("reward_ad", log);
#endif
    }

    private void RewardedOnAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        ShowMessage("Rewarded ad clicked");
    }

    private void RewardedOnAdHiddenEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is hidden. Pre-load the next ad
        ShowMessage("Rewarded ad dismissed - end pause game");

        Invoke("LoadRewardedAd", 0.5f);
    }

    private void RewardedOnAdReceivedRewardEvent(string adUnitId, MaxSdkBase.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        ShowMessage("Rewarded ad received reward - end pause game");

        if (actionRewarded != null)
        {
            StartCoroutine(WaitRewarded());
        }
    }
    private void BannerOnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        ShowMessage("Sending banner ad revenue: " + adInfo.NetworkName + " - " + adInfo.Revenue);
        TrackAdRevenue(adInfo);
#if USE_APPSFLYER
        AppsflyerHelper.instance.OnAdRevenuePaidEvent(adInfo);
#endif
    }

    private void RewardedOnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        ShowMessage("Sending Rewarded ad revenue: " + adInfo.NetworkName + " - " + adInfo.Revenue);
        TrackAdRevenue(adInfo);
#if USE_APPSFLYER
        AppsflyerHelper.instance.OnAdRevenuePaidEvent(adInfo);
#endif
    }
    private void InterstitialOnAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        ShowMessage("Sending Interstitial ad revenue: " + adInfo.NetworkName + " - " + adInfo.Revenue);
        TrackAdRevenue(adInfo);
#if USE_APPSFLYER
        AppsflyerHelper.instance.OnAdRevenuePaidEvent(adInfo);
#endif
    }

    
#endif
    IEnumerator WaitRewarded()
    {
        yield return waitRewarded;
        actionRewarded.Invoke();
    }

#if USE_IRON_SOURCE
    /************* RewardedVideo AdInfo Delegates *************/
    // Indicates that there’s an available ad.
    // The adInfo object includes information about the ad that was loaded successfully
    // This replaces the RewardedVideoAvailabilityChangedEvent(true) event
    void RewardedVideoOnAdAvailable(IronSourceAdInfo adInfo)
    {
        LogEventAppsflyerHelper("af_rewarded_api_called");
        rewardedRetryAttempt = 0;
    }
    // Indicates that no ads are available to be displayed
    // This replaces the RewardedVideoAvailabilityChangedEvent(false) event
    void RewardedVideoOnAdUnavailable()
    {
        ShowMessage("Rewarded ad failed to load with error code: ");

        rewardedRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, rewardedRetryAttempt));
        Invoke("LoadRewardedAd", (float)retryDelay);
    }
    // The Rewarded Video ad view has opened. Your activity will loose focus.
    void RewardedVideoOnAdOpenedEvent(IronSourceAdInfo adInfo)
    {
        LogEventAppsflyerHelper("af_rewarded_ad_displayed");
        ShowMessage("Rewarded ad displayed - pause game show ads reward");
        ShowMessage("RewardedOnAdDisplayedEvent AdInfo :" + adInfo);
        ShowMessage("RewardedOnAdDisplayedEvent AdInfo revenue :" + adInfo.revenue);

        //fix anr
        Invoke("LoadRewardedAd", 0.5f);
        Util.CountShowReward++;
        var count = Util.CountShowReward;
        var log = new Dictionary<string, object>();
        log.Add("CountShowReward", count);
#if USE_FIREBASE_LOG_EVENT
        //FirebaseHelper.LogEvent("reward_ad", log);
#endif
    }
    // The Rewarded Video ad view is about to be closed. Your activity will regain its focus.
    void RewardedVideoOnAdClosedEvent(IronSourceAdInfo adInfo)
    {
        timePausedGane += Util.GetTime(DateTime.Now) - Util.GetTime(timePauseGane);
        // Rewarded ad is hidden. Pre-load the next ad
        ShowMessage("Rewarded ad dismissed - end pause game");
        Invoke("LoadRewardedAd", 0.5f);
        ResetState();
        PlayerPrefs.SetInt("CheckShowedInter",1);
    }

    private void ResetState()
    {
        StartCoroutine(FasleIsShowing());
    }

    private IEnumerator FasleIsShowing()
    {
        yield return .5f;
        isAdsShowing = false;
    }
    
    // The user completed to watch the video, and should be rewarded.
    // The placement parameter will include the reward data.
    // When using server-to-server callbacks, you may ignore this event and wait for the ironSource server callback.
    void RewardedVideoOnAdRewardedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        ShowMessage("Rewarded ad received reward - end pause game");

        if (actionRewarded != null)
        {
            StartCoroutine(WaitRewarded());
        }
        ShowMessage("Sending firebase REWARDED Revenue AdInfo :" + adInfo);
        ShowMessage("Sending firebase REWARDED Revenue AdInfo :" + adInfo.revenue);
//        TrackAdRevenue(adInfo);
//#if USE_APPSFLYER
//        AppsflyerHelper.instance.OnAdRevenuePaidEvent(adInfo);
//#endif
    }
    // The rewarded video ad was failed to show.
    void RewardedVideoOnAdShowFailedEvent(IronSourceError error, IronSourceAdInfo adInfo)
    {
        ResetState();
        // Rewarded ad failed to display. We recommend loading the next ad
        ShowMessage("Rewarded ad failed to display with error code: " + error);
        ShowMessage("Rewarded ad failed to display with adinfo : " + adInfo);

        Invoke("LoadRewardedAd", 0.5f);
        
        Debug.Log("MAX > Rewarded ad failed to load with error code: " + error.ToString());
        
    }
    // Invoked when the video ad was clicked.
    // This callback is not supported by all networks, and we recommend using it only if
    // it’s supported by all networks you included in your build.
    void RewardedVideoOnAdClickedEvent(IronSourcePlacement placement, IronSourceAdInfo adInfo)
    {
        // Rewarded ad was displayed and user should receive the reward
        ShowMessage("Rewarded ad received reward - end pause game");

        if (actionRewarded != null)
        {
            StartCoroutine(WaitRewarded());
        }
    }
#endif
    #endregion





    // -------------------------------------------------------------------------------------------------------------------------------------------------------
    #region app open
    bool isShowingAd = false;
    public void ShowAppOpen()
    {
#if USE_APPLOVIN
        ShowMessage("Show App Open");
        if (MaxSdk.IsAppOpenAdReady(AppOpenAdUnitId))
        {
            MaxSdk.ShowAppOpenAd(AppOpenAdUnitId);
        }
        else
        {
            ShowMessage("Loading App Open Ad");
            MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
        }
#endif
    }
    #endregion


    // -------------------------------------------------------------------------------------------------------------------------------------------------------
    #region Banner Ad Methods
    private void InitializeBannerAds()
    {
#if USE_APPLOVIN
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += BannerOnAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += BannerOnAdRevenuePaidEvent;

        Invoke("DelayInitBanner", 0.5f);
#endif

#if USE_IRON_SOURCE_BANNER
        IronSource.Agent.init(IronSourceAppKey, IronSourceAdUnits.BANNER);
        //Add AdInfo Banner Events
        IronSourceBannerEvents.onAdLoadedEvent += BannerOnAdLoadedEvent;
        IronSourceBannerEvents.onAdLoadFailedEvent += BannerOnAdLoadFailedEvent;
        IronSourceBannerEvents.onAdClickedEvent += BannerOnAdClickedEvent;
        IronSourceBannerEvents.onAdScreenPresentedEvent += BannerOnAdScreenPresentedEvent;
        IronSourceBannerEvents.onAdScreenDismissedEvent += BannerOnAdScreenDismissedEvent;
        IronSourceBannerEvents.onAdLeftApplicationEvent += BannerOnAdLeftApplicationEvent;

        Invoke("DelayInitBanner", 0.5f);
#endif
    }
    private void DelayInitBanner()
    {
#if USE_APPLOVIN
        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets.
        // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments.
        MaxSdk.CreateBanner(BannerAdUnitId, bannerPosition);
        MaxSdk.SetBannerExtraParameter(BannerAdUnitId, "adaptive_banner", "false");
        // Set background or background color for banners to be fully functional.
        MaxSdk.SetBannerBackgroundColor(BannerAdUnitId, Color.black);
        MaxSdk.SetBannerWidth(BannerAdUnitId, (float)Screen.width);
        //ShowBanner();
#endif
#if USE_IRON_SOURCE_BANNER
        IronSourceBannerSize ironSourceBannerSize = IronSourceBannerSize.BANNER;
        ironSourceBannerSize.SetAdaptive(true);
        IronSource.Agent.loadBanner(ironSourceBannerSize, bannerPosition);
#endif
    }

#if USE_APPLOVIN
    public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        ShowMessage("App dismiss load Open Ad");
        MaxSdk.LoadAppOpenAd(AppOpenAdUnitId);
    }
#endif

    public void ShowBanner()
    {
        bool is_ads = true;
#if USE_FIREBASE_REMOTE_CONFIG
        is_ads = RemoteConfigControl.instance.is_ads;
#endif
        if (is_ads)
        {
#if USE_APPLOVIN
        if (!Util.isInternetConection)
        {
            HideBanner();
            return;
        }

        MaxSdk.ShowBanner(BannerAdUnitId);
#endif
#if USE_IRON_SOURCE_BANNER
            //InitializeBannerAds();
            IronSource.Agent.displayBanner();
            //IronSource.Agent.loadBanner(IronSourceBannerSize.BANNER, bannerPosition);
#endif
        }
    }

    public void HideBanner()
    {
#if USE_APPLOVIN
        MaxSdk.HideBanner(BannerAdUnitId);
#endif
#if USE_IRON_SOURCE_BANNER
        IronSource.Agent.hideBanner();
#endif
    }

    private void ToggleBannerVisibility()
    {
#if USE_APPLOVIN
        if (!isBannerShowing)
        {
            MaxSdk.ShowBanner(BannerAdUnitId);
        }
        else
        {
            MaxSdk.HideBanner(BannerAdUnitId);
        }

        isBannerShowing = !isBannerShowing;
#endif
    }

#if USE_APPLOVIN
    // Fired when a banner is loaded
    private void BannerOnAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        bannerRetryAttempt = 0;
        ShowMessage("Banner Loaded:" + adUnitId);
        Util.CountShowBanner++;
        var count = Util.CountShowBanner;
    }

    // Fired when a banner has failed to load
    private void BannerOnAdLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        bannerRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt));

        ShowMessage("Banner failed to load with error code: " + errorInfo);
        ShowMessage("Load Banner Fail:" + adUnitId);

        Invoke("DelayInitBanner", (float)retryDelay);
    }
#endif

    public float GetBannerHeight()
    {
#if USE_APPLOVIN
        return MaxSdkUtils.GetAdaptiveBannerHeight(Screen.width);
#endif
        return 50;
    }

#if USE_IRON_SOURCE_BANNER
    /************* Banner AdInfo Delegates *************/
    //Invoked once the banner has loaded
    void BannerOnAdLoadedEvent(IronSourceAdInfo adInfo)
    {
        bannerRetryAttempt = 0;
        ShowMessage("Banner Loaded:" + adInfo);
        //TrackAdRevenue(adInfo);
        //AppsflyerHelper.instance.OnAdRevenuePaidEvent(adInfo);
        Util.CountShowBanner++;
        var count = Util.CountShowBanner;
    }
    //Invoked when the banner loading process has failed.
    void BannerOnAdLoadFailedEvent(IronSourceError ironSourceError)
    {
        bannerRetryAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, bannerRetryAttempt));

        ShowMessage("Banner failed to load with error code: " + ironSourceError);
        Invoke("DelayInitBanner", (float)retryDelay);
    }
    // Invoked when end user clicks on the banner ad
    void BannerOnAdClickedEvent(IronSourceAdInfo adInfo)
    {
    }
    //Notifies the presentation of a full screen content following user click
    void BannerOnAdScreenPresentedEvent(IronSourceAdInfo adInfo)
    {
       
    }
    //Notifies the presented screen has been dismissed
    void BannerOnAdScreenDismissedEvent(IronSourceAdInfo adInfo)
    {
    }
    //Invoked when the user leaves the app
    void BannerOnAdLeftApplicationEvent(IronSourceAdInfo adInfo)
    {
    }
#endif
#endregion



    public void ShowMessage(string msg)
    {
#if !UNITY_EDITOR && UNITY_ANDROID && USE_SHOWMESSAGE
        AndroidJavaObject @static = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject androidJavaObject = new AndroidJavaClass("android.widget.Toast");
        androidJavaObject.CallStatic<AndroidJavaObject>("makeText", new object[]
        {
                @static,
                msg,
                androidJavaObject.GetStatic<int>("LENGTH_SHORT")
        }).Call("show", Array.Empty<object>());
#endif
        Debug.Log(msg);
    }

    public void ShowMessageNOTCONFIG(string msg)
    {
#if !UNITY_EDITOR && UNITY_ANDROID
        AndroidJavaObject @static = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject androidJavaObject = new AndroidJavaClass("android.widget.Toast");
        androidJavaObject.CallStatic<AndroidJavaObject>("makeText", new object[]
        {
                @static,
                msg,
                androidJavaObject.GetStatic<int>("LENGTH_SHORT")
        }).Call("show", Array.Empty<object>());
#endif
        Debug.Log(msg);
    }

#if USE_APPLOVIN
    /// <summary>
    /// ⑥ When playing advertisements on the monetization platform
    /// </summary>
    /// <param name="adInfo"></param>
    public void TrackAdRevenue(MaxSdkBase.AdInfo adInfo)
    {
        var impressionParameters = new[] {
            new Firebase.Analytics.Parameter ("ad_platform", "AppLovin"),
            new Firebase.Analytics.Parameter ("ad_source", adInfo.NetworkName),
            new Firebase.Analytics.Parameter ("ad_unit_name", adInfo.AdUnitIdentifier),
            new Firebase.Analytics.Parameter ("ad_format", adInfo.Placement),
            new Firebase.Analytics.Parameter ("value", adInfo.Revenue),
            new Firebase.Analytics.Parameter ("currency", "USD"), // All AppLovin revenue is sent in USD
        }; ;
        FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
    }
#endif

#if USE_IRON_SOURCE
    /// <summary>
    /// ⑥ When playing advertisements on the monetization platform
    /// </summary>
    /// <param name="adInfo"></param>
    public void TrackAdRevenue(IronSourceImpressionData adInfo)
    {
        var impressionParameters = new[] {
            new Firebase.Analytics.Parameter ("ad_platform", "IronSource"),
            new Firebase.Analytics.Parameter ("ad_source", adInfo.adNetwork),
            new Firebase.Analytics.Parameter ("ad_unit_name", adInfo.instanceName),
            new Firebase.Analytics.Parameter ("ad_format", adInfo.adUnit),
            new Firebase.Analytics.Parameter ("value", (double) adInfo.revenue),
            new Firebase.Analytics.Parameter ("currency", "USD"), // All AppLovin revenue is sent in USD
        };
        Debug.Log("Track Ad Revenue AdFormat: " + adInfo.adUnit + " value: " + adInfo.revenue.ToString());
        FirebaseAnalytics.LogEvent("ad_impression", impressionParameters);
    }
#endif

    public void LogEventAppsflyerHelper(string eventKey)
    {
        Debug.Log($"LogEventAppsflyer : {eventKey}" );
#if USE_APPSFLYER
        AppsflyerHelper.LogEvent(eventKey);
#endif
    }

}
