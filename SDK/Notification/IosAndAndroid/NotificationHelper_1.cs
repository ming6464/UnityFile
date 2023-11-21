using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#elif UNITY_IOS
using LocalNotification = UnityEngine.iOS.LocalNotification;
using NotificationServices = UnityEngine.iOS.NotificationServices;
#endif

public class NotificationHelper_1 : MonoBehaviour
{
    string _Channel_Id = "notify_daily_reminder";
    string _Icon_Small = "icon"; //this is setup under Project Settings -> Mobile Notifications
    string _Icon_Large = "logo"; //this is setup under Project Settings -> Mobile Notifications
    string _Channel_Title = "Merge Tank Shooter";
    string _Channel_Description = "Conquer it all!";
    private int _IdChannelOffline;
    private bool checkInit;
    
    //create new schedule
    string[] titles = {
        "BobaTea on Morning",
        "Drink for lunch",
        "Chill in night!"
    };
    string[] bodies = {
        "Start your day excitedly with a refreshing smoothie!!!",
        "A delicious cup of milk tea for an enjoyable lunch",
        "Immerse yourself in the night with a gentle, loving cocktail"
    };
    
    private void Awake()
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = false;
    }
    
    private void Start()
    {
        Debug.Log("minh_20231120 _ noti 1");
#if UNITY_ANDROID
        if(GetVersionAndroid() >= 13){
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Debug.Log("minh_20231120 _ noti 2");
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += PermissionPostNotificationGranted;
                callbacks.PermissionDenied += PermissionPostNotificationDenied;
                Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS",callbacks);
                Debug.Log("minh_20231120 _ noti 3");
                PlayerPrefsSave.IsAcceptNotiAndroid13 = false;
            }
        }
        else
        {
            Debug.Log("minh_20231120 _ noti 4");
            PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
            IntialNoti();
        }
        return;
