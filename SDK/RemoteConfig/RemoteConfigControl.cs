using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;

public class RemoteConfigControl : MonoBehaviour
{
    public static RemoteConfigControl instance;
    public Action OnFetchDone;
    public bool isDataLocal = false;
    public bool isDataFetched = false;

    public int ads_interval = 25;
    public bool resume_ads = false;
    public bool rating_popup = false;
    public int rating_location = 2;
    public bool is_ads = false;
    protected bool isFirebaseInitialized = false;

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        InitializeFirebase();
    }

    private void OnEnable()
    {
        this.RegisterListener(EventID.OnRetryCheckInternet, OnRetryCheckInternetHandle);
    }
    private void OnDisable()
    {
        EventDispatcher.Instance?.RemoveListener(EventID.OnRetryCheckInternet, OnRetryCheckInternetHandle);
    }
    
    private void OnRetryCheckInternetHandle(object obj)
    {
        InitializeFirebase();
    }

    public void InitializeFirebase()
    {
        //LoadData();
        Dictionary<string, object> defaults =
            new Dictionary<string, object>
            {
                {"ads_interval", ads_interval},
                {"resume_ads", resume_ads == false?0:1},
                {"rating_popup", rating_popup == false?0:1},
                {"rating_location", rating_location},
            };

        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);

        Debug.Log("RemoteConfig configured and ready!");

        isFirebaseInitialized = true;
        FetchDataAsync();
    }

    public void FetchDataAsync()
    {
        Debug.Log("Fetching data...");
        System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(
            TimeSpan.Zero);
        fetchTask.ContinueWithOnMainThread(FetchComplete);
    }

    void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            Debug.Log("Fetch canceled.");
        }
        else if (fetchTask.IsFaulted)
        {
            Debug.Log("Fetch encountered an error.");
        }
        else if (fetchTask.IsCompleted)
        {
            Debug.Log("Fetch completed successfully!");
        }

        var info = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.Info;
        switch (info.LastFetchStatus)
        {
            case Firebase.RemoteConfig.LastFetchStatus.Success:
                Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                Debug.Log($"Remote data loaded and ready (last fetch time {info.FetchTime}).");
                Invoke(nameof(ReflectProperties),2);
                break;
            case Firebase.RemoteConfig.LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case Firebase.RemoteConfig.FetchFailureReason.Error:
                        Debug.Log("Fetch failed for unknown reason");
                        break;
                    case Firebase.RemoteConfig.FetchFailureReason.Throttled:
                        Debug.Log("Fetch throttled until " + info.ThrottledEndTime);
                        break;
                }

                break;
            case Firebase.RemoteConfig.LastFetchStatus.Pending:
                Debug.Log("Latest Fetch call still pending.");
                break;
        }
        OnFetchDone?.Invoke();
    }

    private void ReflectProperties()
    {
        ads_interval = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("ads_interval").DoubleValue;
        resume_ads = (bool)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("resume_ads").BooleanValue;
        rating_popup = (bool)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("rating_popup").BooleanValue;
        rating_location = (int)Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.GetValue("rating_location").DoubleValue;
        isDataFetched = true;
        this.PostEvent(EventID.OnCompliteLoad);
    }
}