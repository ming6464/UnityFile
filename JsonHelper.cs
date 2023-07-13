using System;
using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
    public static string ToJson<T>(T obj)
    {
        if (obj == null) return "";
        return UnityEngine.JsonUtility.ToJson(obj);
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

    public static T FromJson<T>(string json)
    {
        T obj = UnityEngine.JsonUtility.FromJson<T>(json);
        return obj;
    }

    public static List<T> FromJsonList<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<T>();
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
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