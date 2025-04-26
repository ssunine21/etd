using System;
using System.Collections.Generic;
using ETD.Scripts.Common;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ETD.Scripts.UI.ViewCanvas
{
    public class ViewCanvas : MonoBehaviour
    {
        public ViewAnimationType ViewAnimationType { get; private set; }
        public Canvas Canvas => _canvas;
        public CanvasGroup WrapCanvasGroup => _wrapCanvasGroup == null ? Init()._wrapCanvasGroup : _wrapCanvasGroup;
        public Image WrapBackground => _wrapBackground == null ? Init()._wrapBackground : _wrapBackground;
        public Color OriginBackgroundColor { get; private set; }
        
        public Button[] CloseButtons => closeButtons;
        public Transform WrapTr { get; private set; }
        
        public UnityAction OnBindOpen;
        public UnityAction OnBindClose;

        private const string BaseUrl = "Prefabs/ViewCanvas/";
        private static readonly Dictionary<string, ViewCanvas> Views = new();
        private static GameObject _viewParent;
        
        private Canvas _canvas;
        private CanvasGroup _wrapCanvasGroup;
        private Image _wrapBackground;
        
        [SerializeField] private Button[] closeButtons;

        public static T Get<T>() where T : ViewCanvas
        {
            var typeName = typeof(T).Name;
            var path = $"{BaseUrl}{typeName}";

            try
            {
                if (!Views.ContainsKey(typeName))
                {
                    if (!_viewParent) _viewParent = new GameObject("ViewCanvas");

                    var viewPref = Resources.Load(path);
                    var view = ((GameObject)Instantiate(viewPref, _viewParent.transform)).GetComponent<T>();
                    Views.Add(typeName, view);
                    var viewCanvas = Views[typeName];
                    
                    if (viewCanvas.TryGetComponent(out viewCanvas._canvas))
                    {
                        viewCanvas._canvas.worldCamera = Camera.main;
                        viewCanvas._canvas.sortingLayerName = "UI";
                    }
                    
                    viewCanvas.Init();
                }

                return Views[typeName] as T;
            }
            catch (Exception e)
            {
                Utility.LogError($"[{typeName} : ViewCanvas] " + e);
            }

            return null;
        }

        public virtual void SetActive(bool flag)
        {
            _canvas.enabled = flag;
        }
        
        public void SetViewAnimation(ViewAnimationType animationType)
        {
            ViewAnimationType = animationType;
        }

        protected ViewCanvas Init()
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("@wrap"))
                {
                    WrapTr = child.transform;
                    child.TryGetComponent(out _wrapCanvasGroup);
                    continue;
                }

                if (child.name.Contains("@background"))
                {
                    if (child.TryGetComponent(out _wrapBackground))
                        OriginBackgroundColor = _wrapBackground.color;
                }
            }

            return this;
        }

        private void OnApplicationQuit()
        {
            Destroy(_viewParent);
        }
    }
}