#endif
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;  
        IntialNoti();
    }
    
    void IntialNoti()
    {
        checkInit = true;
        Debug.Log("minh_20231120 _ noti 5");
#if UNITY_ANDROID
        //always remove any currently displayed notifications
        Unity.Notifications.Android.AndroidNotificationCenter.CancelAllDisplayedNotifications();
        Debug.Log("minh_noti_2");
        //check if this was openened from a notification click
        var notification_intent_data = AndroidNotificationCenter.GetLastNotificationIntent();
        Debug.Log("minh_noti_3");
        //if the notification intent is not null and we have not already seen this notification id, do something
        //using a static List to store already handled notification ids
        if (notification_intent_data != null && NotificationHelper.Handled_Ids.Contains(notification_intent_data.Id.ToString()) == false)
        {
            Debug.Log("minh_noti_3.1");
            NotificationHelper.Handled_Ids.Add(notification_intent_data.Id.ToString());
            return;
        }
        
        Debug.Log("minh_noti_4");

        //dont do anything further if the user has disabled notifications
        //this assumes you have additional ui to enabled/disable this preference
        var allow_notifications = PlayerPrefs.GetString("notifications");
        if (allow_notifications?.ToLower() == "false")
        {
            return;
        }
        Debug.Log("minh_noti_5");
#endif
        this.Setup_Notifications();
    }
    internal void Setup_Notifications()
    {
        if(!checkInit) return;
#if UNITY_ANDROID
        //initialize the channel
        //add our channel
        //a channel can be used by more than one notification
        //you do not have to check if the channel is already created, Android OS will take care of that logic
        if (GetVersionAndroid() >= 8.0f)
        {
            AndroidNotificationChannel androidChannel = new AndroidNotificationChannel(this._Channel_Id, this._Channel_Title, this._Channel_Description, Importance.Default);
            AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
        }
#elif UNITY_IOS
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
#endif
        ScheduleNoti();
    }
    
    void ScheduleNoti()
    {
        if(!checkInit) return;
        // Calculate delivery time for daily notification at 12:30PM
        DateTime currentDateTime = DateTime.Now;
#if UNITY_ANDROID
        AndroidNotificationCenter.CancelAllScheduledNotifications();
#elif UNITY_IOS
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();
#endif 
        // Create and schedule daily notification
        for (int i = 0; i < 7; i++)
        {
            //show at the specified time - 12 AM
            //you could also always set this a certain amount of hours ahead, since this code resets the schedule, this could be used to prompt the user to play again if they haven't played in a while
            DateTime delivery_time12 = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 12, 0, 0);
            DateTime delivery_time8 = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 8, 0, 0);
            DateTime delivery_time20 = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 20, 0, 0);

            int add_8 = 0;
            int add_12 = 0;
            int add_20 = 0;


            if (delivery_time12 < DateTime.Now)
            {
                add_12 = 1;
            }
            if (delivery_time8 < DateTime.Now)
            {
                add_8 = 1;
            }
            if (delivery_time20 < DateTime.Now)
            {
                add_20 = 1;
            }

            delivery_time8 = delivery_time8.AddDays(i + add_8);
            delivery_time12 = delivery_time12.AddDays(i + add_12);
            delivery_time20 = delivery_time20.AddDays(i + add_20);
            ScheduleNotification(titles[0], bodies[0], delivery_time8);
            ScheduleNotification(titles[1], bodies[1], delivery_time12);
            ScheduleNotification(titles[2], bodies[2], delivery_time20);
        }
    }

    private int ScheduleNotification(string title,string body,DateTime deliveryTime)
    {
        if(!checkInit) return -1;
#if UNITY_ANDROID
        try
        {
            if (deliveryTime > DateTime.Now)
            {
                Debug.Log("Schedule notification: " + deliveryTime.ToString());
                var scheduled_notification_id = Unity.Notifications.Android.AndroidNotificationCenter.SendNotification(
                    new Unity.Notifications.Android.AndroidNotification()
                    {
                        Title = title,
                        Text = body,
                        FireTime = deliveryTime,
                        SmallIcon = this._Icon_Small,
                        LargeIcon = this._Icon_Large
                    },
                    this._Channel_Id);
                return scheduled_notification_id;
            }
        }
        catch (Exception e)
        {
        }
#elif UNITY_IOS
        try
        {
            LocalNotification notify = new LocalNotification();
            notify.fireDate = deliveryTime;
            notify.alertTitle = title;
            notify.alertBody = body;
            NotificationServices.ScheduleLocalNotification(notify);
            return NotificationServices.scheduledLocalNotifications.Length - 1;
        }
        catch (Exception e)
        {
        }
#endif
        return -1;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if(!checkInit) return;
#if UNITY_ANDROID
        if (pauseStatus)
        {
            try
            {
                _IdChannelOffline = ScheduleNotification("Welcome Back!", 
                    "Bubble tea is ready to serve, waiting for you to enjoy!", DateTime.Now.AddHours(3));
            }
            catch (Exception e)
            {}
        }
        else
        {
            try
            {
                AndroidNotificationCenter.CancelScheduledNotification(_IdChannelOffline);
            }
            catch (Exception e)
            {}
        }
#elif UNITY_IOS
        if (pauseStatus)
        {
            try
            {
                _IdChannelOffline = ScheduleNotification("Welcome Back!", 
                    "Bubble tea is ready to serve, waiting for you to enjoy!", DateTime.Now.AddHours(3));
            }
            catch (Exception e)
            {}
        }
        else
        {
            try
            {
                NotificationServices.CancelLocalNotification(NotificationServices.scheduledLocalNotifications[_IdChannelOffline]);
            }
            catch (Exception e)
            {}
        }
#endif        
    }

#region IOS
#if UNITY_IOS
#endif
#endregion

#region Android
#if UNITY_ANDROID
    private void PermissionPostNotificationDenied(string obj)
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
    }

    private void PermissionPostNotificationGranted(string obj)
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
        IntialNoti();
    }
    private float GetVersionAndroid()
    {
        try
        {
            AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION");
            return float.Parse(version.GetStatic<string>("RELEASE"));
        }
        catch (Exception e)
        {
            return 0;
        }
        return 0;
    }
#endif
#endregion
}
