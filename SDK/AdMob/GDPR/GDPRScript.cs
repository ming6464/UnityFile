using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using GoogleMobileAds.Api;
using UnityEngine;
using GoogleMobileAds.Ump;
using GoogleMobileAds.Ump.Api;

public class GDPRScript : MonoBehaviour
{
    void LoadAds()
    {
        Debug.Log("gdpr _ app open add init");
    }

    void Start()
    {
        var debugSettings = new ConsentDebugSettings
        {
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
            {

            }
        };
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            TagForUnderAgeOfConsent = false,


            //nếu bật preview trong unity thì tắt commet code dưới
            //ConsentDebugSettings = debugSettings,
        };


        Debug.Log("gdpr _ 1");

        // Check the current consent information status.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }



    void OnConsentInfoUpdated(FormError consentError)
    {
        Debug.Log("gdpr _ 2");
        if (consentError != null)
        {
            Debug.Log("gdpr _ 3");
            // Handle the error.
            LoadAds();
            UnityEngine.Debug.Log(consentError);
            return;
        }
        Debug.Log("gdpr _ 4");

        //sử lý bắt sự kiện show bảng gdpr ở đây

        // If the error is null, the consent information state was updated.
        // You are now ready to check if a form is available.
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            Debug.Log("gdpr _ 5");
            if (formError != null)
            {
                Debug.Log("gdpr _ 6");
                // Consent gathering failed.
                UnityEngine.Debug.Log(consentError);
                return;
            }
            Debug.Log("gdpr _ 7");
            //sử lý bắt sự kiện click cho phép ở đây
            // Consent has been gathered.
            if (ConsentInformation.CanRequestAds())
            {
                Debug.Log("gdpr _ 8");
                LoadAds();
            }
        });

    }
}
