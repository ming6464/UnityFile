using UnityEngine;
using System.Linq;

namespace UnityUtils {
    public static class GameObjectExtensions {
        /// <summary>
        /// Phương thức này được sử dụng để ẩn GameObject trong chế độ xem Hierarchy.
        /// </summary>
        /// <param name="gameObject">GameObject cần ẩn</param>
        public static void HideInHierarchy(this GameObject gameObject) {
            gameObject.hideFlags = HideFlags.HideInHierarchy;
        }

        /// <summary>
        /// Lấy một component của loại đã cho gắn vào GameObject. Nếu loại component đó không tồn tại, nó sẽ thêm một cái.
        /// </summary>
        /// <remarks>
        /// Phương thức này hữu ích khi bạn không biết liệu một GameObject có một loại component cụ thể hay không,
        /// nhưng bạn muốn làm việc với component đó bất kể thế nào. Thay vì kiểm tra và thêm component thủ công,
        /// bạn có thể sử dụng phương thức này để thực hiện cả hai thao tác trong một dòng.
        /// </remarks>
        /// <typeparam name="T">Loại của component cần lấy hoặc thêm.</typeparam>
        /// <param name="gameObject">GameObject để lấy component từ đó hoặc thêm component vào.</param>
        /// <returns>Component hiện có của loại đã cho, hoặc một cái mới nếu không có component như vậy tồn tại.</returns>    
        public static T GetOrAdd<T>(this GameObject gameObject) where T : Component {
            T component = gameObject.GetComponent<T>();
            if (!component) component = gameObject.AddComponent<T>();

            return component;
        }

        /// <summary>
        /// Trả về chính đối tượng nếu nó tồn tại, ngược lại trả về null.
        /// </summary>
        /// <remarks>
        /// Phương thức này giúp phân biệt giữa một tham chiếu null và một đối tượng Unity đã bị hủy. Kiểm tra "== null" của Unity
        /// có thể trả về true không chính xác cho các đối tượng đã bị hủy, dẫn đến hành vi gây nhầm lẫn. Phương thức OrNull sử dụng
        /// "kiểm tra null" của Unity, và nếu đối tượng đã được đánh dấu để hủy, nó đảm bảo trả về một tham chiếu null thực sự,
        /// giúp chuỗi các thao tác một cách chính xác và ngăn chặn NullReferenceExceptions.
        /// </remarks>
        /// <typeparam name="T">Loại của đối tượng.</typeparam>
        /// <param name="obj">Đối tượng đang được kiểm tra.</param>
        /// <returns>Chính đối tượng nếu nó tồn tại và chưa bị hủy, ngược lại trả về null.</returns>
        public static T OrNull<T>(this T obj) where T : Object => obj ? obj : null;

        /// <summary>
        /// Hủy tất cả các con của game object
        /// </summary>
        /// <param name="gameObject">GameObject mà các con của nó sẽ bị hủy.</param>
        public static void DestroyChildren(this GameObject gameObject) {
            gameObject.transform.DestroyChildren();
        }

        /// <summary>
        /// Hủy ngay lập tức tất cả các con của GameObject đã cho.
        /// </summary>
        /// <param name="gameObject">GameObject mà các con của nó sẽ bị hủy.</param>
        public static void DestroyChildrenImmediate(this GameObject gameObject) {
            gameObject.transform.DestroyChildrenImmediate();
        }

        /// <summary>
        /// Kích hoạt tất cả các GameObject con liên kết với GameObject đã cho.
        /// </summary>
        /// <param name="gameObject">GameObject mà các GameObject con của nó sẽ được kích hoạt.</param>
        public static void EnableChildren(this GameObject gameObject) {
            gameObject.transform.EnableChildren();
        }

        /// <summary>
        /// Vô hiệu hóa tất cả các GameObject con liên kết với GameObject đã cho.
        /// </summary>
        /// <param name="gameObject">GameObject mà các GameObject con của nó sẽ bị vô hiệu hóa.</param>
        public static void DisableChildren(this GameObject gameObject) {
            gameObject.transform.DisableChildren();
        }

        /// <summary>
        /// Đặt lại vị trí, góc quay và tỷ lệ của transform của GameObject về giá trị mặc định.
        /// </summary>
        /// <param name="gameObject">GameObject mà transformation của nó sẽ được đặt lại.</param>
        public static void ResetTransformation(this GameObject gameObject) {
            gameObject.transform.Reset();
        }

