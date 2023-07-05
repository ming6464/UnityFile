using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DontDestroyObject : MonoBehaviour
{
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }

    private void Start()
    {
        foreach (var format in System.Enum.GetValues(typeof(RenderTextureFormat)))
        {
            if (SystemInfo.SupportsRenderTextureFormat((RenderTextureFormat)format))
            {
                Debug.Log("Supported RenderTextureFormat: " + format.ToString());
            }
        }
    }
}
