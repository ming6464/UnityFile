using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

public class ListJsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<T>();
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }
    public static string ToJson<T>(List<T> array)
    {
        if (array.Count == 0) return null;
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return UnityEngine.JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array)
    {
        if (array == null) return null;
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.ArrItem = array;
        return UnityEngine.JsonUtility.ToJson(wrapper);
    }

    public static T[] FromJsonArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return new T[0];
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.ArrItem;
    }
    
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
        public T[] ArrItem;
    }
}