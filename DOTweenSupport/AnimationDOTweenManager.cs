using System;
using DG.Tweening;
using UnityEngine;

namespace ComponentUtilitys
{
    public class AnimationDOTweenManager : MonoBehaviour
    {
        [SerializeField]
        public AnimationDOTweenData[] _animations;
    }

    [Serializable]
    public struct AnimationDOTweenData
    {
        public DOTweenAnimation animation;
    } 
}