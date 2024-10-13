using System;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VInspector;

namespace ComponentUtilitys
{
    [System.Serializable]
    public class PointerEvent : UnityEvent<Vector2>
    {
    }

    [System.Serializable]
    public class DragEvent : UnityEvent<Vector2, Vector2, Vector2>
    {
    }

    public class InputUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        // delegate
        public delegate void PointerAction(Vector2 position);

        public delegate void DragAction(Vector2 startPosition, Vector2 previousPosition, Vector2 currentPosition);

        public bool checkInteractableZone;

        [Foldout("Events")]
        public UnityEvent onPointerDown;

        public UnityEvent onPointerUp;
        public UnityEvent onDrag;

        [Foldout("Events with param")]
        public PointerEvent onPointerDownWithParam;

        public PointerEvent onPointerUpWithParam;
        public DragEvent    onDragWithParam;

        [Foldout("EventDispatcher")]
        public DispatcherEventInfo eventOnPointDown;

        public DispatcherEventInfo eventOnPointUp;
        public DispatcherEventInfo eventOnDrag;

        public PointerAction onPointerDownAction;
        public PointerAction onPointerUpAction;
        public DragAction    onDragAction;

        [EndFoldout]
        private Vector2 _startPosition;

        private Vector2                _previousPosition;
        private NonInteractableZones[] _nonInteractableZones;
        private int                    _pointerState;

        private void Start()
        {
            if (!checkInteractableZone)
                return;
            _nonInteractableZones = FindObjectsOfType<NonInteractableZones>();
            _pointerState         = -1;
        }

        /// <summary>
        /// Được gọi khi phát hiện sự kiện nhấn chuột.
        /// </summary>
        /// <param name="eventData">Dữ liệu sự kiện chuột.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!CheckInteractable(eventData.position))
            {
                return;
            }

