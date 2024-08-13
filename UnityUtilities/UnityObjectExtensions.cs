using UnityEngine;
namespace UnityUtils
{
    public static class UnityObjectExtensions
    {
        public static void DontDestroyOnLoad(this Object @object)
        {
            #if UNITY_EDITOR
            // ReSharper disable once EnforceIfStatementBraces
            if (UnityEditor.EditorApplication.isPlaying)
                    #endif
                Object.DontDestroyOnLoad(@object);
        }
        
        /// <summary>
        /// Hủy đối tượng Unity <see cref="Object"/> một cách phù hợp tùy thuộc vào chế độ đang chạy là edit hay play.
        /// </summary>
        /// <param name="object">Đối tượng Unity <see cref="Object"/> cần hủy</param>
        /// <param name="t">Thời gian tính bằng giây để hủy đối tượng, nếu áp dụng.</param>
        public static void Destroy(this Object @object, float t = 0.0f)
        {
            if (@object == null) { return; }

            if (Application.isPlaying)
            {
                Object.Destroy(@object, t);
            }
            else
            {
                #if UNITY_EDITOR
                // Must use DestroyImmediate in edit mode but it is not allowed when called from
                // trigger/contact, animation event callbacks or OnValidate. Must use Destroy instead.
                // Delay call to counter this issue in editor.
                UnityEditor.EditorApplication.delayCall += () =>
                        #endif
                        Object.DestroyImmediate(@object);
            }
        }
    }
}