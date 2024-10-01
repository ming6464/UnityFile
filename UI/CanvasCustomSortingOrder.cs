using System;
using UnityEngine;
using VInspector;

namespace ComponentUtilitys
{
    public class CanvasCustomSortingOrder : MonoBehaviour
    {
        [Foldout("Reference")]
        [SerializeField]
        private Canvas _canvas;
        
        [Foldout("Value Info")]
        [SerializeField]
        private LayerMask _sortingLayer;

        [SerializeField]
        private int _order;

        private void Start()
        {
            int layerIndex = Mathf.RoundToInt(Mathf.Log(_sortingLayer.value, 2));
            _canvas.sortingLayerID = layerIndex;
            _canvas.sortingOrder   = _order;
        }
    }
}