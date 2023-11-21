using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

public class NotificationHelper : MonoBehaviour
{
    string _Channel_Id = "notify_daily_reminder";
    string _Icon_Small = "icon"; //this is setup under Project Settings -> Mobile Notifications
    string _Icon_Large = "logo"; //this is setup under Project Settings -> Mobile Notifications
    string _Channel_Title = "Merge Tank Shooter";
    string _Channel_Description = "Conquer it all!";
    private int _IdChannelOffline;
    private bool checkInit;

    private void Awake()
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = false;
    }

    private void PermissionCallbacks_PermissionDenied(string obj)
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
    }

    private void PermissionCallbacks_PermissionGranted(string obj)
    {
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;
        IntialNoti();
    }

    private void Start()
    {
        Debug.Log("minh_20231120 _ noti 1");

#if UNITY_ANDROID && !UNITY_EDITOR
        if(GetVersionAndroid() >= 13){
            if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
            {
                Debug.Log("minh_20231120 _ noti 2");
                var callbacks = new PermissionCallbacks();
                callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
                callbacks.PermissionDenied += PermissionCallbacks_PermissionDenied;
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
#elif UNITY_EDITOR || UNITY_IOS
        PlayerPrefsSave.IsAcceptNotiAndroid13 = true;  
        checkInit = true;
        IntialNoti();
#endif
    }


    void IntialNoti()
    {
        checkInit = true;
        Debug.Log("minh_20231120 _ noti 5");
#if UNITY_ANDROID && !UNITY_EDITOR
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

            //this logic assumes only one type of notification is shown
            //show high scores when the user clicks the notification                
            //UnityEngine.SceneManagement.SceneManager.LoadScene("HighScores");
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
#if UNITY_ANDROID
        //initialize the channel
        this.Initialize();
        Debug.Log("Setup Nortification");
        //schedule the next notification
        this.Schedule_Daily_Reminder();
#endif
#if UNITY_IOS
        this.RequestAuthorization();
#endif
    }

#if UNITY_IOS
    void RequestAuthorization()
    {
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
    }

    void OnApplicationPause(bool pauseStatus) {
        if (pauseStatus) {
            //schedule the next notification
            this.ScheduleNotificationIOS();
        }
    }

    void ScheduleNotificationIOS() {
        UnityEngine.iOS.NotificationServices.ClearLocalNotifications();
        UnityEngine.iOS.NotificationServices.CancelAllLocalNotifications();

        //create new schedule
        string[] titles = { 
            "Get! NEW SKIN!💗", 
            "Get to Rewards! 🎁",
            "Comeback! Here!🔥"
        };
        string[] bodies = { 
            "Get to new skin, and make up character!💗🎨🎨",
            "Rewards Ready to get! NOW 🔥🔥🔥",
            "Make over and dress up new character! NOW 🔥🔥🔥"
        };

        string title = "";
        string body = "";
        
        // send notification in the next 7 days at 12:30PM
        for (int i = 1;i <= 7;i++) {
            System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
            title = titles[rand.Next(titles.Length)];
            body = bodies[rand.Next(bodies.Length)];

            //show at the specified time - 12:30 AM
            //you could also always set this a certain amount of hours ahead, since this code resets the schedule, this could be used to prompt the user to play again if they haven't played in a while
            DateTime delivery_time = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 30, 0);
            if (delivery_time < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                delivery_time = delivery_time.AddDays(i);
            }
            else if ((delivery_time - DateTime.Now).TotalHours <= 0)
            {
                //optional
                //if too close to current time (<= 4 hours away), push delivery date forward 1 day
                delivery_time = delivery_time.AddDays(i);
            }

            Debug.Log("Schedule notification ...");

            UnityEngine.iOS.LocalNotification notify = new UnityEngine.iOS.LocalNotification();
            notify.fireDate = delivery_time;
            notify.alertTitle = title;
            notify.alertBody = body;
            notify.repeatInterval = UnityEngine.iOS.CalendarUnit.Day;

            UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notify);
        }
    }
