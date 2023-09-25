- Vào Window -> Package Manager -> Cài đặt Package Mobile Notifications
- Thêm config icon: Vào Project Setting -> Mobile Notifications
- Trong project setting > player > publishing setting,tích chọn custom proguard file, vào Assets/Plugins/Android xoá bỏ
proguard-user và thay vào của mình


nếu target từ 33 thì: 
- thêm vào start của NotificationHelper
if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
{
  Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
}

- thêm vào mainifest:
<uses-permission android:name="android.permission.POST_NOTIFICATIONS"/>
<uses-permission android:name="android.permission.RECEIVE_BOOT_COMPLETED"/>
<uses-permission android:name="android.permission.SCHEDULE_EXACT_ALARM" />
<uses-permission android:name="android.permission.USE_EXACT_ALARM" /
