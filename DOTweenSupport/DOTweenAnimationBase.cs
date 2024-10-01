using System;
using System.ComponentModel;
using DG.Tweening;
using UnityEngine;
using VInspector;

namespace ComponentUtilitys.DOTweenSupport
{
    public class DOTweenAnimationBase : MonoBehaviour
    {
        [SerializeField] protected GameObject _target;
        [DrawGreenDividingLine(3,5)]
        [SerializeField,Tooltip("Hoạt ảnh Sẽ được play mỗi khi enable")] protected bool     _playAwake     = false;
        [SerializeField, Tooltip("Hoạt ảnh sẽ tự động kill khi complete")] protected bool     _autoKill        = false;
        [DrawGreenDividingLine(3,5)]
        [SerializeField] protected float    _duration        = 1;
        [SerializeField] protected float    _delay           = 0;
        [SerializeField] protected bool     _ignoreTimeScale = false;
        [SerializeField] protected Ease     _easeType        = Ease.OutQuad;
        [SerializeField] protected LoopType _loopType        = LoopType.Restart;
        [SerializeField] protected int      _loopCount       = 1;
    }
}
