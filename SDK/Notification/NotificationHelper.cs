using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android;
#endif

public class NotificationHelper : MonoBehaviour
{
    static List<string> Handled_Ids = new List<string>();

    string _Channel_Id = "notify_daily_reminder";
    string _Icon_Small = "icon"; //this is setup under Project Settings -> Mobile Notifications
    string _Icon_Large = "logo"; //this is setup under Project Settings -> Mobile Notifications
    string _Channel_Title = "Fashion Doll";
    string _Channel_Description = "Conquer it all!";

    private int _IdChannelOffline;

    private List<int> idOfflines = new List<int>();


    string[] titles =
    {
        "Daily Gift!!💗",
        "Get to Rewards! 🎁",
        "Comeback! Here!🔥"
    };

    string[] bodies =
    {
        "Get to new skin, and make up character!💗🎨🎨",
        "Rewards Ready to get! NOW 🔥🔥🔥",
        "Make over and dress up new character! NOW 🔥🔥🔥"
    };

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
#if UNITY_ANDROID

        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }


        //always remove any currently displayed notifications
        Unity.Notifications.Android.AndroidNotificationCenter.CancelAllDisplayedNotifications();

        //check if this was openened from a notification click
        var notification_intent_data = AndroidNotificationCenter.GetLastNotificationIntent();

        SetUpCancelNotiWhenRunning();

        //if the notification intent is not null and we have not already seen this notification id, do something
        //using a static List to store already handled notification ids
        if (notification_intent_data != null &&
            NotificationHelper.Handled_Ids.Contains(notification_intent_data.Id.ToString()) == false)
        {
            NotificationHelper.Handled_Ids.Add(notification_intent_data.Id.ToString());

            //this logic assumes only one type of notification is shown
            //show high scores when the user clicks the notification                
            UnityEngine.SceneManagement.SceneManager.LoadScene("HighScores");
            return;
        }

        //dont do anything further if the user has disabled notifications
        //this assumes you have additional ui to enabled/disable this preference
        var allow_notifications = PlayerPrefs.GetString("notifications");
        if (allow_notifications?.ToLower() == "false")
        {
            return;
        }
#endif

        this.Setup_Notifications();
    }

    internal void Setup_Notifications()
    {
#if UNITY_ANDROID
        //initialize the channel
        this.Initialize();

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
        var androidChannel = new AndroidNotificationChannel(this._Channel_Id, this._Channel_Title,
            this._Channel_Description, Importance.Default);
        AndroidNotificationCenter.RegisterNotificationChannel(androidChannel);
    }

    void SetUpCancelNotiWhenRunning()
    {
        //cancel noti when running
        AndroidNotificationCenter.NotificationReceivedCallback receivedNotificationHandler =
            delegate(AndroidNotificationIntentData data) { AndroidNotificationCenter.CancelNotification(data.Id); };

        AndroidNotificationCenter.OnNotificationReceived += receivedNotificationHandler;
    }

    void Schedule_Daily_Reminder()
    {
        Debug.Log("=========SET NOTI");
        AndroidNotificationCenter.CancelAllDisplayedNotifications();
        AndroidNotificationCenter.CancelAllNotifications();
        AndroidNotificationCenter.CancelAllScheduledNotifications();

        string title = "";
        string body = "";

        int[] times = { 8, 12, 21 };

        for (int i = 0; i < times.Length; i++)
        {
            title = titles[i];
            body = bodies[i];
            DateTime delivery_time =
                new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, times[i], 0, 0);
            for (int j = 0; j < 7; j++)
            {
                var delivery_time1 = delivery_time.AddDays(j);
                if (delivery_time1 > DateTime.Now)
                {
                    Schedule_NOTI(delivery_time1, title, body);
                }
            }
        }
    }

    private int Schedule_NOTI(DateTime time, string title, string body)
    {
        Debug.Log("Schedule notification ...");
        return Unity.Notifications.Android.AndroidNotificationCenter.SendNotification(
            new Unity.Notifications.Android.AndroidNotification()
            {
                Title = title,
                Text = body,
                FireTime = time,
                SmallIcon = this._Icon_Small,
                LargeIcon = this._Icon_Large
            },
            this._Channel_Id);
    }


    private void Schedule_OffLine_Reminder()
    {
        idOfflines = new List<int>();
        string title = "";
        string body = "";

        for (int i = 0; i < 3; i++)
        {
            title = titles[i];
            body = bodies[i];
            float time;

            //bắn noti vào 3 thời điểm 60,720,1440p sau khi off ứng dụng

            if (i == 0)
            {
                time = 60;
            }
            else if (i == 1)
            {
                time = 720;
            }
            else
            {
                time = 1440;
            }

            DateTime delivery_time = DateTime.Now;
            Debug.Log(delivery_time);
            delivery_time = delivery_time.AddMinutes(time);
            Debug.Log(delivery_time);

            Debug.Log("Schedule notification ...");
            var scheduled_notification_id = Unity.Notifications.Android.AndroidNotificationCenter.SendNotification(
                new Unity.Notifications.Android.AndroidNotification()
                {
                    Title = title,
                    Text = body,
                    FireTime = delivery_time,
                    SmallIcon = this._Icon_Small,
                    LargeIcon = this._Icon_Large,
                },
                this._Channel_Id);

            idOfflines.Add(scheduled_notification_id);
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // if (pauseStatus)
        // {
        //     try
        //     {
        //         _IdChannelOffline = Schedule_NOTI(DateTime.Now.AddHours(3), "Comeback! Here!🔥",
        //             "Make over and dress up new character! NOW 🔥🔥🔥");
        //     }
        //     catch (Exception e)
        //     {
        //     }
        // }
        // else
        // {
        //     try
        //     {
        //         AndroidNotificationCenter.CancelScheduledNotification(_IdChannelOffline);
        //     }
        //     catch (Exception e)
        //     {
        //     }
        // }
    }

#endif
}