            _pointerState     = 1;
            _startPosition    = eventData.position;
            _previousPosition = eventData.position;
            onPointerDown?.Invoke();
            onPointerDownWithParam?.Invoke(eventData.position);
            onPointerDownAction?.Invoke(eventData.position);
            eventOnPointDown.PostEvent();
        }

        /// <summary>
        /// Được gọi khi phát hiện sự kiện thả chuột.
        /// </summary>
        /// <param name="eventData">Dữ liệu sự kiện chuột.</param>
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!CheckInteractable(eventData.position))
            {
                return;
            }

            PointerUp(eventData);
        }

        /// <summary>
        /// Xử lý sự kiện thả chuột.
        /// </summary>
        /// <param name="eventData">Dữ liệu sự kiện chuột.</param>
        private void PointerUp(PointerEventData eventData)
        {
            Debug.Log("PointerUp");
            _pointerState = 3;
            onPointerUp?.Invoke();
            onPointerUpWithParam?.Invoke(eventData.position);
            onPointerUpAction?.Invoke(eventData.position);
            eventOnPointUp.PostEvent();
        }

        /// <summary>
        /// Được gọi khi phát hiện sự kiện kéo chuột.
        /// </summary>
        /// <param name="eventData">Dữ liệu sự kiện chuột.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (!CheckInteractable(eventData.position))
            {
                if (_pointerState is 1 or 2)
                {
                    PointerUp(eventData);
                }

                return;
            }
            Debug.Log("OnDrag");

            _pointerState = 2;
            var currentPosition = eventData.position;
            onDrag?.Invoke();
            onDragWithParam?.Invoke(_startPosition, _previousPosition, currentPosition);
            onDragAction?.Invoke(_startPosition, _previousPosition, currentPosition);
            _previousPosition = currentPosition;
            eventOnDrag.PostEvent();
        }

        /// <summary>
        /// Kiểm tra xem vị trí có tương tác được không.
        /// </summary>
        /// <param name="position">Vị trí cần kiểm tra.</param>
        /// <returns>True nếu tương tác được, ngược lại false.</returns>
        private bool CheckInteractable(Vector2 position)
        {
            var check = !checkInteractableZone || _nonInteractableZones == null
                                               || !_nonInteractableZones.Any(nonI =>
                                                          nonI.IsPointerOverNonInteractableZone(position));
            
            Debug.Log("CheckInteractable: " + check);
            
            return check;
        }

        /// <summary>
        /// Thiết lập hành động cho sự kiện nhấn chuột.
        /// </summary>
        /// <param name="action">Hành động nhấn chuột.</param>
        public void SetOnPointerDownAction(PointerAction action)
        {
            onPointerDownAction += action;
        }

        /// <summary>
        /// Thiết lập hành động cho sự kiện thả chuột.
        /// </summary>
        /// <param name="action">Hành động thả chuột.</param>
        public void SetOnPointerUpAction(PointerAction action)
        {
            onPointerUpAction += action;
        }

        /// <summary>
        /// Thiết lập hành động cho sự kiện kéo chuột.
        /// </summary>
        /// <param name="action">Hành động kéo chuột.</param>
        public void SetOnDragAction(DragAction action)
        {
            onDragAction += action;
        }

        /// <summary>
        /// Thiết lập UnityAction cho sự kiện nhấn chuột.
        /// </summary>
        /// <param name="action">UnityAction.</param>
        public void SetOnPointerDownAction(UnityAction action)
        {
            onPointerDown.AddListener(action);
        }

        /// <summary>
        /// Thiết lập UnityAction cho sự kiện thả chuột.
        /// </summary>
        /// <param name="action">UnityAction.</param>
        public void SetOnPointerUpAction(UnityAction action)
        {
            onPointerUp.AddListener(action);
        }

        /// <summary>
        /// Thiết lập UnityAction cho sự kiện kéo chuột.
        /// </summary>
        /// <param name="action">UnityAction.</param>
        public void SetOnDragAction(UnityAction action)
        {
            onDrag.AddListener(action);
        }

        /// <summary>
        /// Xóa hành động cho sự kiện nhấn chuột.
        /// </summary>
        /// <param name="action">Hành động nhấn chuột.</param>
        public void RemoveOnPointerDownAction(PointerAction action)
        {
            onPointerDownAction -= action;
        }

        /// <summary>
        /// Xóa hành động cho sự kiện thả chuột.
        /// </summary>
        /// <param name="action">Hành động thả chuột.</param>
        public void RemoveOnPointerUpAction(PointerAction action)
        {
            onPointerUpAction -= action;
        }

        /// <summary>
        /// Xóa hành động cho sự kiện kéo chuột.
        /// </summary>
        /// <param name="action">Hành động kéo chuột.</param>
        public void RemoveOnDragAction(DragAction action)
        {
            onDragAction -= action;
        }

        /// <summary>
        /// Xóa UnityAction cho sự kiện nhấn chuột.
        /// </summary>
        /// <param name="action">UnityAction.</param>
        public void RemoveOnPointerDownAction(UnityAction action)
        {
            onPointerDown.RemoveListener(action);
        }

        /// <summary>
        /// Xóa UnityAction cho sự kiện thả chuột.
        /// </summary>
        /// <param name="action">UnityAction.</param>
        public void RemoveOnPointerUpAction(UnityAction action)
        {
            onPointerUp.RemoveListener(action);
        }

        /// <summary>
        /// Xóa UnityAction cho sự kiện kéo chuột.
        /// </summary>
        /// <param name="action">UnityAction.</param>
        public void RemoveOnDragAction(UnityAction action)
        {
            onDrag.RemoveListener(action);
        }

        /// <summary>
        /// Xóa tất cả hành động cho sự kiện nhấn chuột.
        /// </summary>
        public void RemoveAllPointerDownActions()
        {
            onPointerDownAction = null;
            onPointerDown.RemoveAllListeners();
        }

        /// <summary>
        /// Xóa tất cả hành động cho sự kiện thả chuột.
        /// </summary>
        public void RemoveAllPointerUpActions()
        {
            onPointerUpAction = null;
            onPointerUp.RemoveAllListeners();
        }

        /// <summary>
        /// Xóa tất cả hành động cho sự kiện kéo chuột.
        /// </summary>
        public void RemoveAllDragActions()
        {
            onDragAction = null;
            onDrag.RemoveAllListeners();
        }

        /// <summary>
        /// Xóa tất cả hành động cho tất cả sự kiện.
        /// </summary>
        public void RemoveAllActions()
        {
            RemoveAllPointerDownActions();
            RemoveAllPointerUpActions();
            RemoveAllDragActions();
        }
    }
}