using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using Firebase.Analytics;
using GoogleMobileAds.Api;
using UnityEngine;
using UnityEngine.UI;

public class NativeAdsAdmob : MonoBehaviour
{
    [SerializeField] private List<InfoNative> _infoNatives;
    private bool isStartLoad;
    private bool isLoadedAll;
    private void Awake()
    {
        foreach (InfoNative info in _infoNatives)
        {
            info.InfoNativeItem.ObjNativeAds?.SetActive(false);
        }
    }

    private void OnEnable()
    {
        EventDispatcher.Instance.RegisterListener(EventID.RemoveEventPaid,RemoveEventPaid);
    }

    private void OnDisable()
    {
        EventDispatcher.Instance.RemoveListener(EventID.RemoveEventPaid,RemoveEventPaid);
    }

    private void Start()
    {
        foreach (InfoNative info in _infoNatives)
        {
            RequestLoad(info);
        }
    }

    private void RemoveEventPaid(object obj)
    {
        for (int i = 0; i < _infoNatives.Count; i++)
        {
            if(_infoNatives[i].NativeAd == null) continue;
            _infoNatives[i].NativeAd.OnPaidEvent -= (sender, args) =>
            {
                HandleNativeAdPaid(sender, args , _infoNatives[i].IdNative);
            };
        }
    }


    private void RequestLoad(InfoNative info)
    {
        AdLoader AdLoader = new AdLoader.Builder(info.IdNative).ForNativeAd().Build();  
        AdLoader.OnNativeAdLoaded += (sender, args) =>
        {
            this.HandleNativeAdLoaded(sender, args, info.IdNative);
        };
        AdLoader.OnAdFailedToLoad += (sender, args) =>
        {
            this.HandleAdFailedToLoad(sender, args, info.IdNative);
        };
        AdLoader.LoadAd(new AdRequest.Builder().Build());
    }

    void Update() {

        if(isLoadedAll) return;
        UpdateNative();
        isLoadedAll = _infoNatives.FindAll(x => x.NativeAdLoaded == false).Count == 0;
    }

    private void UpdateNative()
    {
        foreach (InfoNative i in _infoNatives)
        {
            if (i.LoadAds)
            {
                Debug.Log("UpdateNative");
                i.LoadAds = false;
                NativeAd nativeAd = i.NativeAd;
                i.InfoNativeItem.AdIcon.texture = nativeAd.GetIconTexture();
                i.InfoNativeItem.AdChoice.texture = nativeAd.GetAdChoicesLogoTexture();
                i.InfoNativeItem.BodyText.text = nativeAd.GetBodyText();
                i.InfoNativeItem.AdCallToAction.text = nativeAd.GetCallToActionText();
                if (!nativeAd.RegisterIconImageGameObject(i.InfoNativeItem.AdIcon.gameObject))
                {
                    Debug.Log("Loadfaild AdIcon");
                }
                if (!nativeAd.RegisterAdChoicesLogoGameObject(i.InfoNativeItem.AdChoice.gameObject))
                {
                    Debug.Log("Loadfaild AdChoice");
                }
                if (!nativeAd.RegisterAdvertiserTextGameObject(i.InfoNativeItem.BodyText.gameObject))
                {
                    Debug.Log("Loadfaild BodyText");
                }
                if (!nativeAd.RegisterCallToActionGameObject(i.InfoNativeItem.AdCallToAction.gameObject))
                {
                    Debug.Log("Loadfaild AdCallToAction");
                }

                i.InfoNativeItem.ObjNativeAds?.SetActive(true);
            }
        }
    }
    
    private void HandleNativeAdLoaded(object sender, NativeAdEventArgs args,string id) {
        Debug.Log("Native ad loaded.");
        int index = _infoNatives.FindIndex(x => x.IdNative.Equals(id));
        if (index >= 0)
        {
            _infoNatives[index].LoadAds = true;
            _infoNatives[index].NativeAdLoaded = true;
            _infoNatives[index].NativeAd = args.nativeAd;
            _infoNatives[index].NativeAd.OnPaidEvent += (sender, args) =>
            {
                HandleNativeAdPaid(sender, args , _infoNatives[index].IdNative);
            };
        }
    }

    private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs e,string id)
    {
        // Debug.LogError(id);
        // Debug.LogError(e);
        // Debug.LogError(e.LoadAdError);
        int index = _infoNatives.FindIndex(x => x.IdNative.Equals(id));
        if (index >= 0)
        {
            RequestLoad(_infoNatives[index]);
        }
    }
    
    private void HandleNativeAdPaid(object sender, AdValueEventArgs args,string id)
    {
        int index = _infoNatives.FindIndex(x => x.IdNative.Equals(id));
        if(index < 0) return;
        
        NativeAd nativeAd = _infoNatives[index].NativeAd;
        AdValue adValue = args.AdValue;
        double valueMicros = adValue.Value / 1000000f;;
        string currencyCode = adValue.CurrencyCode;
        ResponseInfo responseInfo = nativeAd.GetResponseInfo();
        AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.GetLoadedAdapterResponseInfo();
        string adSourceId = loadedAdapterResponseInfo.AdSourceId;
        string adSourceName = loadedAdapterResponseInfo.AdSourceName;

#if USE_FIREBASE_LOG_EVENT
        var impressionParameters = new[] {
            new Parameter ("ad_platform", "Admob"),
            new Parameter ("value", valueMicros),
            new Parameter("ad_source", adSourceName),
            new Parameter("ad_unit_name", adSourceId),
            new Parameter("ad_format", "Native Ads"),
            new Parameter ("currency", currencyCode), // All AppLovin revenue is sent in USD
        };
        string text = $"ad_platform : admob\nvalue : {valueMicros}\nad_source : {adSourceName}\nad_unit_name : {adSourceId}\nad_format : Native Ads\ncurrency : {currencyCode}";
        Debug.Log("Track NativeADS Revenue AdFormat (NativeADS)\n" + text);
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
    }
}

[Serializable]
public class InfoNativeItem
{
    public GameObject ObjNativeAds;
    public RawImage AdIcon;
    public RawImage AdChoice;
    public Text BodyText;
    public Text AdCallToAction;
}

[Serializable]
public class InfoNative
{
    public string IdNative;
    [HideInInspector] public bool NativeAdLoaded;
    [HideInInspector] public bool LoadAds;
    public NativeAd NativeAd;
    public InfoNativeItem InfoNativeItem;
}

