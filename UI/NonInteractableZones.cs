using System.Linq;
using UnityEngine;

public class NonInteractableZones : MonoBehaviour
{
    public RectTransform[] nonInteractableUIElements;

    /// <summary>
    /// Kiểm tra xem vị trí con trỏ có nằm trong bất kỳ vùng không tương tác nào không.
    /// </summary>
    /// <param name="pointerPosition">Vị trí của con trỏ trên màn hình.</param>
    /// <returns>Trả về true nếu vị trí con trỏ nằm trong vùng không tương tác, ngược lại trả về false.</returns>
    public bool IsPointerOverNonInteractableZone(Vector2 pointerPosition)
    {
        return nonInteractableUIElements.Any(uiElement =>
                RectTransformUtility.RectangleContainsScreenPoint(uiElement, pointerPosition));
    }
}