using System;
using UnityEngine;
using UnityEngine.UI;
using VInspector;

namespace ComponentUtilitys
{
    public abstract class ToggleBase : MonoBehaviour,IOnChangeValue
    {
        [Tab("Base")]
        [SerializeField]
        protected bool _addEventOnCode;
        [SerializeField,ShowIf("_addEventOnCode",true)]
        protected Toggle _toggle;

        [EndIf]
        [EndTab]

        private void OnValidate()
        {
            if (_addEventOnCode && !_toggle)
            {
                _toggle = GetComponent<Toggle>();
            }
        }

        protected virtual void OnEnable()
        {
            if (_addEventOnCode)
            {
                _toggle.onValueChanged.AddListener(value => OnChangeValue(value));
            }
        }

        protected virtual void OnDisable()
        {
            if (_addEventOnCode)
            {
                _toggle.onValueChanged.RemoveListener(value => OnChangeValue(value));
            }
        }

        public abstract void OnChangeValue<T>(T value);
    }
}