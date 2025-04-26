using UnityEngine;

namespace ETD.Scripts.InGame.View
{
    public abstract class View : MonoBehaviour{
        private const string BaseUrl = "Prefabs/View/";

        public static T Get<T>(string path = "") where T : View{
            var typeName = typeof(T).Name;

            if (!string.IsNullOrEmpty(path))
                path += "/";
            
            var viewPath = $"{BaseUrl}{path}{typeName}";
            var prefabs = Resources.Load(viewPath);
            if(prefabs == null) {
                return null;
            }

            if(((GameObject)Instantiate(prefabs)).TryGetComponent<T>(out var view))
            {
                view.SetActive(false);
                return view;
            }

            return null;
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }
    }
}