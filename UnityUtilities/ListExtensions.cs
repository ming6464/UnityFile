using System;
using System.Collections.Generic;
using System.Linq;
using VInspector.Libs;

namespace UnityUtils {
    public static class ListExtensions {
        static Random rng;
        
        /// <summary>
        /// Xác định xem một tập hợp có null hoặc không có phần tử nào không
        /// mà không cần phải đếm toàn bộ tập hợp để lấy số lượng.
        ///
        /// Sử dụng phương thức Any() của LINQ để xác định xem tập hợp có rỗng không,
        /// vì vậy có một số chi phí GC.
        /// </summary>
        /// <param name="list">Danh sách cần đánh giá</param>
        public static bool IsNullOrEmpty<T>(this IList<T> list) {
            return list == null || !list.Any();
        }

        /// <summary>
        /// Tạo một danh sách mới là bản sao của danh sách gốc.
        /// </summary>
        /// <param name="list">Danh sách gốc cần được sao chép.</param>
        /// <returns>Một danh sách mới là bản sao của danh sách gốc.</returns>
        public static List<T> Clone<T>(this IList<T> list) {
            List<T> newList = new List<T>();
            foreach (T item in list) {
                newList.Add(item);
            }

            return newList;
        }

        /// <summary>
        /// Hoán đổi hai phần tử trong danh sách tại các chỉ số được chỉ định.
        /// </summary>
        /// <param name="list">Danh sách.</param>
        /// <param name="indexA">Chỉ số của phần tử thứ nhất.</param>
        /// <param name="indexB">Chỉ số của phần tử thứ hai.</param>
        public static void Swap<T>(this IList<T> list, int indexA, int indexB) {
            (list[indexA], list[indexB]) = (list[indexB], list[indexA]);
        }

        /// <summary>
        /// Xáo trộn các phần tử trong danh sách sử dụng thuật toán Fisher-Yates với cài đặt Durstenfeld.
        /// Phương thức này sửa đổi danh sách đầu vào trực tiếp, đảm bảo mỗi hoán vị có xác suất như nhau, và trả về danh sách để chuỗi phương thức.
        /// </summary>
        /// <param name="list">Danh sách cần được xáo trộn.</param>
        /// <typeparam name="T">Kiểu của các phần tử trong danh sách.</typeparam>
        /// <returns>Danh sách đã được xáo trộn.</returns>
        public static IList<T> Shuffle<T>(this IList<T> list) {
            if (rng == null) rng = new Random();
            int count = list.Count;
            while (count > 1) {
                --count;
                var index = rng.Next(count + 1);
                list.Swap(index, count);
            }
            return list;
        }
        /// <summary>
        /// Lấy một phần tử ngẫu nhiên từ danh sách.
        /// </summary>
        /// <typeparam name="T">Kiểu của các phần tử trong danh sách.</typeparam>
        /// <param name="list">Danh sách đầu vào.</param>
        /// <returns>Một phần tử ngẫu nhiên từ danh sách.</returns>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default(T);
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Xóa và trả về phần tử cuối cùng của danh sách.
        /// </summary>
        /// <typeparam name="T">Kiểu của các phần tử trong danh sách.</typeparam>
        /// <param name="list">Danh sách đầu vào.</param>
        /// <returns>Phần tử cuối cùng đã bị xóa.</returns>
        public static T PopLast<T>(this IList<T> list)
        {
            int[] a = new int[10];
            if (list == null || list.Count == 0) throw new System.InvalidOperationException("List is empty");
            T item = list[^1];
            list.RemoveAt(list.Count - 1);
            return item;
        }
        
        
        /// <summary>
        /// Xóa và trả về phần tử đầu tiên của một danh sách.
        /// 
        /// Nếu danh sách rỗng, một ngoại lệ `InvalidOperationException` sẽ được ném ra.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của các phần tử trong danh sách.</typeparam>
        /// <param name="list">Danh sách cần thao tác.</param>
        /// <returns>Phần tử đầu tiên đã bị xóa.</returns>
        public static T PopFirst<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Danh sách không thể rỗng.");

            T item = list[0];
            list.RemoveAt(0);
            return item;
        }
        
        /// <summary>
        /// Xóa tất cả các phần tử null trong danh sách.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của các phần tử trong danh sách.</typeparam>
        /// <param name="list">Danh sách cần xóa các phần tử null.</param>
        public static void RemoveAllNull<T>(this List<T> list) where T : class
        {
            list.RemoveAll(item => item == null);
        }

        /// <summary>
        /// Xóa các phần tử có index nằm trong danh sách các index cần xóa.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của các phần tử trong danh sách.</typeparam>
        /// <param name="list">Danh sách cần xóa các phần tử.</param>
        /// <param name="indicesToRemove">Danh sách các index cần xóa.</param>
        public static void RemoveAtIndices<T>(this List<T> list, List<int> indicesToRemove)
        {
            indicesToRemove.Sort();
            for (int i = indicesToRemove.Count - 1; i >= 0; i--)
            {
                list.RemoveAt(indicesToRemove[i]);
            }
        }
        
        /// <summary>
        /// Xóa các phần tử trong danh sách hiện tại có mặt trong danh sách các phần tử cần xóa.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của các phần tử trong danh sách.</typeparam>
        /// <param name="list">Danh sách cần xóa các phần tử.</param>
        /// <param name="itemsToRemove">Danh sách các phần tử cần xóa.</param>
        public static void RemoveAll<T>(this List<T> list, IEnumerable<T> itemsToRemove)
        {
            list.RemoveAll(item => itemsToRemove.Contains(item));
        }
        
        /// <summary>
        /// Thay thế tất cả các phần tử cũ bằng phần tử mới trong danh sách.
        /// </summary>
        /// <typeparam name="T">Kiểu dữ liệu của các phần tử trong danh sách.</typeparam>
        /// <param name="list">Danh sách cần thay thế.</param>
        /// <param name="oldValue">Giá trị cần thay thế.</param>
        /// <param name="newValue">Giá trị thay thế mới.</param>
        public static void ReplaceAll<T>(this List<T> list, T oldValue, T newValue)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Equals(oldValue))
                {
                    list[i] = newValue;
                }
            }
        }
        
    }
}