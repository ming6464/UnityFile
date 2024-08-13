using System;
using System.Collections.Generic;

namespace UnityUtils {
    public static class EnumerableExtensions {
        /// <summary>
        /// Thực hiện một hành động trên mỗi phần tử trong chuỗi.
        /// </summary>
        /// <typeparam name="T">Kiểu của các phần tử trong chuỗi.</typeparam>
        /// <param name="sequence">Chuỗi để lặp qua.</param>
        /// <param name="action">Hành động để thực hiện trên mỗi phần tử.</param>    
        public static void ForEach<T>(this IEnumerable<T> sequence, Action<T> action) {
            foreach (var item in sequence) {
                action(item);
            }
        }
    }
}