using System;

/// <summary>
/// Cung cấp các phương thức kiểm tra điều kiện tiên quyết cho các đối số và trạng thái.
/// </summary>
public class Preconditions {
    /// <summary>
    /// Constructor riêng tư để ngăn việc tạo instance.
    /// </summary>
    private Preconditions() { }
    
    /// <summary>
    /// Kiểm tra xem một tham chiếu có null hay không.
    /// </summary>
    /// <typeparam name="T">Kiểu của tham chiếu cần kiểm tra.</typeparam>
    /// <param name="reference">Tham chiếu cần kiểm tra.</param>
    /// <returns>Tham chiếu nếu nó không null.</returns>
    /// <exception cref="ArgumentNullException">Nếu tham chiếu là null.</exception>
    public static T CheckNotNull<T>(T reference) {
        return CheckNotNull(reference, null);
    }

    /// <summary>
    /// Kiểm tra xem một tham chiếu có null hay không, với một thông báo tùy chỉnh.
    /// </summary>
    /// <typeparam name="T">Kiểu của tham chiếu cần kiểm tra.</typeparam>
    /// <param name="reference">Tham chiếu cần kiểm tra.</param>
    /// <param name="message">Thông báo lỗi tùy chỉnh.</param>
    /// <returns>Tham chiếu nếu nó không null.</returns>
    /// <exception cref="ArgumentNullException">Nếu tham chiếu là null.</exception>
    public static T CheckNotNull<T>(T reference, string message) {
        if (reference is UnityEngine.Object obj && obj == null) {
            throw new ArgumentNullException(message);
        }
        if (reference is null) {
            throw new ArgumentNullException(message);
        }
        return reference;
    }

    /// <summary>
    /// Kiểm tra một biểu thức boolean.
    /// </summary>
    /// <param name="expression">Biểu thức boolean cần kiểm tra.</param>
    /// <exception cref="InvalidOperationException">Nếu biểu thức là false.</exception>
    public static void CheckState(bool expression) {
        CheckState(expression, null);
    }

    /// <summary>
    /// Kiểm tra một biểu thức boolean với một thông báo lỗi được định dạng.
    /// </summary>
    /// <param name="expression">Biểu thức boolean cần kiểm tra.</param>
    /// <param name="messageTemplate">Mẫu thông báo lỗi.</param>
    /// <param name="messageArgs">Các đối số cho mẫu thông báo lỗi.</param>
    /// <exception cref="InvalidOperationException">Nếu biểu thức là false.</exception>
    public static void CheckState(bool expression, string messageTemplate, params object[] messageArgs) {
        CheckState(expression, string.Format(messageTemplate, messageArgs));
    }

    /// <summary>
    /// Kiểm tra một biểu thức boolean với một thông báo lỗi tùy chỉnh.
    /// </summary>
    /// <param name="expression">Biểu thức boolean cần kiểm tra.</param>
    /// <param name="message">Thông báo lỗi tùy chỉnh.</param>
    /// <exception cref="InvalidOperationException">Nếu biểu thức là false.</exception>
    public static void CheckState(bool expression, string message) {
        if (expression) {
            return;
        }

        throw message == null ? new InvalidOperationException() : new InvalidOperationException(message);
    }
}