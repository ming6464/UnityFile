using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UnityUtils {
    public static class TransformExtensions {
        /// <summary>
        /// Kiểm tra xem transform có nằm trong một khoảng cách nhất định và tùy chọn trong một góc nhất định (FOV) so với transform mục tiêu hay không.
        /// </summary>
        /// <param name="source">Transform cần kiểm tra.</param>
        /// <param name="target">Transform mục tiêu để so sánh khoảng cách và góc tùy chọn.</param>
        /// <param name="maxDistance">Khoảng cách tối đa cho phép giữa hai transform.</param>
        /// <param name="maxAngle">Góc tối đa cho phép giữa vector hướng về phía trước của transform và hướng đến mục tiêu (mặc định là 360).</param>
        /// <returns>True nếu transform nằm trong phạm vi và góc (nếu được cung cấp) của mục tiêu, ngược lại là false.</returns>
        public static bool InRangeOf(this Transform source, Transform target, float maxDistance, float maxAngle = 360f) {
            Vector3 directionToTarget = (target.position - source.position).With(y: 0);
            return directionToTarget.magnitude <= maxDistance && Vector3.Angle(source.forward, directionToTarget) <= maxAngle / 2;
        }
        
        /// <summary>
        /// Lấy tất cả các con của một Transform đã cho.
        /// </summary>
        /// <remarks>
        /// Phương thức này có thể được sử dụng với LINQ để thực hiện các thao tác trên tất cả các Transform con. Ví dụ,
        /// bạn có thể sử dụng nó để tìm tất cả các con có một tag cụ thể, để vô hiệu hóa tất cả các con, v.v.
        /// Transform triển khai IEnumerable và phương thức GetEnumerator trả về một IEnumerator của tất cả các con của nó.
        /// </remarks>
        /// <param name="parent">Transform để lấy các con từ đó.</param>
        /// <returns>Một IEnumerable&lt;Transform&gt; chứa tất cả các Transform con của parent.</returns>    
        public static IEnumerable<Transform> Children(this Transform parent) {
            foreach (Transform child in parent) {
                yield return child;
            }
        }

        /// <summary>
        /// Đặt lại vị trí, tỷ lệ và góc quay của transform
        /// </summary>
        /// <param name="transform">Transform để sử dụng</param>
        public static void Reset(this Transform transform) {
            transform.position = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
        
        /// <summary>
        /// Hủy tất cả các game object con của transform đã cho.
        /// </summary>
        /// <param name="parent">Transform mà các game object con của nó sẽ bị hủy.</param>
        public static void DestroyChildren(this Transform parent) {
            parent.ForEveryChild(child => Object.Destroy(child.gameObject));
        }

        /// <summary>
        /// Hủy ngay lập tức tất cả các game object con của transform đã cho.
        /// </summary>
        /// <param name="parent">Transform mà các game object con của nó sẽ bị hủy ngay lập tức.</param>
        public static void DestroyChildrenImmediate(this Transform parent) {
            parent.ForEveryChild(child => Object.DestroyImmediate(child.gameObject));
        }

        /// <summary>
        /// Kích hoạt tất cả các game object con của transform đã cho.
        /// </summary>
        /// <param name="parent">Transform mà các game object con của nó sẽ được kích hoạt.</param>
        public static void EnableChildren(this Transform parent) {
            parent.ForEveryChild(child => child.gameObject.SetActive(true));
        }

        /// <summary>
        /// Vô hiệu hóa tất cả các game object con của transform đã cho.
        /// </summary>
        /// <param name="parent">Transform mà các game object con của nó sẽ bị vô hiệu hóa.</param>
        public static void DisableChildren(this Transform parent) {
            parent.ForEveryChild(child => child.gameObject.SetActive(false));
        }

        /// <summary>
        /// Thực thi một hành động được chỉ định cho mỗi con của một transform đã cho.
        /// </summary>
        /// <param name="parent">Transform cha.</param>
        /// <param name="action">Hành động sẽ được thực hiện trên mỗi con.</param>
        /// <remarks>
        /// Phương thức này lặp qua tất cả các transform con theo thứ tự ngược và thực thi một hành động đã cho trên chúng.
        /// Hành động là một delegate nhận một Transform làm tham số.
        /// </remarks>
        public static void ForEveryChild(this Transform parent, System.Action<Transform> action) {
            for (var i = parent.childCount - 1; i >= 0; i--) {
                action(parent.GetChild(i));
            }
        }

        [Obsolete("Đổi tên thành ForEveryChild")]
        static void PerformActionOnChildren(this Transform parent, System.Action<Transform> action) {
            parent.ForEveryChild(action);
        }
        
        /// <summary>
        /// Đặt vị trí toàn cục của transform.
        /// </summary>
        /// <param name="transform">Transform cần đặt vị trí.</param>
        /// <param name="x">Tọa độ x.</param>
        /// <param name="y">Tọa độ y.</param>
        /// <param name="z">Tọa độ z.</param>
        public static void SetGlobalPosition(this Transform transform, float? x = null, float? y = null, float? z = null)
        {
            transform.position = transform.position.With(x,y,z);
        }


        /// <summary>
        /// Đặt tỷ lệ toàn cục của transform.
        /// </summary>
        /// <param name="transform">Transform cần đặt tỷ lệ.</param>
        /// <param name="scale">Tỷ lệ mới.</param>
        public static void SetGlobalScale(this Transform transform, Vector3 scale)
        {
            var lossyScale = transform.Scale();
            transform.localScale = new Vector3(
                    scale.x / lossyScale.x,
                    scale.y / lossyScale.y,
                    scale.z / lossyScale.z
            );
        }

        public static Vector3 Scale(this Transform transform)
        {
            return transform.lossyScale;
        }
        
        /// <summary>
        /// Liệt kê tất cả các con trong hệ thống phân cấp bắt đầu từ đối tượng gốc ngoại trừ các nhánh trong ignore.
        /// </summary>
        /// <param name="root">Điểm bắt đầu của tập hợp duyệt</param>
        /// <param name="ignore">Các Transform và tất cả các con của nó để bỏ qua</param>
        private static IEnumerable<Transform> EnumerateHierarchyCore(this Transform root, ICollection<Transform> ignore)
        {
            var transformQueue = new Queue<Transform>();
            transformQueue.Enqueue(root);

            while (transformQueue.Count > 0)
            {
                var parentTransform = transformQueue.Dequeue();

                if (!parentTransform || ignore.Contains(parentTransform)) { continue; }

                for (var i = 0; i < parentTransform.childCount; i++)
                {
                    transformQueue.Enqueue(parentTransform.GetChild(i));
                }

                yield return parentTransform;
            }
        }
        
        /// <summary>
        /// Liệt kê tất cả các đối tượng con trong hệ thống phân cấp bắt đầu từ đối tượng gốc, ngoại trừ các nhánh được chỉ định trong ignore.
        /// </summary>
        /// <param name="root">Điểm bắt đầu của quá trình duyệt</param>
        /// <param name="ignore">Các Transform và tất cả các đối tượng con của chúng cần bỏ qua</param>
        public static IEnumerable<Transform> EnumerateHierarchy(this Transform root, ICollection<Transform> ignore)
        {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            if (ignore == null) { throw new ArgumentNullException(nameof(ignore), "Ignore collection can't be null, use EnumerateHierarchy(root) instead."); }
            return root.EnumerateHierarchyCore(ignore);
        }
        
        // <summary>
        /// Liệt kê tất cả các đối tượng con trong hệ thống phân cấp bắt đầu từ đối tượng gốc.
        /// </summary>
        /// <param name="root">Điểm bắt đầu của quá trình duyệt</param>
        public static IEnumerable<Transform> EnumerateHierarchy(this Transform root)
        {
            if (root == null) { throw new ArgumentNullException(nameof(root)); }
            return root.EnumerateHierarchyCore(new List<Transform>(0));
        }
        
        /// <summary>
        /// Tính toán giới hạn của tất cả các collider được gắn vào GameObject này và tất cả các con của nó.
        /// </summary>
        /// <param name="transform">
        /// Transform của GameObject gốc mà các collider được gắn vào.
        /// </param>
        /// <param name="syncTransform">
        /// True, theo mặc định, điều này sẽ đồng bộ hóa xoay của transform để tính toán hướng trục căn chỉnh.
        /// </param>
        /// <param name="colliders">Bộ sưu tập Collider được tham chiếu sẽ được điền dữ liệu nếu không được truyền vào.</param>
        /// <returns>Giới hạn tổng thể của tất cả các collider được gắn vào GameObject này.</returns>
        public static Bounds GetColliderBounds(this Transform transform, ref Collider[] colliders, bool syncTransform = true)
        {
            // Store current rotation then zero out the rotation so that the bounds
            // are computed when the object is in its 'axis aligned orientation'.
            var currentRotation = transform.rotation;

            if (syncTransform)
            {
                transform.rotation = Quaternion.identity;
                Physics.SyncTransforms(); // Update collider bounds
            }

            if (colliders == null)
            {
                colliders = transform.GetComponentsInChildren<Collider>();
            }

            if (colliders.Length == 0) { return default; }

            var bounds = colliders[0].bounds;

            for (var i = 1; i < colliders.Length; i++)
            {
                bounds.Encapsulate(colliders[i].bounds);
            }

            if (syncTransform)
            {
                // After bounds are computed, restore rotation...
                // ReSharper disable once Unity.InefficientPropertyAccess
                transform.rotation = currentRotation;
                Physics.SyncTransforms();
            }

            return bounds;
        }
        
        /// <summary>
        /// Cho hai Transform, trả về Transform gốc chung (hoặc null).
        /// </summary>
        /// <param name="t1">Transform để so sánh</param>
        /// <param name="t2">Transform để so sánh</param>
        public static Transform FindCommonRoot(this Transform t1, Transform t2)
        {
            if (t1 == null || t2 == null)
            {
                return null;
            }

            var root = t2;

            while (t1 != null)
            {
                while (t2 != null)
                {
                    if (t1 == t2)
                    {
                        return t1;
                    }

                    t2 = t2.parent;

                    if (t2 == null)
                    {
                        if (t1 == null)
                        {
                            break;
                        }

                        t1 = t1.parent;
                        t2 = root;
                    }
                }
            }

            return null;
        }
        
        /// <summary>
        /// Thiết lập trạng thái hoạt động của collider và tất cả các collider con với giá trị được cung cấp.
        /// </summary>
        /// <param name="transform">Transform để lấy các collider từ đó.</param>
        /// <param name="isActive">Trạng thái hoạt động của collider.</param>
        /// <param name="colliders">Bộ sưu tập collider được cache để sử dụng thay vì tìm kiếm tất cả.</param>
        /// <returns>Bộ sưu tập các collider đã được tác động. Bộ sưu tập này có thể được sử dụng sau khi gọi phương thức này lại.</returns>
        public static Collider[] SetCollidersActive(this Transform transform, bool isActive, Collider[] colliders = null)
        {
            colliders ??= transform.GetComponentsInChildren<Collider>();

            foreach (var t in colliders)
            {
                t.enabled = isActive;
            }

            return colliders;
        }
        
        /// <summary>
        /// Thiết lập trạng thái hoạt động của collider và tất cả các collider con với giá trị được cung cấp.
        /// </summary>
        /// <param name="transform">Transform để lấy các collider từ đó.</param>
        /// <param name="isActive">Trạng thái hoạt động của collider.</param>
        /// <param name="colliders">Bộ sưu tập collider được cache để sử dụng thay vì tìm kiếm tất cả.</param>
        /// <returns>Bộ sưu tập các collider đã được tác động. Bộ sưu tập này có thể được sử dụng sau khi gọi phương thức này lại.</returns>
        public static void SetCollidersActive(this Transform transform, bool isActive, ref Collider[] colliders)
        {
            colliders ??= transform.GetComponentsInChildren<Collider>();

            foreach (var t in colliders)
            {
                t.enabled = isActive;
            }
        }
    }
}