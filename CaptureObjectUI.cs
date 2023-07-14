using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CaptureObjectUI : MonoBehaviour
{
    [SerializeField] private RectTransform targetRect;
    private string folderPath;
    private Camera _camera;
     
    private void Start()
    {
        _camera = Camera.main;
        
        //tạo folder để lưu ảnh vảo
        string pathSafe;
#if UNITY_EDITOR
        pathSafe = Application.dataPath;
#else
        pathSafe = Application.persistentDataPath;
#endif

        string folderName = "screenshot";
        
        folderPath = Path.Combine(pathSafe, folderName);

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        //
    }

    public void CaptureTargetRect()
    {
        StartCoroutine(TakePathScreenShot());
    }

    public void CaptureFullScreen()
    {
        StartCoroutine(TakeScreenshotFull());
    }
    private IEnumerator TakePathScreenShot()
    {
        yield return new WaitForEndOfFrame();
     
        var corners = new Vector3[4];
        targetRect.GetWorldCorners(corners);
        var bl = RectTransformUtility.WorldToScreenPoint(_camera, corners[0]);
        var tl = RectTransformUtility.WorldToScreenPoint(_camera, corners[1]);
        var tr = RectTransformUtility.WorldToScreenPoint(_camera, corners[2]);
     
        var height = tl.y - bl.y;
        var width = tr.x - bl.x;
     
        Texture2D tex = new Texture2D((int)width, (int)height, TextureFormat.RGB24, false);
        Rect rex = new Rect(bl.x,bl.y,width,height);
        tex.ReadPixels(rex, 0, 0);
        tex.Apply();
        var bytes = tex.EncodeToPNG();
        Destroy(tex);
        
        File.WriteAllBytes(GetPath(), bytes);
    }

    private string GetPath()
    {
        long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        string filename = milliseconds + ".png";
        string filePath = Path.Combine(folderPath, filename);
        Debug.Log("Path Capture : " + filePath);
        return filePath;
    }

    // Coroutine để chụp màn hình và lưu ảnh
    private IEnumerator TakeScreenshotFull()
    {
        yield return new WaitForEndOfFrame();

        // Tạo texture mới với kích thước màn hình
        Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        // Đọc dữ liệu từ màn hình vào texture
        texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        texture.Apply();

        // Lưu texture thành ảnh
        byte[] bytes = texture.EncodeToPNG();
        Destroy(texture);


        // Lưu ảnh vào đường dẫn đã chỉ định
        System.IO.File.WriteAllBytes(GetPath(), bytes);
    }

    //Thêm ảnh vào media để hiển thị trên các ứng dụng album của điện thoại
    public void AddToMedia(string filePath)
    {
        using (AndroidJavaClass mediaClass = new AndroidJavaClass("android.provider.MediaStore$Images$Media"))
        {
            using (AndroidJavaObject imageFile = new AndroidJavaObject("java.io.File", filePath))
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                {
                    using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                    {
                        // Lấy đường dẫn của ứng dụng Unity
                        string unityPackageName = currentActivity.Call<string>("getPackageName");
                        AndroidJavaObject unityContentResolver = currentActivity.Call<AndroidJavaObject>("getContentResolver");

                        // Thêm ảnh vào MediaStore
                        AndroidJavaObject imageUri = mediaClass.CallStatic<AndroidJavaObject>("insertImage", 
                            unityContentResolver, imageFile.Call<string>("getAbsolutePath"), "Title", "Description");
                        Debug.Log("Đã thêm ảnh vào MediaStore: " + imageUri);
                    }
                }
            }
        }
    }
}
