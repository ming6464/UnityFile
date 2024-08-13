using UnityEngine;

namespace UnityUtils {
    #region Vector3

    public static class Vector3Extensions {
        /// <summary>
        /// Thiết lập các giá trị x, y, z của một Vector3.
        /// </summary>
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null) {
            return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }

        /// <summary>
        /// Cộng thêm giá trị vào các thành phần x, y, z của một Vector3.
        /// </summary>
        public static Vector3 Add(this Vector3 vector, float x = 0, float y = 0, float z = 0) {
            return new Vector3(vector.x + x, vector.y + y, vector.z + z);

        }

        /// <summary>
        /// Kiểm tra xem một Vector3 có nằm trong phạm vi của một Vector3 khác hay không.
        /// </summary>
        /// <param name="current">Vị trí Vector3 hiện tại.</param>
        /// <param name="target">Vị trí Vector3 mục tiêu.</param>
        /// <param name="range">Khoảng cách tối đa.</param>
        /// <returns>True nếu Vector3 hiện tại nằm trong phạm vi của Vector3 mục tiêu, false nếu không.</returns>
        public static bool InRangeOf(this Vector3 current, Vector3 target, float range) {
            return (current - target).sqrMagnitude <= range * range;
        }

        /// <summary>
        /// Chia từng thành phần của hai Vector3.
        /// </summary>
        /// <remarks>
        /// Mỗi thành phần của v0 (x, y, z) được chia cho thành phần tương ứng của v1 nếu thành phần đó của v1 khác 0.
        /// Nếu không, thành phần của v0 giữ nguyên.
        /// </remarks>
        /// <example>
        /// Sử dụng 'ComponentDivide' để thay đổi kích thước của một đối tượng theo tỷ lệ:
        /// <code>
        /// myObject.transform.localScale = originalScale.ComponentDivide(targetDimensions);
        /// </code>
        /// Điều này sẽ thay đổi kích thước của đối tượng để phù hợp với kích thước mục tiêu trong khi giữ nguyên tỷ lệ ban đầu.
        ///</example>
        /// <param name="v0">Đối tượng Vector3 được mở rộng.</param>
        /// <param name="v1">Đối tượng Vector3 để chia v0.</param>
        /// <returns>Một đối tượng Vector3 mới là kết quả của phép chia từng thành phần.</returns>
        public static Vector3 ComponentDivide(this Vector3 v0, Vector3 v1) {
            return new Vector3(
                v1.x != 0 ? v0.x / v1.x : v0.x,
                v1.y != 0 ? v0.y / v1.y : v0.y,
                v1.z != 0 ? v0.z / v1.z : v0.z);
        }

