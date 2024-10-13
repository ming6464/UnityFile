using UnityEngine;
using UnityEngine.Events;
using VInspector;

namespace ComponentUtilitys
{
    public class ToggleEvent : ToggleBase
    {
        [Tab("Toggle Event")]
        [SerializeField, Variants("Unity Event","Event Dispatcher")]
        private string _actionType;
    
        [ShowIf("_actionType", "Unity Event")]
        [SerializeField]
        private UnityEvent _eventOn;
        [SerializeField]
        private UnityEvent _eventOff;
    
        [ShowIf("_actionType", "Event Dispatcher")]
        [SerializeField]
        private DispatcherEventInfo _eventOnInfo;
        [SerializeField]
        private DispatcherEventInfo _eventOffInfo;
        [EndIf]
        
        [EndTab]
        private void HandleEventDispatcher(bool state)
        {
            if (state)
            {
                _eventOnInfo.PostEvent();
            }
            else
            {
                _eventOffInfo.PostEvent();
            }
        }
    
        private void HandleEventUnity(bool state)
        {
            if (state)
            {
                _eventOn?.Invoke();   
            }
            else
            {
                _eventOff?.Invoke();
            }
        }
    
        public override void OnChangeValue<T>(T value)
        {
            if(value is not bool boolValue) return;
            if (_actionType.Equals("Unity Event"))
            {
                HandleEventUnity(boolValue);
            }
            else
            {
                HandleEventDispatcher(boolValue);
            }
        }
    }
}

