using System.Linq;
using UnityEngine;

public class NonInteractableZones : MonoBehaviour
{
    [Header("Non-Interactable UI Elements")]
    public RectTransform[] nonInteractableUIElements;

    public bool IsPointerOverNonInteractableZone(Vector2 pointerPosition)
    {
        return nonInteractableUIElements.Any(uiElement => RectTransformUtility.RectangleContainsScreenPoint(uiElement, pointerPosition));
    }
}