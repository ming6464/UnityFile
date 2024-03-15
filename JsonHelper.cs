using System;
using System.Collections.Generic;
using UnityEngine;

public class JsonHelper
{
    public static string ToJson<T>(T obj)
    {
        if (obj == null)
        {
            return "";
        }

        return JsonUtility.ToJson(obj);
    }

    public static string ToJson<T>(List<T> array)
    {
        if (array.Count == 0)
        {
            return null;
        }

        Wrapper<T> wrapper = new();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array)
    {
        if (array == null)
        {
            return null;
        }

        Wrapper<T> wrapper = new();
        wrapper.ArrItem = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static T FromJson<T>(string json)
    {
        return JsonUtility.FromJson<T>(json);
    }


    public static List<T> FromJsonList<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new List<T>();
        }

        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static T[] FromJsonArray<T>(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            return new T[0];
        }

        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
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
    public static T CloneObjectViaJson<T>(T source)
    {
        return JsonHelper.FromJson<T>(JsonHelper.ToJson(source));
    }

    public static List<T> CloneListViaJson<T>(List<T> source)
    {
        return JsonHelper.FromJsonList<T>(JsonHelper.ToJson(source));
    }

    public static T[] CloneArrayViaJson<T>(T[] source)
    {
        return JsonHelper.FromJsonArray<T>(JsonHelper.ToJson(source));
    }
}