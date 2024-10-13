using System;
using UnityEngine;
using UnityEngine.Events;
using VInspector;

namespace ComponentUtilitys
{
    public class ButtonEvent : ButtonBase
    {
            [Tab("Button Event")]
            [SerializeField, Variants("Unity Event","Event Dispatcher")]
            private string _actionType;
            
            //One Way
            [ShowIf("_actionType", "Unity Event","_buttonTypeWay","One Way")]
            [SerializeField]
            private UnityEvent _event;
            [ShowIf("_actionType", "Event Dispatcher","_buttonTypeWay","One Way")]
            [SerializeField]
            private DispatcherEventInfo _eventInfo;
            //Two Way
            [ShowIf("_actionType", "Unity Event","_buttonTypeWay","Two Way")]
            [SerializeField]
            private UnityEvent _eventOn;
            [SerializeField]
            private UnityEvent _eventOff;

            [ShowIf("_actionType", "Event Dispatcher","_buttonTypeWay","Two Way")]
            [SerializeField]
            private DispatcherEventInfo _eventOnInfo;
            [SerializeField]
            private DispatcherEventInfo _eventOffInfo;
            [EndIf]
            //
            [EndTab]
            private void HandleEventDispatcher(bool state = false)
            {
                if (_buttonTypeWay.Equals("Two Way"))
                {
                    if (state)
                    {
                        _eventOnInfo.PostEvent();
                    }
                    else
                    {
                        _eventOffInfo.PostEvent();
                    }
                    return;
                }

                _eventInfo.PostEvent();
                
            }
        
            private void HandleEventUnity(bool state = false)
            {
                var eventAction = _event;

                if (_buttonTypeWay.Equals("Two Way"))
                {
                    eventAction = state ? _eventOn : _eventOff;
                }
                eventAction?.Invoke();
            }

            public override void OnClick()
            {
                if (_actionType.Equals("Unity Event"))
                {
                    HandleEventUnity();
                }
                else
                {
                    HandleEventDispatcher();
                }
            }

            public override void OnChangeValue<T>(T value)
            {
                if (value is not bool boolValue) return;
                if (_actionType.Equals("Unity Event"))
                {
                    HandleEventUnity(boolValue);
                }
                else
                {
                    HandleEventDispatcher(boolValue);
                }
            }

            public void OnSetValue(bool state)
            {
                _currentState = state;
                OnChangeValue(_currentState);
            }
        
    }


    [Serializable]
    public struct DispatcherEventInfo
    {
        public EventID        eventID;
        public EventPostValue eventValue;
        
        public bool PostEvent()
        {
            if (eventID == EventID.None) return false;
            EventDispatcher.Instance.PostEvent(eventID,eventValue.GetValuePost());
            return true;
        }
    }

    [Serializable]
    public struct EventPostValue
    {
        public PrimitiveDataType valuePostType;
        public int @int;
        public float @float;
        public string @string;
        public bool @bool;

        public object GetValuePost()
        {
            object value = valuePostType switch
            {
                    PrimitiveDataType.Int => @int,
                    PrimitiveDataType.Float => @float,
                    PrimitiveDataType.String => @string,
                    PrimitiveDataType.Bool => @bool,
                    _                     => null
            };

            return value;
        }
    }
}