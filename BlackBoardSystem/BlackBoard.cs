using System;
using System.Collections.Generic;

namespace BlackBoardSystem
{
   [Serializable]
   public class BlackBoard
   {
      private static BlackBoard                       _instance;
      public static BlackBoard Instance => _instance ??= new BlackBoard();
      
      private Dictionary<string, BlackBoardKey> _keyRegistry = new();
      private Dictionary<BlackBoardKey, object> _entities   = new();

      public BlackBoardKey GetOrRegisterKey(string keyName)
      {
         Preconditions.CheckNotNull(keyName);

         if (!_keyRegistry.TryGetValue(keyName, out var key))
         {
            key                   = new(keyName);
            _keyRegistry[keyName] = key;
         }
         
         return key;
      }

      public bool TryGetValue<T>(BlackBoardKey key, out T value)
      {
         if (_entities.TryGetValue(key, out var entity) && entity is BlackBoardEntity<T> castedEntity)
         {
            value = castedEntity.Value;

            return true;
         }

         value = default;

         return false;
      }
      
      public bool TryGetValue<T>(string keyName, out T value)
      {
         if (ContainsKeyInStore(keyName))
         {
            return TryGetValue(_keyRegistry[keyName],out value);
         }

         value = default;

         return false;
      }

      public T GetValue<T>(BlackBoardKey key)
      {
         TryGetValue(key, out T value);

         return value;
      }

      public T GetValue<T>(string keyName)
      {
         TryGetValue(keyName, out T value);

         return value;
      }
      
      public void SetValue<T>(string keyName, T value)
      {
         var key = GetOrRegisterKey(keyName);
         _entities[key] = new BlackBoardEntity<T>(key,value);
      }
      
      public void SetValue<T>(BlackBoardKey key, T value)
      {
         _entities[key] = new BlackBoardEntity<T>(key,value);
      }

      public bool ContainsKey(BlackBoardKey key) => _entities.ContainsKey(key);

      public bool ContainsKeyInStore(string keyName)
      {
         return _keyRegistry.TryGetValue(keyName, out var key) && ContainsKey(key);
      }

      public bool RemoveKey(BlackBoardKey key) => _entities.Remove(key);

   }

   [Serializable]
   public class BlackBoardEntity<T>
   {
      public BlackBoardKey Key       { get; }
      public T             Value     { get; }
      public Type          ValueType { get; }

      public BlackBoardEntity(BlackBoardKey key, T value)
      {
         this.Key       = key;
         this.Value     = value;
         this.ValueType = typeof(T);
      }

      public override bool Equals(object obj) => obj is BlackBoardEntity<T> other && other.Key == Key;

      public override int GetHashCode() => Key.GetHashCode();
   }
   
   
   [Serializable]
   public readonly struct BlackBoardKey : IEquatable<BlackBoardKey>
   {
      private readonly string _name;
      private readonly int    _hashedKey;

      public BlackBoardKey(string name)
      {
         _name      = name;
         _hashedKey = name.GetHashCode(StringComparison.Ordinal);
      }

      public bool Equals(BlackBoardKey other) => other._hashedKey == _hashedKey;

      public override bool Equals(object obj) => obj is BlackBoardKey other && Equals(other);

      public override int GetHashCode() => _hashedKey;

      public override string ToString() => _name;

      public static bool operator ==(BlackBoardKey lhs, BlackBoardKey rhs) => lhs._hashedKey == rhs._hashedKey;
      public static bool operator !=(BlackBoardKey lhs, BlackBoardKey rhs) => !(lhs == rhs);
   }
}