        /// <summary>
        /// Trả về đường dẫn phân cấp trong cấu trúc phân cấp cảnh Unity cho GameObject này.
        /// </summary>
        /// <param name="gameObject">GameObject cần lấy đường dẫn.</param>
        /// <returns>Một chuỗi đại diện cho đường dẫn phân cấp đầy đủ của GameObject này trong cảnh Unity.
        /// Đây là một chuỗi được phân tách bằng '/' trong đó mỗi phần là tên của một cha, bắt đầu từ cha gốc và kết thúc
        /// bằng tên của cha của GameObject được chỉ định.</returns>
        public static string Path(this GameObject gameObject) {
            return "/" + string.Join("/",
                gameObject.GetComponentsInParent<Transform>().Select(t => t.name).Reverse().ToArray());
        }

        /// <summary>
        /// Trả về đường dẫn phân cấp đầy đủ trong cấu trúc phân cấp cảnh Unity cho GameObject này.
        /// </summary>
        /// <param name="gameObject">GameObject cần lấy đường dẫn.</param>
        /// <returns>Một chuỗi đại diện cho đường dẫn phân cấp đầy đủ của GameObject này trong cảnh Unity.
        /// Đây là một chuỗi được phân tách bằng '/' trong đó mỗi phần là tên của một cha, bắt đầu từ cha gốc và kết thúc
        /// bằng tên của chính GameObject được chỉ định.</returns>
        public static string PathFull(this GameObject gameObject) {
            return gameObject.Path() + "/" + gameObject.name;
        }

        /// <summary>
        /// Đặt lớp đã cung cấp cho GameObject này và tất cả các con cháu của nó trong cấu trúc phân cấp cảnh Unity một cách đệ quy.
        /// </summary>
        /// <param name="gameObject">GameObject cần đặt lớp.</param>
        /// <param name="layer">Số lớp cần đặt cho GameObject và tất cả các con cháu của nó.</param>
        public static void SetLayersRecursively(this GameObject gameObject, int layer) {
            gameObject.layer = layer;
            gameObject.transform.ForEveryChild(child => child.gameObject.SetLayersRecursively(layer));
        }
        
        
        /// <summary>
        /// Tìm hoặc tạo một child GameObject với tên đã cho.
        /// </summary>
        /// <param name="parent">GameObject cha.</param>
        /// <param name="name">Tên của child GameObject cần tìm hoặc tạo.</param>
        /// <returns>Child GameObject đã tìm thấy hoặc mới tạo.</returns>
        public static GameObject FindOrCreateChild(this GameObject parent, string name)
        {
            Transform child = parent.transform.Find(name);
            if (child == null)
            {
                GameObject childObject = new GameObject(name);
                childObject.transform.SetParent(parent.transform, false);
                return childObject;
            }
            return child.gameObject;
        }

        /// <summary>
        /// Đặt layer cho GameObject và tất cả các con của nó.
        /// </summary>
        /// <param name="gameObject">GameObject gốc.</param>
        /// <param name="layerName">Tên của layer cần đặt.</param>
        public static void SetLayersRecursively(this GameObject gameObject, string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            gameObject.SetLayersRecursively(layer);
        }

        /// <summary>
        /// Tạo một bản sao của GameObject và đặt nó như một sibling.
        /// </summary>
        /// <param name="original">GameObject gốc cần sao chép.</param>
        /// <returns>Bản sao của GameObject.</returns>
        public static GameObject DuplicateAsSibling(this GameObject original)
        {
            GameObject copy = UnityEngine.Object.Instantiate(original, original.transform.parent);
            copy.name = original.name + " (Clone)";
            return copy;
        }
        
        
        /// <summary>
        /// Cho hai GameObject, trả về GameObject gốc chung (hoặc null).
        /// </summary>
        /// <param name="g1">GameObject để so sánh</param>
        /// <param name="g2">GameObject để so sánh</param>
        public static GameObject FindCommonRoot(this GameObject g1, GameObject g2)
        {
            if (g1 == null || g2 == null)
            {
                return null;
            }

            var t1 = g1.transform;

            while (t1 != null)
            {
                var t2 = g2.transform;

                while (t2 != null)
                {
                    if (t1 == t2)
                    {
                        return t1.gameObject;
                    }

                    t2 = t2.parent;
                }

                t1 = t1.parent;
            }

            return null;
        }
        
        /// <summary>
        /// Lấy một thành phần (Component) trên GameObject nếu nó đã được gắn vào.
        /// Nếu thành phần không tồn tại trên GameObject, nó sẽ được thêm vào và trả về.
        /// </summary>
        /// <typeparam name="T">Kiểu của thành phần để tìm kiếm trên GameObject.</typeparam>
        /// <param name="gameObject">Instance của GameObject.</param>
        /// <returns>Instance hiện có hoặc mới của thành phần.</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }

            return component;
        }
        
    }
}