using System;
using UnityEngine;
using UnityEngine.Events;

namespace ComponentUtilitys
{
    public class CatchEvenAction : MonoBehaviour
    {
        [SerializeField]
        private EventID _eventIDCatch;
        [SerializeField]
        private EventPostValue _eventValueCompare;
        [SerializeField]
        private UnityEvent _event;
        private void OnEnable()
        {
            EventDispatcher.Instance.RegisterListener(_eventIDCatch,OnCatchEvent);
        }

        private void OnDisable()
        {
            _event = null;
            EventDispatcher.Instance.RemoveListener(_eventIDCatch,OnCatchEvent);
        }

        private void OnCatchEvent(object obj)
        {
            bool isCatch = true;

            if (_eventValueCompare.valuePostType != PrimitiveDataType.Default)
            {
                try
                {
                    switch (_eventValueCompare.valuePostType)
                    {
                        case PrimitiveDataType.Int:
                            var objInt       = (int)obj;
                            isCatch = objInt.Equals(_eventValueCompare.@int);
                            break;
                        case PrimitiveDataType.Float:
                            var objFloat       = (float)obj;
                            isCatch = objFloat.Equals(_eventValueCompare.@float);
                            break;
                        case PrimitiveDataType.String:
                            var objString       = (string)obj;
                            isCatch = objString.Equals(_eventValueCompare.@string);
                            break;
                        case PrimitiveDataType.Bool:
                            var objBool       = (bool)obj;
                            isCatch = objBool.Equals(_eventValueCompare.@bool);
                            break;
                    }
                }
                catch (Exception)
                {
                    Debug.Log("Ép kiểu lỗi");
                    isCatch = false;
                }
            }
            if(!isCatch) return;
            _event?.Invoke();
        }
    }
}