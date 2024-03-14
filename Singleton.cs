using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    // private static instance
    static T instance;

    // public static instance used to refer to Singleton (e.g. MyClass.Instance)
    public static T Instance
    {
        get
        {
            // if no instance is found, find the first GameObject of type T
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType<T>();

                // if no instance exists in the Scene, create a new GameObject and add the Component T 
                if (instance == null)
                {
                    GameObject singleton = new GameObject(typeof(T).Name);
                    instance = singleton.AddComponent<T>();
                }
            }
            // return the singleton instance
            return instance;
        }
    }

    public virtual void Awake()
    {
        MakeSingleton();
    }

    public void MakeSingleton()
    {
        if (instance == null)
        {
            instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