#endif

#if UNITY_ANDROID
    void Initialize()
    {
        //add our channel
        //a channel can be used by more than one notification
        //you do not have to check if the channel is already created, Android OS will take care of that logic
        Debug.Log("minh_noti_Version Android : " + GetVersionAndroid());
        if (GetVersionAndroid() >= 8.0f)
        {
            AndroidNotificationChannel androidChannel = new AndroidNotificationChannel(this._Channel_Id, this._Channel_Title, this._Channel_Description, Importance.Default);
            AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
        }
    }

    private float GetVersionAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
    try
        {
            AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION");
            return float.Parse(version.GetStatic<string>("RELEASE"));
        }
        catch (Exception e)
        {
            return 0;
        }
#endif
        return 0;
    }
    

    void Schedule_Daily_Reminder()
    {
        if(!checkInit) return;
        //since this is the only notification I have, I will cancel any currently pending notifications
        //if I create more types of notifications, additional logic will be needed
        AndroidNotificationCenter.CancelAllScheduledNotifications();

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

        string title = "";
        string body = "";

        // Calculate delivery time for daily notification at 12:30PM
        DateTime currentDateTime = DateTime.Now;
        //DateTime deliveryTimeDaily = new DateTime(currentDateTime.Year, currentDateTime.Month, currentDateTime.Day, 12, 30, 0);

        // Create and schedule daily notification
        //ScheduleNotification(titles[2], bodies[2], deliveryTimeDaily);


        for (int i = 0; i < 7; i++)
        {
            //System.Random rand = new System.Random(Guid.NewGuid().GetHashCode());
            title = titles[2];
            body = bodies[2];

            //show at the specified time - 12 AM
            //you could also always set this a certain amount of hours ahead, since this code resets the schedule, this could be used to prompt the user to play again if they haven't played in a while

            DateTime delivery_time12 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0);
            DateTime delivery_time8 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 8, 0, 0);
            DateTime delivery_time20 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 20, 0, 0);

            int add_8 = 0;
            int add_12 = 0;
            int add_20 = 0;


            if (delivery_time12 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_12 = 1;
            }
            if (delivery_time8 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_8 = 1;
            }
            if (delivery_time20 < DateTime.Now)
            {
                //if in the past (ex: this code runs at 11:00 AM), push delivery date forward 1 day
                //delivery_time = delivery_time.AddDays(i + 1);
                add_20 = 1;
            }
            // else if ((delivery_time - DateTime.Now).TotalHours <= 0)
            // {
            //     //optional
            //     //if too close to current time (<= 4 hours away), push delivery date forward 1 day
            //     delivery_time = delivery_time.AddDays(i);
            // }

            delivery_time8 = delivery_time8.AddDays(i + add_8);
            delivery_time12 = delivery_time12.AddDays(i + add_12);
            delivery_time20 = delivery_time20.AddDays(i + add_20);


            //delivery_time = DateTime.Now.AddSeconds(10);

            ScheduleNotification(titles[0], bodies[0], delivery_time8);
            ScheduleNotification(titles[1], bodies[1], delivery_time12);
            ScheduleNotification(titles[2], bodies[2], delivery_time20);

        }
        
        // offline
        // DateTime deliveryTime60min = currentDateTime.AddMinutes(60);
        // DateTime deliveryTime720min = currentDateTime.AddMinutes(720);
        // DateTime deliveryTime1440min = currentDateTime.AddMinutes(1440);

        // ScheduleNotification(titles[0], bodies[0], deliveryTime60min);
        // ScheduleNotification(titles[1], bodies[1], deliveryTime720min);
        // ScheduleNotification(titles[2], bodies[2], deliveryTime1440min);
    }
    
    int ScheduleNotification(string title, string body, DateTime deliveryTime)
    {
        if(!checkInit) return -1;
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

        return -1;
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if(!checkInit) return;
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
    }
#endif
}
