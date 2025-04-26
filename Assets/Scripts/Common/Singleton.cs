using System.Threading;
using UnityEngine;

namespace ETD.Scripts.Common
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance {
            get {
                if (_instance) return _instance;
            
                _instance = FindAnyObjectByType(typeof(T)) as T;
                if (_instance) return _instance;
            
                var typeName = $"@{typeof(T).Name}";
                var temp = new GameObject(typeName);
                temp.AddComponent<T>();
                if(temp.TryGetComponent(out _instance))
                {
                    DontDestroyOnLoad(temp);
                    return _instance;
                }

                return null;
            }
        }

        public abstract void Init(CancellationTokenSource cts);
    }
}