        /// <summary>
        /// Nhân từng thành phần của hai Vector3 với nhau.
        /// </summary>
        /// <param name="a">Vector3 đầu tiên.</param>
        /// <param name="b">Vector3 thứ hai.</param>
        /// <returns>Vector3 mới là kết quả của phép nhân từng thành phần.</returns>
        public static Vector3 ComponentMultiply(this Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        /// <summary>
        /// Chuyển đổi một Vector2 thành một Vector3 với giá trị y là 0.
        /// </summary>
        /// <param name="v2">Vector2 cần chuyển đổi.</param>
        /// <returns>Một Vector3 với giá trị x và z của Vector2 và giá trị y là 0.</returns>
        public static Vector3 ToVector3(this Vector2 v2) {
            return new Vector3(v2.x, 0, v2.y);
        }

        /// <summary>
        /// Tính toán một điểm ngẫu nhiên trong một vòng tròn rỗng (annulus) dựa trên bán kính tối thiểu và tối đa xung quanh một điểm trung tâm Vector3 (origin).
        /// </summary>
        /// <param name="origin">Điểm trung tâm Vector3 của vòng tròn rỗng.</param>
        /// <param name="minRadius">Bán kính tối thiểu của vòng tròn rỗng.</param>
        /// <param name="maxRadius">Bán kính tối đa của vòng tròn rỗng.</param>
        /// <returns>Một điểm Vector3 ngẫu nhiên trong vòng tròn rỗng đã chỉ định.</returns>
        public static Vector3 RandomPointInAnnulus(this Vector3 origin, float minRadius, float maxRadius) {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Bình phương và sau đó căn bậc hai các bán kính để đảm bảo phân bố đều trong vòng tròn rỗng
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Chuyển đổi vector hướng 2D thành vector vị trí 3D
            Vector3 position = new Vector3(direction.x, 0, direction.y) * distance;
            return origin + position;
        }
        
        
        /// <summary>
        /// Tính toán điểm giữa của hai vector.
        /// </summary>
        /// <param name="start">Vector bắt đầu.</param>
        /// <param name="end">Vector kết thúc.</param>
        /// <returns>Điểm giữa của hai vector.</returns>
        public static Vector3 Midpoint(this Vector3 start, Vector3 end)
        {
            return (start + end) * 0.5f;
        }

        /// <summary>
        /// Xoay một vector quanh một trục.
        /// </summary>
        /// <param name="v">Vector cần xoay.</param>
        /// <param name="axis">Trục xoay.</param>
        /// <param name="angle">Góc xoay (độ).</param>
        /// <returns>Vector đã xoay.</returns>
        public static Vector3 RotateAround(this Vector3 v, Vector3 axis, float angle)
        {
            return Quaternion.AngleAxis(angle, axis) * v;
        }

        /// <summary>
        /// Tạo một vector ngẫu nhiên trong phạm vi cho trước.
        /// </summary>
        /// <param name="min">Giá trị tối thiểu cho mỗi thành phần.</param>
        /// <param name="max">Giá trị tối đa cho mỗi thành phần.</param>
        /// <returns>Vector ngẫu nhiên.</returns>
        public static Vector3 RandomRange(this Vector3 min, Vector3 max)
        {
            var a = min - max;
            return new Vector3(
                    Random.Range(min.x, max.x),
                    Random.Range(min.y, max.y),
                    Random.Range(min.z, max.z)
            );
        }
        
        /// <summary>
        /// Tạo một Vector3 với độ dài cho trước theo hướng của vector hiện tại.
        /// </summary>
        /// <param name="v">Vector3 gốc.</param>
        /// <param name="length">Độ dài mong muốn.</param>
        /// <returns>Vector3 mới với độ dài đã cho.</returns>
        public static Vector3 WithMagnitude(this Vector3 v, float length)
        {
            return v.normalized * length;
        }
        
        
        /// <summary>
        /// Chuyển đổi Vector3 thành Vector2 với giá trị z = 0.
        /// </summary>
        /// <param name="v">Vector3 cần chuyển đổi.</param>
        /// <returns>Vector3 tương ứng.</returns>
        public static Vector2 ToVector2(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
        }
        
        
        /// <summary>
        /// Kiểm tra xem hai Vector3 có gần bằng nhau hay không, 
        /// sử dụng một ngưỡng sai số nhỏ để so sánh các thành phần.
        /// </summary>
        /// <param name="a">Vector3 đầu tiên.</param>
        /// <param name="b">Vector3 thứ hai.</param>
        /// <returns>True nếu hai vector gần bằng nhau, ngược lại là false.</returns>
        public static bool Approximately(this Vector3 a, Vector3 b)
        {
            return Mathf.Approximately(a.x, b.x) && 
                   Mathf.Approximately(a.y, b.y) &&
                   Mathf.Approximately(a.z,   
                           b.z);
        }
    }

    #endregion

    #region Vector2

    public static class Vector2Extensions {
        /// <summary>
        /// Cộng thêm giá trị vào các thành phần x, y của một Vector2.
        /// </summary>
        public static Vector2 Add(this Vector2 vector2, float x = 0, float y = 0) {
            return new Vector2(vector2.x + x, vector2.y + y);
        }

        /// <summary>
        /// Thiết lập các giá trị x, y của một Vector2.
        /// </summary>
        public static Vector2 With(this Vector2 vector2, float? x = null, float? y = null) {
            return new Vector2(x ?? vector2.x, y ?? vector2.y);
        }

        /// <summary>
        /// Kiểm tra xem một Vector2 có nằm trong phạm vi của một Vector2 khác hay không.
        /// </summary>
        /// <param name="current">Vị trí Vector2 hiện tại.</param>
        /// <param name="target">Vị trí Vector2 mục tiêu.</param>
        /// <param name="range">Khoảng cách tối đa.</param>
        /// <returns>True nếu Vector2 hiện tại nằm trong phạm vi của Vector2 mục tiêu, false nếu không.</returns>
        public static bool InRangeOf(this Vector2 current, Vector2 target, float range) {
            return (current - target).sqrMagnitude <= range * range;
        }
        
