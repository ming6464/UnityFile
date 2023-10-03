using System;
using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
    public static string ToJson<T>(T obj)
    {
        if (obj == null) return "";
        return JsonUtility.ToJson(obj);
    }

    public static string ToJson<T>(List<T> array)
    {
        if (array.Count == 0) return null;
        var wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array)
    {
        if (array == null) return null;
        var wrapper = new Wrapper<T>();
        wrapper.ArrItem = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static T FromJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }

    public static List<T> FromJsonList<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return new List<T>();
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static T[] FromJsonArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json)) return new T[0];
        var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.ArrItem;
    }


    [Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
        public T[] ArrItem;
    }
}

public static class JsonHelperExtension
{
    public static T CloneViaJson<T>(T source)
    {
        return JsonHelper.FromJson<T>(JsonHelper.ToJson(source));
    }
}