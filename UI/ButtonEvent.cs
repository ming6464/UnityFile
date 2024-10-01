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
            private EventID _eventID;
            [SerializeField]
            private EventPostValue _eventValue;
            //Two Way
            [ShowIf("_actionType", "Unity Event","_buttonTypeWay","Two Way")]
            [SerializeField]
            private UnityEvent _eventOn;
            [SerializeField]
            private UnityEvent _eventOff;

            [ShowIf("_actionType", "Event Dispatcher","_buttonTypeWay","Two Way")]
            [SerializeField]
            private EventID _eventIDOn;
            [SerializeField]
            private EventPostValue _eventValueOn;
            [SerializeField]
            private EventID _eventIDOff;
            [SerializeField]
            private EventPostValue _eventValueOff;
            [EndIf]
            //
            [EndTab]
            private void HandleEventDispatcher(bool state = false)
            {
                var eventId   = _eventID;
                var eventInfo = _eventValue;
                if (_buttonTypeWay.Equals("Two Way"))
                {
                    if (state)
                    {
                        eventId   = _eventIDOn;
                        eventInfo = _eventValueOn;
                    }
                    else
                    {
                        eventId   = _eventIDOff;
                        eventInfo = _eventValueOff; 
                    }
                }
                if(eventId == EventID.None) return;
                EventDispatcher.Instance.PostEvent(eventId,eventInfo.GetValuePost());
                
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
    public class EventPostValue
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