        /// <summary>
        /// Kiểm tra xem hai Vector2 có gần bằng nhau hay không, 
        /// sử dụng một ngưỡng sai số nhỏ để so sánh các thành phần.
        /// </summary>
        /// <param name="a">Vector2 đầu tiên.</param>
        /// <param name="b">Vector2 thứ hai.</param>
        /// <returns>True nếu hai vector gần bằng nhau, ngược lại là false.</returns>
        public static bool Approximately(this Vector2 a, Vector2 b)
        {
            return Mathf.Approximately(a.x, b.x) && Mathf.Approximately(a.y, b.y);
        }
        
        /// <summary>
        /// Nhân từng thành phần của hai Vector2 với nhau.
        /// </summary>
        /// <param name="a">Vector2 đầu tiên.</param>
        /// <param name="b">Vector2 thứ hai.</param>
        /// <returns>Vector2 mới là kết quả của phép nhân từng thành phần.</returns>
        public static Vector2 ComponentMultiply(this Vector2 a, Vector2 b)
        {
            return new(a.x * b.x, a.y * b.y);
        }

        /// <summary>
        /// Tính toán một điểm ngẫu nhiên trong một vòng tròn rỗng (annulus) dựa trên bán kính tối thiểu và tối đa xung quanh một điểm trung tâm Vector2 (origin).
        /// </summary>
        /// <param name="origin">Điểm trung tâm Vector2 của vòng tròn rỗng.</param>
        /// <param name="minRadius">Bán kính tối thiểu của vòng tròn rỗng.</param>
        /// <param name="maxRadius">Bán kính tối đa của vòng tròn rỗng.</param>
        /// <returns>Một điểm Vector2 ngẫu nhiên trong vòng tròn rỗng đã chỉ định.</returns>
        public static Vector2 RandomPointInAnnulus(this Vector2 origin, float minRadius, float maxRadius) {
            float angle = Random.value * Mathf.PI * 2f;
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

            // Bình phương và sau đó căn bậc hai các bán kính để đảm bảo phân bố đều trong vòng tròn rỗng
            float minRadiusSquared = minRadius * minRadius;
            float maxRadiusSquared = maxRadius * maxRadius;
            float distance = Mathf.Sqrt(Random.value * (maxRadiusSquared - minRadiusSquared) + minRadiusSquared);

            // Tính toán vector vị trí
            Vector2 position = direction * distance;
            return origin + position;
        }
        
        /// <summary>
        /// Tạo một vector ngẫu nhiên trong phạm vi cho trước.
        /// </summary>
        /// <param name="min">Giá trị tối thiểu cho mỗi thành phần.</param>
        /// <param name="max">Giá trị tối đa cho mỗi thành phần.</param>
        /// <returns>Vector ngẫu nhiên.</returns>
        public static Vector2 RandomRange(this Vector2 min, Vector2 max)
        {
            return new Vector2(
                    Random.Range(min.x, max.x),
                    Random.Range(min.y, max.y)
            );
        }
        
        
        /// <summary>
        /// Chuyển đổi Vector2 thành Vector3 với giá trị z cho trước.
        /// </summary>
        /// <param name="v">Vector2 cần chuyển đổi.</param>
        /// <param name="z">Giá trị z cho Vector3 kết quả.</param>
        /// <returns>Vector3 tương ứng.</returns>
        public static Vector3 ToVector3(this Vector2 v, float z = 0f)
        {
            return new Vector3(v.x, v.y, z);
        }

        /// <summary>
        /// Tạo một Vector2 với độ dài cho trước theo hướng của vector hiện tại.
        /// </summary>
        /// <param name="v">Vector2 gốc.</param>
        /// <param name="length">Độ dài mong muốn.</param>
        /// <returns>Vector2 mới với độ dài đã cho.</returns>
        public static Vector2 WithMagnitude(this Vector2 v, float length)
        {
            return v.normalized * length;
        }
        
        /// <summary>
        /// Tính toán điểm giữa của hai vector.
        /// </summary>
        /// <param name="start">Vector bắt đầu.</param>
        /// <param name="end">Vector kết thúc.</param>
        /// <returns>Điểm giữa của hai vector.</returns>
        public static Vector2 Midpoint(this Vector2 start, Vector2 end)
        {
            return (start + end) * 0.5f;
        }

    }

    #endregion
}